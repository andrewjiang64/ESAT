using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AgBMPTool.DBModel.Model.Scenario;
using ESAT.Import.ESATException;

namespace ESAT.Import.Utils
{
    public class EffectivenessTableToObjectConverter : ScenarioResultToObjectConverter, IEffectivenessTableToObjectConverter
    {
        public UnitScenarioEffectiveness BuildUnitScenarioEffectiveness(int unitScenarioId, int bmpEffectivenessTypeId, int year, decimal value)
        {
            return new UnitScenarioEffectiveness
            {
                UnitScenarioId = unitScenarioId,
                BMPEffectivenessTypeId = bmpEffectivenessTypeId,
                Year = year,
                Value = value
            };
        }

        public void RecordToUnitScenario(Dictionary<string, object> record)
        {
            // Get watershed Id based on watershed name
            int watershedId = this.GetWatershedId(record["Watershed"].ToString());

            // Get MC type Id based on spatial unit header
            string mcHeader = "";

            // Get MC model id
            int mcModelId = 0;
            if (record.ContainsKey("Reach") && record.ContainsKey("BMPType")) // reach BMP
            {
                mcHeader = record["LocationType"].ToString();
                mcModelId = Convert.ToInt16(record["BMPId"]);
            }
            else if (record.ContainsKey("SubArea")) // Field BMP
            {
                mcHeader = "SubArea";
                mcModelId = Convert.ToInt16(record["SubArea"]);
            }
            else
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                    // following is the message send to console
                    $"No valid model component type column!");
            }
            int mcTypeId = this.GetModelComponentTypeId(mcHeader);

            // Get MC Id based on watershed Id, model Id, and MC type Id
            int modelComponentId = this.GetModelComponentId(watershedId, mcTypeId, mcModelId);

            // Get scenario type Id based on scenario name
            int scenTypeId = this.GetScenarioTypeId(record["Scenario"].ToString());

            // Get scenario Id based on watershed Id and scenario type Id
            int baseScenId = this.GetBaselineScenarioId(watershedId, scenTypeId);
            
            // Get bmp type ids
            List<int> bmpIds = this.GetBmpIdsFromText(record["BMPType"].ToString());

            // Get bmp combo Id
            int comboId = this.GetBmpCombinationId(bmpIds);

            // Get UnitScenarioId
            var unitScenario = this.GetUnitScenario(modelComponentId, baseScenId, comboId, CurrentUSId);

            // Update CurrentUSId
            CurrentUSId = unitScenario.Id;

            // Get year
            int year = Convert.ToInt16(record["Year"]);
            
            foreach (string header in record.Keys)
            {
                // Get SMR type Id based on value header
                int bmpEffectivenessTypeId = this.GetBmpEffectivenessTypeId(header);

                // If SMR type Id is valid and value is not NO_DATA_VALUE
                if (bmpEffectivenessTypeId != Program.INVALID_VALUE && Convert.ToDecimal(record[header]) != Program.NO_DATA_VALUE)
                {
                    if (unitScenario.UnitScenarioEffectivenesses == null)
                    {
                        unitScenario.UnitScenarioEffectivenesses = new List<UnitScenarioEffectiveness>();
                    }
                    // Build a USE
                    unitScenario.UnitScenarioEffectivenesses.Add(this.BuildUnitScenarioEffectiveness(unitScenario.Id, bmpEffectivenessTypeId, year, Convert.ToDecimal(record[header])));
                }
            }
        }

        public List<UnitScenario> TableToUnitScenarios(ListTable table)
        {
            // Define output as list
            //List<object> output = new List<object>();

            if (this.Database.UnitScenarios.Count() > 0)
            {
                CurrentUSId = this.Database.UnitScenarios.Select(o => o.Id).Max();
            } else
            {
                CurrentUSId = 0;
            }

            for (int i = 0; i < table.Size; i++)
            {
                // Convert record to objects and add to output list
                this.RecordToUnitScenario(table.GetRecord(i));
            }

            // Return output
            return this.UnitScenarios.Values.ToList();
        }

        public int CurrentUSId { get; set; }


        private Dictionary<string, UnitScenario> UnitScenarios { get; set; } = new Dictionary<string, UnitScenario>();
        public UnitScenario GetUnitScenario(int modelComponentId, int scenarioId, int comboId, int currUnitScenarioId)
        {
            string type = $"{modelComponentId}_{scenarioId}_{comboId}";
            if (this.UnitScenarios.ContainsKey(type))
            {
                return this.UnitScenarios[type];
            }

            this.UnitScenarios[type] = new UnitScenario
            {
                Id = currUnitScenarioId + 1,
                ModelComponentId = modelComponentId,
                ScenarioId = scenarioId,
                BMPCombinationId = comboId,
                UnitScenarioEffectivenesses = new List<UnitScenarioEffectiveness>()
            };
            return this.UnitScenarios[type];
        }
    }
}
