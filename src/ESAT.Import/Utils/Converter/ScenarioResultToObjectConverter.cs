using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using ESAT.Import.ESATException;

namespace ESAT.Import.Utils
{
    public class ScenarioResultToObjectConverter : EsatDbRetriever, IScenarioResulToObjectConverter
    {


        public ScenarioModelResult BuildScenarioModelResult(int scenarioId, int modelComponentId, int scenarioModelResultTypeId, int year, decimal value)
        {
            return new ScenarioModelResult
            {
                ScenarioId = scenarioId,
                ModelComponentId = modelComponentId,
                ScenarioModelResultTypeId = scenarioModelResultTypeId,
                Year = year,
                Value = value
            };
        }

        public virtual List<object> RecordToObjects(Dictionary<string, object> record)
        {
            // Get watershed Id based on watershed name
            int watershedId = this.GetWatershedId(record["Watershed"].ToString());

            // Get MC type Id based on spatial unit header
            string mcHeader = "";
            if (record.ContainsKey("Reach"))
            {
                mcHeader = "Reach";
            } else if (record.ContainsKey("SubArea"))
            {
                mcHeader = "SubArea";
            }
            else
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                    // following is the message send to console
                    $"No valid model component type column!");
            }
            int mcTypeId = this.GetModelComponentTypeId(mcHeader);

            // Get MC model id
            int mcModelId = Convert.ToInt16(record[mcHeader]);

            // Get MC Id based on watershed Id, model Id, and MC type Id
            int modelComponentId = this.GetModelComponentId(watershedId, mcTypeId, mcModelId);

            // Get scenario type Id based on scenario name
            int scenTypeId = this.GetScenarioTypeId(record["Scenario"].ToString());

            // Get scenario Id based on watershed Id and scenario type Id
            int baseScenarioId = this.GetBaselineScenarioId(watershedId, scenTypeId);

            // Get year
            int year = Convert.ToInt16(record["Year"]);

            // Define output
            List<object> output = new List<object>();

            foreach (string header in record.Keys)
            {
                // Get SMR type Id based on value header
                int smrTypeId = this.GetScenarioModelResultTypeId(header);

                // If SMR type Id is valid and value is not NO_DATA_VALUE
                if (smrTypeId != Program.INVALID_VALUE && Convert.ToDecimal(record[header]) != Program.NO_DATA_VALUE)
                {
                    // Build a SMR
                    output.Add(this.BuildScenarioModelResult(baseScenarioId, modelComponentId, smrTypeId, year, Convert.ToDecimal(record[header])));
                }
            }

            return output;
        }

        public virtual List<object> TableToObjects(ListTable table)
        {
            // Define output as list
            List<object> output = new List<object>();

            //List<int> recIds = new List<int>();

            //for (int i = 0; i < table.Size; i++)
            //{
            //    recIds.Add(i);
            //}

            //Object lockMe = new object();

            //// Foreach record
            //Parallel.ForEach(recIds, i =>
            //{
            //    var r = this.RecordToObjects(table.GetRecord(i));
            //    lock (lockMe)
            //    {
            //        output.AddRange(r);
            //    }
            //});

            for (int i = 0; i < table.Size; i++)
            {
                // Convert record to objects and add to output list
                output.AddRange(this.RecordToObjects(table.GetRecord(i)));
            }

            // Return output
            return output;
        }
    }
}
