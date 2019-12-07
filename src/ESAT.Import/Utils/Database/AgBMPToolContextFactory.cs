using AgBMPTool.BLL.Services.Projects;
using AgBMPTool.BLL.Services.Utilities;
using AgBMPTool.DBModel;
using AgBMTool.DBL;
using AgBMTool.DBL.Interface;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Utils.Database
{
    public class AgBMPToolContextFactory
    {
        //connection string should be read from configuration file
        public static string ConnectionString = "Host=localhost;Port=5433;Database=AgBMPTool;Username=postgres;Password=123456;Timeout=150";

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
            new ProjectSummaryServiceDBToMemory(_instance_UnitOfWork, _instance_UtilitiesSummaryService);

        public static IProjectSummaryService PSS
        {
            get
            {
                return _instance_ProjectSummaryService;
            }
        }

        // Singleton
        private static readonly AgBMPToolContext _instance = new AgBMPToolContext(
                            new DbContextOptionsBuilder<AgBMPToolContext>()
                            .UseNpgsql(ConnectionString, o => o.UseNetTopologySuite())
                            .Options);

        public static AgBMPToolContext AgBMPToolContextSingleton
        {
            get
            {
                return _instance;
            }
        }

        public static AgBMPToolContext AgBMPToolContext
        {
            get
            {
                var optionsBuilder = new DbContextOptionsBuilder<AgBMPToolContext>();
                optionsBuilder.UseNpgsql(ConnectionString, o => o.UseNetTopologySuite());

                return new AgBMPToolContext(optionsBuilder.Options);
            }
        }

        public static NpgsqlConnection AgBMPToolNpgsqlConnection
        {
            get
            {
                return new NpgsqlConnection(ConnectionString);
            }
        }
    }
}
