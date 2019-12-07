using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AgBMPTool.DBModel;
using AgBMPTool.DBModel.Model;
using AgBMPTool.DBModel.Model.Scenario;
using ESAT.Import.ESATException;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ESAT.Import.Utils
{
    public class EsatDbRetriever : PgDbConnector, IDbRetriever
    {
        public AgBMPToolContext Database
        {
            get
            {
                return ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext;
            }
        }

        public bool ContainsColumn(string schemaName, string tableName, string columnName)
        {
            throw new NotImplementedException();
        }

        public bool ContainsTable(string schemaName, string tableName)
        {
            string sql = $@"SELECT 1 FROM information_schema.tables WHERE table_schema = '{schemaName}' AND table_name = '{tableName}';";
            try
            {
                // Connect to a PostgreSQL database
                NpgsqlConnection conn = GetConnection(ESAT.Import.Utils.Database.AgBMPToolContextFactory.ConnectionString);

                // Define a query returning a single row result set
                NpgsqlCommand command = new NpgsqlCommand(sql, conn);

                // Execute query
                NpgsqlDataReader dRead = command.ExecuteReader();

                while (dRead.Read())
                {
                    return true;
                }

                // Close connection
                conn.Close();
            }
            catch (System.Exception ex)
            {
                throw new MainException(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                    // following is the message send to console
                    $"Executing following query failed!\n\n{sql}");
            }

            return false;
        }

        #region Table and column
        public string GetColumnName(string schemaName, string tableName, int columnId)
        {
            throw new NotImplementedException();
        }

        public List<string> GetColumnNames(string schemaName, string tableName)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetColumnNameTypes(string schemaName, string tableName)
        {
            throw new NotImplementedException();
        }

        public string GetColumnType(string schemaName, string tableName, int columnId)
        {
            throw new NotImplementedException();
        }

        public string GetColumnType(string schemaName, string tableName, string columnName)
        {
            throw new NotImplementedException();
        }

        public List<string> GetTableNames()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Boundary

        #endregion

        #region ModelComponent

        private Dictionary<string, int> ModelComponentTypeId { get; set; } = new Dictionary<string, int>();
        public int GetModelComponentTypeId(string type)
        {
            if (this.ModelComponentTypeId.ContainsKey(type))
            {
                return this.ModelComponentTypeId[type];
            }

            BaseItem output = this.Database.ModelComponentTypes
                                .Where(m => m.Name.ToLower().Equals(type.ToLower()))
                                .FirstOrDefault();

            if (output == null)
            {
                this.ModelComponentTypeId[type] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            } else
            {
                this.ModelComponentTypeId[type] = output.Id;
                return output.Id;
            }
        }

        private Dictionary<string, int> ModelComponentId { get; set; } = new Dictionary<string, int>();
        public int GetModelComponentId(int watershedId, int modelComponentTypeId, int modelId)
        {
            string type = $"{watershedId}_{modelComponentTypeId}_{modelId}";

            if (this.ModelComponentId.ContainsKey(type))
            {
                return this.ModelComponentId[type];
            }

            BaseItem output = this.Database.ModelComponents
                                .Where(m => m.WatershedId == watershedId
                                        && m.ModelComponentTypeId == modelComponentTypeId
                                        && m.ModelId == modelId)
                                .FirstOrDefault();

            if (output == null)
            {
                ModelComponentId[type] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            }
            else
            {
                ModelComponentId[type] = output.Id;
                return output.Id;
            }
        }

        private Dictionary<string, int> WatershedId { get; set; } = new Dictionary<string, int>();
        public int GetWatershedId(string name)
        {
            if (this.WatershedId.ContainsKey(name))
            {
                return this.WatershedId[name];
            }

            BaseItem output = this.Database.Watersheds
                                .Where(m => m.Name.ToLower().Equals(name.ToLower()))
                                .FirstOrDefault();

            if (output == null)
            {
                this.WatershedId[name] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            }
            else
            {
                this.WatershedId[name] = output.Id;
                return output.Id;
            }
        }
        #endregion

        #region Optimization

        #endregion

        #region Project

        #endregion

        #region Scenario

        private Dictionary<string, int> BaselineScenarioId { get; set; } = new Dictionary<string, int>();
        public int GetBaselineScenarioId(int watershedId, int scenarioTypeId)
        {
            string type = $"{watershedId}_{scenarioTypeId}";
            if (this.BaselineScenarioId.ContainsKey(type))
            {
                return this.BaselineScenarioId[type];
            }

            BaseItem output = this.Database.Scenarios
                                .Where(m => m.WatershedId == watershedId
                                        && m.ScenarioTypeId == scenarioTypeId)
                                .FirstOrDefault();

            if (output == null)
            {
                BaselineScenarioId[type] = new EsatDbWriter().AddBaselineScenario(watershedId, scenarioTypeId);
                return BaselineScenarioId[type];
            }
            else
            {
                this.BaselineScenarioId[type] = output.Id;
                return output.Id;
            }
        }

        private Dictionary<string, int> ScenarioTypeId { get; set; } = new Dictionary<string, int>();
        public int GetScenarioTypeId(string type)
        {
            if (this.ScenarioTypeId.ContainsKey(type))
            {
                return this.ScenarioTypeId[type];
            }

            BaseItem output = this.Database.ScenarioTypes
                                 .Where(m => m.Name.ToLower().Equals(type.ToLower()))
                                 .FirstOrDefault();

            if (output == null)
            {
                this.ScenarioTypeId[type] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            }
            else
            {
                this.ScenarioTypeId[type] = output.Id;
                return output.Id;
            }
        }

        private Dictionary<string, int> UnitScenarioId { get; set; } = new Dictionary<string, int>();
        public int GetUnitScenarioId(int modelComponentId, int scenarioId, int comboId)
        {
            string type = $"{modelComponentId}_{scenarioId}_{comboId}";
            if (this.UnitScenarioId.ContainsKey(type))
            {
                return this.UnitScenarioId[type];
            }

            BaseItem output = this.Database.UnitScenarios
                                .Where(m => m.ModelComponentId == modelComponentId
                                        && m.ScenarioId == scenarioId
                                        && m.BMPCombinationId == comboId)
                                .FirstOrDefault();

            if (output == null)
            {
                this.UnitScenarioId[type] = new EsatDbWriter().AddUnitScenario(modelComponentId, scenarioId, comboId);
                return this.UnitScenarioId[type];
            }
            else
            {
                this.UnitScenarioId[type] = output.Id;
                return output.Id;
            }
        }
        
        private Dictionary<string, int> BmpEffectivenessTypeId { get; set; } = new Dictionary<string, int>();
        public int GetBmpEffectivenessTypeId(string type)
        {
            if (this.BmpEffectivenessTypeId.ContainsKey(type))
            {
                return this.BmpEffectivenessTypeId[type];
            }

            BaseItem output = this.Database.BMPEffectivenessTypes
                                .Where(m => m.Name.ToLower().Equals(type.ToLower()))
                                .FirstOrDefault();

            if (output == null)
            {
                this.BmpEffectivenessTypeId[type] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            }
            else
            {
                this.BmpEffectivenessTypeId[type] = output.Id;
                return output.Id;
            }
        }

        private Dictionary<string, int> BMPTypeId { get; set; } = new Dictionary<string, int>();
        public int GetBMPTypeId(string type)
        {
            if (this.BMPTypeId.ContainsKey(type))
            {
                return this.BMPTypeId[type];
            }

            BaseItem output = this.Database.BMPTypes
                                .Where(m => m.Name.ToLower().Equals(type.ToLower()))
                                .FirstOrDefault();

            if (output == null)
            {
                this.BMPTypeId[type] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            }
            else
            {
                this.BMPTypeId[type] = output.Id;
                return output.Id;
            }
        }

        public List<int> GetBmpIdsFromText(string v)
        {
            string[] bmps = v.Split("_");

            List<int> bmpIds = new List<int>();

            foreach (string bmp in bmps)
            {
                int id = this.GetBMPTypeId(bmp);

                if (id != Program.INVALID_VALUE)
                {
                    bmpIds.Add(id);
                }
                else
                {
                    throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                    // following is the message send to console
                        $"BMPType {bmp} not found!");
                }
            }

            return bmpIds;
        }

        private Dictionary<string, int> BMPTypeIdsToBmpCombinationId { get; set; } = new Dictionary<string, int>();

        public int GetBmpCombinationId(List<int> bmpIds)
        {
            // If only one BMP type, combo Id is the same
            if (bmpIds.Count == 1)
            {
                return bmpIds[0];
            }

            string idString = "";

            foreach (int id in bmpIds)
            {
                idString += $"{id},";
            }

            idString = idString.Substring(0, idString.Length - 1);

            if (BMPTypeIdsToBmpCombinationId.ContainsKey(idString))
            {
                return BMPTypeIdsToBmpCombinationId[idString];
            }

            string sql = $"SELECT \"BMPCombinationTypeId\" FROM \"BMPCombinationBMPTypes\" WHERE \"BMPTypeId\" IN ({idString}) GROUP BY \"BMPCombinationTypeId\" HAVING count(*) = {bmpIds.Count} -- single ID list and count of the list\n" +
                            $"INTERSECT\n" +
                            $"SELECT \"BMPCombinationTypeId\" FROM public.\"BMPCombinationBMPTypes\" GROUP BY \"BMPCombinationTypeId\" HAVING count(*) = {bmpIds.Count}; -- count of list\n";
            try
            {
                // Connect to a PostgreSQL database
                NpgsqlConnection conn = GetConnection(ESAT.Import.Utils.Database.AgBMPToolContextFactory.ConnectionString);

                // Define a query returning a single row result set
                NpgsqlCommand command = new NpgsqlCommand(sql, conn);

                // Execute query
                NpgsqlDataReader dRead = command.ExecuteReader();

                // Get combo id
                int output = Program.INVALID_VALUE;

                while (dRead.Read())
                {
                    output = Convert.ToInt16(dRead[0]);
                    break;
                }

                // Close connection
                conn.Close();

                // return combo id if get one
                if (output != Program.INVALID_VALUE) {
                    BMPTypeIdsToBmpCombinationId[idString] = output;
                    return output;
                }
            }
            catch (System.Exception ex)
            {
                throw new MainException(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                    // following is the message send to console
                    $"Executing following query failed!\n\n{sql}");
            }

            // If not found combo id, input one and return the newly added Id
            BMPTypeIdsToBmpCombinationId[idString] = new EsatDbWriter().AddBMPCombination(bmpIds);
            return BMPTypeIdsToBmpCombinationId[idString];
        }

        #endregion

        #region ScenarioModelResult

        private Dictionary<string, int> ScenarioModelResultVariableTypeId { get; set; } = new Dictionary<string, int>();
        public int GetScenarioModelResultVariableTypeId(string type)
        {
            if (this.ScenarioModelResultVariableTypeId.ContainsKey(type))
            {
                return this.ScenarioModelResultVariableTypeId[type];
            }

            BaseItem output = this.Database.ScenarioModelResultVariableTypes
                                .Where(m => m.Name.ToLower().Equals(type.ToLower()))
                                .FirstOrDefault();

            if (output == null)
            {
                this.ScenarioModelResultVariableTypeId[type] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            }
            else
            {
                this.ScenarioModelResultVariableTypeId[type] = output.Id;
                return output.Id;
            }
        }


        public int GetScenarioModelResultTypeId(int unitTypeId, int modelComponentTypeId, int scenarioModelResultVariableTypeId)
        {
            string type = $"{unitTypeId}_{modelComponentTypeId}_{scenarioModelResultVariableTypeId}";
            if (this.ScenarioModelResultTypeId.ContainsKey(type))
            {
                return this.ScenarioModelResultTypeId[type];
            }

            BaseItem output = this.Database.ScenarioModelResultTypes
                                .Where(m => m.ModelComponentTypeId == modelComponentTypeId
                                        && m.UnitTypeId == unitTypeId
                                        && m.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .FirstOrDefault();

            if (output == null)
            {
                this.ScenarioModelResultTypeId[type] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            }
            else
            {
                this.ScenarioModelResultTypeId[type] = output.Id;
                return output.Id;
            }
        }

        private Dictionary<string, int> ScenarioModelResultTypeId { get; set; } = new Dictionary<string, int>();
        public int GetScenarioModelResultTypeId(string type)
        {
            if (this.ScenarioModelResultTypeId.ContainsKey(type))
            {
                return this.ScenarioModelResultTypeId[type];
            }

            BaseItem output = this.Database.ScenarioModelResultTypes
                                .Where(m => m.Name.ToLower().Equals(type.ToLower()))
                                .FirstOrDefault();

            if (output == null)
            {
                this.ScenarioModelResultTypeId[type] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            }
            else
            {
                this.ScenarioModelResultTypeId[type] = output.Id;
                return output.Id;
            }
        }

        #endregion

        #region Solution

        #endregion

        #region Type

        public Dictionary<string, int> UnitTypeId { get; set; } = new Dictionary<string, int>();
        public int GetUnitTypeId(string type)
        {
            if (this.UnitTypeId.ContainsKey(type))
            {
                return this.UnitTypeId[type];
            }

            BaseItem output = this.Database.UnitTypes
                                .Where(m => m.Name.ToLower().Equals(type.ToLower()))
                                .FirstOrDefault();

            if (output == null)
            {
                this.UnitTypeId[type] = Program.INVALID_VALUE;
                return Program.INVALID_VALUE;
            }
            else
            {
                this.UnitTypeId[type] = output.Id;
                return output.Id;
            }
        }
        #endregion











    }
}
