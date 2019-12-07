using System;
using System.Collections.Generic;
using System.Text;
using AgBMPTool.DBModel.Model;
using AgBMPTool.DBModel.Model.Type;
using AgBMPTool.DBModel.Model.ModelComponent;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using AgBMPTool.DBModel.Model.Optimization;
using AgBMPTool.DBModel.Model.Boundary;
using AgBMPTool.DBModel.Model.User;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.Solution;

namespace AgBMPTool.DBModel
{
    public class AgBMPToolContext : DbContext
    {
        public AgBMPToolContext(DbContextOptions<AgBMPToolContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("postgis");

            builder.Entity<User>().ToTable("User");

            //This will singularize all table names
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var names = entityType.Name.Split(".");
                entityType.Relational().TableName = names[names.Length - 1];
            }

            //seed data
            AddSeedData(builder);

            //create spatial index
            //AddSpatialIndex(builder); // Index can be added after add a watershed

        }


        public int ExecuteProcedure(string sqlQuery)
        {
            return Database.ExecuteSqlCommand(sqlQuery);
        }
        public List<T> ExecuteProcedure<T>(string sqlQuery)
        {
            var results = new List<T>();

            using (var command = Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sqlQuery;
                Database.OpenConnection();

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        //get column names
                        var schema = reader.GetSchemaTable();

                        while (reader.Read())
                        {
                            var result = (T)Activator.CreateInstance(typeof(T));
                            for (int i = 0; i < schema.Rows.Count; i++)
                            {
                                //get column name
                                var columnName = schema.Rows[i][0].ToString();

                                //get the property and set the value if found
                                var prop = result.GetType().GetProperty(columnName);
                                if (prop != null && prop.CanWrite)
                                    prop.SetValue(result, reader[i], null);
                            }
                            results.Add(result);
                        }
                    }
                }
                catch { }
                finally
                {
                    Database.CloseConnection();
                }
            }

            return results;
        }

        #region Spatial Index

        private void AddSpatialIndex(ModelBuilder builder)
        {
            builder.Entity<Municipality>().HasIndex(b => b.Geometry);

            builder.Entity<CatchBasin>().HasIndex(b => b.Geometry);
            builder.Entity<ClosedDrain>().HasIndex(b => b.Geometry);
            builder.Entity<Dugout>().HasIndex(b => b.Geometry);
            builder.Entity<Feedlot>().HasIndex(b => b.Geometry);
            builder.Entity<FlowDiversion>().HasIndex(b => b.Geometry);
            builder.Entity<GrassedWaterway>().HasIndex(b => b.Geometry);
            builder.Entity<IsolatedWetland>().HasIndex(b => b.Geometry);
            builder.Entity<Lake>().HasIndex(b => b.Geometry);
            builder.Entity<ManureStorage>().HasIndex(b => b.Geometry);
            builder.Entity<PointSource>().HasIndex(b => b.Geometry);
            builder.Entity<Reach>().HasIndex(b => b.Geometry);
            builder.Entity<Reservoir>().HasIndex(b => b.Geometry);
            builder.Entity<RiparianBuffer>().HasIndex(b => b.Geometry);
            builder.Entity<RiparianWetland>().HasIndex(b => b.Geometry);
            builder.Entity<RockChute>().HasIndex(b => b.Geometry);
            builder.Entity<SmallDam>().HasIndex(b => b.Geometry);
            builder.Entity<VegetativeFilterStrip>().HasIndex(b => b.Geometry);
            builder.Entity<Wascob>().HasIndex(b => b.Geometry);

            builder.Entity<Farm>().HasIndex(b => b.Geometry);
            builder.Entity<LegalSubDivision>().HasIndex(b => b.Geometry);
            builder.Entity<Parcel>().HasIndex(b => b.Geometry);
            builder.Entity<SubArea>().HasIndex(b => b.Geometry);
            builder.Entity<Subbasin>().HasIndex(b => b.Geometry);
            builder.Entity<SubWatershed>().HasIndex(b => b.Geometry);
            builder.Entity<Watershed>().HasIndex(b => b.Geometry);
        }


        #endregion

        #region Seed Data

        private void AddSeedData(ModelBuilder builder)
        {
            AddSeedDataCountryProvince(builder);
            AddSeedDataUnitType(builder);
            AddSeedDataScenario(builder);
            AddSeedDataProject(builder);
            AddSeedDataUser(builder);
            AddSeedDataAnimalType(builder);
            AddSeedDataGeometryLayerStyle(builder);
        }

        private void AddSeedDataGeometryLayerStyle(ModelBuilder builder)
        {
            builder.Entity<GeometryLayerStyle>().HasData(
               new GeometryLayerStyle() { Id = 1, layername = "Parcel", type = "simple-fill", style = "vertical", color = "rgb(158, 0, 0, 0.6)", simplelinewidth = "", outlinecolor = "white", outlinewidth = "1" },
               new GeometryLayerStyle() { Id = 2, layername = "LSD", type = "simple-fill", style = "horizontal", color = "purple", simplelinewidth = "", outlinecolor = "white", outlinewidth = "1" },
               new GeometryLayerStyle() { Id = 3, layername = "Reach", type = "simple-line", style = "", color = "blue", simplelinewidth = "4", outlinecolor = "", outlinewidth = "" },
               new GeometryLayerStyle() { Id = 4, layername = "Farm", type = "simple-fill", style = "horizontal", color = "purple", simplelinewidth = "", outlinecolor = "white", outlinewidth = "1" },
               new GeometryLayerStyle() { Id = 5, layername = "Municipality", type = "simple-fill", style = "diagonal-cross", color = "blue", simplelinewidth = "", outlinecolor = "white", outlinewidth = "1" },
               new GeometryLayerStyle() { Id = 6, layername = "SubWaterShed", type = "simple-fill", style = "cross", color = "yellow", simplelinewidth = "", outlinecolor = "white", outlinewidth = "1" },
               new GeometryLayerStyle() { Id = 7, layername = "WaterShed", type = "simple-fill", style = "backward-diagonal", color = "purple", simplelinewidth = "", outlinecolor = "white", outlinewidth = "1" }
               );
        }

        private void AddSeedDataUser(ModelBuilder builder)
        {
            builder.Entity<UserType>().HasData(
                new UserType() { Id = 1, Name = "Admin", Description = "Admin", SortOrder = 1 },
                new UserType() { Id = 2, Name = "Watershed Manager", Description = "Watershed Manager", SortOrder = 2 },
                new UserType() { Id = 3, Name = "Municipality Manager", Description = "Municipality Manager", SortOrder = 3 },
                new UserType() { Id = 4, Name = "Farmer", Description = "Farmer", SortOrder = 4 }
                );
        }

        private void AddSeedDataUnitType(ModelBuilder builder)
        {
            builder.Entity<UnitType>().HasData(
                new UnitType() { Id = 1, Name = "Elevation", Description = "Elevation", SortOrder = 1, UnitSymbol = "m" },
                new UnitType() { Id = 2, Name = "Percentage", Description = "Percentage", SortOrder = 2, UnitSymbol = "%" },
                new UnitType() { Id = 3, Name = "Precipitation", Description = "Precipitation", SortOrder = 3, UnitSymbol = "mm" },
                new UnitType() { Id = 4, Name = "Temperature", Description = "Temperature", SortOrder = 4, UnitSymbol = "oC" },
                new UnitType() { Id = 5, Name = "Soil moisture", Description = "Soil moisture", SortOrder = 5, UnitSymbol = "mm" },
                new UnitType() { Id = 6, Name = "ET", Description = "ET", SortOrder = 6, UnitSymbol = "mm" },
                new UnitType() { Id = 7, Name = "Groundwater recharge", Description = "Groundwater recharge", SortOrder = 7, UnitSymbol = "mm" },
                new UnitType() { Id = 8, Name = "Runoff", Description = "Runoff", SortOrder = 8, UnitSymbol = "mm" },
                new UnitType() { Id = 9, Name = "TSS Yield", Description = "TSS Yield", SortOrder = 9, UnitSymbol = "ton" },
                new UnitType() { Id = 10, Name = "N/P Yield", Description = "N/P Yield", SortOrder = 10, UnitSymbol = "kg" },
                new UnitType() { Id = 11, Name = "Flow", Description = "Flow", SortOrder = 11, UnitSymbol = "m3/s" },
                new UnitType() { Id = 12, Name = "Cost", Description = "Cost", SortOrder = 12, UnitSymbol = "$" },
                new UnitType() { Id = 13, Name = "Volume", Description = "Volume", SortOrder = 13, UnitSymbol = "m3" },
                new UnitType() { Id = 14, Name = "Soil carbon", Description = "Soil carbon", SortOrder = 14, UnitSymbol = "ton" },
                new UnitType() { Id = 15, Name = "Unitless", Description = "Unitless", SortOrder = 15, UnitSymbol = "-" }
            );
        }
        private void AddSeedDataCountryProvince(ModelBuilder builder)
        {
            builder.Entity<Country>().HasData(
                 new Country() { Id = 1, Name = "Canada", Description = "Canada", SortOrder = 1 },
                 new Country() { Id = 2, Name = "USA", Description = "USA", SortOrder = 2 }
                );

            builder.Entity<Province>().HasData(
                new Province() { Id = 1, CountryId = 1, Name = "Alberta", Description = "Alberta", SortOrder = 1 },
                new Province() { Id = 2, CountryId = 1, Name = "British Columbia", Description = "British Columbia", SortOrder = 2 },
                new Province() { Id = 3, CountryId = 1, Name = "Manitoba", Description = "Manitoba", SortOrder = 3 },
                new Province() { Id = 4, CountryId = 1, Name = "New Brunswick", Description = "New Brunswick", SortOrder = 4 },
                new Province() { Id = 5, CountryId = 1, Name = "Newfoundland and Labrador", Description = "Newfoundland and Labrador", SortOrder = 5 },
                new Province() { Id = 6, CountryId = 1, Name = "Nova Scotia", Description = "Nova Scotia", SortOrder = 6 },
                new Province() { Id = 7, CountryId = 1, Name = "Ontario", Description = "Ontario", SortOrder = 7 },
                new Province() { Id = 8, CountryId = 1, Name = "Prince Edward Island", Description = "Prince Edward Island", SortOrder = 8 },
                new Province() { Id = 9, CountryId = 1, Name = "Quebec", Description = "Quebec", SortOrder = 9 },
                new Province() { Id = 10, CountryId = 1, Name = "Saskatchewan", Description = "Saskatchewan", SortOrder = 10 },
                new Province() { Id = 11, CountryId = 1, Name = "Northwest Territories", Description = "Northwest Territories", SortOrder = 11 },
                new Province() { Id = 12, CountryId = 1, Name = "Nunavut", Description = "Nunavut", SortOrder = 12 },
                new Province() { Id = 13, CountryId = 1, Name = "Yukon Territory", Description = "Yukon Territory", SortOrder = 13 },
                new Province() { Id = 14, CountryId = 2, Name = "Alabama", Description = "Alabama", SortOrder = 14 },
                new Province() { Id = 15, CountryId = 2, Name = "Alaska", Description = "Alaska", SortOrder = 15 },
                new Province() { Id = 16, CountryId = 2, Name = "Arizona", Description = "Arizona", SortOrder = 16 },
                new Province() { Id = 17, CountryId = 2, Name = "Arkansas", Description = "Arkansas", SortOrder = 17 },
                new Province() { Id = 18, CountryId = 2, Name = "California", Description = "California", SortOrder = 18 },
                new Province() { Id = 19, CountryId = 2, Name = "Colorado", Description = "Colorado", SortOrder = 19 },
                new Province() { Id = 20, CountryId = 2, Name = "Connecticut", Description = "Connecticut", SortOrder = 20 },
                new Province() { Id = 21, CountryId = 2, Name = "Delaware", Description = "Delaware", SortOrder = 21 },
                new Province() { Id = 22, CountryId = 2, Name = "Florida", Description = "Florida", SortOrder = 22 },
                new Province() { Id = 23, CountryId = 2, Name = "Georgia", Description = "Georgia", SortOrder = 23 },
                new Province() { Id = 24, CountryId = 2, Name = "Hawaii", Description = "Hawaii", SortOrder = 24 },
                new Province() { Id = 25, CountryId = 2, Name = "Idaho", Description = "Idaho", SortOrder = 25 },
                new Province() { Id = 26, CountryId = 2, Name = "Illinois", Description = "Illinois", SortOrder = 26 },
                new Province() { Id = 27, CountryId = 2, Name = "Indiana", Description = "Indiana", SortOrder = 27 },
                new Province() { Id = 28, CountryId = 2, Name = "Iowa", Description = "Iowa", SortOrder = 28 },
                new Province() { Id = 29, CountryId = 2, Name = "Kansas", Description = "Kansas", SortOrder = 29 },
                new Province() { Id = 30, CountryId = 2, Name = "Kentucky", Description = "Kentucky", SortOrder = 30 },
                new Province() { Id = 31, CountryId = 2, Name = "Louisiana", Description = "Louisiana", SortOrder = 31 },
                new Province() { Id = 32, CountryId = 2, Name = "Maine", Description = "Maine", SortOrder = 32 },
                new Province() { Id = 33, CountryId = 2, Name = "Maryland", Description = "Maryland", SortOrder = 33 },
                new Province() { Id = 34, CountryId = 2, Name = "Massachusetts", Description = "Massachusetts", SortOrder = 34 },
                new Province() { Id = 35, CountryId = 2, Name = "Michigan", Description = "Michigan", SortOrder = 35 },
                new Province() { Id = 36, CountryId = 2, Name = "Minnesota", Description = "Minnesota", SortOrder = 36 },
                new Province() { Id = 37, CountryId = 2, Name = "Mississippi", Description = "Mississippi", SortOrder = 37 },
                new Province() { Id = 38, CountryId = 2, Name = "Missouri", Description = "Missouri", SortOrder = 38 },
                new Province() { Id = 39, CountryId = 2, Name = "Montana", Description = "Montana", SortOrder = 39 },
                new Province() { Id = 40, CountryId = 2, Name = "Nebraska", Description = "Nebraska", SortOrder = 40 },
                new Province() { Id = 41, CountryId = 2, Name = "Nevada", Description = "Nevada", SortOrder = 41 },
                new Province() { Id = 42, CountryId = 2, Name = "New Hampshire", Description = "New Hampshire", SortOrder = 42 },
                new Province() { Id = 43, CountryId = 2, Name = "New Jersey", Description = "New Jersey", SortOrder = 43 },
                new Province() { Id = 44, CountryId = 2, Name = "New Mexico", Description = "New Mexico", SortOrder = 44 },
                new Province() { Id = 45, CountryId = 2, Name = "New York", Description = "New York", SortOrder = 45 },
                new Province() { Id = 46, CountryId = 2, Name = "North Carolina", Description = "North Carolina", SortOrder = 46 },
                new Province() { Id = 47, CountryId = 2, Name = "North Dakota", Description = "North Dakota", SortOrder = 47 },
                new Province() { Id = 48, CountryId = 2, Name = "Ohio", Description = "Ohio", SortOrder = 48 },
                new Province() { Id = 49, CountryId = 2, Name = "Oklahoma", Description = "Oklahoma", SortOrder = 49 },
                new Province() { Id = 50, CountryId = 2, Name = "Oregon", Description = "Oregon", SortOrder = 50 },
                new Province() { Id = 51, CountryId = 2, Name = "Pennsylvania", Description = "Pennsylvania", SortOrder = 51 },
                new Province() { Id = 52, CountryId = 2, Name = "Rhode Island", Description = "Rhode Island", SortOrder = 52 },
                new Province() { Id = 53, CountryId = 2, Name = "South Carolina", Description = "South Carolina", SortOrder = 53 },
                new Province() { Id = 54, CountryId = 2, Name = "South Dakota", Description = "South Dakota", SortOrder = 54 },
                new Province() { Id = 55, CountryId = 2, Name = "Tennessee", Description = "Tennessee", SortOrder = 55 },
                new Province() { Id = 56, CountryId = 2, Name = "Texas", Description = "Texas", SortOrder = 56 },
                new Province() { Id = 57, CountryId = 2, Name = "Utah", Description = "Utah", SortOrder = 57 },
                new Province() { Id = 58, CountryId = 2, Name = "Vermont", Description = "Vermont", SortOrder = 58 },
                new Province() { Id = 59, CountryId = 2, Name = "Virginia", Description = "Virginia", SortOrder = 59 },
                new Province() { Id = 60, CountryId = 2, Name = "Washington", Description = "Washington", SortOrder = 60 },
                new Province() { Id = 61, CountryId = 2, Name = "West Virginia", Description = "West Virginia", SortOrder = 61 },
                new Province() { Id = 62, CountryId = 2, Name = "Wisconsin", Description = "Wisconsin", SortOrder = 62 },
                new Province() { Id = 63, CountryId = 2, Name = "Wyoming", Description = "Wyoming", SortOrder = 63 },
                new Province() { Id = 64, CountryId = 2, Name = "Washington DC", Description = "Washington DC", SortOrder = 64 },
                new Province() { Id = 65, CountryId = 2, Name = "Puerto Rico", Description = "Puerto Rico", SortOrder = 65 },
                new Province() { Id = 66, CountryId = 2, Name = "U.S. Virgin Islands", Description = "U.S. Virgin Islands", SortOrder = 66 },
                new Province() { Id = 67, CountryId = 2, Name = "American Samoa", Description = "American Samoa", SortOrder = 67 },
                new Province() { Id = 68, CountryId = 2, Name = "Guam", Description = "Guam", SortOrder = 68 },
                new Province() { Id = 69, CountryId = 2, Name = "Northern Mariana Islands", Description = "Northern Mariana Islands", SortOrder = 69 }

                );
        }
        private void AddSeedDataScenario(ModelBuilder builder)
        {
            builder.Entity<ModelComponentType>().HasData(
                new ModelComponentType() { Id = 1, Name = "SubArea", Description = "Basic unit of ESAT dataset (intersect between parcel, LSD, and subbasin)", SortOrder = 1, IsStructure = false },
                new ModelComponentType() { Id = 2, Name = "Reach", Description = "Reach", SortOrder = 2, IsStructure = false },
                new ModelComponentType() { Id = 3, Name = "IsolatedWetland", Description = "Isolated wetland", SortOrder = 3, IsStructure = true },
                new ModelComponentType() { Id = 4, Name = "RiparianWetland", Description = "Riparian wetland", SortOrder = 4, IsStructure = true },
                new ModelComponentType() { Id = 5, Name = "Lake", Description = "Lake", SortOrder = 5, IsStructure = true },
                new ModelComponentType() { Id = 6, Name = "VegetativeFilterStrip", Description = "Vegetative filter strip", SortOrder = 6, IsStructure = true },
                new ModelComponentType() { Id = 7, Name = "RiparianBuffer", Description = "Riparian buffer", SortOrder = 7, IsStructure = true },
                new ModelComponentType() { Id = 8, Name = "GrassedWaterway", Description = "Grassed waterway", SortOrder = 8, IsStructure = true },
                new ModelComponentType() { Id = 9, Name = "FlowDiversion", Description = "Flow diversion", SortOrder = 9, IsStructure = true },
                new ModelComponentType() { Id = 10, Name = "Reservoir", Description = "Reservoir", SortOrder = 10, IsStructure = true },
                new ModelComponentType() { Id = 11, Name = "SmallDam", Description = "Small dam", SortOrder = 11, IsStructure = true },
                new ModelComponentType() { Id = 12, Name = "Wascob", Description = "Water and sediment control basin", SortOrder = 12, IsStructure = true },
                new ModelComponentType() { Id = 13, Name = "Dugout", Description = "Small water pond for animal drinking", SortOrder = 13, IsStructure = true },
                new ModelComponentType() { Id = 14, Name = "CatchBasin", Description = "Used to control surface runoff from a feeding operation or manure storage facility", SortOrder = 14, IsStructure = true },
                new ModelComponentType() { Id = 15, Name = "Feedlot", Description = "Animal feeding operation with an intensive animal farming ", SortOrder = 15, IsStructure = true },
                new ModelComponentType() { Id = 16, Name = "ManureStorage", Description = "On-farm manure storage", SortOrder = 16, IsStructure = true },
                new ModelComponentType() { Id = 17, Name = "RockChute", Description = "A structure that directs flow to a stream ", SortOrder = 17, IsStructure = true },
                new ModelComponentType() { Id = 18, Name = "PointSource", Description = "Point source", SortOrder = 18, IsStructure = true },
                new ModelComponentType() { Id = 19, Name = "ClosedDrain", Description = "An underground pipe that directs head surface water to a mainstream", SortOrder = 19, IsStructure = true }
            );


            builder.Entity<ScenarioType>().HasData(
                new ScenarioType() { Id = 1, Name = "Conventional", Description = "Conventional", SortOrder = 1, IsBaseLine = true },
                new ScenarioType() { Id = 2, Name = "Existing", Description = "Existing", SortOrder = 2, IsBaseLine = true, IsDefault = true }
                );

            builder.Entity<ScenarioResultSummarizationType>().HasData(
                new ScenarioResultSummarizationType() { Id = 1, Name = "LSD", Description = "LSD", SortOrder = 1 },
                new ScenarioResultSummarizationType() { Id = 2, Name = "Parcel", Description = "Parcel", SortOrder = 2, IsDefault = true },
                new ScenarioResultSummarizationType() { Id = 3, Name = "Farm", Description = "Farm", SortOrder = 3 },
                new ScenarioResultSummarizationType() { Id = 4, Name = "Municipality", Description = "Municipality", SortOrder = 4 },
                new ScenarioResultSummarizationType() { Id = 5, Name = "Subwatershed", Description = "Subwatershed", SortOrder = 5 },
                new ScenarioResultSummarizationType() { Id = 6, Name = "Watershed", Description = "Watershed", SortOrder = 6 }
                );

            builder.Entity<ScenarioModelResultVariableType>().HasData(
                new ScenarioModelResultVariableType() { Id = 1, Name = "Precipitation", Description = "Precipitation", SortOrder = 1, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 2, Name = "Temperature", Description = "Temperature", SortOrder = 2, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 3, Name = "Soil moisture", Description = "Soil moisture", SortOrder = 3, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 4, Name = "ET", Description = "ET", SortOrder = 4, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 5, Name = "Groundwater recharge", Description = "Groundwater recharge", SortOrder = 5, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 6, Name = "Runoff", Description = "Runoff", SortOrder = 6, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 7, Name = "TSS", Description = "TSS", SortOrder = 7, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 8, Name = "DN", Description = "DN", SortOrder = 8, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 9, Name = "PN", Description = "PN", SortOrder = 9, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 10, Name = "TN", Description = "TN", SortOrder = 10, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 11, Name = "DP", Description = "DP", SortOrder = 11, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 12, Name = "PP", Description = "PP", SortOrder = 12, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 13, Name = "TP", Description = "TP", SortOrder = 13, IsDefault = true },
                new ScenarioModelResultVariableType() { Id = 14, Name = "Soil carbon", Description = "Soil carbon", SortOrder = 14, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 15, Name = "Biodiversity", Description = "Biodiversity", SortOrder = 15, IsDefault = false },
                new ScenarioModelResultVariableType() { Id = 16, Name = "Outflow", Description = "Outflow", SortOrder = 16, IsDefault = false }
                );

            builder.Entity<BMPType>().HasData(
                new BMPType() { Id = 1, Name = "ISWET", Description = "Isolated wetland ", SortOrder = 1, ModelComponentTypeId = 3 },
                new BMPType() { Id = 2, Name = "RIWET", Description = "Riparian wetland ", SortOrder = 2, ModelComponentTypeId = 4 },
                new BMPType() { Id = 3, Name = "LAKE", Description = "Lake ", SortOrder = 3, ModelComponentTypeId = 5 },
                new BMPType() { Id = 4, Name = "VFST", Description = "Vegetative filter strip", SortOrder = 4, ModelComponentTypeId = 1 },
                new BMPType() { Id = 5, Name = "RIBUF", Description = "Riparian buffer", SortOrder = 5, ModelComponentTypeId = 1 },
                new BMPType() { Id = 6, Name = "GWW", Description = "Grassed waterway", SortOrder = 6, ModelComponentTypeId = 1 },
                new BMPType() { Id = 7, Name = "FLDV", Description = "Flow diversion", SortOrder = 7, ModelComponentTypeId = 9 },
                new BMPType() { Id = 8, Name = "RESV", Description = "Reservoir ", SortOrder = 8, ModelComponentTypeId = 10 },
                new BMPType() { Id = 9, Name = "SMDM", Description = "Small dam", SortOrder = 9, ModelComponentTypeId = 11 },
                new BMPType() { Id = 10, Name = "WASCOB", Description = "Water and sediment control basin", SortOrder = 10, ModelComponentTypeId = 12 },
                new BMPType() { Id = 11, Name = "CLDR", Description = "Closed drain", SortOrder = 11, ModelComponentTypeId = 19 },
                new BMPType() { Id = 12, Name = "DGOT", Description = "Dugout", SortOrder = 12, ModelComponentTypeId = 13 },
                new BMPType() { Id = 13, Name = "MCBI", Description = "Manure catch basin/impondment", SortOrder = 13, ModelComponentTypeId = 14 },
                new BMPType() { Id = 14, Name = "FDLT", Description = "Livestock feedlot ", SortOrder = 14, ModelComponentTypeId = 15 },
                new BMPType() { Id = 15, Name = "MSCD", Description = "Manure storage ", SortOrder = 15, ModelComponentTypeId = 16 },
                new BMPType() { Id = 16, Name = "RKCH", Description = "Rock chute", SortOrder = 16, ModelComponentTypeId = 17 },
                new BMPType() { Id = 17, Name = "PTSR", Description = "Point source ", SortOrder = 17, ModelComponentTypeId = 18 },
                new BMPType() { Id = 18, Name = "MI48H", Description = "Manure incorporation with 48h", SortOrder = 18, ModelComponentTypeId = 1 },
                new BMPType() { Id = 19, Name = "MASB", Description = "Manure application setback", SortOrder = 19, ModelComponentTypeId = 1 },
                new BMPType() { Id = 20, Name = "NAOS", Description = "No manure application on snow", SortOrder = 20, ModelComponentTypeId = 1 },
                new BMPType() { Id = 21, Name = "SAFA", Description = "Manure application in spring rather than fall", SortOrder = 21, ModelComponentTypeId = 1 },
                new BMPType() { Id = 22, Name = "ASNL", Description = "Manure application based on soil nitrogen limit", SortOrder = 22, ModelComponentTypeId = 1 },
                new BMPType() { Id = 23, Name = "ASPL", Description = "Manure application based on soil phosphorous limit", SortOrder = 23, ModelComponentTypeId = 1 },
                new BMPType() { Id = 24, Name = "WSMG", Description = "Livestock wintering site", SortOrder = 24, ModelComponentTypeId = 1 },
                new BMPType() { Id = 25, Name = "OFSW", Description = "Livestock off-site watering", SortOrder = 25, ModelComponentTypeId = 1 },
                new BMPType() { Id = 26, Name = "SAMG", Description = "Livestock stream access management", SortOrder = 26, ModelComponentTypeId = 1 },
                new BMPType() { Id = 27, Name = "ROGZ", Description = "Rotational grazing", SortOrder = 27, ModelComponentTypeId = 1 },
                new BMPType() { Id = 28, Name = "WDBR", Description = "Windbreak", SortOrder = 28, ModelComponentTypeId = 1 },
                new BMPType() { Id = 29, Name = "CVCR", Description = "Cover crop", SortOrder = 29, ModelComponentTypeId = 1 },
                new BMPType() { Id = 30, Name = "CSTL", Description = "Conservation tillage", SortOrder = 30, ModelComponentTypeId = 1 },
                new BMPType() { Id = 31, Name = "CRRO", Description = "Crop rotation", SortOrder = 31, ModelComponentTypeId = 1 },
                new BMPType() { Id = 32, Name = "FRCV", Description = "Forage conversion", SortOrder = 32, ModelComponentTypeId = 1 },
                new BMPType() { Id = 33, Name = "TLDMG", Description = "Tile drain management", SortOrder = 33, ModelComponentTypeId = 1 },
                new BMPType() { Id = 34, Name = "TERR", Description = "Terrace", SortOrder = 34, ModelComponentTypeId = 1 },
                new BMPType() { Id = 35, Name = "RDMG", Description = "Residule management", SortOrder = 35, ModelComponentTypeId = 1 },
                new BMPType() { Id = 36, Name = "MTHS", Description = "Minimum tillage on high slope", SortOrder = 36, ModelComponentTypeId = 1 },
                new BMPType() { Id = 37, Name = "PSTPS", Description = "Plant species in tame pasture", SortOrder = 37, ModelComponentTypeId = 1 },
                new BMPType() { Id = 38, Name = "SUNA", Description = "Sustainable use of natural area", SortOrder = 38, ModelComponentTypeId = 1 },
                new BMPType() { Id = 39, Name = "IRRMG", Description = "Irrigation management", SortOrder = 39, ModelComponentTypeId = 1 },
                new BMPType() { Id = 40, Name = "FERMG", Description = "Fertilizer management", SortOrder = 40, ModelComponentTypeId = 1 },
                new BMPType() { Id = 41, Name = "AOPANoMASB", Description = "MI48H+NAOS+ASNL", SortOrder = 41, ModelComponentTypeId = 1 }
                );

            builder.Entity<BMPCombinationType>().HasData(
                new BMPCombinationType() { Id = 1, Name = "ISWET", Description = "Isolated wetland ", SortOrder = 1, ModelComponentTypeId = 3 },
                new BMPCombinationType() { Id = 2, Name = "RIWET", Description = "Riparian wetland ", SortOrder = 2, ModelComponentTypeId = 4 },
                new BMPCombinationType() { Id = 3, Name = "LAKE", Description = "Lake ", SortOrder = 3, ModelComponentTypeId = 5 },
                new BMPCombinationType() { Id = 4, Name = "VFST", Description = "Vegetative filter strip", SortOrder = 4, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 5, Name = "RIBUF", Description = "Riparian buffer", SortOrder = 5, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 6, Name = "GWW", Description = "Grassed waterway", SortOrder = 6, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 7, Name = "FLDV", Description = "Flow diversion", SortOrder = 7, ModelComponentTypeId = 9 },
                new BMPCombinationType() { Id = 8, Name = "RESV", Description = "Reservoir ", SortOrder = 8, ModelComponentTypeId = 10 },
                new BMPCombinationType() { Id = 9, Name = "SMDM", Description = "Small dam", SortOrder = 9, ModelComponentTypeId = 11 },
                new BMPCombinationType() { Id = 10, Name = "WASCOB", Description = "Water and sediment control basin", SortOrder = 10, ModelComponentTypeId = 12 },
                new BMPCombinationType() { Id = 11, Name = "CLDR", Description = "Closed drain", SortOrder = 11, ModelComponentTypeId = 19 },
                new BMPCombinationType() { Id = 12, Name = "DGOT", Description = "Dugout", SortOrder = 12, ModelComponentTypeId = 13 },
                new BMPCombinationType() { Id = 13, Name = "MCBI", Description = "Manure catch basin/impondment", SortOrder = 13, ModelComponentTypeId = 14 },
                new BMPCombinationType() { Id = 14, Name = "FDLT", Description = "Livestock feedlot ", SortOrder = 14, ModelComponentTypeId = 15 },
                new BMPCombinationType() { Id = 15, Name = "MSCD", Description = "Manure storage ", SortOrder = 15, ModelComponentTypeId = 16 },
                new BMPCombinationType() { Id = 16, Name = "RKCH", Description = "Rock chute", SortOrder = 16, ModelComponentTypeId = 17 },
                new BMPCombinationType() { Id = 17, Name = "PTSR", Description = "Point source ", SortOrder = 17, ModelComponentTypeId = 18 },
                new BMPCombinationType() { Id = 18, Name = "MI48H", Description = "Manure incorporation with 48h", SortOrder = 18, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 19, Name = "MASB", Description = "Manure application setback", SortOrder = 19, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 20, Name = "NAOS", Description = "No manure application on snow", SortOrder = 20, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 21, Name = "SAFA", Description = "Manure application in spring rather than fall", SortOrder = 21, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 22, Name = "ASNL", Description = "Manure application based on soil nitrogen limit", SortOrder = 22, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 23, Name = "ASPL", Description = "Manure application based on soil phosphorous limit", SortOrder = 23, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 24, Name = "WSMG", Description = "Livestock wintering site", SortOrder = 24, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 25, Name = "OFSW", Description = "Livestock off-site watering", SortOrder = 25, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 26, Name = "SAMG", Description = "Livestock stream access management", SortOrder = 26, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 27, Name = "ROGZ", Description = "Rotational grazing", SortOrder = 27, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 28, Name = "WDBR", Description = "Windbreak", SortOrder = 28, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 29, Name = "CVCR", Description = "Cover crop", SortOrder = 29, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 30, Name = "CSTL", Description = "Conservation tillage", SortOrder = 30, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 31, Name = "CRRO", Description = "Crop rotation", SortOrder = 31, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 32, Name = "FRCV", Description = "Forage conversion", SortOrder = 32, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 33, Name = "TLDMG", Description = "Tile drain management", SortOrder = 33, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 34, Name = "TERR", Description = "Terrace", SortOrder = 34, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 35, Name = "RDMG", Description = "Residule management", SortOrder = 35, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 36, Name = "MTHS", Description = "Minimum tillage on high slope", SortOrder = 36, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 37, Name = "PSTPS", Description = "Plant species in tame pasture", SortOrder = 37, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 38, Name = "SUNA", Description = "Sustainable use of natural area", SortOrder = 38, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 39, Name = "IRRMG", Description = "Irrigation management", SortOrder = 39, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 40, Name = "FERMG", Description = "Fertilizer management", SortOrder = 40, ModelComponentTypeId = 1 },
                new BMPCombinationType() { Id = 41, Name = "AOPANoMASB", Description = "MI48H+NAOS+ASNL", SortOrder = 41, ModelComponentTypeId = 1 }
                );

            builder.Entity<BMPCombinationBMPTypes>().HasData(
                new BMPCombinationBMPTypes() { Id = 1, BMPTypeId = 1, BMPCombinationTypeId = 1 },
                new BMPCombinationBMPTypes() { Id = 2, BMPTypeId = 2, BMPCombinationTypeId = 2 },
                new BMPCombinationBMPTypes() { Id = 3, BMPTypeId = 3, BMPCombinationTypeId = 3 },
                new BMPCombinationBMPTypes() { Id = 4, BMPTypeId = 4, BMPCombinationTypeId = 4 },
                new BMPCombinationBMPTypes() { Id = 5, BMPTypeId = 5, BMPCombinationTypeId = 5 },
                new BMPCombinationBMPTypes() { Id = 6, BMPTypeId = 6, BMPCombinationTypeId = 6 },
                new BMPCombinationBMPTypes() { Id = 7, BMPTypeId = 7, BMPCombinationTypeId = 7 },
                new BMPCombinationBMPTypes() { Id = 8, BMPTypeId = 8, BMPCombinationTypeId = 8 },
                new BMPCombinationBMPTypes() { Id = 9, BMPTypeId = 9, BMPCombinationTypeId = 9 },
                new BMPCombinationBMPTypes() { Id = 10, BMPTypeId = 10, BMPCombinationTypeId = 10 },
                new BMPCombinationBMPTypes() { Id = 11, BMPTypeId = 11, BMPCombinationTypeId = 11 },
                new BMPCombinationBMPTypes() { Id = 12, BMPTypeId = 12, BMPCombinationTypeId = 12 },
                new BMPCombinationBMPTypes() { Id = 13, BMPTypeId = 13, BMPCombinationTypeId = 13 },
                new BMPCombinationBMPTypes() { Id = 14, BMPTypeId = 14, BMPCombinationTypeId = 14 },
                new BMPCombinationBMPTypes() { Id = 15, BMPTypeId = 15, BMPCombinationTypeId = 15 },
                new BMPCombinationBMPTypes() { Id = 16, BMPTypeId = 16, BMPCombinationTypeId = 16 },
                new BMPCombinationBMPTypes() { Id = 17, BMPTypeId = 17, BMPCombinationTypeId = 17 },
                new BMPCombinationBMPTypes() { Id = 18, BMPTypeId = 18, BMPCombinationTypeId = 18 },
                new BMPCombinationBMPTypes() { Id = 19, BMPTypeId = 19, BMPCombinationTypeId = 19 },
                new BMPCombinationBMPTypes() { Id = 20, BMPTypeId = 20, BMPCombinationTypeId = 20 },
                new BMPCombinationBMPTypes() { Id = 21, BMPTypeId = 21, BMPCombinationTypeId = 21 },
                new BMPCombinationBMPTypes() { Id = 22, BMPTypeId = 22, BMPCombinationTypeId = 22 },
                new BMPCombinationBMPTypes() { Id = 23, BMPTypeId = 23, BMPCombinationTypeId = 23 },
                new BMPCombinationBMPTypes() { Id = 24, BMPTypeId = 24, BMPCombinationTypeId = 24 },
                new BMPCombinationBMPTypes() { Id = 25, BMPTypeId = 25, BMPCombinationTypeId = 25 },
                new BMPCombinationBMPTypes() { Id = 26, BMPTypeId = 26, BMPCombinationTypeId = 26 },
                new BMPCombinationBMPTypes() { Id = 27, BMPTypeId = 27, BMPCombinationTypeId = 27 },
                new BMPCombinationBMPTypes() { Id = 28, BMPTypeId = 28, BMPCombinationTypeId = 28 },
                new BMPCombinationBMPTypes() { Id = 29, BMPTypeId = 29, BMPCombinationTypeId = 29 },
                new BMPCombinationBMPTypes() { Id = 30, BMPTypeId = 30, BMPCombinationTypeId = 30 },
                new BMPCombinationBMPTypes() { Id = 31, BMPTypeId = 31, BMPCombinationTypeId = 31 },
                new BMPCombinationBMPTypes() { Id = 32, BMPTypeId = 32, BMPCombinationTypeId = 32 },
                new BMPCombinationBMPTypes() { Id = 33, BMPTypeId = 33, BMPCombinationTypeId = 33 },
                new BMPCombinationBMPTypes() { Id = 34, BMPTypeId = 34, BMPCombinationTypeId = 34 },
                new BMPCombinationBMPTypes() { Id = 35, BMPTypeId = 35, BMPCombinationTypeId = 35 },
                new BMPCombinationBMPTypes() { Id = 36, BMPTypeId = 36, BMPCombinationTypeId = 36 },
                new BMPCombinationBMPTypes() { Id = 37, BMPTypeId = 37, BMPCombinationTypeId = 37 },
                new BMPCombinationBMPTypes() { Id = 38, BMPTypeId = 38, BMPCombinationTypeId = 38 },
                new BMPCombinationBMPTypes() { Id = 39, BMPTypeId = 39, BMPCombinationTypeId = 39 },
                new BMPCombinationBMPTypes() { Id = 40, BMPTypeId = 40, BMPCombinationTypeId = 40 },
                new BMPCombinationBMPTypes() { Id = 41, BMPTypeId = 41, BMPCombinationTypeId = 41 }
                );

            builder.Entity<ScenarioModelResultType>().HasData(
                new ScenarioModelResultType() { Id = 1, Name = "Precipitation", Description = "Subarea yearly precipitation", SortOrder = 1, UnitTypeId = 3, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 1 },
                new ScenarioModelResultType() { Id = 2, Name = "Temperature", Description = "Suarea annual average temperature", SortOrder = 2, UnitTypeId = 4, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 2 },
                new ScenarioModelResultType() { Id = 3, Name = "Soil moisture", Description = "Subarea annual average soil water content", SortOrder = 3, UnitTypeId = 5, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 3 },
                new ScenarioModelResultType() { Id = 4, Name = "ET", Description = "Subarea yearly ET", SortOrder = 4, UnitTypeId = 6, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 4 },
                new ScenarioModelResultType() { Id = 5, Name = "Groundwater recharge", Description = "Subarea yearly GW recharge", SortOrder = 5, UnitTypeId = 7, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 5 },
                new ScenarioModelResultType() { Id = 6, Name = "Runoff", Description = "Subarea yearly runoff", SortOrder = 6, UnitTypeId = 8, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 6 },
                new ScenarioModelResultType() { Id = 7, Name = "TSS Yield", Description = "Subarea yearly TSS yield", SortOrder = 7, UnitTypeId = 9, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 7 },
                new ScenarioModelResultType() { Id = 8, Name = "DN Yield", Description = "Subarea yearly DN yield", SortOrder = 8, UnitTypeId = 10, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 8 },
                new ScenarioModelResultType() { Id = 9, Name = "PN Yield", Description = "Subarea yearly PN yield", SortOrder = 9, UnitTypeId = 10, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 9 },
                new ScenarioModelResultType() { Id = 10, Name = "TN Yield", Description = "Subarea yearly TN yield", SortOrder = 10, UnitTypeId = 10, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 10 },
                new ScenarioModelResultType() { Id = 11, Name = "DP Yield", Description = "Subarea yearly DP yield", SortOrder = 11, UnitTypeId = 10, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 11 },
                new ScenarioModelResultType() { Id = 12, Name = "PP Yield", Description = "Subarea yearly PP yield", SortOrder = 12, UnitTypeId = 10, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 12 },
                new ScenarioModelResultType() { Id = 13, Name = "TP Yield", Description = "Subarea yearly TP yield", SortOrder = 13, UnitTypeId = 10, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 13 },
                new ScenarioModelResultType() { Id = 14, Name = "Soil carbon", Description = "Subarea yearly average soil carbon sequestration", SortOrder = 14, UnitTypeId = 14, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 14 },
                new ScenarioModelResultType() { Id = 15, Name = "Biodiversity", Description = "Subarea yearly biodiversity index", SortOrder = 15, UnitTypeId = 15, ModelComponentTypeId = 1, ScenarioModelResultVariableTypeId = 15 },
                new ScenarioModelResultType() { Id = 16, Name = "Runoff reach outflow", Description = "Annual average flow rate at reach outlet", SortOrder = 16, UnitTypeId = 11, ModelComponentTypeId = 2, ScenarioModelResultVariableTypeId = 16 },
                new ScenarioModelResultType() { Id = 17, Name = "TSS reach loading", Description = "Yearly TSS loading at reach outlet", SortOrder = 17, UnitTypeId = 9, ModelComponentTypeId = 2, ScenarioModelResultVariableTypeId = 7 },
                new ScenarioModelResultType() { Id = 18, Name = "DN reach loading", Description = "Yearly DN loading at reach outlet", SortOrder = 18, UnitTypeId = 10, ModelComponentTypeId = 2, ScenarioModelResultVariableTypeId = 8 },
                new ScenarioModelResultType() { Id = 19, Name = "PN reach loading", Description = "Yearly PN loading at reach outlet", SortOrder = 19, UnitTypeId = 10, ModelComponentTypeId = 2, ScenarioModelResultVariableTypeId = 9 },
                new ScenarioModelResultType() { Id = 20, Name = "TN reach loading", Description = "Yearly TN loading at reach outlet", SortOrder = 20, UnitTypeId = 10, ModelComponentTypeId = 2, ScenarioModelResultVariableTypeId = 10 },
                new ScenarioModelResultType() { Id = 21, Name = "DP reach loading", Description = "Yearly DP loading at reach outlet", SortOrder = 21, UnitTypeId = 10, ModelComponentTypeId = 2, ScenarioModelResultVariableTypeId = 11 },
                new ScenarioModelResultType() { Id = 22, Name = "PP reach loading", Description = "Yearly PP loading at reach outlet", SortOrder = 22, UnitTypeId = 10, ModelComponentTypeId = 2, ScenarioModelResultVariableTypeId = 12 },
                new ScenarioModelResultType() { Id = 23, Name = "TP reach loading", Description = "Yearly TP loading at reach outlet", SortOrder = 23, UnitTypeId = 10, ModelComponentTypeId = 2, ScenarioModelResultVariableTypeId = 13 }
                );

            builder.Entity<BMPEffectivenessType>().HasData(
                new BMPEffectivenessType() { Id = 1, Name = "Soil moisture onsite", Description = "BMP onsite effectiveness on annual average soil water", SortOrder = 1, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 3, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 3, UserEditableConstraintBoundTypeId = 1, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0},
                new BMPEffectivenessType() { Id = 2, Name = "ET onsite", Description = "BMP onsite effectiveness on yearly ET", SortOrder = 2, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 4, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 4, UserEditableConstraintBoundTypeId = 1, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0},
                new BMPEffectivenessType() { Id = 3, Name = "Groundwater recharge onsite", Description = "BMP onsite effectiveness on yearly GW recharge", SortOrder = 3, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 5, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 5, UserEditableConstraintBoundTypeId = 1, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 4, Name = "Runoff onsite", Description = "BMP onsite effectiveness on yearly runoff", SortOrder = 4, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 6, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 6, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 5, Name = "TSS onsite", Description = "BMP onsite effectiveness on yearly TSS", SortOrder = 5, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 7, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 7, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 6, Name = "DN onsite", Description = "BMP onsite effectiveness on yearly DN", SortOrder = 6, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 8, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 8, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 7, Name = "PN onsite", Description = "BMP onsite effectiveness on yearly PN", SortOrder = 7, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 9, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 9, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 8, Name = "TN onsite", Description = "BMP onsite effectiveness on yearly TN", SortOrder = 8, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 10, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 10, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 9, Name = "DP onsite", Description = "BMP onsite effectiveness on yearly DP", SortOrder = 9, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 11, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 11, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 10, Name = "PP onsite", Description = "BMP onsite effectiveness on yearly PP", SortOrder = 10, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 12, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 12, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 11, Name = "TP onsite", Description = "BMP onsite effectiveness on yearly TP", SortOrder = 11, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 13, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 13, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 12, Name = "Soil carbon onsite", Description = "BMP onsite effectiveness on yearly soil carbon", SortOrder = 12, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 14, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 14, UserEditableConstraintBoundTypeId = 1, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 13, Name = "Biodiversity onsite", Description = "BMP onsite effectiveness on yearly biodiversity index", SortOrder = 13, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 15, BMPEffectivenessLocationTypeId = 1, ScenarioModelResultTypeId = 15, UserEditableConstraintBoundTypeId = 1, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 14, Name = "Runoff offsite", Description = "BMP offsite effectiveness on yearly outlet flow rate", SortOrder = 14, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 16, BMPEffectivenessLocationTypeId = 2, ScenarioModelResultTypeId = 16, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 15, Name = "TSS offsite", Description = "BMP offsite effectiveness on yearly outlet TSS", SortOrder = 15, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 7, BMPEffectivenessLocationTypeId = 2, ScenarioModelResultTypeId = 17, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 16, Name = "DN offsite", Description = "BMP offsite effectiveness on yearly outlet DN", SortOrder = 16, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 8, BMPEffectivenessLocationTypeId = 2, ScenarioModelResultTypeId = 18, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 17, Name = "PN offsite", Description = "BMP offsite effectiveness on yearly outlet PN", SortOrder = 17, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 9, BMPEffectivenessLocationTypeId = 2, ScenarioModelResultTypeId = 19, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 18, Name = "TN offsite", Description = "BMP offsite effectiveness on yearly outlet TN", SortOrder = 18, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 10, BMPEffectivenessLocationTypeId = 2, ScenarioModelResultTypeId = 20, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 19, Name = "DP offsite", Description = "BMP offsite effectiveness on yearly outlet DP", SortOrder = 19, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 11, BMPEffectivenessLocationTypeId = 2, ScenarioModelResultTypeId = 21, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 20, Name = "PP offsite", Description = "BMP offsite effectiveness on yearly outlet PP", SortOrder = 20, UnitTypeId = 2, DefaultWeight = 0, ScenarioModelResultVariableTypeId = 12, BMPEffectivenessLocationTypeId = 2, ScenarioModelResultTypeId = 22, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 21, Name = "TP offsite", Description = "BMP offsite effectiveness on yearly outlet TP", SortOrder = 21, UnitTypeId = 2, DefaultWeight = 100, ScenarioModelResultVariableTypeId = 13, BMPEffectivenessLocationTypeId = 2, DefaultConstraintTypeId = 1, DefaultConstraint = -20, ScenarioModelResultTypeId = 23, UserEditableConstraintBoundTypeId = 2, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 },
                new BMPEffectivenessType() { Id = 22, Name = "BMP cost", Description = "BMP yearly cost", SortOrder = 22, UnitTypeId = 12, DefaultWeight = 0, ScenarioModelResultVariableTypeId = null, BMPEffectivenessLocationTypeId = 2, ScenarioModelResultTypeId = null, UserEditableConstraintBoundTypeId = 1, UserNotEditableConstraintValueTypeId = 1, UserNotEditableConstraintBoundValue = 0 }
            );

            builder.Entity<OptimizationSolutionLocationType>().HasData(
                new OptimizationSolutionLocationType() { Id = 1, Name = "LegalSubDivision", Description = "Legal Sub-division ", SortOrder = 1 },
                new OptimizationSolutionLocationType() { Id = 2, Name = "Parcel", Description = "Parcel ", SortOrder = 2 },
                new OptimizationSolutionLocationType() { Id = 3, Name = "ReachBMP", Description = "Reach BMP ", SortOrder = 3 }
                );

        }
        private void AddSeedDataProject(ModelBuilder builder)
        {
            builder.Entity<ProjectSpatialUnitType>().HasData(
                new ProjectSpatialUnitType() { Id = 1, Name = "LSD", Description = "LSD", SortOrder = 1, IsDefault = false },
                new ProjectSpatialUnitType() { Id = 2, Name = "Parcel", Description = "Parcel", SortOrder = 2, IsDefault = true }
                );


            builder.Entity<OptimizationType>().HasData(
                new OptimizationType() { Id = 1, Name = "Eco-service", Description = "Eco-service", SortOrder = 1, IsDefault = false },
                new OptimizationType() { Id = 2, Name = "Budget", Description = "Budget", SortOrder = 2, IsDefault = true }
                );

            builder.Entity<BMPEffectivenessLocationType>().HasData(
                new BMPEffectivenessLocationType() { Id = 1, Name = "Onsite", Description = "Onsite", SortOrder = 1, IsDefault = false },
                new BMPEffectivenessLocationType() { Id = 2, Name = "Offsite", Description = "Offsite", SortOrder = 2, IsDefault = true }
                );

            builder.Entity<OptimizationConstraintValueType>().HasData(
                new OptimizationConstraintValueType() { Id = 1, Name = "Percentage", Description = "Percentage", SortOrder = 1, IsDefault = true },
                new OptimizationConstraintValueType() { Id = 2, Name = "Absolute Value", Description = "Absolute Value", SortOrder = 2, IsDefault = false }
                );

            builder.Entity<OptimizationConstraintBoundType>().HasData(
                new OptimizationConstraintBoundType() { Id = 1, Name = "Upper", Description = "Upper bound", SortOrder = 1 },
                new OptimizationConstraintBoundType() { Id = 2, Name = "Lower", Description = "Lower bound", SortOrder = 2 }
                );
        }
        private void AddSeedDataAnimalType(ModelBuilder builder)
        {
            builder.Entity<AnimalType>().HasData(
                new AnimalType() { Id = 1, Name = "Dairy", Description = "Dairy manure", SortOrder = 1 },
                new AnimalType() { Id = 2, Name = "Beef", Description = "Beef manure", SortOrder = 2 },
                new AnimalType() { Id = 3, Name = "Cow-Calf", Description = "Cow-Calf manure", SortOrder = 3 },
                new AnimalType() { Id = 4, Name = "Swine", Description = "Swine manure", SortOrder = 4 },
                new AnimalType() { Id = 5, Name = "Sheep", Description = "Sheep manure", SortOrder = 5 },
                new AnimalType() { Id = 6, Name = "Goat", Description = "Goat manure", SortOrder = 6 },
                new AnimalType() { Id = 7, Name = "Horse", Description = "Horse manure", SortOrder = 7 },
                new AnimalType() { Id = 8, Name = "Turkey", Description = "Turkey manure", SortOrder = 8 },
                new AnimalType() { Id = 9, Name = "Duck", Description = "Duck manure", SortOrder = 9 }
            );
        }

        #endregion

        #region CostEffectiveness

        public DbSet<UnitScenarioEffectiveness> UnitScenarioCostEffectiveness { get; set; }

        #endregion

        #region Boundary

        public DbSet<LegalSubDivision> LegalSubDivisions { get; set; }
        public DbSet<Parcel> Parcels { get; set; }
        public DbSet<Municipality> Municipalities { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<GeometryLayerStyle> geometryStyles { get; set; }
        #endregion

        #region Model Components

        public DbSet<AnimalType> AnimalTypes { get; set; }
        public DbSet<CatchBasin> CatchBasins { get; set; }
        public DbSet<ClosedDrain> ClosedDrains { get; set; }
        public DbSet<Dugout> Dugouts { get; set; }
        public DbSet<Feedlot> Feedlots { get; set; }
        public DbSet<FlowDiversion> FlowDiversions { get; set; }
        public DbSet<GrassedWaterway> GrassedWaterways { get; set; }
        public DbSet<IsolatedWetland> IsolatedWetlands { get; set; }
        public DbSet<Lake> Lakes { get; set; }
        public DbSet<ManureStorage> ManureStorages { get; set; }
        public DbSet<ModelComponent> ModelComponents { get; set; }
        public DbSet<ModelComponentBMPTypes> ModelComponentBMPTypes { get; set; }
        public DbSet<ModelComponentType> ModelComponentTypes { get; set; }
        public DbSet<PointSource> PointSources { get; set; }
        public DbSet<Reach> Reaches { get; set; }
        public DbSet<Reservoir> Reservoirs { get; set; }
        public DbSet<RiparianBuffer> RiparianBuffers { get; set; }
        public DbSet<RiparianWetland> RiparianWetlands { get; set; }
        public DbSet<RockChute> RockChutes { get; set; }
        public DbSet<SmallDam> SmallDams { get; set; }
        public DbSet<SubArea> SubAreas { get; set; }
        public DbSet<Subbasin> Subbasins { get; set; }
        public DbSet<SubWatershed> SubWatersheds { get; set; }
        public DbSet<VegetativeFilterStrip> VegetativeFilterStrips { get; set; }
        public DbSet<Wascob> Wascobs { get; set; }
        public DbSet<Watershed> Watersheds { get; set; }
        #endregion

        #region Model Result

        public DbSet<ScenarioModelResult> ScenarioModelResults { get; set; }
        public DbSet<ScenarioModelResultType> ScenarioModelResultTypes { get; set; }
        public DbSet<ScenarioModelResultVariableType> ScenarioModelResultVariableTypes { get; set; }
        public DbSet<ScenarioResultSummarizationType> ScenarioResultSummarizationTypes { get; set; }
        #endregion

        #region Optimization

        public DbSet<Optimization> Optimizations { get; set; }
        public DbSet<OptimizationConstraints> OptimizationConstraints { get; set; }
        public DbSet<OptimizationWeights> OptimizationWeights { get; set; }
        public DbSet<OptimizationLegalSubDivisions> OptimizationLegalSubDivisions { get; set; }
        public DbSet<OptimizationParcels> OptimizationParcels { get; set; }
        public DbSet<OptimizationModelComponents> OptimizationModelComponents { get; set; }
        public DbSet<OptimizationType> OptimizationTypes { get; set; }
        public DbSet<OptimizationConstraintBoundType> OptimizationConstraintBoundTypes { get; set; }
        public DbSet<OptimizationConstraintValueType> OptimizationConstraintValueTypes { get; set; }

        #endregion

        #region Project

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMunicipalities> ProjectMunicipalities { get; set; }
        public DbSet<ProjectWatersheds> ProjectWatersheds { get; set; }
        public DbSet<ProjectSpatialUnitType> ProjectSpatialUnitTypes { get; set; }
        #endregion

        #region Scenario
        public DbSet<BMPEffectivenessLocationType> BMPEffectivenessLocationTypes { get; set; }
        public DbSet<BMPEffectivenessType> BMPEffectivenessTypes { get; set; }
        public DbSet<ScenarioType> ScenarioTypes { get; set; }
        public DbSet<BMPType> BMPTypes { get; set; }
        public DbSet<BMPCombinationType> BMPCombinationTypes { get; set; }
        public DbSet<BMPCombinationBMPTypes> BMPCombinationBMPTypes { get; set; }
        public DbSet<Scenario> Scenarios { get; set; }
        public DbSet<UnitScenario> UnitScenarios { get; set; }
        public DbSet<UnitScenarioEffectiveness> UnitScenarioEffectivenesses { get; set; }
        public DbSet<WatershedExistingBMPType> WatershedExistingBMPTypes { get; set; }
        public DbSet<Investor> Investors { get; set; }
        public DbSet<OptimizationSolutionLocationType> OptimizationSolutionLocationTypes { get; set; }
        public DbSet<UnitOptimizationSolution> UnitOptimizationSolutions { get; set; }
        public DbSet<UnitOptimizationSolutionEffectiveness> UnitOptimizationSolutionEffectivenesses { get; set; }
        #endregion

        #region User

        public DbSet<UserParcels> UserParcels { get; set; }
        public DbSet<UserMunicipalities> UserMunicipalities { get; set; }
        public DbSet<UserWatersheds> UserWatersheds { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        #endregion

        #region Solution

        public DbSet<Solution> Solutions { get; set; }
        public DbSet<SolutionLegalSubDivisions> SolutionLegalSubDivisions { get; set; }
        public DbSet<SolutionParcels> SolutionParcels { get; set; }

        #endregion

        #region Types

        public DbSet<UnitType> UnitTypes { get; set; }

        #endregion

        public DbSet<User> Users { get; set; }
    }
}
