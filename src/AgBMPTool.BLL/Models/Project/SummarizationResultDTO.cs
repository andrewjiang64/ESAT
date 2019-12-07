using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class SingleSummarizationResultDTO
    {
        public int LocationId { get; set; }
        public int ResultTypeId { get; set; }

        public decimal Value { get; set; }

        public decimal STD { get; set; }
    }

    public class SummarizationResultDTO
    {
        public int LocationId { get; set; }

        public int SubAreaId { get; set; } = -1;

        public decimal Area { get; set; }

        public decimal Elevation { get; set; }

        public decimal Slope { get; set; }

        public string Landuse { get; set; }

        public string SoilTexture { get; set; }

        public List<SingleSummarizationResultDTO> Results { get; set; } = new List<SingleSummarizationResultDTO>();

        #region Aggregate

        /// <summary>
        /// Aggregate subarea result to a certain level defined in locaiton
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="subAreaResults"></param>
        /// <returns></returns>
        public static List<SummarizationResultDTO> AggregateSummary(List<SummarizationResultDTO> locations, List<SubAreaResultDTO> subAreaResults)
        {
            var ret = AggregateBasicProperties(locations);
            var values = AggregateValues(locations, subAreaResults);

            foreach (var r in ret)
                r.Results = values.Where(x => x.LocationId == r.LocationId).ToList();

            return ret;
        }

        #region Model Results

        /// <summary>
        /// Is average weighted average will be applied to given model result type. If it's not, we will use sum. 
        /// </summary>
        /// <param name="modelResultTypeId"></param>
        /// <returns></returns>
        private static bool IsAreaWeightedAverage(int modelResultTypeId)
        {
            return modelResultTypeId <= 6 || modelResultTypeId >= 15;
        }

        private static List<SingleSummarizationResultDTO> AggregateValues(List<SummarizationResultDTO> locations, List<SubAreaResultDTO> subAreaResults)
        {
            //we will loop all result types and do the aggregate
            var resultTypeIds = subAreaResults.Select(x => x.modelresulttypeid).Distinct().ToList();

            var ret = new List<SingleSummarizationResultDTO>();
            foreach (var typeId in resultTypeIds)
                ret.AddRange(AggregateValues(locations, subAreaResults, typeId));

            return ret;
        }
  
        /// <summary>
        /// Aggregate value 
        /// </summary>
        /// <param name="locations">the mapping table from subarea to location</param>
        /// <param name="subAreaResults">the subarea results of all years and all types</param>
        /// <param name="modelResultTypeId">the model result type id</param>
        /// <returns></returns>
        private static List<SingleSummarizationResultDTO> AggregateValues(List<SummarizationResultDTO> locations, List<SubAreaResultDTO> subAreaResults, int modelResultTypeId)
        {
            //filter the result with model result type id
            var allresults = from loc in locations
                             join results in subAreaResults on loc.SubAreaId equals results.subareaid
                             where results.modelresulttypeid == modelResultTypeId
                             select new
                             {
                                 loc.LocationId,
                                 loc.SubAreaId,
                                 loc.Area,
                                 results.resultyear,
                                 results.resultvalue
                             };

            //we will do aggregate for each year 
            //the aggregate method will be different for different model result type per design
            var yearlyresult = from results in allresults
                               group results by new { results.LocationId, results.resultyear } into groups
                               select new
                               {
                                   groups.Key.LocationId,
                                   groups.Key.resultyear,
                                   resultvalue = IsAreaWeightedAverage(modelResultTypeId) ? groups.Sum(x => x.resultvalue * x.Area) / groups.Sum(x => x.Area) : groups.Sum(x => x.resultvalue)
                               };

            //we will get annual average first
            var avgresult = from results in yearlyresult
                            group results by results.LocationId into groups
                            select new
                            {
                                LocationId = groups.Key,
                                Avg = groups.Average(x => x.resultvalue),
                            };

            //then calculate the std
            var finalresult = from results in yearlyresult
                              join avg in avgresult on results.LocationId equals avg.LocationId
                              group new { results.LocationId, results.resultvalue, results.resultyear, avg.Avg } by results.LocationId into groups
                              select new SingleSummarizationResultDTO
                              {
                                  LocationId = groups.Key,
                                  ResultTypeId = modelResultTypeId,
                                  Value = groups.First().Avg,
                                  STD = Convert.ToDecimal(Math.Sqrt(groups.Sum(x => Math.Pow(Convert.ToDouble(x.resultvalue - x.Avg), 2)) / groups.Count())) //calculate std
                              };

            return finalresult.ToList();
        }

        #endregion

        #region Basic Properties

        /// <summary>
        /// Aggregate all basic properties for locaton
        /// </summary>
        /// <param name="locations"></param>
        /// <returns></returns>
        private static List<SummarizationResultDTO> AggregateBasicProperties(List<SummarizationResultDTO> locations)
        {
            var areaElevationAndSlope = AggregateAreaElevationAndSlope(locations);
            var landuse = AggregateLanduse(locations);
            var soiltexture = AggregateSoilTexture(locations);

            var query = from part1 in areaElevationAndSlope
                   join part2 in landuse on part1.LocationId equals part2.LocationId
                   join part3 in soiltexture on part1.LocationId equals part3.LocationId
                   select new SummarizationResultDTO
                   {
                       LocationId = part1.LocationId,
                       Area = part1.Area,
                       Elevation = part1.Elevation,
                       Slope = part1.Slope,
                       Landuse = part2.Landuse,
                       SoilTexture = part3.SoilTexture
                   };
            return query.ToList();
        }

        /// <summary>
        /// Aggregate Area, Elevation and Slope for subbarea to location 
        /// </summary>
        /// <param name="locations"></param>
        /// <returns></returns>
        private static List<SummarizationResultDTO> AggregateAreaElevationAndSlope(List<SummarizationResultDTO> locations)
        {
            var query = 
                from loc in locations
                group new
                {
                    loc.LocationId,
                    loc.Area,
                    loc.Elevation,
                    loc.Slope
                } by loc.LocationId into groups
                orderby groups.Key
                select new SummarizationResultDTO
                {
                    LocationId = groups.Key,
                    Area = groups.Sum(x => x.Area),
                    Elevation = groups.Sum(x => x.Elevation * x.Area) / groups.Sum(x => x.Area),
                    Slope = groups.Sum(x => x.Slope * x.Area) / groups.Sum(x => x.Area)
                };
            return query.ToList();
        }

        /// <summary>
        /// Aggregate landuse
        /// </summary>
        /// <param name="locations"></param>
        /// <returns></returns>
        private static List<SummarizationResultDTO> AggregateLanduse(List<SummarizationResultDTO> locations)
        {
            //get the total area of each landuse at each location
            var aggregate_landuse = from loc in locations
                                    group new
                                    {
                                        loc.LocationId,
                                        loc.Landuse,
                                        loc.Area
                                    } by new { loc.LocationId, loc.Landuse } into groups
                                    select new
                                    {
                                        groups.Key.LocationId,
                                        groups.Key.Landuse,
                                        Area = groups.Sum(x => x.Area)
                                    };

            //get the landuse with largest area
            var query =              from landuse in aggregate_landuse
                                     group landuse by landuse.LocationId into groups
                                     select new SummarizationResultDTO
                                     {
                                         LocationId = groups.Key,
                                         Landuse = groups.OrderBy(x => x.Area).First().Landuse
                                     };
            return query.ToList();
        }

        /// <summary>
        /// Aggregate landuse
        /// </summary>
        /// <param name="locations"></param>
        /// <returns></returns>
        private static List<SummarizationResultDTO> AggregateSoilTexture(List<SummarizationResultDTO> locations)
        {
            //get the total area of each landuse at each location
            var aggregate_soiltexture = from loc in locations
                                    group new
                                    {
                                        loc.LocationId,
                                        loc.SoilTexture,
                                        loc.Area
                                    } by new { loc.LocationId, loc.SoilTexture } into groups
                                    select new
                                    {
                                        groups.Key.LocationId,
                                        groups.Key.SoilTexture,
                                        Area = groups.Sum(x => x.Area)
                                    };

            //get the landuse with largest area
            var query = from landuse in aggregate_soiltexture
                   group landuse by landuse.LocationId into groups
                   select new SummarizationResultDTO
                   {
                       LocationId = groups.Key,
                       SoilTexture = groups.OrderBy(x => x.Area).First().SoilTexture
                   };

            return query.ToList();
        }

        #endregion

        #endregion

    }
}
