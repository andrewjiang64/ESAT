using AgBMPTool.BLL.Services.Projects;
using AgBMPTool.BLL.Services.Users;
using AgBMPTool.BLL.Services.Utilities;
using AgBMPTool.DBModel;
using AgBMPTool.DBModel.Model;
using AgBMTool.DBL;
using AgBMTool.DBL.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace AgBMPTool.BLL.Test.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = ResolveDI();
            //TestSummaryTable(serviceProvider);
            //TestStartEndYearForUserAndScenarioType(serviceProvider);
            //TestStartEndYearForNewProject(serviceProvider);
            //TestExportShapefile(serviceProvider);
            //TestUserExtent(serviceProvider);
            //TestProjectBMPLocations(serviceProvider);
            //TestProjectDefaultBMPLocation(serviceProvider);
            //TestBMPScopeSummaryGridData(serviceProvider);
            //TestOverviewSummaryTable(serviceProvider);
            //TestGeometrySimplify(serviceProvider);
            TestProjectMunicipalities(serviceProvider);
        }

        private static ServiceProvider ResolveDI()
        {
            //connection string
            var connectionString = "Host=localhost;Database=AgBMPTool_20191115;Username=postgres;Password=123456";

            //setup our DI
            return new ServiceCollection()
                .AddDbContext<AgBMPToolContext>(options => options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()), ServiceLifetime.Scoped)
                .AddScoped<IRepository<BaseItem>, AgBMPToolRepository<BaseItem>>()
                .AddScoped<IUnitOfWork, AgBMPToolUnitOfWork>()
                .AddScoped<IUserDataServices, UserDataServices>()
                .AddScoped<IProjectSummaryService, ProjectSummaryServiceUnitOptimizationSolution>()
                .AddScoped<IProjectDataService, ProjectDataService>()
                .AddScoped<IUtilitiesDataService, UtilitiesDataService>()
                .AddScoped<IUtilitiesSummaryService, UtilitiesSummaryService>()
                .AddScoped<IInteligentRecommendationService, IntelligentRecommendationServiceLpSolve>()
                .BuildServiceProvider();
        }

        private static void TestSummaryTable(ServiceProvider serviceProvider)
        {
            //run project data service
            var projectDataService = serviceProvider.GetService<IProjectDataService>();

            System.Diagnostics.Debug.WriteLine("******* Test Summary Table *******");
            var results = projectDataService.GetOverviewSummaryTable(1, 2, -1, -1, -1, 0, 0, 1);

            //output
            var table = results.SummaryTableData;
            var cols = table.Columns;
            for (int j = 0; j < table.Rows.Count; j++)
            {
                System.Diagnostics.Debug.WriteLine(j + "th Result ---------------------");
                for (int i = 0; i < cols.Count; i++)
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} = {1}", cols[i].Caption, table.Rows[j][i]));
            }
        }

        private static void TestStartEndYearForUserAndScenarioType(ServiceProvider serviceProvider)
        {
            //run project data service
            var projectDataService = serviceProvider.GetService<IProjectDataService>();

            //
            System.Diagnostics.Debug.WriteLine("******* Test Start End Year For Overview *******");
            var yearRange = projectDataService.GetStartAndEndYearForOverviewByUserIdAndUserType(1, 1);
            System.Diagnostics.Debug.WriteLine(string.Format("User Id = {0}, Start Year = {1}, End Year = {2}", 1, yearRange.StartYear, yearRange.EndYear));

        }

        private static void TestStartEndYearForNewProject(ServiceProvider serviceProvider)
        {
            //run project data service
            var projectDataService = serviceProvider.GetService<IProjectDataService>();

            System.Diagnostics.Debug.WriteLine("******* Test Start End Year For New Project *******");
            var yearRange = projectDataService.GetStartAndEndYearForAddProjectByUserIdAndUserType(1);
            System.Diagnostics.Debug.WriteLine(string.Format("User Id = {0}, Start Year = {1}, End Year = {2}", 1, yearRange.StartYear, yearRange.EndYear));
        }

        private static void TestExportShapefile(ServiceProvider serviceProvider)
        {
            //run project data service
            var projectDataService = serviceProvider.GetService<IProjectDataService>();

            //export
            projectDataService.ExportToShapefile(1, 1);
        }

        private static void TestUserExtent(ServiceProvider serviceProvider)
        {
            //run project data service
            var userDataService = serviceProvider.GetService<IUserDataServices>();

            //export
            var extent = userDataService.getUserExtent(1);

            //output
            System.Diagnostics.Debug.WriteLine(extent.ToString());
        }

        private static void TestProjectBMPLocations(ServiceProvider serviceProvider)
        {
            //run project data service
            var projectDataService = serviceProvider.GetService<IProjectDataService>();

            //export
            var locations = projectDataService.getProjectDefaultBMPLocations(1);
            foreach(var loc in locations)
            {
                System.Diagnostics.Debug.WriteLine("BMP = {0}, Location Type = {1}, Location Id = {2}",
                    loc.Name, loc.modelComponentTypeId, loc.modelComponentId);
            }
        }

        private static void TestProjectDefaultBMPLocation(ServiceProvider serviceProvider)
        {
            //run project data service
            var uow = serviceProvider.GetService<IUnitOfWork>();

            //export
            uow.ExecuteProcedure("call agbmptool_setprojectdefaultbmplocations(2)");
        }

        private static void TestBMPScopeSummaryGridData(ServiceProvider serviceProvider)
        {
            //run project data service
            var projectDataService = serviceProvider.GetService<IProjectDataService>();

            //get bmp scope summary grid
            projectDataService.GetBMPScopeSummaryGridData(2, 1);
        }

        private static void TestGeometrySimplify(ServiceProvider serviceProvider)
        {
            //run project data service
            var userDataService = serviceProvider.GetService<IUserDataServices>();

            //get bmp scope summary grid
            var municipalities = userDataService.getUserMunicipalities(1, -1,-1,-1);
            var reaches = userDataService.getUserReach(1, -1, -1, -1);
            var parcels = userDataService.getUserParcel(1, -1, -1, -1);
            var lsds = userDataService.getUserLSD(1, -1, -1, -1);
            var watershed = userDataService.getUserWaterShed(1, -1, -1, -1);
            var subwatershed = userDataService.getUserSubWaterShed(1, -1, -1, -1);

            //
            foreach (var m in municipalities)
            {
                System.Diagnostics.Debug.WriteLine(m.coordinates.Count);
            }
        }

        private static void TestProjectMunicipalities(ServiceProvider serviceProvider)
        {
            //run project data service
            var projectDataService = serviceProvider.GetService<IProjectDataService>();

            //
            var municipalities = projectDataService.GetProjectMunicipalitiesByProjectId(1, 1, 2);
            foreach(var m in municipalities)
                System.Diagnostics.Debug.WriteLine("Id = {0}, Name = {1}",
                    m.Id, m.Name);
        }
    }
}
