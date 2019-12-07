using AgBMPTool.DBModel;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using ESAT.Import.Utils;
using ESAT.Import.Utils.Database;
using ESAT.Import.Utils.TextFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESAT.Import.Model
{
    public class ResultImportor
    {
        public void ImportScenarioResults(string subAreaCsvFile, string reachCsvFile)
        {
            // Import subarea results
            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT SUBAREA RESULTS ****"); }
            this.ScenarioResultToDBBulkInsert(subAreaCsvFile);

            // Import reach results
            if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** IMPORT REACH RESULTS ****"); }
            this.ScenarioResultToDBBulkInsert(reachCsvFile);
        }

        public int RemoveScenarioResults(string watershedName)
        {
            EsatDbRetriever retriever = new EsatDbRetriever();

            // Get watershed Id
            int watershedId = retriever.GetWatershedId(watershedName);

            // Make sql
            string sql = $"DELETE FROM public.\"ScenarioModelResult\" WHERE \"ScenarioId\" IN " +
                $"(SELECT \"Id\" FROM public.\"Scenario\" WHERE \"WatershedId\" = {watershedId});";

            // Execute sql
            return new EsatDbWriter().ExecuteNonQuery(sql);
        }

        private void ScenarioResultsToDb(string csvFile)
        {
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                db.ChangeTracker.AutoDetectChangesEnabled = false;

                int cnt = 0;

                foreach (var item in
                    new ScenarioResultToObjectConverter()
                    .TableToObjects(new TextTableReader().CsvToTable(csvFile)))
                {

                    db.ScenarioModelResults.Add((ScenarioModelResult)item);
                    cnt++;

                    if (cnt > 1e5)
                    {
                        if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** SAVE 100K RESULTS ****"); }
                        db.SaveChanges();
                        cnt = 0;
                    }
                }

                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** SAVE REMAINING RESULTS ****"); }
                // Save to database
                db.SaveChanges();

                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        private readonly int ONE_TIME_INSERT_COUNT = 100000;

        private void ScenarioResultToDBBulkInsert(string csvFile)
        {
            List<ScenarioModelResult> smrs = new ScenarioResultToObjectConverter()
                .TableToObjects(new TextTableReader().CsvToTable(csvFile))
                .ConvertAll(new Converter<object, ScenarioModelResult>(ObjectToScenarioModelResult));

            var smrsHelper = PostgresBulkInsertHelper.CreateHelper<ScenarioModelResult>("public", nameof(ScenarioModelResult));

            if (Program.IS_TESTING) { Console.WriteLine($"*** Start inserting ScenarioResultToDBBulkInsert {smrs.Count} records ***"); }            

            using (var connection = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolNpgsqlConnection)
            {
                connection.Open();                

                if (smrs.Count <= this.ONE_TIME_INSERT_COUNT)
                {
                    smrsHelper.SaveAll(connection, smrs);
                } else
                {
                    int index = 0;
                    int left = smrs.Count;

                    while (left > 0)
                    {
                        int count = Math.Min(ONE_TIME_INSERT_COUNT, left);

                        if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** SAVE {count} RESULTS ****"); }
                        smrsHelper.SaveAll(connection, smrs.GetRange(index, count));

                        index += count;
                        left -= count;
                    }
                }

                connection.Close();
            }

            if (Program.IS_TESTING) { Console.WriteLine($"*** End inserting ScenarioResultToDBBulkInsert ***"); }            
        }

        private ScenarioModelResult ObjectToScenarioModelResult(object pf)
        {
            return (ScenarioModelResult)pf;
        }

        public void ImportBmpEffectiveness(List<string> csvFiles)
        {
            // Import all BMP effectiveness file to database
            foreach (var csvFile in csvFiles)
            {
                //this.BmpEffectivenessToDb(csvFile);
                this.BmpEffectivenessToDbBulkInsert(csvFile);
            }
        }

        private void BmpEffectivenessToDb(string csvFile)
        {
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                db.ChangeTracker.AutoDetectChangesEnabled = false;

                int cnt = 0;
                foreach (var item in
                    new EffectivenessTableToObjectConverter()
                    .TableToUnitScenarios(new TextTableReader().CsvToTable(csvFile)))
                {
                    db.UnitScenarios.Add(item);
                    db.UnitScenarioEffectivenesses.AddRange(item.UnitScenarioEffectivenesses);

                    cnt += item.UnitScenarioEffectivenesses.Count;

                    if (cnt > 1e5)
                    {
                        if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** SAVE 100K RESULTS ****"); }
                        db.SaveChanges();
                        cnt = 0;
                    }
                }

                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** SAVE REMAINING RESULTS ****"); }
                // Save to database
                db.SaveChanges();
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        private void BmpEffectivenessToDbBulkInsert(string csvFile)
        {
            var uss = new EffectivenessTableToObjectConverter()
                    .TableToUnitScenarios(new TextTableReader().CsvToTable(csvFile));

            var uses = uss.SelectMany(o => o.UnitScenarioEffectivenesses).ToList();

            var ussHelper = PostgresBulkInsertHelper.CreateHelper<UnitScenario>("public", nameof(UnitScenario));
            var usesHelper = PostgresBulkInsertHelper.CreateHelper<UnitScenarioEffectiveness>("public", nameof(UnitScenarioEffectiveness));            

            if (Program.IS_TESTING) { Console.WriteLine($"*** Start inserting BmpEffectivenessToDbBulkInsert - uss {uss.Count} records - uses {uses.Count} records ***"); }

            using (var connection = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolNpgsqlConnection)
            {
                connection.Open();

                if (uss.Count <= this.ONE_TIME_INSERT_COUNT)
                {
                    ussHelper.SaveAll(connection, uss);
                }
                else
                {
                    int index = 0;
                    int left = uss.Count;

                    while (left > 0)
                    {
                        int count = Math.Min(ONE_TIME_INSERT_COUNT, left);

                        if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** SAVE {count} RESULTS ****"); }
                        ussHelper.SaveAll(connection, uss.GetRange(index, count));

                        index += count;
                        left -= count;
                    }
                }


                if (uses.Count <= this.ONE_TIME_INSERT_COUNT)
                {
                    usesHelper.SaveAll(connection, uses);
                }
                else
                {
                    int index = 0;
                    int left = uses.Count;

                    while (left > 0)
                    {
                        int count = Math.Min(ONE_TIME_INSERT_COUNT, left);

                        if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** SAVE {count} RESULTS ****"); }
                        usesHelper.SaveAll(connection, uses.GetRange(index, count));

                        index += count;
                        left -= count;
                    }
                }

                connection.Close();
            }

            if (Program.IS_TESTING) { Console.WriteLine($"*** End inserting ScenarioResultToDBBulkInsert ***"); }
        }

        public int RemoveEffectiveness(string watershedName)
        {
            EsatDbRetriever retriever = new EsatDbRetriever();

            // Get watershed Id
            int watershedId = retriever.GetWatershedId(watershedName);

            int output = 0;

            // Make sql
            EsatDbWriter w = new EsatDbWriter();

            output += w.ExecuteNonQuery($"DELETE FROM public.\"UnitScenarioEffectiveness\" WHERE \"UnitScenarioId\" IN  " +
                $"(SELECT \"Id\" FROM public.\"UnitScenario\" WHERE \"ScenarioId\" IN " +
                $"(SELECT \"Id\" FROM public.\"Scenario\" WHERE \"WatershedId\" = {watershedId}));");
            output += w.ExecuteNonQuery($"DELETE FROM public.\"UnitScenario\" WHERE \"ScenarioId\" IN " +
                $"(SELECT \"Id\" FROM public.\"Scenario\" WHERE \"WatershedId\" = {watershedId});");

            // Execute sql
            return output;
        }
    }
}
