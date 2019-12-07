﻿using AgBMPTool.BLL.Services.Projects;
using AgBMPTool.BLL.Services.Users;
using AgBMPTool.BLL.Services.Utilities;
using AgBMPTool.DBModel;
using AgBMTool.DBL;
using AgBMTool.DBL.Interface;
using Microsoft.EntityFrameworkCore;

namespace AgBMPTool.BLL.TestDB.Test
{
    public class ServicesTestDB
    {
        //connection string should be read from configuration file
        private static string ConnectionString = "Host=localhost;Port=5433;Database=AgBMPTool_BulkInsert;Username=postgres;Password=123456";

        private static readonly AgBMPToolContext _instance_DbContext = new AgBMPToolContext(
                            new DbContextOptionsBuilder<AgBMPToolContext>()
                            .UseNpgsql(ConnectionString, o => o.UseNetTopologySuite())
                            .Options);

        private static readonly IUnitOfWork _instance_UnitOfWork = new AgBMPToolUnitOfWork(_instance_DbContext);

        public static IUnitOfWork UoW
        {
            get
            {
                return _instance_UnitOfWork;
            }
        }

        private static readonly IUtilitiesSummaryService _instance_UtilitiesSummaryService = new UtilitiesSummaryService(_instance_UnitOfWork);

        public static IUtilitiesSummaryService USS
        {
            get
            {
                return _instance_UtilitiesSummaryService;
            }
        }


        // Singleton IProjectSummaryService
        private static readonly IProjectSummaryService _instance_ProjectSummaryService = 
            new ProjectSummaryServiceUnitOptimizationSolution(_instance_UnitOfWork, _instance_UtilitiesSummaryService);

        public static IProjectSummaryService PSS
        {
            get
            {
                return _instance_ProjectSummaryService;
            }
        }

        // Singleton IntelligentRecommendationServiceLpSolve
        private static readonly IntelligentRecommendationServiceLpSolve _instance_IntelligentService = 
            new IntelligentRecommendationServiceLpSolve(_instance_UnitOfWork, _instance_ProjectSummaryService, _instance_UtilitiesSummaryService);

        public static IntelligentRecommendationServiceLpSolve IRS
        {
            get
            {
                return _instance_IntelligentService;
            }
        }

        // Singleton UserDataServices
        private static readonly IUserDataServices _instance_UserDataServices =
            new UserDataServices(_instance_UnitOfWork);

        public static IUserDataServices UserDataServices
        {
            get
            {
                return _instance_UserDataServices;
            }
        }

        // Singleton AgBMPToolContext
        private static readonly IUtilitiesDataService _instance_UtilitiesDataService =
            new UtilitiesDataService(_instance_UnitOfWork);

        public static IUtilitiesDataService UtilitiesDataService
        {
            get
            {
                return _instance_UtilitiesDataService;
            }
        }
        
        public static IInteligentRecommendationService IInteligentRecommendationService
        {
            get
            {
                return _instance_IntelligentService;
            }
        }

        // Singleton ProjectDataService
        private static readonly IProjectDataService _instance_ProjectDataService =
          new ProjectDataService(_instance_UnitOfWork, _instance_UtilitiesDataService, _instance_ProjectSummaryService, _instance_UserDataServices, _instance_IntelligentService);

        public static IProjectDataService ProjectDataService
        {
            get
            {
                return _instance_ProjectDataService;
            }
        }
    }
}