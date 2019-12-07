using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.DBModel
{
    /// <summary>
    /// This is only used for design time
    /// </summary>
    public class AgBMPToolContextFactory : IDesignTimeDbContextFactory<AgBMPToolContext>
    {
        public AgBMPToolContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AgBMPToolContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=AgBMPTool;Username=postgres;Password=LOVE1205", o => o.UseNetTopologySuite());

            return new AgBMPToolContext(optionsBuilder.Options);
        }
    }
}
