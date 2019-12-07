using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using AgBMTool.DBL.Interface;
using AgBMPTool.BLL.Models.Project;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.User;
using AgBMPTool.BLL.Models.Overview;
using AgBMPTool.BLL.Models.Shared;
using AgBMPTool.BLL.Services.Utilities;
using System.Reflection;
using System.ComponentModel;
using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.Boundary;
using static AgBMPTool.BLL.Enumerators.Enumerators;
using System.Data;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using AgBMPTool.BLL.Models.BaselineInformation;
using AgBMPTool.DBModel.Model.Optimization;
using AgBMPTool.BLL.Models.Utility;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using MoreLinq;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO;
using System.IO;
using NetTopologySuite.Features;
using AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData;
using AgBMPTool.BLL.Services.Users;
using AgBMPTool.DBModel.Model.Solution;

namespace AgBMPTool.BLL.Services.Projects
{
    public class ProjectDataService : IProjectDataService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUtilitiesDataService _IUtilitiesDataService;
        private readonly IProjectSummaryService _IProjectSummaryService;
        private readonly IUserDataServices _iUserDataService;
        private readonly IInteligentRecommendationService _iInteligentRecommendationService;

        public ProjectDataService(IUnitOfWork _iUnitOfWork, IUtilitiesDataService IUtilitiesDataService, IProjectSummaryService IProjectSummaryService, IUserDataServices _iUserDataService,
            IInteligentRecommendationService iInteligentRecommendationService)
        {
            this._uow = _iUnitOfWork;
            this._IUtilitiesDataService = IUtilitiesDataService;
            this._IProjectSummaryService = IProjectSummaryService;
            this._iUserDataService = _iUserDataService;
            this._iInteligentRecommendationService = iInteligentRecommendationService;
        }

        /// <summary>
        /// Get Projects List by UserId
        /// </summary>
        /// <param name="userId">Logged in User Id</param>
        /// <returns>List of Projects for User</returns>
        public List<ProjectDTO> GetProjectListByUserId(int userId)
        {
            var res = new List<ProjectDTO>();

            var userProjects = _uow.GetRepository<Project>().Get(x => x.UserId == userId && x.Active == true).Select(x => x).ToList();

            foreach (var project in userProjects)
            {
                res.Add(new ProjectDTO()
                {
                    Id = project.Id,
                    Description = project.Description,
                    Name = project.Name,
                    CreatedDate = project.Created,
                    ScenarioType = project.ScenarioType
                });
            }


            return res;
        }

        public List<GridColumnsDTO> GetOverviewSummaryColumns(int summerizationLevelId)
        {
            var columns = new List<GridColumnsDTO>();

            try
            {
                var summarizationType = _IUtilitiesDataService.GetSummarizationTypeOptionBySummerizationLevelId(summerizationLevelId).ToList().Select(x => x.ItemName).FirstOrDefault().ToString();

                var subareaModelResultType = GetSubAreaModelResultType();

                GridColumnsDTO idCol = new GridColumnsDTO();
                idCol.FieldName = "id";
                idCol.FieldTitle = summarizationType;
                columns.Add(idCol);

                //here some fixed comlumns
                idCol = new GridColumnsDTO();
                idCol.FieldName = "Area";
                idCol.FieldTitle = "Area (ha)";
                columns.Add(idCol);

                idCol = new GridColumnsDTO();
                idCol.FieldName = "Elevation";
                idCol.FieldTitle = "Elevation (m)";
                columns.Add(idCol);

                idCol = new GridColumnsDTO();
                idCol.FieldName = "Slope";
                idCol.FieldTitle = "Slope (%)";
                columns.Add(idCol);

                idCol = new GridColumnsDTO();
                idCol.FieldName = "LandUse";
                idCol.FieldTitle = "Dominant Landuse";
                columns.Add(idCol);

                idCol = new GridColumnsDTO();
                idCol.FieldName = "SoilTexture";
                idCol.FieldTitle = "Dominant Soil Texture";
                columns.Add(idCol);

                foreach (var column in subareaModelResultType)
                {
                    string unit = string.Empty;
                    if (column.ScenarioModelResultVariableTypeId <= 2)
                        unit = column.UnitSymbol;
                    else if (column.ScenarioModelResultVariableTypeId <= 6)
                        unit = "mm";
                    else if (column.ScenarioModelResultVariableTypeId <= 14)
                        unit = column.UnitSymbol + "/ha";

                    if (!string.IsNullOrEmpty(unit))
                        unit = " (" + unit + ")";

                    GridColumnsDTO col = new GridColumnsDTO();
                    col.FieldName = column.Name.Replace(" ", "");
                    col.FieldTitle = column.Name + unit;
                    col.ModelResultTypeId = column.Id;
                    columns.Add(col);

                    col = new GridColumnsDTO();
                    col.FieldName = column.Name.Replace(" ", "") + "_STD";
                    col.FieldTitle = column.Name + " STD" + unit;
                    col.ModelResultTypeId = -column.Id; // we use negative id for the std
                    columns.Add(col);
                }
            }
            catch (Exception ex)
            {

            }
            return columns;
        }

        /// <summary>
        /// Get all Model Result Types for subarea
        /// </summary>
        /// <returns></returns>
        public List<ScenarioModelResultTypeDTO> GetSubAreaModelResultType()
        {
            var scenarioModelResultType = new List<ScenarioModelResultTypeDTO>();
            try
            {
                scenarioModelResultType = _uow.GetRepository<ScenarioModelResultType>().Query().Include(x => x.UnitType).
                    Where(x => x.ModelComponentTypeId == 1).
                    Select(x => new ScenarioModelResultTypeDTO(x, x.UnitType)).ToList().OrderBy(x => x.SortOrder).ToList();
            }
            catch (Exception ex)
            {

            }
            return scenarioModelResultType;
        }

        public SummaryTableDTO GetOverviewSummaryTable(int baselineId, int summerizationLevelId, int locationFilter_MunicipalityId, int locationFilter_WatershedId,
          int locationFilter_SubwatershedId, int startYear, int endYear, int userId)
        {
            SummaryTableDTO summaryTable = new SummaryTableDTO();

            var overviewSummaryDataTable = new DataTable();

            try
            {
                summaryTable.SummaryTableColumns = GetOverviewSummaryColumns(summerizationLevelId);

                foreach (var resultType in summaryTable.SummaryTableColumns)
                {
                    DataColumn column = new DataColumn();
                    column.ColumnName = resultType.FieldName.ToLower();
                    column.DataType = typeof(string);
                    column.Caption = resultType.FieldTitle;
                    overviewSummaryDataTable.Columns.Add(column);
                }

                overviewSummaryDataTable = GetOverviewSummaryData(summaryTable.SummaryTableColumns, overviewSummaryDataTable, baselineId, summerizationLevelId, locationFilter_MunicipalityId,
                    locationFilter_WatershedId, locationFilter_SubwatershedId, startYear, endYear, userId);

                summaryTable.SummaryTableData = overviewSummaryDataTable;

            }
            catch (Exception ex)
            {

            }
            return summaryTable;
        }



        /// <summary>
        /// Dummy function to get the data for overview summary table. Need to be finished by Michael.
        /// </summary>
        /// <param name="baselineId"></param>
        /// <param name="summerizationLevelId"></param>
        /// <param name="locationFilter_MunicipalityId"></param>
        /// <param name="locationFilter_WatershedId"></param>
        /// <param name="locationFilter_SubwatershedId"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private DataTable GetOverviewSummaryData(List<GridColumnsDTO> summaryTableColumns, DataTable overviewSummaryDataTable, int baselineId,
            int summerizationLevelId, int locationFilter_MunicipalityId, int locationFilter_WatershedId, int locationFilter_SubwatershedId,
            int startYear, int endYear, int userId)
        {
            //get model results first
            var modelResults = _uow.ExecuteProcedure<UserModelResultDTO>(
                string.Format("select * from agbmptool_getusersummaryresult({0},{1},{2},{3},{4},{5},{6},{7})",
                userId, locationFilter_MunicipalityId, locationFilter_WatershedId, locationFilter_SubwatershedId,
                summerizationLevelId, startYear, endYear, baselineId));

            var locationInfo = _uow.ExecuteProcedure<UserLocationInfo>(
                string.Format("select * from agbmptool_getuserlocationinfo({0},{1},{2},{3},{4})",
                userId, locationFilter_MunicipalityId, locationFilter_WatershedId, locationFilter_SubwatershedId,
                summerizationLevelId));


            //get column index for each result type
            var resultIndex = new Dictionary<int, int>();
            for (int i = 0; i < summaryTableColumns.Count; i++)
            {
                if (summaryTableColumns[i].ModelResultTypeId != 0)
                    resultIndex.Add(summaryTableColumns[i].ModelResultTypeId, i);
            }

            //insert table rows
            foreach (var loc in locationInfo)
            {
                DataRow dr = null;
                dr = overviewSummaryDataTable.NewRow(); // have new row on each iteration

                dr["id"] = loc.locationid;
                dr["Area"] = Math.Round(loc.area, 4);
                dr["Elevation"] = Math.Round(loc.elevation);
                dr["Slope"] = Math.Round(loc.slope, 2);
                dr["LandUse"] = loc.landuse;
                dr["SoilTexture"] = loc.soiltexture;

                //add results with three decimal
                foreach (var r in modelResults.Where(x => x.locationid == loc.locationid))
                {
                    dr[resultIndex[r.resulttype]] = Math.Round(r.resultvalue, 3);
                    dr[resultIndex[-r.resulttype]] = Math.Round(r.stdvalue, 3);
                }

                overviewSummaryDataTable.Rows.Add(dr);
            }
            return overviewSummaryDataTable;
        }

        private List<SummarizationResultDTO> GetAggregateLocationFromSubArea(int summerizationLevelId, List<int> subareaids)
        {
            IQueryable<SummarizationResultDTO> locations = null;
            switch (summerizationLevelId)
            {
                case (int)(ScenarioResultSummarizationTypeEnum.LSD):
                    locations = from lsd in _uow.GetRepository<LegalSubDivision>().Query()
                                join subarea in _uow.GetRepository<SubArea>().Query() on lsd.Id equals subarea.LegalSubDivisionId
                                where subareaids.Contains(subarea.Id)
                                select new SummarizationResultDTO
                                {
                                    LocationId = lsd.Id,
                                    SubAreaId = subarea.Id,
                                    Area = subarea.Area,
                                    Elevation = subarea.Elevation,
                                    Slope = subarea.Slope,
                                    Landuse = subarea.LandUse,
                                    SoilTexture = subarea.SoilTexture
                                };

                    break;
                case (int)(ScenarioResultSummarizationTypeEnum.Parcel):
                    locations = from parcel in _uow.GetRepository<Parcel>().Query()
                                join subarea in _uow.GetRepository<SubArea>().Query() on parcel.Id equals subarea.ParcelId
                                where subareaids.Contains(subarea.Id)
                                select new SummarizationResultDTO
                                {
                                    LocationId = parcel.Id,
                                    SubAreaId = subarea.Id,
                                    Area = subarea.Area,
                                    Elevation = subarea.Elevation,
                                    Slope = subarea.Slope,
                                    Landuse = subarea.LandUse,
                                    SoilTexture = subarea.SoilTexture
                                };
                    break;
                case (int)(ScenarioResultSummarizationTypeEnum.Subwatershed):
                    locations = from subwatershed in _uow.GetRepository<SubWatershed>().Query()
                                join subbasin in _uow.GetRepository<Subbasin>().Query() on subwatershed.Id equals subbasin.SubWatershedId
                                join subarea in _uow.GetRepository<SubArea>().Query() on subbasin.Id equals subarea.SubbasinId
                                where subareaids.Contains(subarea.Id)
                                select new SummarizationResultDTO
                                {
                                    LocationId = subwatershed.Id,
                                    SubAreaId = subarea.Id,
                                    Area = subarea.Area,
                                    Elevation = subarea.Elevation,
                                    Slope = subarea.Slope,
                                    Landuse = subarea.LandUse,
                                    SoilTexture = subarea.SoilTexture
                                };

                    break;
                case (int)(ScenarioResultSummarizationTypeEnum.Watershed):
                    locations = from watershed in _uow.GetRepository<Watershed>().Query()
                                join subwatershed in _uow.GetRepository<SubWatershed>().Query() on watershed.Id equals subwatershed.WatershedId
                                join subbasin in _uow.GetRepository<Subbasin>().Query() on subwatershed.Id equals subbasin.SubWatershedId
                                join subarea in _uow.GetRepository<SubArea>().Query() on subbasin.Id equals subarea.SubbasinId
                                where subareaids.Contains(subarea.Id)
                                select new SummarizationResultDTO
                                {
                                    LocationId = watershed.Id,
                                    SubAreaId = subarea.Id,
                                    Area = subarea.Area,
                                    Elevation = subarea.Elevation,
                                    Slope = subarea.Slope,
                                    Landuse = subarea.LandUse,
                                    SoilTexture = subarea.SoilTexture
                                };

                    break;
                case (int)(ScenarioResultSummarizationTypeEnum.Municipality):
                    locations = from municipality in _uow.GetRepository<Municipality>().Query()
                                from subarea in _uow.GetRepository<SubArea>().Query()
                                where subareaids.Contains(subarea.Id) && subarea.Geometry.Intersects(municipality.Geometry)
                                select new SummarizationResultDTO
                                {
                                    LocationId = municipality.Id,
                                    SubAreaId = subarea.Id,
                                    Area = subarea.Area,
                                    Elevation = subarea.Elevation,
                                    Slope = subarea.Slope,
                                    Landuse = subarea.LandUse,
                                    SoilTexture = subarea.SoilTexture
                                };

                    break;
                case (int)(ScenarioResultSummarizationTypeEnum.Farm):
                    locations = from farm in _uow.GetRepository<Farm>().Query()
                                from subarea in _uow.GetRepository<SubArea>().Query()
                                where subareaids.Contains(subarea.Id) && subarea.Geometry.Intersects(farm.Geometry)
                                select new SummarizationResultDTO
                                {
                                    LocationId = farm.Id,
                                    SubAreaId = subarea.Id,
                                    Area = subarea.Area,
                                    Elevation = subarea.Elevation,
                                    Slope = subarea.Slope,
                                    Landuse = subarea.LandUse,
                                    SoilTexture = subarea.SoilTexture
                                };
                    break;
                default:

                    break;
            }

            return locations.ToList();
        }

        /// <summary>
        /// Get the chart data for Project cost effectiveness (Function added by University of Guelph)
        /// </summary>
        /// <param name="userId">logged in user id</param>
        /// <param name="effectivenessTypeId">user selected effectiveness</param>
        /// <returns></returns>
        public List<ProjectsCostEffectivenessChartDTO> GetProjectsBMPCostByEffectivenessTypeId(int userId, int effectivenessTypeId)
        {
            var projectBMPCostEffectiveness = new List<ProjectsCostEffectivenessChartDTO>();
            try
            {
                var userProjects = GetProjectListByUserId(userId);

                var projectSummaries = _IProjectSummaryService.GetProjectSummaryDTOs(userProjects);

                foreach (var projectSummary in projectSummaries)
                {
                    var effectDTO = projectSummary.EffectivenessSummaryDTOs.Find(x => x.BMPEffectivenessType.Id == effectivenessTypeId);
                    ProjectsCostEffectivenessChartDTO projectBMPCost = new ProjectsCostEffectivenessChartDTO
                    {
                        ProjectId = projectSummary.Id,
                        ProjectName = projectSummary.Name,
                        Description = projectSummary.Description,
                        BMPCost = (double)projectSummary.Cost,
                        Effectiveness = effectDTO == null ? 0 : (double)effectDTO.ValueChange,
                        EffectivenessTypeId = effectivenessTypeId
                    };

                    projectBMPCostEffectiveness.Add(projectBMPCost);
                }

            }
            catch (Exception ex)
            {

            }
            return projectBMPCostEffectiveness;
        }

        #region Location Filters (Overview and Basline information Page)

        /// <summary>
        /// Get Municipalties by UserId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userTypeId"></param>
        /// <returns></returns>
        public List<ProjectScopeDTO> GetMunicipalitiesByUserId(int userId, int userTypeId)
        {
            var scopeList = new List<ProjectScopeDTO>();

            if (userTypeId == (int)Enumerators.Enumerators.UserType.MunicipalityManager)
            {
                scopeList = (from um in _uow.GetRepository<UserMunicipalities>().Query()
                             join m in _uow.GetRepository<Municipality>().Query() on um.MunicipalityId equals m.Id
                             where um.UserId == userId
                             select new ProjectScopeDTO(m.Id, m.Name, (int)ScenarioResultSummarizationTypeEnum.Municipality)).Distinct().ToList();

            }
            else if (userTypeId == (int)Enumerators.Enumerators.UserType.WatershedManager)
            {
                scopeList = (from uw in _uow.GetRepository<UserWatersheds>().Query()
                             from municipality in _uow.GetRepository<Municipality>().Query()
                             join w in _uow.GetRepository<Watershed>().Query() on uw.WatershedId equals w.Id
                             where uw.UserId == userId || municipality.Geometry.Intersects(w.Geometry)
                             select new ProjectScopeDTO(municipality.Id, municipality.Name, (int)ScenarioResultSummarizationTypeEnum.Municipality)).Distinct().ToList();

            }

            return scopeList;
        }

        public List<BaseItemDTO> GetWatershedsByMunicipality(int userId, int userTypeId, int municipalityId)
        {
            var resultList = new List<BaseItemDTO>();

            if (userTypeId == (int)Enumerators.Enumerators.UserType.MunicipalityManager)
            {
                if (municipalityId > 0)
                    resultList = (from um in _uow.GetRepository<UserMunicipalities>().Query()
                                  from w in _uow.GetRepository<Watershed>().Query()
                                  join otherM in _uow.GetRepository<Municipality>().Query() on um.MunicipalityId equals otherM.Id
                                  where um.UserId == userId && otherM.Geometry.Intersects(w.Geometry) && um.MunicipalityId == municipalityId
                                  select new BaseItemDTO { Id = w.Id, Name = w.Name }).Distinct().ToList();
                else
                    resultList = (from um in _uow.GetRepository<UserMunicipalities>().Query()
                                  from w in _uow.GetRepository<Watershed>().Query()
                                  join otherM in _uow.GetRepository<Municipality>().Query() on um.MunicipalityId equals otherM.Id
                                  where um.UserId == userId && otherM.Geometry.Intersects(w.Geometry)
                                  select new BaseItemDTO { Id = w.Id, Name = w.Name }).Distinct().ToList();
            }
            else if (userTypeId == (int)Enumerators.Enumerators.UserType.WatershedManager)
            {
                if (municipalityId > 0)
                    resultList = (from uw in _uow.GetRepository<UserWatersheds>().Query()
                                  from m in _uow.GetRepository<Municipality>().Query()
                                  join w in _uow.GetRepository<Watershed>().Query() on uw.WatershedId equals w.Id
                                  where uw.UserId == userId && m.Geometry.Intersects(w.Geometry) && m.Id == municipalityId
                                  select new BaseItemDTO { Id = w.Id, Name = w.Name }).Distinct().ToList();
                else
                    resultList = (from uw in _uow.GetRepository<UserWatersheds>().Query()
                                  from m in _uow.GetRepository<Municipality>().Query()
                                  join w in _uow.GetRepository<Watershed>().Query() on uw.WatershedId equals w.Id
                                  where uw.UserId == userId && m.Geometry.Intersects(w.Geometry)
                                  select new BaseItemDTO { Id = w.Id, Name = w.Name }).Distinct().ToList();
            }

            return resultList;
        }

        public List<BaseItemDTO> GetSubWatershedsByWatershedId(int userId, int userTypeId, int municipalityId, int watershedId)
        {
            var resultList = new List<BaseItemDTO>();

            if (watershedId > 0)
            {
                resultList = (from subWatershed in _uow.GetRepository<SubWatershed>().Query()
                              where subWatershed.WatershedId == watershedId
                              select new BaseItemDTO { Id = subWatershed.Id, Name = subWatershed.Name }).Distinct().ToList();
            }
            else
            {
                var watershedIds = GetWatershedsByMunicipality(userId, userTypeId, municipalityId).ToList().Select(x => x.Id).ToList();

                resultList = (from subWatershed in _uow.GetRepository<SubWatershed>().Query()
                              where watershedIds.Contains(subWatershed.WatershedId)
                              select new BaseItemDTO { Id = subWatershed.Id, Name = subWatershed.Name }).Distinct().ToList();
            }
            return resultList;
        }

        #endregion

        #region Location Filters (BMP Scope and BMP Selection Page)

        /// <summary>
        /// Get Municipalties by UserId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userTypeId"></param>
        /// <returns></returns>
        public List<ProjectScopeDTO> GetProjectMunicipalitiesByProjectId(int projectId, int userId, int userTypeId)
        {
            return _uow.ExecuteProcedure<ProjectScopeDTO>(
                string.Format("select * from agbmptool_getprojectmunicipalities({0})",
                projectId)).ToList();
        }

        public List<BaseItemDTO> GetProjectWatershedsByMunicipality(int projectId, int userId, int userTypeId, int municipalityId)
        {
            return _uow.ExecuteProcedure<BaseItemDTO>(
                string.Format("select * from agbmptool_getprojectwatersheds({0},{1})",
                projectId, municipalityId)).ToList();
        }

        #endregion


        /// <summary>
        /// Get Start and End Year for Overview Page
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userType"></param>
        /// <returns></returns>
        public StartAndEndRangeDTO GetStartAndEndYearForOverviewByUserIdAndUserType(int userId, int scenarioTypeId)
        {
            return _uow.ExecuteProcedure<StartAndEndRangeDTO>(
                string.Format("select * from agbmptool_getuserstartendyear({0},{1},'false',-1,-1,-1)",
                userId, scenarioTypeId)).FirstOrDefault();
        }

        /// <summary>
        /// Dummy Function to get Start and End Year for Add Project Popup
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userType"></param>
        /// <returns></returns>
        public StartAndEndRangeDTO GetStartAndEndYearForAddProjectByUserIdAndUserType(int userId)
        {
            return _uow.ExecuteProcedure<StartAndEndRangeDTO>(
                string.Format("select * from agbmptool_getuserstartendyear({0},1,'true',-1,-1,-1)",
                userId)).FirstOrDefault();
        }

        public List<BaseItemDTO> GetProjectSpatialUnitType()
        {
            return _uow.GetRepository<ProjectSpatialUnitType>().Get().Select(x => new BaseItemDTO() { Id = x.Id, Name = x.Name }).ToList();
        }

        public void SaveProject(int userId, ProjectDTO project)
        {
            // Add project
            var pro = new Project();
            pro.Name = project.Name;
            pro.Description = project.Description;
            pro.Created = DateTime.Now;
            pro.Modified = DateTime.Now;
            pro.StartYear = project.StartYear;
            pro.EndYear = project.EndYear;
            //pro.ScenariodId
            pro.ProjectSpatialUnitTypeId = project.SpatialUnitId;
            pro.ScenarioTypeId = project.ScenarioTypeId;
            pro.UserId = userId;
            pro.Active = true;

            _uow.GetRepository<Project>().Add(pro);
            _uow.Commit();

            foreach (var scope in project.Scope)
            {
                if (scope.ScenarioResultSummarizationTypeId == (int)ScenarioResultSummarizationTypeEnum.Watershed)
                {
                    var projectWatersheds = _uow.GetRepository<ProjectWatersheds>().Get(x => x.ProjectId == pro.Id && x.WatershedId == scope.Id).FirstOrDefault();
                    if (projectWatersheds == null)
                    {
                        projectWatersheds = new ProjectWatersheds();
                        projectWatersheds.ProjectId = pro.Id;
                        projectWatersheds.WatershedId = scope.Id;

                        _uow.GetRepository<ProjectWatersheds>().Add(projectWatersheds);
                    }
                }
                else if (scope.ScenarioResultSummarizationTypeId == (int)ScenarioResultSummarizationTypeEnum.Municipality)
                {
                    var projectMunicipality = _uow.GetRepository<ProjectMunicipalities>().Get(x => x.ProjectId == pro.Id && x.MunicipalityId == scope.Id).FirstOrDefault();
                    if (projectMunicipality == null)
                    {
                        projectMunicipality = new ProjectMunicipalities();
                        projectMunicipality.ProjectId = pro.Id;
                        projectMunicipality.MunicipalityId = scope.Id;

                        _uow.GetRepository<ProjectMunicipalities>().Add(projectMunicipality);
                    }
                }
            }

            _uow.Commit();

            // Save Default Optimization Constraint And Weight
            AddDefaultConstraintAndWeight(pro.Id);
            AddDefaultBMPLocations(pro.Id);
        }

        /// <summary>
        /// Add Default Optimization Constraint And Weight
        /// </summary>
        public void AddDefaultConstraintAndWeight(int projectId)
        {
            if (projectId != 0)
            {
                // Save default optimization
                var optimization = new Optimization();
                optimization.ProjectId = projectId;
                optimization.OptimizationTypeId = (int)OptimizationTypeEnum.EcoService;

                _uow.GetRepository<Optimization>().Add(optimization);
                _uow.Commit();

                // save default Optimization Constraints and Weights
                // we will remove cost from consideration
                var bmpEffectivenessTypeList = _uow.GetRepository<BMPEffectivenessType>().Query().
                    Where(x => !x.Name.ToLower().Contains("cost"));

                foreach (var type in bmpEffectivenessTypeList)
                {
                    //we only default constraint here for new projects
                    if (type.DefaultConstraint != null && type.DefaultConstraintTypeId != null)
                    {
                        var optimizationConstraints = new OptimizationConstraints();
                        optimizationConstraints.OptimizationId = optimization.Id;
                        optimizationConstraints.BMPEffectivenessTypeId = type.Id;
                        optimizationConstraints.OptimizationConstraintValueTypeId = type.DefaultConstraintTypeId.Value;
                        optimizationConstraints.Constraint = type.DefaultConstraint.Value;

                        _uow.GetRepository<OptimizationConstraints>().Add(optimizationConstraints);
                    }

                    //we will add all weights for the new project
                    var optimizationWeights = new OptimizationWeights();
                    optimizationWeights.OptimizationId = optimization.Id;
                    optimizationWeights.BMPEffectivenessTypeId = type.Id;
                    optimizationWeights.Weight = type.DefaultWeight;
                    _uow.GetRepository<OptimizationWeights>().Add(optimizationWeights);
                }

                _uow.Commit();
            }
        }

        /// <summary>
        /// Add default bmp locations
        /// </summary>
        /// <param name="projectId"></param>
        private void AddDefaultBMPLocations(int projectId)
        {
            try
            {
                _uow.ExecuteProcedure(string.Format("call agbmptool_setprojectdefaultbmplocations({0})", projectId));
            }
            catch { }
        }
        public void DeleteProject(int projectId)
        {
            var project = _uow.GetRepository<Project>().Get(x => x.Id == projectId);
            if (project.FirstOrDefault() != null)
            {
                project.FirstOrDefault().Active = false;
                _uow.Commit();
            }
        }


        #region Baseline Information

        /// <summary>
        /// Get Summary table data (function added by University of Guelph)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<BaselineBMPSummaryDTO> GetBaselineBMPSummaryData(int userId, int projectId)
        {
            var result = new List<BaselineBMPSummaryDTO>();

            foreach (var summary in
                _IProjectSummaryService.GetProjectBaselineBMPSummaryDTOs(projectId))
            {
                result.Add(new BaselineBMPSummaryDTO
                {
                    Id = summary.BMPTypeId,
                    BMP = summary.BMPTypeName,
                    Count = summary.ModelComponentCount,
                    Area = summary.TotalArea,
                    Cost = summary.TotalCost
                });
            }

            return result;
        }

        /// <summary>
        /// Get effectiveness type table data (function added by University of Guelph)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<BMPEffectivenessSummaryDTO> GetBaselineBMPEffectivenessData(int userId, int projectId)
        {
            var result = new List<BMPEffectivenessSummaryDTO>();

            foreach (var summary in _IProjectSummaryService.GetProjectBaselineEffectivenessSummaryDTOs(projectId))
            {
                result.Add(new BMPEffectivenessSummaryDTO
                {
                    Id = summary.BMPEffectivenessType.Id,
                    Parameter = summary.Parameter,
                    Value = summary.Value
                });
            }

            return result;
        }


        /// <summary>
        /// Dummy Function to get summary table for baseline
        /// </summary>
        /// <param name="summerizationLevelId"></param>
        /// <param name="locationFilter_MunicipalityId"></param>
        /// <param name="locationFilter_WatershedId"></param>
        /// <param name="locationFilter_SubwatershedId"></param>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public SummaryTableDTO GetBaselineSummaryTable(int summerizationLevelId, int locationFilter_MunicipalityId, int locationFilter_WatershedId,
            int locationFilter_SubwatershedId, int projectId, int userId)
        {
            SummaryTableDTO summaryTable = new SummaryTableDTO();

            var summaryDataTable = new DataTable();

            try
            {
                summaryTable.SummaryTableColumns = GetOverviewSummaryColumns(summerizationLevelId);

                foreach (var resultType in summaryTable.SummaryTableColumns)
                {
                    DataColumn column = new DataColumn();
                    column.ColumnName = resultType.FieldName.ToLower();
                    column.DataType = typeof(string);
                    column.Caption = resultType.FieldTitle;
                    summaryDataTable.Columns.Add(column);
                }

                summaryDataTable = GetBaselineSummaryData(summaryTable.SummaryTableColumns, summaryDataTable, summerizationLevelId, locationFilter_MunicipalityId,
                    locationFilter_WatershedId, locationFilter_SubwatershedId, projectId, userId);

                summaryTable.SummaryTableData = summaryDataTable;

            }
            catch (Exception ex)
            {

            }
            return summaryTable;
        }


        /// <summary>
        /// Dummy function to get the data for overview summary table. Need to be finished by Michael.
        /// </summary>
        /// <param name="baselineId"></param>
        /// <param name="summerizationLevelId"></param>
        /// <param name="locationFilter_MunicipalityId"></param>
        /// <param name="locationFilter_WatershedId"></param>
        /// <param name="locationFilter_SubwatershedId"></param>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private DataTable GetBaselineSummaryData(List<GridColumnsDTO> summaryTableColumns, DataTable summaryDataTable,
            int summerizationLevelId, int locationFilter_MunicipalityId, int locationFilter_WatershedId, int locationFilter_SubwatershedId, int projectId, int userId)
        {
            //get base scenario id and start and end year
            var project = _uow.GetRepository<Project>().Query().Where(x => x.Id == projectId).
                Select(x => new ProjectDTO() { ScenarioTypeId = x.ScenarioTypeId, StartYear = x.StartYear, EndYear = x.EndYear }).FirstOrDefault();

            //use the overview function
            return GetOverviewSummaryData(summaryTableColumns, summaryDataTable, project.ScenarioTypeId, summerizationLevelId,
                locationFilter_MunicipalityId, locationFilter_WatershedId, locationFilter_SubwatershedId, project.StartYear, project.EndYear, userId);
        }
        #endregion


        #region BMP Scope and Intelligence

        public bool CheckIfBMPsSelectedinProject(int projectId, int bmptypeId, string locationType)
        {
            int count = 0;
            if (locationType == "LSD")
            {
                count = (from lsd in _uow.GetRepository<SolutionLegalSubDivisions>().Query()
                         join sol in _uow.GetRepository<Solution>().Query() on lsd.SolutionId equals sol.Id
                         where sol.ProjectId == projectId && lsd.BMPTypeId == bmptypeId && lsd.IsSelected == true
                         select lsd.Id).Count();
            }
            else if (locationType == "Parcel")
            {
                count = (from parcel in _uow.GetRepository<SolutionParcels>().Query()
                         join sol in _uow.GetRepository<Solution>().Query() on parcel.SolutionId equals sol.Id
                         where sol.ProjectId == projectId && parcel.BMPTypeId == bmptypeId && parcel.IsSelected == true
                         select parcel.Id).Count();
            }
            else
            {
                count = (from model in _uow.GetRepository<SolutionModelComponents>().Query()
                         join sol in _uow.GetRepository<Solution>().Query() on model.SolutionId equals sol.Id
                         where sol.ProjectId == projectId && model.BMPTypeId == bmptypeId && model.IsSelected == true
                         select model.Id).Count();
            }

            if (count > 0)
                return true;
            else
                return false;
        }


        /// <summary>
        /// Get Optimization Types
        /// </summary>
        /// <returns></returns>
        public List<DropdownDTO> GetOptimizationTypeList(int projectId)
        {
            var existingOptimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();
            var optimizationTypeList = _uow.GetRepository<OptimizationType>().Get().Select(x => new DropdownDTO() { ItemId = x.Id, ItemName = x.Name, IsDefault = x.IsDefault }).ToList();

            if (existingOptimization == null)
            {
                SaveOptimizationType(projectId, optimizationTypeList.Where(x => x.IsDefault).ToList().First().ItemId);
            }
            else
            {
                foreach (var type in optimizationTypeList)
                {
                    if (type.ItemId == existingOptimization.OptimizationTypeId)
                        type.IsDefault = true;
                    else
                        type.IsDefault = false;
                }
            }

            return optimizationTypeList;
        }

        /// <summary>
        /// get uesr projects and their watersheds
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <remarks>This logic could be simplified as only one table (of ProjectMunicipalities and ProjectWatersheds) has values. Check function agbmptool_getprojectwatersheds. </remarks>
        private Dictionary<ProjectScenario, IEnumerable<Watershed>> getUserProjectWaterSheds(int userId)
        {
            //project ids for the user
            var userProjectIds = _uow.GetRepository<Project>().Get(x => x.UserId == userId && x.Active == true).Select(x => x.Id).ToList();

            // get dictionary of project to minicipality geometries
            var municitpilitiesGeometries = (from pmu in _uow.GetRepository<ProjectMunicipalities>().Query().Where(x => userProjectIds.Contains(x.ProjectId))
                                             join pro in _uow.GetRepository<Project>().Query() on pmu.ProjectId equals pro.Id
                                             join mu in _uow.GetRepository<Municipality>().Query() on pmu.MunicipalityId equals mu.Id
                                             select new
                                             {
                                                 pro,
                                                 mu.Geometry
                                             }).GroupBy(x => x.pro).ToDictionary(
                                                g => new ProjectScenario()
                                                {
                                                    projectId = g.Key.Id,
                                                    scenarioTypeId = g.Key.ScenarioTypeId
                                                },
                                                g => g.Select(x => x.Geometry));

            //all watersheds
            var watersheds = _uow.GetRepository<Watershed>().Query().ToList();
            var filteredwatersheds = new Dictionary<ProjectScenario, IEnumerable<Watershed>>();

            //loop through all municipality geometries and find watershes that intersect with municipalities.
            foreach (var projectmuniciapility in municitpilitiesGeometries)
            {
                var key = projectmuniciapility.Key;
                var value = projectmuniciapility.Value;
                var projectwatershed = watersheds.Where(
                     x =>
                     (value.Any(
                         y => y.Intersects(x.Geometry))));
                filteredwatersheds.Add(key, projectwatershed);
            }

            //get dictionary of project to watersheds
            var projectwatersheds = _uow.GetRepository<ProjectWatersheds>().Query()
                    .Include(x => x.Watershed)
                    .Include(x => x.Project)
                    .Where(x => userProjectIds.Contains(x.ProjectId))
                    .GroupBy(x => x.Project)
                    .ToDictionary(g => new ProjectScenario()
                    {
                        projectId = g.Key.Id,
                        scenarioTypeId = g.Key.ScenarioTypeId
                    }, g => g.Select(x => x.Watershed).ToList());

            //combine the watershed get from municipalities
            foreach (var filteredwatershed in filteredwatersheds)
            {
                if (projectwatersheds.ContainsKey(filteredwatershed.Key))
                {
                    filteredwatershed.Value.Concat(projectwatersheds[filteredwatershed.Key]);
                }
            }
            foreach (var projectwatershed in projectwatersheds)
            {
                if (!filteredwatersheds.ContainsKey(projectwatershed.Key))
                {
                    filteredwatersheds.Add(projectwatershed.Key, projectwatershed.Value);
                }
            }
            return filteredwatersheds;

        }

        /// <summary>
        /// Get ScenarioType option (Conventional & Existing) by project Id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>List of Scenario Type</returns>
        public OptimizationDTO GetOptimizationByProjectId(int projectId, List<BMPEffectivenessTypeDTO> bmpEffectivenessTypes)
        {
            var optimizationDTO = new OptimizationDTO();
            optimizationDTO.optimizationTypeId = (int)OptimizationTypeEnum.Budget;
            optimizationDTO.budgetTarget = 0;
            optimizationDTO.addedOptimizationConstraint = new List<BMPEffectivenessTypeDTO>();

            var optimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();
            if (optimization != null)
            {
                var optimizationWeights = _uow.GetRepository<OptimizationWeights>().Query().Where(x => x.OptimizationId == optimization.Id).ToList();
                foreach (var type in bmpEffectivenessTypes)
                {
                    var optimizationWeight = optimizationWeights.Where(x => x.BMPEffectivenessTypeId == type.id).FirstOrDefault();
                    if (optimizationWeight != null) type.defaultWeight = optimizationWeight.Weight;
                }

                var optimizationConstraints = _uow.GetRepository<OptimizationConstraints>().Query().Where(x => x.OptimizationId == optimization.Id).ToList();
                foreach (var type in bmpEffectivenessTypes)
                {
                    var constraints = optimizationConstraints.Where(x => x.BMPEffectivenessTypeId == type.id).FirstOrDefault();
                    if (constraints != null)
                    {
                        type.defaultConstraint = constraints.Constraint;
                        type.defaultConstraintTypeId = constraints.OptimizationConstraintValueTypeId;

                        optimizationDTO.addedOptimizationConstraint.Add(type);
                    }
                }

                optimizationDTO.Id = optimization.Id;
                optimizationDTO.projectId = optimization.ProjectId;
                optimizationDTO.optimizationTypeId = optimization.OptimizationTypeId;
                optimizationDTO.budgetTarget = optimization.BudgetTarget;
            }

            optimizationDTO.bmpEffectivenessTypes = bmpEffectivenessTypes;

            return optimizationDTO;
        }

        /// <summary>
        /// Get Optimization Constraint Value Types
        /// </summary>
        /// <returns></returns>
        public List<DropdownDTO> GetOptimizationConstraintValueTypeList()
        {
            return _uow.GetRepository<OptimizationConstraintValueType>().Get().Select(x => new DropdownDTO() { ItemId = x.Id, ItemName = x.Name, IsDefault = x.IsDefault }).ToList();
        }


        private List<Watershed> getProjectWaterSheds(int projectId)
        {
            var res = new List<Watershed>();
            var municitpilitiesGeometries = (from pmu in _uow.GetRepository<ProjectMunicipalities>().Query().Where(x => x.ProjectId == projectId)
                                             join pro in _uow.GetRepository<Project>().Query() on pmu.ProjectId equals pro.Id
                                             join mu in _uow.GetRepository<Municipality>().Query() on pmu.MunicipalityId equals mu.Id
                                             select mu.Geometry);
            var watersheds = _uow.GetRepository<Watershed>().Query().ToList();
            foreach (var projectmuniciapility in municitpilitiesGeometries)
            {
                var municipilitywatershed = watersheds.Where(
                     x =>
                     (projectmuniciapility.Any(
                         y => y.Intersects(x.Geometry))));
                res.AddRange(municipilitywatershed);
            }

            var projectwatersheds = _uow.GetRepository<ProjectWatersheds>().Query()
                    .Include(x => x.Watershed)
                    .Where(x => x.ProjectId == projectId)
                    .Select(x => x.Watershed).ToList();
            res.AddRange(projectwatersheds);
            return res;
        }

        private List<Municipality> getProjectMuniciapilities(int projectId)
        {
            var res = new List<Municipality>();
            var waterhedsGeometries = (from pwt in _uow.GetRepository<ProjectWatersheds>().Query().Where(x => x.ProjectId == projectId)
                                       join pro in _uow.GetRepository<Project>().Query() on pwt.ProjectId equals pro.Id
                                       join mt in _uow.GetRepository<Watershed>().Query() on pwt.WatershedId equals mt.Id
                                       select mt.Geometry);
            var municipalities = _uow.GetRepository<Municipality>().Query().ToList();
            foreach (var waterhedsGeometry in waterhedsGeometries)
            {
                var watershedmunicipility = municipalities.Where(
                     x =>
                     (waterhedsGeometries.Any(
                         y => y.Intersects(x.Geometry))));
                res.AddRange(watershedmunicipility);
            }

            var projectMunicipalities = _uow.GetRepository<ProjectMunicipalities>().Query()
                    .Include(x => x.Municipality)
                    .Where(x => x.ProjectId == projectId)
                    .Select(x => x.Municipality).ToList();
            res.AddRange(projectMunicipalities);
            return res;
        }

        public List<MutiPolyGon> getProjectWatershedGeometry(int projectId)
        {
            var res = new List<MutiPolyGon>();
            var geometrylayerstyle = _iUserDataService.getGeometryLayerStyle("WaterShed");
            var watersheds = getProjectWaterSheds(projectId);
            return watersheds.Select(x =>
                new MutiPolyGon
                {
                    coordinates = MutiPolyGon.convertPolygonString(x.Geometry),
                    attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(x.Id) },
                    style = new PolyGonStyle(geometrylayerstyle)
                }
          ).ToList();
        }

        public List<MutiPolyGon> getProjectMunicipilitiesGeometry(int projectId)
        {
            var res = new List<MutiPolyGon>();
            var municipalities = getProjectMuniciapilities(projectId);
            var geometrylayerstyle = _iUserDataService.getGeometryLayerStyle("Municipality");
            return municipalities.Select(x =>
                new MutiPolyGon
                {
                    coordinates = MutiPolyGon.convertPolygonString(x.Geometry),
                    attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(x.Id) },
                    style = new PolyGonStyle(geometrylayerstyle)
                }
          ).ToList();
        }

        public List<PolyLine> getProjectReachesGeometry(int projectId)
        {
            var res = new List<PolyLine>();
            var watersheids = getProjectWaterSheds(projectId).Select(x => x.Id);

            var modelComponentIds = _uow.GetRepository<ModelComponent>()
                .Get(x => watersheids.Contains(x.WatershedId)).Select(x => x.Id).ToList();

            var lsds = _uow.GetRepository<Reach>().Get(x => modelComponentIds.Contains(x.ModelComponentId));
            var geometrylayerstyle = this._iUserDataService.getGeometryLayerStyle("Reach");
            foreach (var lsd in lsds)
            {
                var polylinegeometry = lsd.Geometry;
                var coordinates = PolyLine.convertPolygonString(polylinegeometry);
                PolyLine polyline = new PolyLine();
                polyline.coordinates = coordinates;
                polyline.attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(lsd.Id) };
                polyline.style = new SimpleLineStyle(geometrylayerstyle);
                res.Add(polyline);

            }
            return res;
        }

        /// <summary>
        /// This has been replaced by postgres functions
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<BMPTypeDTO> getProjectDefaultBMPLocations(int projectId)
        {
            //define return value
            var retValue = new List<BMPTypeDTO>();

            //get watershed ids for the project
            var watershedIds = getProjectWaterSheds(projectId).Select(x => x.Id).ToList();

            //get all bmp types and model component ids the project
            var bmpmodelcomponents = (from sc in _uow.GetRepository<Scenario>().Query()
                                      join usc in _uow.GetRepository<UnitScenario>().Query() on sc.Id equals usc.ScenarioId
                                      join bmpt in _uow.GetRepository<BMPCombinationType>().Query() on usc.BMPCombinationId equals bmpt.Id
                                      join bmpts in _uow.GetRepository<BMPCombinationBMPTypes>().Query() on bmpt.Id equals bmpts.BMPCombinationTypeId
                                      join bmpty in _uow.GetRepository<BMPType>().Query() on bmpts.BMPTypeId equals bmpty.Id
                                      join prject in _uow.GetRepository<Project>().Query() on sc.ScenarioTypeId equals prject.ScenarioTypeId
                                      join mc in _uow.GetRepository<ModelComponent>().Query() on usc.ModelComponentId equals mc.Id
                                      where watershedIds.Contains(sc.WatershedId) && prject.Id == projectId
                                      select new BMPTypeDTO()
                                      {
                                          Id = bmpty.Id,
                                          Name = bmpty.Name,
                                          modelComponentId = usc.ModelComponentId,
                                          modelComponentTypeId = mc.ModelComponentTypeId
                                      }).Distinct().OrderBy(x => x.Id);


            //separate to subarea and others
            var bmpsubareas = bmpmodelcomponents.Where(x => x.modelComponentTypeId == 1);
            var bmpothers = bmpmodelcomponents.Where(x => x.modelComponentTypeId != 1);

            //get parcel or lsd from subarea
            var projectSpatialUnitTypeId = _uow.GetRepository<Project>()
                .Get(x => x.Id == projectId).Select(x => x.ProjectSpatialUnitTypeId)
                .FirstOrDefault();
            if (projectSpatialUnitTypeId == (int)(ProjectSpatialUnitTypeEnum.LSD))
            {
                var bmpLSDs = (from comp in bmpsubareas.AsQueryable()
                               join subarea in _uow.GetRepository<SubArea>().Query() on comp.modelComponentId equals subarea.Id
                               select new BMPTypeDTO()
                               {
                                   Id = comp.Id,
                                   Name = comp.Name,
                                   modelComponentId = subarea.LegalSubDivisionId,
                                   modelComponentTypeId = -1
                               }).Distinct();
                retValue.AddRange(bmpLSDs.ToList());
            }
            else
            {
                var bmpParcels = (from comp in bmpsubareas.AsQueryable()
                                  join subarea in _uow.GetRepository<SubArea>().Query() on comp.modelComponentId equals subarea.Id
                                  select new BMPTypeDTO()
                                  {
                                      Id = comp.Id,
                                      Name = comp.Name,
                                      modelComponentId = subarea.ParcelId,
                                      modelComponentTypeId = -2
                                  }).Distinct();
                retValue.AddRange(bmpParcels.ToList());
            }

            retValue.AddRange(bmpothers.ToList());

            return retValue;
        }

        public Dictionary<int, List<BMPTypeDTO>> getUserProjectBMPTypes(int userId)
        {
            var btmptypelist = ((from pt in _uow.GetRepository<Project>().Query()
                                 join opt in _uow.GetRepository<Optimization>().Query() on pt.Id equals opt.ProjectId
                                 join optlsd in _uow.GetRepository<OptimizationLegalSubDivisions>().Query() on opt.Id equals optlsd.OptimizationId
                                 join bmt in _uow.GetRepository<BMPType>().Query() on optlsd.BMPTypeId equals bmt.Id
                                 where pt.UserId == userId
                                 select new BMPTypeDTO()
                                 {
                                     Id = bmt.Id,
                                     Name = bmt.Name,
                                     projectId = pt.Id
                                 })
             .Union(from pt in _uow.GetRepository<Project>().Query()
                    join opt in _uow.GetRepository<Optimization>().Query() on pt.Id equals opt.ProjectId
                    join optpr in _uow.GetRepository<OptimizationParcels>().Query() on opt.Id equals optpr.OptimizationId
                    join bmt in _uow.GetRepository<BMPType>().Query() on optpr.BMPTypeId equals bmt.Id
                    where pt.UserId == userId
                    select new BMPTypeDTO()
                    {
                        Id = bmt.Id,
                        Name = bmt.Name,
                        projectId = pt.Id
                    })
             .Union(from pt in _uow.GetRepository<Project>().Query()
                    join opt in _uow.GetRepository<Optimization>().Query() on pt.Id equals opt.ProjectId
                    join optmc in _uow.GetRepository<OptimizationModelComponents>().Query() on opt.Id equals optmc.OptimizationId
                    join bmt in _uow.GetRepository<BMPType>().Query() on optmc.BMPTypeId equals bmt.Id
                    where pt.UserId == userId
                    select new BMPTypeDTO()
                    {
                        Id = bmt.Id,
                        Name = bmt.Name,
                        projectId = pt.Id
                    })).ToList();

            var tmp = btmptypelist.GroupBy(
                   x => x.projectId
               ).ToDictionary(
                 g => g.Key,
                 g => g.ToList()
               );

            var bmptypedictionary = btmptypelist.GroupBy(
                    x => x.projectId
                ).ToDictionary(
                  g => g.Key,
                  g => g.DistinctBy(x => x.Id).ToList()
                );
            return bmptypedictionary;
        }
        private modelCompoentGeometryLayer getOptimizationLSDGeometies(int bmptypeId, List<int> optimizationIds)
        {
            modelCompoentGeometryLayer res = new modelCompoentGeometryLayer();
            List<GeometryDTO> geometries = new List<GeometryDTO>();
            var selectedlsds = _uow.GetRepository<OptimizationLegalSubDivisions>().Query().Include(x => x.LegalSubDivision)
                .Where(x => optimizationIds.Contains(x.OptimizationId) && x.BMPTypeId == bmptypeId);
            foreach (var lsd in selectedlsds)
            {
                geometries.Add(new MutiPolyGon()
                {
                    Id = lsd.LegalSubDivisionId,
                    coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)lsd.LegalSubDivision.Geometry),
                    type = "MultiPolygon",
                    selected = lsd.IsSelected ? "Selected" : "Unselected",
                });
            }
            if (geometries.Count > 0)
            {
                res.geometries = geometries;
                res.geometrytype = geometries[0].type;
                res.layername = "LSD";
                return res;
            }
            return null;
        }

        private modelCompoentGeometryLayer getOptimizationParcelsGeometies(int bmptypeId, List<int> optimizationIds)
        {
            modelCompoentGeometryLayer res = new modelCompoentGeometryLayer();
            List<GeometryDTO> geometries = new List<GeometryDTO>();
            var selectedparcels = _uow.GetRepository<OptimizationParcels>().Query().Include(x => x.Parcel)
                .Where(x => optimizationIds.Contains(x.OptimizationId) && x.BMPTypeId == bmptypeId);
            foreach (var parcel in selectedparcels)
            {
                geometries.Add(new MutiPolyGon()
                {
                    Id = parcel.ParcelId,
                    coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)parcel.Parcel.Geometry),
                    type = "MultiPolygon",
                    selected = parcel.IsSelected ? "Selected" : "Unselected",
                });
            }
            if (geometries.Count > 0)
            {
                res.geometries = geometries;
                res.geometrytype = geometries[0].type;
                res.layername = "Parcel";
                return res;
            }
            return null;
        }

        private modelCompoentGeometryLayer getOptimizationModelComponentsGeometies(int bmptypeId, List<int> optimizationIds)
        {
            var selectedmodelcomponents = _uow.GetRepository<OptimizationModelComponents>().Query().Include(x => x.ModelComponent)
                .Include(x => x.ModelComponent.ModelComponentType)
                .Where(x => optimizationIds.Contains(x.OptimizationId) && x.BMPTypeId == bmptypeId);
            if (selectedmodelcomponents.Count() == 0)
                return null;
            modelCompoentGeometryLayer res = new modelCompoentGeometryLayer();
            String layername = "";
            HashSet<int> geometryIds = new HashSet<int>();
            foreach (var selectedmodelcomponent in selectedmodelcomponents)
            {
                var geometries = getModelComponentGeometry(selectedmodelcomponent.ModelComponent, selectedmodelcomponent.IsSelected ? "Selected" : "Unselected").ToList();
                layername = selectedmodelcomponent.ModelComponent.ModelComponentType.Name;
                res.geometries.AddRange(geometries);
            }
            res.geometrytype = res.geometries[0].type;
            res.layername = layername;
            return res;
        }

        private modelCompoentGeometryLayer getSolutionLSDGeometies(int bmptypeId, List<int> solutionIds)
        {
            modelCompoentGeometryLayer res = new modelCompoentGeometryLayer();
            List<GeometryDTO> geometries = new List<GeometryDTO>();
            var selectedlsds = _uow.GetRepository<SolutionLegalSubDivisions>().Query().Include(x => x.LegalSubDivision)
                .Where(x => solutionIds.Contains(x.SolutionId) && x.BMPTypeId == bmptypeId);
            foreach (var lsd in selectedlsds)
            {
                geometries.Add(new MutiPolyGon()
                {
                    Id = lsd.LegalSubDivisionId,
                    coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)lsd.LegalSubDivision.Geometry),
                    type = "MultiPolygon",
                    selected = lsd.IsSelected ? "Selected" : "Unselected",
                });
            }
            if (geometries.Count > 0)
            {
                res.geometries = geometries;
                res.geometrytype = geometries[0].type;
                res.layername = "LSD";
                return res;
            }
            return null;
        }

        private modelCompoentGeometryLayer getSolutionParcelsGeometies(int bmptypeId, List<int> solutionIds)
        {
            modelCompoentGeometryLayer res = new modelCompoentGeometryLayer();
            List<GeometryDTO> geometries = new List<GeometryDTO>();
            var selectedparcels = _uow.GetRepository<SolutionParcels>().Query().Include(x => x.Parcel)
                .Where(x => solutionIds.Contains(x.SolutionId) && x.BMPTypeId == bmptypeId);
            foreach (var parcel in selectedparcels)
            {
                geometries.Add(new MutiPolyGon()
                {
                    Id = parcel.ParcelId,
                    coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)parcel.Parcel.Geometry),
                    type = "MultiPolygon",
                    selected = parcel.IsSelected ? "Selected" : "Unselected",
                });
            }
            if (geometries.Count > 0)
            {
                res.geometries = geometries;
                res.geometrytype = geometries[0].type;
                res.layername = "Parcel";
                return res;
            }
            return null;
        }

        private modelCompoentGeometryLayer getSolutionModelComponentsGeometies(int bmptypeId, List<int> solutionIds)
        {
            var selectedmodelcomponents = _uow.GetRepository<SolutionModelComponents>().Query().Include(x => x.ModelComponent)
                .Include(x => x.ModelComponent.ModelComponentType)
                .Where(x => solutionIds.Contains(x.SolutionId) && x.BMPTypeId == bmptypeId);
            if (selectedmodelcomponents.Count() == 0)
                return null;
            modelCompoentGeometryLayer res = new modelCompoentGeometryLayer();
            String layername = "";
            HashSet<int> geometryIds = new HashSet<int>();
            foreach (var selectedmodelcomponent in selectedmodelcomponents)
            {
                var geometries = getModelComponentGeometry(selectedmodelcomponent.ModelComponent, selectedmodelcomponent.IsSelected ? "Selected" : "Unselected").ToList();
                layername = selectedmodelcomponent.ModelComponent.ModelComponentType.Name;
                res.geometries.AddRange(geometries);
            }
            res.geometrytype = res.geometries[0].type;
            res.layername = layername;
            return res;
        }

        /// <summary>
        /// Get model component geometry for given model component. The subarea will be aggregate to parcel or lsd based on project setting. 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="modelComponent"></param>
        /// <returns></returns>
        private IEnumerable<GeometryDTO> getModelComponentGeometry(ModelComponent modelComponent, string selected)
        {
            var modelComponentId = modelComponent.Id;
            var modelComponentType = modelComponent.ModelComponentTypeId;
            switch (modelComponentType)
            {
                case 2:
                    return _uow.GetRepository<Reach>()
                             .Get(x => x.ModelComponentId == modelComponentId)
                             .Select(x => new PolyLine()
                             {
                                 Id = modelComponentId,
                                 selected = selected,
                                 coordinates = PolyLine.convertPolygonString((MultiLineString)x.Geometry),
                                 type = "MultiLineString"
                             });
                case 3:
                    return _uow.GetRepository<IsolatedWetland>()
                            .Get(x => x.ModelComponentId == modelComponentId)
                             .Select(x => new MutiPolyGon()
                             {
                                 Id = modelComponentId,
                                 selected = selected,
                                 coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                                 type = "MultiPolygon",
                             });
                case 4:
                    return _uow.GetRepository<RiparianWetland>()
                           .Get(x => x.ModelComponentId == modelComponentId)
                            .Select(x => new MutiPolyGon()
                            {
                                Id = modelComponentId,
                                selected = selected,
                                coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                                type = "MultiPolygon",
                            });
                case 5:
                    return _uow.GetRepository<Lake>()
                           .Get(x => x.ModelComponentId == modelComponentId)
                              .Select(x => new MutiPolyGon()
                              {
                                  Id = modelComponentId,
                                  selected = selected,
                                  coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                                  type = "MultiPolygon"
                              });
                case 6:
                    return _uow.GetRepository<VegetativeFilterStrip>()
                           .Get(x => x.ModelComponentId == modelComponentId)
                             .Select(x => new MutiPolyGon()
                             {
                                 Id = modelComponentId,
                                 selected = selected,
                                 coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                                 type = "MultiPolygon",

                             });
                case 7:
                    return _uow.GetRepository<RiparianBuffer>()
                          .Get(x => x.ModelComponentId == modelComponentId)
                             .Select(x => new MutiPolyGon()
                             {
                                 Id = modelComponentId,
                                 selected = selected,
                                 coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                                 type = "MultiPolygon",
                             });
                case 8:
                    return _uow.GetRepository<GrassedWaterway>()
                          .Get(x => x.ModelComponentId == modelComponentId)
                           .Select(x => new MutiPolyGon()
                           {
                               Id = modelComponentId,
                               selected = selected,
                               coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                               type = "MultiPolygon",

                           });
                case 9:
                    return _uow.GetRepository<FlowDiversion>()
                          .Get(x => x.ModelComponentId == modelComponentId)
                              .Select(x => new MutiPointDTO()
                              {
                                  Id = modelComponentId,
                                  selected = selected,
                                  coordinates = MutiPointDTO.convertMutiplePoints((MultiPoint)x.Geometry),
                                  type = "MultiPoint",

                              });
                case 10:
                    return _uow.GetRepository<Wascob>()
                         .Get(x => x.ModelComponentId == modelComponentId)
                           .Select(x => new MutiPolyGon()
                           {
                               Id = modelComponentId,
                               selected = selected,
                               coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                               type = "MultiPolygon",

                           });
                case 11:
                    return _uow.GetRepository<SmallDam>()
                         .Get(x => x.ModelComponentId == modelComponentId)
                            .Select(x => new MutiPolyGon()
                            {
                                Id = modelComponentId,
                                selected = selected,
                                coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                                type = "MultiPolygon",

                            });
                case 12:
                    return _uow.GetRepository<Wascob>()
                         .Get(x => x.ModelComponentId == modelComponentId)
                          .Select(x => new MutiPolyGon()
                          {
                              Id = modelComponentId,
                              selected = selected,
                              coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                              type = "MultiPolygon",

                          });
                case 13:
                    return _uow.GetRepository<Dugout>()
                         .Get(x => x.ModelComponentId == modelComponentId)
                           .Select(x => new MutiPolyGon()
                           {
                               Id = modelComponentId,
                               selected = selected,
                               coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                               type = "MultiPolygon",

                           });
                case 14:
                    return _uow.GetRepository<CatchBasin>()
                         .Get(x => x.ModelComponentId == modelComponentId)
                            .Select(x => new MutiPolyGon()
                            {
                                Id = modelComponentId,
                                selected = selected,
                                coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                                type = "MultiPolygon",

                            });
                case 15:
                    return _uow.GetRepository<Feedlot>()
                          .Get(x => x.ModelComponentId == modelComponentId)
                            .Select(x => new MutiPolyGon()
                            {
                                Id = modelComponentId,
                                selected = selected,
                                coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)x.Geometry),
                                type = "MultiPolygon"
                            });
                case 16:
                    return _uow.GetRepository<ManureStorage>()
                          .Get(x => x.ModelComponentId == modelComponentId)
                              .Select(x => new MutiPointDTO()
                              {
                                  Id = modelComponentId,
                                  selected = selected,
                                  coordinates = MutiPointDTO.convertMutiplePoints((MultiPoint)x.Geometry),
                                  type = "MultiPoint",

                              });
                case 17:
                    return _uow.GetRepository<RockChute>()
                         .Get(x => x.ModelComponentId == modelComponentId)
                             .Select(x => new MutiPointDTO()
                             {
                                 Id = modelComponentId,
                                 selected = selected,
                                 coordinates = MutiPointDTO.convertMutiplePoints((MultiPoint)x.Geometry),
                                 type = "MultiPoint",

                             });
                case 18:
                    return _uow.GetRepository<PointSource>()
                        .Get(x => x.ModelComponentId == modelComponentId)
                            .Select(x => new MutiPointDTO()
                            {
                                Id = modelComponentId,
                                selected = selected,
                                coordinates = MutiPointDTO.convertMutiplePoints((MultiPoint)x.Geometry),
                                type = "MultiPoint",

                            });
                case 19:
                    return _uow.GetRepository<ClosedDrain>()
                         .Get(x => x.ModelComponentId == modelComponentId)
                               .Select(x => new MutiPointDTO()
                               {
                                   Id = modelComponentId,
                                   selected = selected,
                                   coordinates = MutiPointDTO.convertMutiplePoints((MultiPoint)x.Geometry),
                                   type = "MultiPoint",
                               });
                default:
                    return null;
            }
        }

        public List<modelCompoentGeometryLayer> getBMPTypeListGeometries(int projectId, int userId, List<int> bmptypeIds, Boolean isoptimization)
        {
            var res = new List<modelCompoentGeometryLayer>();
            Dictionary<String, Dictionary<int, GeometryDTO>> dictionary = new Dictionary<string, Dictionary<int, GeometryDTO>>();
            Dictionary<String, modelCompoentGeometryLayer> layerdictinary = new Dictionary<string, modelCompoentGeometryLayer>();
            if (bmptypeIds == null || bmptypeIds.Count == 0)
                return null;
            var bmptypenamedic = _uow.GetRepository<BMPType>().Get(x => bmptypeIds.Contains(x.Id))
                .GroupBy(x => x.Id)
                .ToDictionary(
                  x => x.Key,
                  x => x.FirstOrDefault().Name
                );
            foreach (var bmptypeId in bmptypeIds)
            {
                var layer = getBMPTypeGeometries(projectId, userId, bmptypeId, isoptimization);
                var bmptypename = bmptypenamedic[bmptypeId];
                if (layer == null)
                    continue;
                if (!dictionary.ContainsKey(layer.layername))
                {
                    dictionary.Add(layer.layername, new Dictionary<int, GeometryDTO>());
                }
                if (!layerdictinary.ContainsKey(layer.layername))
                {
                    layerdictinary.Add(layer.layername, layer);
                }
                var geometries = layer.geometries;
                foreach (var geometry in geometries)
                {
                    if (!dictionary[layer.layername].ContainsKey(geometry.Id))
                    {
                        geometry.attributes = new List<GeometryAttriBute>();
                        geometry.attributes.Add(new GeometryAttriBute()
                        {
                            Name = "selectedstatus",
                            Value = (geometry.selected == "Selected" ? bmptypename : geometry.selected)
                        });
                        geometry.attributes.Add(new GeometryAttriBute()
                        {
                            Name = "Id",
                            Value = geometry.Id + "",
                        });
                        dictionary[layer.layername].Add(geometry.Id, geometry);
                    }
                    else
                    {
                        var geometrydto = dictionary[layer.layername][geometry.Id];
                        string previousseletedstatus = "";
                        string newselectedstatus = "";
                        GeometryAttriBute modifiedattribue = null;
                        foreach (var attribute in geometrydto.attributes)
                        {
                            if (attribute.Name == "selectedstatus")
                            {
                                previousseletedstatus = attribute.Value;
                                modifiedattribue = attribute;
                            }
                        }
                        foreach (var attribute in geometry.attributes)
                        {
                            if (attribute.Name == "selected")
                            {
                                newselectedstatus = attribute.Value;
                            }
                        }
                        if (newselectedstatus == "Selected") {
                            if (previousseletedstatus != "Existing" && previousseletedstatus != "UnSelected")
                            {
                                modifiedattribue.Value = "Multiple";
                            }
                            else {
                                modifiedattribue.Value = bmptypename;
                            }
                        }
                    }
                }

            }
            foreach (var entry in layerdictinary)
            {
                entry.Value.geometries = new List<GeometryDTO>();
                var dictonarygeometries = dictionary[entry.Key];
                foreach (var geometry in dictonarygeometries)
                {
                    entry.Value.geometries.Add(geometry.Value);
                }
                res.Add(entry.Value);
            }
            return res;
        }
        /// <summary>
        /// Get bmp geometries for a given project. 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <param name="modelcomponentIds"></param>
        /// <returns></returns>
        /// <remarks>This could be revised to load from project table directly.</remarks>
        public modelCompoentGeometryLayer getBMPTypeGeometries(int projectId, int userId,
            int bmptypeId, Boolean isoptimization)
        {
            var bmptype = _uow.GetRepository<BMPType>().Get(x => x.Id == bmptypeId).FirstOrDefault();
            var bmptypelayerstyle = _iUserDataService.getGeometryLayerStyle(bmptype.Name);
            if (bmptypelayerstyle == null)
            {
                return null;
            }
            var project = _uow.GetRepository<Project>().Get(x => x.Id == projectId).FirstOrDefault();
            var scenarioTypeId = project.ScenarioTypeId;
            var spatialUnitTypeId = project.ProjectSpatialUnitTypeId;
            var optimizationIds = _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId).Select(x => x.Id).ToList();
            var solutionIds = _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId).Select(x => x.Id).ToList();
            //this line could be replaced by calling function agbmptool_getprojectwatersheds
            var projectwatershedIds = getUserProjectWaterSheds(userId)
                .Where(x => x.Key.projectId == projectId)
                .Select(x => x.Value.Select(y => y.Id)).SelectMany(y => y).ToList();

            var notselectedmodelIds = _uow.GetRepository<ModelComponent>().Query().Include(x => x.ModelComponentType)
                .Where(x => projectwatershedIds.Contains(x.WatershedId)).Select(x => x.Id).ToList();
            var notselectedmodels = _uow.GetRepository<WatershedExistingBMPType>().Query().Include(x => x.ModelComponent)
               .Where(x => notselectedmodelIds.Contains(x.ModelComponentId)
               && x.ScenarioTypeId == scenarioTypeId && x.BMPTypeId == bmptypeId).Select(x => x.ModelComponent);

            modelCompoentGeometryLayer res = new modelCompoentGeometryLayer();
            var modelcompoentTypeId = bmptype.ModelComponentTypeId;
            if (isoptimization)
            {
                if (modelcompoentTypeId == 1)
                {
                    if (spatialUnitTypeId == 1)
                    {
                        res = getOptimizationLSDGeometies(bmptypeId, optimizationIds);
                    }
                    else
                    {
                        res = getOptimizationParcelsGeometies(bmptypeId, optimizationIds);
                    }
                }
                else
                {
                    res = getOptimizationModelComponentsGeometies(bmptypeId, optimizationIds);
                }
            }
            else
            {
                if (modelcompoentTypeId == 1)
                {
                    if (spatialUnitTypeId == 1)
                    {
                        res = getSolutionLSDGeometies(bmptypeId, solutionIds);
                    }
                    else
                    {
                        res = getSolutionParcelsGeometies(bmptypeId, solutionIds);
                    }
                }
                else
                {
                    res = getSolutionModelComponentsGeometies(bmptypeId, solutionIds);
                }
            }
            if (res == null)
                return null;
            var exisitingGeometries = new List<GeometryDTO>();
            foreach (var modelcomponent in notselectedmodels)
            {
                var geometries = getExistingModelComponetGeometries(modelcomponent, spatialUnitTypeId);
                if (res.geometries.Count == 0)
                {
                    if (modelcomponent.ModelComponentTypeId == 1)
                    {
                        if (spatialUnitTypeId == 1)
                        {
                            res.layername = "LSD";
                        }
                        else
                        {
                            res.layername = "Parcel";
                        }
                    }
                    else
                    {
                        res.layername = modelcomponent.ModelComponentType.Name;
                    }
                }
                exisitingGeometries.AddRange(geometries);
            }
            var selectedgeometries = new List<GeometryDTO>();
            HashSet<int> exisitinggeometryIds = new HashSet<int>();
            foreach (var geometry in exisitingGeometries)
            {
                if (!exisitinggeometryIds.Contains(geometry.Id))
                {
                    geometry.attributes = new List<GeometryAttriBute>()
                                           {
                                               GeometryAttriBute.getGeometryIdAttribue(geometry.Id),
                                               GeometryAttriBute.getGeometryIsSelectedAttribue("Existing")
                                           };
                    selectedgeometries.Add(geometry);
                    exisitinggeometryIds.Add(geometry.Id);
                }
            }
            foreach (var geometry in res.geometries)
            {
                if (exisitinggeometryIds.Contains(geometry.Id))
                    continue;
                geometry.attributes = new List<GeometryAttriBute>()
                                           {
                                               GeometryAttriBute.getGeometryIdAttribue(geometry.Id),
                                               GeometryAttriBute.getGeometryIsSelectedAttribue(geometry.selected)
                                           };
                selectedgeometries.Add(geometry);
            }
            res.geometries = new List<GeometryDTO>();
            res.geometries = selectedgeometries;
            if (res.geometries.Count > 0)
            {
                res.geometryStyle = GeometryStyle.getGeometryStyle(bmptypelayerstyle, res.geometries[0].type);
                res.geometrytype = res.geometries[0].type;
            }

            return res;
        }

        private IEnumerable<GeometryDTO> getExistingModelComponetGeometries(ModelComponent modelComponent, int projectspatialunittypeId)
        {
            var modelcompoenttypeId = modelComponent.ModelComponentTypeId;
            if (modelcompoenttypeId == 1)
            {
                if (projectspatialunittypeId == 1)
                {
                    return _uow.GetRepository<SubArea>()
                        .Query().Include(x => x.LegalSubDivision)
                            .Where(x => x.ModelComponentId == modelComponent.Id)
                             .Select(x => new MutiPolyGon()
                             {
                                 Id = x.LegalSubDivisionId,
                                 selected = "Existing",
                                 coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)(x.LegalSubDivision.Geometry)),
                                 type = "MultiPolygon",
                             });
                }
                else
                {
                    return _uow.GetRepository<SubArea>()
                        .Query().Include(x => x.Parcel)
                            .Where(x => x.ModelComponentId == modelComponent.Id)
                             .Select(x => new MutiPolyGon()
                             {
                                 Id = x.ParcelId,
                                 selected = "Existing",
                                 coordinates = MutiPolyGon.convertPolygonString((MultiPolygon)(x.Parcel.Geometry)),
                                 type = "MultiPolygon",
                                 attributes = new List<GeometryAttriBute>()
                                {
                                               GeometryAttriBute.getGeometryIdAttribue(x.Id),
                                               GeometryAttriBute.getGeometryIsSelectedAttribue("Existing")
                                }
                             });
                }
            }
            else
            {
                return getModelComponentGeometry(modelComponent, "Existing");
            }
        }

        public SummaryTableDTO GetBMPScopeSummaryGridData(int projectId, int bmpTypeId)
        {
            SummaryTableDTO summaryGridData = new SummaryTableDTO();

            var locationType = GetLocationTypeByProjectIdAndBMPTypeId(projectId, bmpTypeId);

            summaryGridData.SummaryTableColumns = GetBMPScopeSummaryGridColumns(projectId, bmpTypeId, locationType);

            var summaryDataTable = new DataTable();

            foreach (var resultType in summaryGridData.SummaryTableColumns)
            {
                DataColumn column = new DataColumn();
                column.ColumnName = resultType.FieldName.ToLower();
                if (resultType.FieldType == "")
                    column.DataType = typeof(string);
                else if (resultType.FieldType == "boolean")
                    column.DataType = typeof(bool);
                else if (resultType.FieldType == "integer")
                    column.DataType = typeof(int);
                else if (resultType.FieldType == "double")
                    column.DataType = typeof(double);

                column.Caption = resultType.FieldTitle;
                summaryDataTable.Columns.Add(column);
            }

            summaryGridData.SummaryTableData = GetBMPScopeGridData(summaryGridData.SummaryTableColumns, summaryDataTable, projectId, bmpTypeId);

            return summaryGridData;
        }

        private DataTable GetBMPScopeGridData(List<GridColumnsDTO> summaryTableColumns, DataTable summaryDataTable, int projectId, int bmpTypeId)
        {
            //get all effectiveness
            var bmpCostEffectiveness = _IProjectSummaryService.GetSingleBMPCostAllEffectivenessDTOForBMPScopeAndIntelligentRecommendation(projectId, bmpTypeId).ToList();

            //get all bmp effectiveness
            //one record is added for one effective type at one location to match interface design.
            //interface will switch over different types by use the effectivetype
            if (bmpCostEffectiveness.Count > 0)
            {
                foreach (var r in bmpCostEffectiveness)
                {
                    //for all result types
                    foreach (var resultType in r.EffectivenessDTOs.Select(x => x.ScenarioModelResultVariableTypeId).Distinct())
                    {
                        DataRow dr = null;
                        dr = summaryDataTable.NewRow(); // have new row on each iteration

                        dr["isSelectable"] = r.IsSelectable;    // Used to enable and disable row
                        dr["isSelected"] = r.IsSelected;                // Need to set isSelected Column as Suggested by Michael to toggle checkbox in row (currently set static to false)
                        dr["scenarioTypeId"] = resultType;
                        dr["locationid"] = r.LocationId;
                        dr["farm"] = r.FarmId;
                        dr["bmpArea"] = Math.Round(r.BMPArea, 3); // ha
                        dr["cost"] = Math.Round(r.Cost, 2); // $ per ha

                        //get on site effectiveness
                        var onsiteEff = r.EffectivenessDTOs.Where(x => x.ScenarioModelResultVariableTypeId == resultType &&
                        x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).FirstOrDefault();
                        if (onsiteEff != null)
                        {
                            dr["onsiteeffectiveness"] = Math.Round(onsiteEff.EffectivenessValue, 3);
                            dr["onsitecosteffectiveness"] = Math.Round(onsiteEff.CostEffectivenessValue, 2);
                        }

                        //get off-site effectiveness
                        var offsiteEff = r.EffectivenessDTOs.Where(x => x.ScenarioModelResultVariableTypeId == resultType &&
                        x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).FirstOrDefault();
                        if (offsiteEff != null)
                        {
                            dr["offsiteeffectiveness"] = Math.Round(offsiteEff.EffectivenessValue, 3);
                            dr["offsitecosteffectiveness"] = Math.Round(offsiteEff.CostEffectivenessValue, 2);
                        }
                        summaryDataTable.Rows.Add(dr);
                    }
                }
            }
            return summaryDataTable;
        }

        public SummaryTableDTO GetSingleBMPCostGridData(int projectId, int bmpTypeId)
        {
            SummaryTableDTO summaryGridData = new SummaryTableDTO();

            var locationType = GetLocationTypeByProjectIdAndBMPTypeId(projectId, bmpTypeId);

            summaryGridData.SummaryTableColumns = GetBMPScopeSummaryGridColumns(projectId, bmpTypeId, locationType);

            var summaryDataTable = new DataTable();

            foreach (var resultType in summaryGridData.SummaryTableColumns)
            {
                DataColumn column = new DataColumn();
                column.ColumnName = resultType.FieldName.ToLower();
                if (resultType.FieldType == "")
                    column.DataType = typeof(string);
                else if (resultType.FieldType == "boolean")
                    column.DataType = typeof(bool);
                column.Caption = resultType.FieldTitle;
                summaryDataTable.Columns.Add(column);
            }

            summaryGridData.SummaryTableData = GetSingleBMPCostGridData(summaryGridData.SummaryTableColumns, summaryDataTable, projectId, bmpTypeId);

            return summaryGridData;
        }

        private DataTable GetSingleBMPCostGridData(List<GridColumnsDTO> summaryTableColumns, DataTable summaryDataTable, int projectId, int bmpTypeId)
        {
            //get all effectiveness
            var bmpCostEffectiveness = _IProjectSummaryService.GetSingleBMPCostAllEffectivenessDTOForBMPSelectionAndOverview(projectId, bmpTypeId).ToList();

            //get all bmp effectiveness
            //one record is added for one effective type at one location to match interface design.
            //interface will switch over different types by use the effectivetype
            if (bmpCostEffectiveness.Count > 0)
            {
                foreach (var r in bmpCostEffectiveness)
                {
                    //for all result types
                    foreach (var resultType in r.EffectivenessDTOs.Select(x => x.ScenarioModelResultVariableTypeId).Distinct())
                    {
                        DataRow dr = null;
                        dr = summaryDataTable.NewRow(); // have new row on each iteration

                        dr["isSelectable"] = r.IsSelectable;    // Used to enable and disable row
                        dr["isSelected"] = r.IsSelected;                // Need to set isSelected Column as Suggested by Michael to toggle checkbox in row (currently set static to false)
                        dr["scenarioTypeId"] = resultType;
                        dr["locationid"] = r.LocationId;
                        dr["farm"] = r.FarmId;
                        dr["bmpArea"] = r.BMPArea.ToString("N3"); // ha
                        dr["cost"] = r.Cost.ToString("C2"); // $ per ha

                        //get on site effectiveness
                        var onsiteEff = r.EffectivenessDTOs.Where(x => x.ScenarioModelResultVariableTypeId == resultType &&
                        x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).FirstOrDefault();
                        if (onsiteEff != null)
                        {
                            dr["onsiteeffectiveness"] = onsiteEff.EffectivenessValue.ToString("N3");
                            dr["onsitecosteffectiveness"] = onsiteEff.CostEffectivenessValue.ToString("C2");
                        }

                        //get off-site effectiveness
                        var offsiteEff = r.EffectivenessDTOs.Where(x => x.ScenarioModelResultVariableTypeId == resultType &&
                        x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).FirstOrDefault();
                        if (offsiteEff != null)
                        {
                            dr["offsiteeffectiveness"] = offsiteEff.EffectivenessValue.ToString("N3");
                            dr["offsitecosteffectiveness"] = offsiteEff.CostEffectivenessValue.ToString("C2");
                        }
                        summaryDataTable.Rows.Add(dr);
                    }
                }
            }
            return summaryDataTable;
        }

        private bool IsEffectivessUnitVolume(int scenarioModelResultVariableTypeId, int bmpEffectivenessLocationTypeId)
        {
            return (from b in _uow.GetRepository<BMPEffectivenessType>().Query()
                    join s in _uow.GetRepository<ScenarioModelResultType>().Query() on b.ScenarioModelResultTypeId equals s.Id
                    where b.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationTypeId && b.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId
                    select s.UnitTypeId).FirstOrDefault() == 13; // 13 is volume unit with s m3
        }

        // Generate a random number between two numbers
        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        private List<GridColumnsDTO> GetBMPScopeSummaryGridColumns(int projectId, int bmpTypeId, string locationType)
        {
            List<GridColumnsDTO> bmpScopeGridColumns = new List<GridColumnsDTO>();

            GridColumnsDTO isSelectableCol = new GridColumnsDTO();
            isSelectableCol.FieldName = "isSelectable";
            isSelectableCol.FieldTitle = "Select";
            isSelectableCol.FieldType = "boolean";
            isSelectableCol.IsHidden = true;
            bmpScopeGridColumns.Add(isSelectableCol);

            GridColumnsDTO isSelectedCol = new GridColumnsDTO();
            isSelectedCol.FieldName = "isSelected";
            isSelectedCol.FieldTitle = "Selected";
            isSelectedCol.FieldType = "boolean";
            isSelectedCol.IsHidden = false;
            bmpScopeGridColumns.Add(isSelectedCol);

            GridColumnsDTO scenarioTypeCol = new GridColumnsDTO();
            scenarioTypeCol.FieldName = "scenarioTypeId";
            scenarioTypeCol.FieldTitle = "";
            scenarioTypeCol.IsHidden = true;
            bmpScopeGridColumns.Add(scenarioTypeCol);

            GridColumnsDTO LSDOrParcelCol = new GridColumnsDTO();
            LSDOrParcelCol.FieldName = "locationid";
            LSDOrParcelCol.FieldTitle = locationType;
            LSDOrParcelCol.FieldType = "integer";
            bmpScopeGridColumns.Add(LSDOrParcelCol);

            GridColumnsDTO farmCol = new GridColumnsDTO();
            farmCol.FieldName = "farm";
            farmCol.FieldTitle = "Farm";
            farmCol.FieldType = "integer";
            bmpScopeGridColumns.Add(farmCol);

            GridColumnsDTO bmpAreaCol = new GridColumnsDTO();
            bmpAreaCol.FieldName = "bmpArea";
            bmpAreaCol.FieldTitle = "Area (ha)";
            bmpAreaCol.FieldType = "double";
            bmpScopeGridColumns.Add(bmpAreaCol);

            GridColumnsDTO onsiteEffectivenessCol = new GridColumnsDTO();
            onsiteEffectivenessCol.FieldName = "onsiteeffectiveness";
            onsiteEffectivenessCol.FieldTitle = "On-site effectiveness";
            onsiteEffectivenessCol.FieldType = "double";
            bmpScopeGridColumns.Add(onsiteEffectivenessCol);

            GridColumnsDTO offsiteEffectivenessCol = new GridColumnsDTO();
            offsiteEffectivenessCol.FieldName = "offsiteeffectiveness";
            offsiteEffectivenessCol.FieldTitle = "Off-site effectiveness";
            offsiteEffectivenessCol.FieldType = "double";
            bmpScopeGridColumns.Add(offsiteEffectivenessCol);

            GridColumnsDTO costCol = new GridColumnsDTO();
            costCol.FieldName = "cost";
            costCol.FieldTitle = "Cost ($)";
            costCol.FieldType = "double";
            bmpScopeGridColumns.Add(costCol);

            GridColumnsDTO onsitecosteffectivenessCol = new GridColumnsDTO();
            onsitecosteffectivenessCol.FieldName = "onsitecosteffectiveness";
            onsitecosteffectivenessCol.FieldTitle = "On-site cost-effectiveness";
            onsitecosteffectivenessCol.FieldType = "double";
            bmpScopeGridColumns.Add(onsitecosteffectivenessCol);

            GridColumnsDTO offsitecosteffectivenessCol = new GridColumnsDTO();
            offsitecosteffectivenessCol.FieldName = "offsitecosteffectiveness";
            offsitecosteffectivenessCol.FieldTitle = "Off-site cost-effectiveness";
            offsitecosteffectivenessCol.FieldType = "double";
            bmpScopeGridColumns.Add(offsitecosteffectivenessCol);

            return bmpScopeGridColumns;
        }

        /// <summary>
        /// Function to get location type
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="bmpTypeId"></param>
        /// <returns></returns>
        public string GetBMPDefaultSelectionColorByBMPType(string bmpType)
        {
            string defaultColor = "";
            var geometryStyle = _uow.GetRepository<GeometryLayerStyle>().Get(x => x.layername == bmpType).FirstOrDefault();

            if (geometryStyle != null)
                defaultColor = geometryStyle.color;
            else
                defaultColor = "blue";

            return defaultColor;
        }

        /// <summary>
        /// Function to get location type
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="bmpTypeId"></param>
        /// <returns></returns>
        public string GetLocationTypeByProjectIdAndBMPTypeId(int projectId, int bmpTypeId)
        {
            string locationType = "";

            var modelComponentTypeId = _uow.GetRepository<BMPType>().Get(x => x.Id == bmpTypeId).FirstOrDefault().ModelComponentTypeId;

            if (modelComponentTypeId == (int)(ModelComponentTypeEnum.SubArea))
            {
                var project = _uow.GetRepository<Project>().Get(x => x.Id == projectId).FirstOrDefault();

                if (project.ProjectSpatialUnitTypeId == (int)(ProjectSpatialUnitTypeEnum.LSD))
                    locationType = "LSD";
                else
                    locationType = "Parcel";
            }
            else if (modelComponentTypeId == (int)(ModelComponentTypeEnum.Reach))
            {
                locationType = "Reach";
            }
            else
            {
                locationType = _uow.GetRepository<ModelComponentType>().Get(x => x.Id == modelComponentTypeId).FirstOrDefault().Name;
            }

            // Add Logic to get Location Type
            return locationType;
        }

        /// <summary>
        /// Run BMPScope and Intelligence
        /// </summary>
        /// <param name="projectId"></param>
        public bool RunIntelligentRecommendation(int projectId)
        {
            if (projectId != 0)
            {
                try
                {
                    return _iInteligentRecommendationService.BuildRecommendedSolution(projectId, false);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Save optimization by project id
        /// </summary>
        /// <param name="projectId"></param>
        public void SaveOptimizationType(int projectId, int OptimizationTypeId)
        {
            var isNewOptimization = false;
            var optimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

            if (optimization == null)
            {
                isNewOptimization = true;
                optimization = new Optimization();
                optimization.ProjectId = projectId;
            }

            optimization.OptimizationTypeId = OptimizationTypeId;

            if (isNewOptimization) _uow.GetRepository<Optimization>().Add(optimization);
            else _uow.GetRepository<Optimization>().Update(optimization);

            _uow.Commit();
        }

        /// <summary>
        /// Save Budget
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="budget"></param>
        public void SaveBudget(int projectId, decimal budget)
        {

            if (projectId != 0)
            {
                // Save or update default optimization
                var optimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                if (optimization != null)
                {
                    optimization.BudgetTarget = budget;

                    _uow.GetRepository<Optimization>().Update(optimization);

                    _uow.Commit();
                }
            }
        }

        /// <summary>
        /// Save Weight for project in BMP Scope and Intelligent
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationWeightDTO"></param>
        public void SaveWeight(int projectId, List<OptimizationWeightDTO> optimizationWeightDTO)
        {
            if (projectId != 0)
            {
                // Find optimization
                var optimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                if (optimization != null)
                {
                    var optimizationWeightList = _uow.GetRepository<OptimizationWeights>().Query().Where(x => x.OptimizationId == optimization.Id).ToList();

                    // save Optimization Weights
                    foreach (var type in optimizationWeightDTO)
                    {
                        var isNewWeights = false;
                        var weight = optimizationWeightList.Where(x => x.OptimizationId == optimization.Id && x.BMPEffectivenessTypeId == type.BMPEffectivenessTypeId).FirstOrDefault();

                        if (weight == null)
                        {
                            isNewWeights = true;
                            weight = new OptimizationWeights();
                        }

                        weight.OptimizationId = optimization.Id;
                        weight.BMPEffectivenessTypeId = type.BMPEffectivenessTypeId;
                        weight.Weight = type.Weight;

                        if (isNewWeights) _uow.GetRepository<OptimizationWeights>().Add(weight);
                        else _uow.GetRepository<OptimizationWeights>().Update(weight);
                    }

                    _uow.Commit();
                }
            }
        }

        /// <summary>
        /// Save Constraint for project in BMP Scope and Intelligent
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationConstraintDTO"></param>
        public void SaveConstraint(int projectId, OptimizationConstraintDTO optimizationConstraintDTO)
        {
            if (projectId != 0)
            {
                // Find optimization
                var optimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                if (optimization != null)
                {
                    // delete existing optimization weight
                    var constraints = _uow.GetRepository<OptimizationConstraints>().Query().Where(x => x.OptimizationId == optimization.Id &&
                            x.BMPEffectivenessTypeId == optimizationConstraintDTO.BMPEffectivenessTypeId).FirstOrDefault();

                    // update Optimization Constraints
                    var isNewConstraints = false;

                    if (constraints == null)
                    {
                        isNewConstraints = true;
                        constraints = new OptimizationConstraints();
                    }

                    constraints.OptimizationId = optimization.Id;
                    constraints.BMPEffectivenessTypeId = optimizationConstraintDTO.BMPEffectivenessTypeId;
                    constraints.OptimizationConstraintValueTypeId = optimizationConstraintDTO.OptimizationConstraintValueTypeId;
                    constraints.Constraint = optimizationConstraintDTO.Constraint;

                    if (isNewConstraints) _uow.GetRepository<OptimizationConstraints>().Add(constraints);
                    else _uow.GetRepository<OptimizationConstraints>().Update(constraints);

                    _uow.Commit();
                }
            }
        }

        /// <summary>
        /// Save Constraint for project in BMP Scope and Intelligent
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="bmpEffectivenessTypeId"></param>
        public void DeleteConstraint(int projectId, int bmpEffectivenessTypeId)
        {
            if (projectId != 0)
            {
                // Find optimization
                var optimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                if (optimization != null)
                {
                    // delete existing optimization weight
                    var optimizationConstraint = _uow.GetRepository<OptimizationConstraints>().Query().Where(x => x.OptimizationId == optimization.Id &&
                            x.BMPEffectivenessTypeId == bmpEffectivenessTypeId).FirstOrDefault();

                    if (optimizationConstraint != null)
                    {
                        _uow.GetRepository<OptimizationConstraints>().Delete(optimizationConstraint);
                    }

                    _uow.Commit();
                }
            }
        }

        /// <summary>
        /// Save optimization LSD
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationLSDDTO"></param>
        public void SaveLegalSubDivisions(int projectId, LegalSubDivisionDTO subDivisionDTOLSDDTO, Boolean isOptimization)
        {
            if (isOptimization)
            {
                if (projectId != 0)
                {
                    // Find optimization
                    var optimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                    if (optimization != null)
                    {
                        // delete existing optimization weight
                        var optimizationLSD = _uow.GetRepository<OptimizationLegalSubDivisions>().Query().Where(x => x.OptimizationId == optimization.Id &&
                                x.BMPTypeId == subDivisionDTOLSDDTO.BMPTypeId && x.LegalSubDivisionId == subDivisionDTOLSDDTO.LegalSubDivisionId).FirstOrDefault();

                        // update Optimization Constraints
                        var isNewOptimizationLegalSubDivision = false;

                        if (optimizationLSD == null)
                        {
                            isNewOptimizationLegalSubDivision = true;
                            optimizationLSD = new OptimizationLegalSubDivisions();
                            optimizationLSD.BMPTypeId = subDivisionDTOLSDDTO.BMPTypeId;
                            optimizationLSD.LegalSubDivisionId = subDivisionDTOLSDDTO.LegalSubDivisionId;
                            optimizationLSD.OptimizationId = optimization.Id;
                        }

                        optimizationLSD.IsSelected = subDivisionDTOLSDDTO.IsSelected;

                        if (isNewOptimizationLegalSubDivision) _uow.GetRepository<OptimizationLegalSubDivisions>().Add(optimizationLSD);
                        else _uow.GetRepository<OptimizationLegalSubDivisions>().Update(optimizationLSD);

                        _uow.Commit();
                    }
                }

            }
            else {
                if (projectId != 0)
                {
                    // Find optimization
                    var solution = _uow.GetRepository<Solution>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                    if (solution != null)
                    {
                        // delete existing optimization weight
                        var solutionLSD = _uow.GetRepository<SolutionLegalSubDivisions>().Query().Where(x => x.SolutionId == solution.Id &&
                                x.BMPTypeId == subDivisionDTOLSDDTO.BMPTypeId && x.LegalSubDivisionId == subDivisionDTOLSDDTO.LegalSubDivisionId).FirstOrDefault();

                        // update Optimization Constraints
                        var isNewsolutionLSDLegalSubDivision = false;

                        if (solutionLSD == null)
                        {
                            isNewsolutionLSDLegalSubDivision = true;
                            solutionLSD = new SolutionLegalSubDivisions();
                            solutionLSD.BMPTypeId = subDivisionDTOLSDDTO.BMPTypeId;
                            solutionLSD.LegalSubDivisionId = subDivisionDTOLSDDTO.LegalSubDivisionId;
                            solutionLSD.SolutionId = solution.Id;
                        }

                        solutionLSD.IsSelected = subDivisionDTOLSDDTO.IsSelected;

                        if (isNewsolutionLSDLegalSubDivision) _uow.GetRepository<SolutionLegalSubDivisions>().Add(solutionLSD);
                        else _uow.GetRepository<SolutionLegalSubDivisions>().Update(solutionLSD);

                        _uow.Commit();
                    }
                }
            }

        }

        /// <summary>
        /// Save optimization parcels
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationParcels"></param>
        public void SaveParcels(int projectId, ParcelDTO parcelDTO, Boolean isOptimization)
        {
            if (isOptimization)
            {
                if (projectId != 0)
                {
                    // Find optimization
                    var optimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                    if (optimization != null)
                    {
                        // delete existing optimization weight
                        var optimizationParcel = _uow.GetRepository<OptimizationParcels>().Query().Where(x => x.OptimizationId == optimization.Id &&
                                x.BMPTypeId == parcelDTO.BMPTypeId && x.ParcelId == parcelDTO.ParcelId).FirstOrDefault();

                        // update Optimization Constraints
                        var isNewOptimizationParcel = false;

                        if (optimizationParcel == null)
                        {
                            isNewOptimizationParcel = true;
                            optimizationParcel = new OptimizationParcels();
                        }

                        optimizationParcel.IsSelected = parcelDTO.IsSelected;

                        if (isNewOptimizationParcel) _uow.GetRepository<OptimizationParcels>().Add(optimizationParcel);
                        else _uow.GetRepository<OptimizationParcels>().Update(optimizationParcel);

                        _uow.Commit();
                    }
                }
            }
            else {
                if (projectId != 0)
                {
                    // Find optimization
                    var solution = _uow.GetRepository<Solution>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                    if (solution != null)
                    {
                        // delete existing optimization weight
                        var solutionParcel = _uow.GetRepository<SolutionParcels>().Query().Where(x => x.SolutionId == solution.Id &&
                                x.BMPTypeId == parcelDTO.BMPTypeId && x.ParcelId == parcelDTO.ParcelId).FirstOrDefault();

                        // update Optimization Constraints
                        var isNewSolutionParcel = false;

                        if (solutionParcel == null)
                        {
                            isNewSolutionParcel = true;
                            solutionParcel = new SolutionParcels();
                        }

                        solutionParcel.IsSelected = parcelDTO.IsSelected;

                        if (isNewSolutionParcel) _uow.GetRepository<SolutionParcels>().Add(solutionParcel);
                        else _uow.GetRepository<SolutionParcels>().Update(solutionParcel);

                        _uow.Commit();
                    }
                }
            }

        }

        /// <summary>
        /// Save optimization Model Component
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationModelComponent"></param>
        public void SaveModelComponents(int projectId, ModelComponentDTO modelComponentDTO, Boolean isOptimization)
        {
            if (isOptimization)
            {
                if (projectId != 0)
                {
                    // Find optimization
                    var optimization = _uow.GetRepository<Optimization>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                    if (optimization != null)
                    {
                        // delete existing optimization weight
                        var optimizationModelComponent = _uow.GetRepository<OptimizationModelComponents>().Query().Where(x => x.OptimizationId == optimization.Id &&
                                x.BMPTypeId == modelComponentDTO.BMPTypeId && x.ModelComponentId == modelComponentDTO.ModelComponentId).FirstOrDefault();

                        // update Optimization Constraints
                        var isNewOptimizationModelComponent = false;

                        if (optimizationModelComponent == null)
                        {
                            isNewOptimizationModelComponent = true;
                            optimizationModelComponent = new OptimizationModelComponents();
                        }

                        optimizationModelComponent.IsSelected = modelComponentDTO.IsSelected;

                        if (isNewOptimizationModelComponent) _uow.GetRepository<OptimizationModelComponents>().Add(optimizationModelComponent);
                        else _uow.GetRepository<OptimizationModelComponents>().Update(optimizationModelComponent);

                        _uow.Commit();
                    }
                }
            }
            else {
                if (projectId != 0)
                {
                    // Find optimization
                    var solution = _uow.GetRepository<Solution>().Query().Where(x => x.ProjectId == projectId).FirstOrDefault();

                    if (solution != null)
                    {
                        // delete existing optimization weight
                        var solutionModelComponent = _uow.GetRepository<SolutionModelComponents>().Query().Where(x => x.SolutionId == solution.Id &&
                                x.BMPTypeId == modelComponentDTO.BMPTypeId && x.ModelComponentId == modelComponentDTO.ModelComponentId).FirstOrDefault();

                        // update Optimization Constraints
                        var isNewSolutionModelComponent = false;

                        if (solutionModelComponent == null)
                        {
                            isNewSolutionModelComponent = true;
                            solutionModelComponent = new SolutionModelComponents();
                        }

                        solutionModelComponent.IsSelected = modelComponentDTO.IsSelected;

                        if (isNewSolutionModelComponent) _uow.GetRepository<SolutionModelComponents>().Add(solutionModelComponent);
                        else _uow.GetRepository<SolutionModelComponents>().Update(solutionModelComponent);

                        _uow.Commit();
                    }
                }
            }

        }

        /// <summary>
        /// function to store Selection of Locations in BMP Scope Summary grid When user Clicks Select All and Deselect All
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="bmptypeId"></param>
        /// <param name="locationIdsString"></param>
        /// <param name="isSelected"></param>
        /// <param name="locationType"></param>
        /// <param name="userId"></param>
        public List<int> ApplyQuickSelection(int projectId, int bmptypeId, Boolean isOptimization, Boolean isSelected, int municipalityId, int watershedId, int subwatershedId)
        {
            //get model results first
            var modelResults = _uow.ExecuteProcedure<LocationDTO>(
                string.Format("select * from agbmptool_applyquickchanges({0},{1},{2},{3},{4},{5},{6})",
                projectId, bmptypeId, isOptimization, isSelected,
                municipalityId, watershedId, subwatershedId));

            //
            return modelResults.Select(x => x.locationid).ToList();
        }

        public List<BaseItemDTO> GetInverstorList(int userId, int projectId, int bmptypeId) {
            var project = _uow.GetRepository<Project>().Get(x => x.Id == projectId).FirstOrDefault();
            var scenarioTypeId = project.ScenarioTypeId;
            var projectwatershedIds = getUserProjectWaterSheds(userId)
               .Where(x => x.Key.projectId == projectId)
               .Select(x => x.Value.Select(y => y.Id)).SelectMany(y => y).ToList();

            var notselectedmodelIds = _uow.GetRepository<ModelComponent>().Query()
                .Where(x => projectwatershedIds.Contains(x.WatershedId)).Select(x => x.Id).ToList();
            return _uow.GetRepository<WatershedExistingBMPType>().Query().Include(x => x.Investor)
               .Where(x => notselectedmodelIds.Contains(x.ModelComponentId)
               && x.ScenarioTypeId == scenarioTypeId && x.BMPTypeId == bmptypeId).Select(x => new BaseItemDTO() {
                    Id = x.InvestorId,
                    Name = x.Investor.Name
               }).ToList();
        }
        public Dictionary<String, GeometryStyle> getBMPTypeGeometryLayerDic(List<int> bmptypeIds) {
            var bmptypenames = _uow.GetRepository<BMPType>().Get(x => bmptypeIds.Contains(x.Id)).Select(x => x.Name).ToList();
            var bmptypestyle = _uow.GetRepository<GeometryLayerStyle>().Get(x => bmptypenames.Contains(x.layername))
                .GroupBy(x => x.layername)
                .ToDictionary(
                  x => x.Key,
                  x => GeometryStyle.getGeometryStyle(x.FirstOrDefault(), (x.FirstOrDefault().type == "simple-fill" ? "MultiPolygon" : (x.FirstOrDefault().type == "simple-line" ? "MultiLineString" : "MultiPoint")))
                );
            return bmptypestyle;

        }

        #endregion


        #region Export to Shapefile

        public System.IO.Stream ExportToShapefile(int projectId, int bmpTypeId)
        {
            //get all features
            //we use watershed as dummy for now
            var features = new List<Feature>();
            var watersheds = _uow.GetRepository<Watershed>().Query();
            foreach (var w in watersheds)
            {
                var attribute = new AttributesTable();
                attribute.Add("Id", w.Id);
                attribute.Add("Name", w.Name);
                features.Add(new Feature(w.Geometry, attribute));
            }

            //save to shapefile
            string path = System.IO.Path.GetTempPath();
            string shapeFileName = "shapefilename";

            //save to shapefile
            var writer = new ShapefileDataWriter(Path.Combine(path, shapeFileName + ".shp"));
            writer.Header = ShapefileDataWriter.GetHeader(features.First(), 2);
            writer.Write(features);

            return null;

            //zip file
            //string[] filePaths = Directory.GetFiles(path).Where(x => x.Contains(shapeFileName)).ToArray();
            //ZipFile loanZip = new ZipFile();
            //loanZip.AddFiles(filePaths, "");
            //loanZip.Name = shapeFileName.ToString() + ".zip";

            //save to meory stream
            //Stream outputData = new MemoryStream();
            //loanZip.Save(outputData);

            ////remove shapefiles
            //for (int i = 0; i < filePaths.Length; i++)
            //{
            //    File.Delete(filePaths[i]);
            //}

            //return outputData;
        }

        #endregion


        public string GetProjectSpatialUnitTypeName(int projectId)
        {
            return _uow.GetRepository<Project>().Query().Include(x => x.ProjectSpatialUnitType).Where(x => x.Id == projectId).FirstOrDefault().ProjectSpatialUnitType.Name;
        }
    }
}
