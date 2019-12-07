using AgBMPTool.DBModel;
using AgBMPTool.DBModel.Model.Boundary;
using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMTool.DBL;
using ESAT.Import.Model;
using ESAT.Import.Utils;
using ESAT.Import.VectorObjectConvertor.Model;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql;
using OSGeo.OGR;
using System;
using System.Collections.Generic;

namespace ESAT.Import
{

    public class Program
    {
        public static string PROGRAM_NAME = "ESAT-Importor";
        public static string VERSION = "0.0.1";
        public static bool IS_TESTING = true;
        public static Int16 NO_DATA_VALUE = -999;
        public static Int16 INVALID_VALUE = -32768;

        public class SubWatershedInfo
        {
            public int subwatershedid { get; set; }
        }

        public class SubAreaResult
        {
            public int subareaid { get; set; }

            public int modelresulttypeid { get; set; }

            public int resultyear { get; set; }

            public decimal resultvalue { get; set; }
        }

        public static List<SubAreaResult> ExecuteProcedure()
        {
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                return db.ExecuteProcedure<SubAreaResult>("select * from agbmptool_getsubarearesult(1, 1, -1, -1, -1, 2000, 2001)");
            }
        }

        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;

            //AddTestDB();

            AddIFCDB();

            Console.WriteLine($"Total import time: {(DateTime.Now - start).TotalSeconds} seconds ...");

            Console.ReadLine();
        }

        static void AddTestDB()
        {
            //ExecuteProcedure();

            GdalConfiguration.ConfigureGdal();
            GdalConfiguration.ConfigureOgr();
            //convert(@"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190911_import_shape", 3400);

            Dictionary<string, VectorFile> tableToPath = new Dictionary<string, VectorFile>();
            int projectionCode = 3400;
            string projectionWKT = "PROJCS[\"NAD_1983_Transverse_Mercator\",GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",500000.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",-115.0],PARAMETER[\"Scale_Factor\",0.9992],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0]]";
            string vectorDriver = "ESRI Shapefile";
            string dir = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\191003_Test_Database\Input\Vectors\";
            tableToPath.Add("Watershed", new VectorFile { Path = $"{dir}Watershed.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Parcel", new VectorFile { Path = $"{dir}Parcel.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("LegalSubDivision", new VectorFile { Path = $"{dir}LegalSubDivision.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Farm", new VectorFile { Path = $"{dir}Farm.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Municipality", new VectorFile { Path = $"{dir}Municipality.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("SubWatershed", new VectorFile { Path = $"{dir}SubWatershed.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Subbasin", new VectorFile { Path = $"{dir}Subbasin.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Reach", new VectorFile { Path = $"{dir}Reach.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("SubArea", new VectorFile { Path = $"{dir}SubArea.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("IsolatedWetland", new VectorFile { Path = $"{dir}IsolatedWetland.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("RiparianWetland", new VectorFile { Path = $"{dir}RiparianWetland.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Lake", new VectorFile { Path = $"{dir}Lake.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("VegetativeFilterStrip", new VectorFile { Path = $"{dir}VegetativeFilterStrip.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("RiparianBuffer", new VectorFile { Path = $"{dir}RiparianBuffer.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("GrassedWaterway", new VectorFile { Path = $"{dir}GrassedWaterway.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("FlowDiversion", new VectorFile { Path = $"{dir}FlowDiversion.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Reservoir", new VectorFile { Path = $"{dir}Reservoir.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("SmallDam", new VectorFile { Path = $"{dir}SmallDam.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Wascob", new VectorFile { Path = $"{dir}Wascob.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Dugout", new VectorFile { Path = $"{dir}Dugout.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("CatchBasin", new VectorFile { Path = $"{dir}CatchBasin.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Feedlot", new VectorFile { Path = $"{dir}Feedlot.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("ManureStorage", new VectorFile { Path = $"{dir}ManureStorage.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("RockChute", new VectorFile { Path = $"{dir}RockChute.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("PointSource", new VectorFile { Path = $"{dir}PointSource.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("ClosedDrain", new VectorFile { Path = $"{dir}ClosedDrain.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT VECTORS ****"); }
            VectorImportor v = new VectorImportor();
            v.ImportAllVectorsOCR(tableToPath);

            ResultImportor r = new ResultImportor();
            dir = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\191003_Test_Database\Input\Results\";

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT SCENARIO RESULTS ****"); }
            r.ImportScenarioResults(
                $"{dir}ScenarioResultsSubArea.csv",
                $"{dir}ScenarioResultsReach.csv"
                );

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT BMP EFFECTIVENESS RESULTS ****"); }
            r.ImportBmpEffectiveness(new List<string>()
            {
                $"{dir}BmpEffectivenessSubArea.csv",
                $"{dir}BmpEffectivenessReach.csv"
            });

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT USERS & INVESTORS ****"); }
            string testProjectPath = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\191109_Add_user\Add_user_investor.txt";
            new TestProjectImportor(new AgBMPToolUnitOfWork(ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContextSingleton)).ImportTestProjectFromText(testProjectPath, 1, v.GetWatershedId());

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT PROCEDURES ****"); }
            List<string> procedureFiles = new List<string>
            {
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\project_bmp_locations.sql",
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\Summary Result.sql",
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\update agbmptool_getprojectdefaultbmplocations_aggregate.sql",
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\doc\2019-11-01\insert_script",
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\Add Materialized Views and Functions for Model Result Summary.sql",
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\Add Materialized Views and Functions for Start and End Year.sql",
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\Update Materialized Views and Functions for Model Result Summary.sql.sql",
            };
            new ProcedureImportor().ImportProcedureFromTextFiles(procedureFiles);

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** BUILD UNITOPTIMIZATIONSOLUTION TABLES ****"); }
            new UnitOptimizationSolutionBuilder(ESAT.Import.Utils.Database.AgBMPToolContextFactory.UoW,
                ESAT.Import.Utils.Database.AgBMPToolContextFactory.USS)
                .BuildUnitOptimizationSolutionBulkInsert(v.GetWatershedId());

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** DONE ****"); }
            else Console.WriteLine("Done!");
        }

        static void AddIFCDB()
        {
            //ExecuteProcedure();

            GdalConfiguration.ConfigureGdal();
            GdalConfiguration.ConfigureOgr();

            VectorImportor v = new VectorImportor();
            ResultImportor r = new ResultImportor();

            string dir = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\IndianFarmCreek\ProjectOnGoing\2019\191114_IFC_ESAT_DB_1\Vectors\";

            Dictionary<string, VectorFile> tableToPath = new Dictionary<string, VectorFile>();
            int projectionCode = 3400;
            string projectionWKT = "PROJCS[\"NAD_1983_Transverse_Mercator\",GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",500000.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",-115.0],PARAMETER[\"Scale_Factor\",0.9992],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0]]";
            string vectorDriver = "ESRI Shapefile";
            tableToPath.Add("Watershed", new VectorFile { Path = $"{dir}Watershed.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Parcel", new VectorFile { Path = $"{dir}Parcel.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("LegalSubDivision", new VectorFile { Path = $"{dir}LegalSubDivision.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Farm", new VectorFile { Path = $"{dir}Farm.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Municipality", new VectorFile { Path = $"{dir}Municipality.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("SubWatershed", new VectorFile { Path = $"{dir}SubWatershed.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Subbasin", new VectorFile { Path = $"{dir}Subbasin.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Reach", new VectorFile { Path = $"{dir}Reach.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("SubArea", new VectorFile { Path = $"{dir}SubArea.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("IsolatedWetland", new VectorFile { Path = $"{dir}IsolatedWetland.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("RiparianWetland", new VectorFile { Path = $"{dir}RiparianWetland.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Lake", new VectorFile { Path = $"{dir}Lake.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("VegetativeFilterStrip", new VectorFile { Path = $"{dir}VegetativeFilterStrip.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("RiparianBuffer", new VectorFile { Path = $"{dir}RiparianBuffer.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("GrassedWaterway", new VectorFile { Path = $"{dir}GrassedWaterway.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("FlowDiversion", new VectorFile { Path = $"{dir}FlowDiversion.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Reservoir", new VectorFile { Path = $"{dir}Reservoir.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("SmallDam", new VectorFile { Path = $"{dir}SmallDam.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Wascob", new VectorFile { Path = $"{dir}Wascob.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Dugout", new VectorFile { Path = $"{dir}Dugout.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("CatchBasin", new VectorFile { Path = $"{dir}CatchBasin.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("Feedlot", new VectorFile { Path = $"{dir}Feedlot.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("ManureStorage", new VectorFile { Path = $"{dir}ManureStorage.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("RockChute", new VectorFile { Path = $"{dir}RockChute.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("PointSource", new VectorFile { Path = $"{dir}PointSource.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });
            tableToPath.Add("ClosedDrain", new VectorFile { Path = $"{dir}ClosedDrain.shp", Driver = vectorDriver, ProjectionCode = projectionCode, ProjectionWKT = projectionWKT });

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT VECTORS ****"); }
            v.ImportAllVectorsOCR(tableToPath);

            dir = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\IndianFarmCreek\ProjectOnGoing\2019\191114_IFC_ESAT_DB_1\Results\";

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT SCENARIO RESULTS ****"); }
            r.ImportScenarioResults(
                $"{dir}ScenarioResultsSubArea.csv",
                $"{dir}ScenarioResultsReach.csv"
                );

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT BMP EFFECTIVENESS RESULTS ****"); }
            r.ImportBmpEffectiveness(new List<string>()
            {
                $"{dir}BmpEffectivenessSubArea.csv",
                $"{dir}BmpEffectivenessReach.csv"
            });

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT USERS & INVESTORS ****"); }
            string testProjectPath = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\191109_Add_user\Add_user_investor.txt";
            new TestProjectImportor(new AgBMPToolUnitOfWork(ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContextSingleton)).ImportTestProjectFromText(testProjectPath, 1, v.GetWatershedId());

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT PROCEDURES ****"); }
            List<string> procedureFiles = new List<string>
            {
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\project_bmp_locations.sql",
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\Summary Result.sql",
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\update agbmptool_getprojectdefaultbmplocations_aggregate.sql",
                @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\doc\2019-11-01\insert_script",
                //@"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\Add Materialized Views and Functions for Model Result Summary.sql",
                //@"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Programing\GitHub\Ecosystem-Services-Assessment-Tool\src\Database Changes\Add Materialized Views and Functions for Start and End Year.sql",
            };
            new ProcedureImportor().ImportProcedureFromTextFiles(procedureFiles);

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** BUILD UNITOPTIMIZATIONSOLUTION TABLES ****"); }
            new UnitOptimizationSolutionBuilder(ESAT.Import.Utils.Database.AgBMPToolContextFactory.UoW,
                ESAT.Import.Utils.Database.AgBMPToolContextFactory.USS)
                .BuildUnitOptimizationSolutionBulkInsert(v.GetWatershedId());

            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** DONE ****"); }
            else Console.WriteLine("Done!");
        }
    }


}
