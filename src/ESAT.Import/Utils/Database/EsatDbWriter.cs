using AgBMPTool.DBModel;
using AgBMPTool.DBModel.Model;
using AgBMPTool.DBModel.Model.Scenario;
using ESAT.Import.ESATException;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ESAT.Import.Utils
{
    public class EsatDbWriter : PgDbConnector
    {
        public AgBMPToolContext Database { get => ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext; }

        public int ExecuteNonQuery(StringBuilder sb)
        {
            return ExecuteNonQuery(sb.ToString());
        }

        public int ExecuteNonQuery(string sql)
        {
            int result = -1;
            try
            {
                // Connect to a PostgreSQL database
                NpgsqlConnection conn = GetConnection(ESAT.Import.Utils.Database.AgBMPToolContextFactory.ConnectionString);

                // Define a query returning a single row result set
                NpgsqlCommand command = new NpgsqlCommand(sql, conn);

                // Execute query
                result = command.ExecuteNonQuery();

                // Close connection
                conn.Close();
            }
            catch (System.Exception ex)
            {
                throw new MainException(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                    // following is the message send to console
                    $"Executing following query failed!\n\n{sql}");
            }
            return result;
        }

        public int AddBaselineScenario(int watershedId, int scenarioTypeId)
        {
            string name = $"Baseline_{Database.ScenarioTypes.Find(scenarioTypeId).Name}_{Database.Watersheds.Find(watershedId).Name}";

            BaseItem baseline = this.Database.Scenarios
                                .Where(m => m.Name.ToLower().Equals(name.ToLower()))
                                .FirstOrDefault();
            // If baseline already exists, return its id
            if (baseline != null)
            {
                return baseline.Id;
            }

            string description = $"Baseline {Database.ScenarioTypes.Find(scenarioTypeId).Name} scenario for {Database.Watersheds.Find(watershedId).Name}.";

            // Create new scneario
            Scenario scen = new Scenario
            {
                WatershedId = watershedId,
                ScenarioTypeId = scenarioTypeId,
                Name = name,
                Description = description,
            };

            return this.AddScenario(scen);
        }

        public int AddUserScenario(int watershedId, int scenarioTypeId, string name, string description)
        {
            // Create new UnitScenario
            Scenario scen = new Scenario
            {
                WatershedId = watershedId,
                ScenarioTypeId = scenarioTypeId,
                Name = name,
                Description = description
            };

            return this.AddScenario(scen);
        }

        private int AddScenario(Scenario scen)
        {
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                // Add a unit scenario to database
                db.Scenarios.Add(scen);

                // Save changes to database
                db.SaveChanges();

                // return newly added id
                return scen.Id; 
            }
        }

        public int AddBMPCombination(List<int> bmpIds)
        {
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                // Get current max combo id
                int maxCombinationId = db.BMPCombinationTypes.Max(p => p.Id);

                // Add 1
                maxCombinationId++;

                // Get name
                string name = "";
                foreach (int id in bmpIds)
                {
                    name += $"{db.BMPCombinationTypes.Find(id).Name}_";
                }
                name = name.Substring(0, name.Length - 1);

                // Define a BMPCombo
                BMPCombinationType bc = new BMPCombinationType
                {
                    Id = maxCombinationId,
                    SortOrder = maxCombinationId,
                    Name = name,
                    Description = name,
                    ModelComponentTypeId = 1
                };

                // Add new combo to database
                db.Add(bc);

                // Save changes to database
                db.SaveChanges();

                // Get max BmpComboBmpTypesId
                int maxId = db.BMPCombinationBMPTypes.Max(p => p.Id);

                // Create BMPCombo list
                List<BMPCombinationBMPTypes> list = new List<BMPCombinationBMPTypes>();

                foreach (int id in bmpIds)
                {
                    maxId++;
                    list.Add(new BMPCombinationBMPTypes() { Id = maxId, BMPTypeId = id, BMPCombinationTypeId = maxCombinationId });
                }

                // Add list to BMPCombo table
                db.BMPCombinationBMPTypes.AddRange(list);

                // Save changes to database
                db.SaveChanges();

                // Return newly added BMP comboId
                return maxCombinationId; 
            }
        }

        public int AddUnitScenario(int modelComponentId, int scenarioId, int combinationId)
        {
            // Check if database already contain this UnitScenario
            BaseItem output = this.Database.UnitScenarios
                                .Where(m => m.ModelComponentId == modelComponentId
                                        && m.ScenarioId == scenarioId
                                        && m.BMPCombinationId == combinationId)
                                .FirstOrDefault();
            if (output != null)
            {
                return output.Id;
            }

            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                // Create new UnitScenario
                UnitScenario us = new UnitScenario
                {
                    ModelComponentId = modelComponentId,
                    ScenarioId = scenarioId,
                    BMPCombinationId = combinationId
                };

                // Add a unit scenario to database
                db.UnitScenarios.Add(us);

                // Save changes to database
                db.SaveChanges();

                // return newly added id
                return us.Id; 
            }
        }
    }
}
