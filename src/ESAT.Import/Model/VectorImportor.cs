using AgBMPTool.DBModel;
using AgBMPTool.DBModel.Model.Boundary;
using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Optimization;
using AgBMPTool.DBModel.Model.Solution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AgBMPTool.DBModel.Model;
using ESAT.Import.Utils;
using ESAT.Import.Utils.Converter;
using ESAT.Import.VectorObjectConvertor.Model;
using NetTopologySuite.Geometries;
using VectorFeatureToObjectConverter;
using AgBMPTool.BLL.DLLException;
using System.Reflection;

namespace ESAT.Import.Model
{

    public class VectorImportor
    {

        /** Doc
         * This method is used to import all shapefiles to the ESAT DB.
         * 
         * The sequence of importing the shapefiles is as follows
         * 
         * ---- Tier 1 ----
         * Watershed, Parcel, Municipality, LegalSubDivision
         * 
         * ---- Tier 2 ----
         * SubWatershed
         * 
         * ---- Tier 3 ----
         * Reach,SubArea
         * 
         * ---- Tier 4 ----
         * IsolatedWetland, RiparianWetland, Lake, VegetativeFilterStrip, RiparianBuffer, 
         * GrassedWaterway, FlowDiversion, Reservoir, SmallDam, Wascob, Dugout, CatchBasin,
         * Feedlot, ManureStorage, RockChute, PointSource, ClosedDrain
         */
        public void ImportAllVectorsOCR(Dictionary<string, VectorFile> shapeTableNameToPath)
        {
            ImportedFeatures = new Dictionary<string, Dictionary<int, object>>();

            // Update current MC id
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                CurrentMcId = this.GetCurrentId(db, new ModelComponent().GetType());
            }

            // --- Tier 1 ---
            List<Type> tier1 = new List<Type> { new Watershed().GetType(), new Parcel().GetType(), new LegalSubDivision().GetType(), new Farm().GetType(), new Municipality().GetType() };

            foreach (var type in tier1)
            {
                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> Import to Watershed table from {shapeTableNameToPath[type.Name].Path}"); }
                ImportedFeatures[type.Name] = shapeTableNameToPath.ContainsKey(type.Name) ? ImportOneVectorOCR(type, shapeTableNameToPath[type.Name]) : null;
                if (Program.IS_TESTING) { if (ImportedFeatures[type.Name] != null) Console.WriteLine($"{Program.PROGRAM_NAME}>> {ImportedFeatures[type.Name].Count} records imported to {type.Name} table."); }
            }

            // --- Tier 2 ---
            List<Type> tier2 = new List<Type> { new SubWatershed().GetType(), new Subbasin().GetType() };

            foreach (var type in tier2)
            {
                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> Import to Watershed table from {shapeTableNameToPath[type.Name].Path}"); }
                ImportedFeatures[type.Name] = shapeTableNameToPath.ContainsKey(type.Name) ? ImportOneVectorOCR(type, shapeTableNameToPath[type.Name]) : null;
                if (Program.IS_TESTING) { if (ImportedFeatures[type.Name] != null) Console.WriteLine($"{Program.PROGRAM_NAME}>> {ImportedFeatures[type.Name].Count} records imported to {type.Name} table."); }
            }

            // --- Tier 3 ---
            List<Type> tier3 = new List<Type> { new Reach().GetType(), new SubArea().GetType() };

            foreach (var type in tier3)
            {
                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> Import to Watershed table from {shapeTableNameToPath[type.Name].Path}"); }
                ImportedFeatures[type.Name] = shapeTableNameToPath.ContainsKey(type.Name) ? ImportOneVectorOCR(type, shapeTableNameToPath[type.Name]) : null;
                if (Program.IS_TESTING) { if (ImportedFeatures[type.Name] != null) Console.WriteLine($"{Program.PROGRAM_NAME}>> {ImportedFeatures[type.Name].Count} records imported to {type.Name} table."); }
            }

            // --- Tier 4 ---
            List<Type> tier4 = new List<Type> { new IsolatedWetland().GetType(),new RiparianWetland().GetType(),new Lake().GetType(),new VegetativeFilterStrip().GetType(),
                    new RiparianBuffer().GetType(),new GrassedWaterway().GetType(),new FlowDiversion().GetType(),new Reservoir().GetType(),new SmallDam().GetType(),new Wascob().GetType(),new Dugout().GetType(),
                    new CatchBasin().GetType(),new Feedlot().GetType(),new ManureStorage().GetType(),new RockChute().GetType(),new PointSource().GetType(),new ClosedDrain().GetType()};

            foreach (var type in tier4)
            {
                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> Import to Watershed table from {shapeTableNameToPath[type.Name].Path}"); }
                ImportedFeatures[type.Name] = shapeTableNameToPath.ContainsKey(type.Name) ? ImportOneVectorOCR(type, shapeTableNameToPath[type.Name]) : null;
                if (Program.IS_TESTING) { if (ImportedFeatures[type.Name] != null) Console.WriteLine($"{Program.PROGRAM_NAME}>> {ImportedFeatures[type.Name].Count} records imported to {type.Name} table."); }
            }

            // Update watershed outlet reach Id
            this.UpdateWatershedOutletReachId();
        }

        /** Doc
         * This method is used to import all shapefiles to the ESAT DB.
         * 
         * The sequence of importing the shapefiles is as follows
         * 
         * ---- Tier 1 ----
         * Watershed, Parcel, Municipality, LegalSubDivision
         * 
         * ---- Tier 2 ----
         * SubWatershed
         * 
         * ---- Tier 3 ----
         * Reach,SubArea
         * 
         * ---- Tier 4 ----
         * IsolatedWetland, RiparianWetland, Lake, VegetativeFilterStrip, RiparianBuffer, 
         * GrassedWaterway, FlowDiversion, Reservoir, SmallDam, Wascob, Dugout, CatchBasin,
         * Feedlot, ManureStorage, RockChute, PointSource, ClosedDrain
         */
        public void ImportAllVectorsNTS(Dictionary<string, VectorFile> shapeTableNameToPath)
        {
            ImportedFeatures = new Dictionary<string, Dictionary<int, object>>();

            // --- Tier 1 ---
            List<Type> tier1 = new List<Type> { new Watershed().GetType(), new Parcel().GetType(), new LegalSubDivision().GetType(), new Farm().GetType(), new Municipality().GetType() };

            foreach (var type in tier1)
            {
                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> Import to Watershed table from {shapeTableNameToPath[type.Name].Path}"); }
                ImportedFeatures[type.Name] = shapeTableNameToPath.ContainsKey(type.Name) ? ImportOneVectorNTS(type, shapeTableNameToPath[type.Name]) : null;
                if (Program.IS_TESTING) { if (ImportedFeatures[type.Name] != null) Console.WriteLine($"{Program.PROGRAM_NAME}>> {ImportedFeatures[type.Name].Count} records imported to {type.Name} table."); }
            }

            // --- Tier 2 ---
            List<Type> tier2 = new List<Type> { new SubWatershed().GetType(), new Subbasin().GetType() };

            foreach (var type in tier2)
            {
                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> Import to Watershed table from {shapeTableNameToPath[type.Name].Path}"); }
                ImportedFeatures[type.Name] = shapeTableNameToPath.ContainsKey(type.Name) ? ImportOneVectorNTS(type, shapeTableNameToPath[type.Name]) : null;
                if (Program.IS_TESTING) { if (ImportedFeatures[type.Name] != null) Console.WriteLine($"{Program.PROGRAM_NAME}>> {ImportedFeatures[type.Name].Count} records imported to {type.Name} table."); }
            }

            // --- Tier 3 ---
            List<Type> tier3 = new List<Type> { new Reach().GetType(), new SubArea().GetType() };

            foreach (var type in tier3)
            {
                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> Import to Watershed table from {shapeTableNameToPath[type.Name].Path}"); }
                ImportedFeatures[type.Name] = shapeTableNameToPath.ContainsKey(type.Name) ? ImportOneVectorNTS(type, shapeTableNameToPath[type.Name]) : null;
                if (Program.IS_TESTING) { if (ImportedFeatures[type.Name] != null) Console.WriteLine($"{Program.PROGRAM_NAME}>> {ImportedFeatures[type.Name].Count} records imported to {type.Name} table."); }
            }

            // --- Tier 4 ---
            List<Type> tier4 = new List<Type> { new IsolatedWetland().GetType(),new RiparianWetland().GetType(),new Lake().GetType(),new VegetativeFilterStrip().GetType(),
                    new RiparianBuffer().GetType(),new GrassedWaterway().GetType(),new FlowDiversion().GetType(),new Reservoir().GetType(),new SmallDam().GetType(),new Wascob().GetType(),new Dugout().GetType(),
                    new CatchBasin().GetType(),new Feedlot().GetType(),new ManureStorage().GetType(),new RockChute().GetType(),new PointSource().GetType(),new ClosedDrain().GetType()};

            foreach (var type in tier4)
            {
                if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> Import to Watershed table from {shapeTableNameToPath[type.Name].Path}"); }
                ImportedFeatures[type.Name] = shapeTableNameToPath.ContainsKey(type.Name) ? ImportOneVectorNTS(type, shapeTableNameToPath[type.Name]) : null;
                if (Program.IS_TESTING) { if (ImportedFeatures[type.Name] != null) Console.WriteLine($"{Program.PROGRAM_NAME}>> {ImportedFeatures[type.Name].Count} records imported to {type.Name} table."); }
            }

            // Update watershed outlet reach Id
            this.UpdateWatershedOutletReachId();
        }

        public int CurrentMcId { get; set; }

        /** Doc
         * This method import one vector into ESAT table and returns all features in the dictionary
         */
        private Dictionary<int, object> ImportOneVectorOCR(Type type, VectorFile vectorFile)
        {
            // Convert shapefile features to objects
            Dictionary<int, Dictionary<string, string>> feaToObj = new VectorFeatureToObjectConverterOCR().ToOCRObjects(vectorFile);

            // if no object returned
            if (feaToObj == null) return null;

            // Create a dictionary to save all imported objects
            Dictionary<int, object> output = new Dictionary<int, object>();

            // Create a stringbuilder for updating geometry
            StringBuilder sb = new StringBuilder();

            // Flag if this is a model component object
            bool isMC = this.IsValidModelComponentType(type);

            // Loop through all feature objects
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                db.ChangeTracker.AutoDetectChangesEnabled = false;

                // Get currentId
                int currentId = this.GetCurrentId(db, type);

                foreach (var f in feaToObj)
                {
                    ModelComponent mc = null;

                    // If this object is MC
                    if (isMC)
                    {
                        CurrentMcId++;
                        mc = AddModelComponent(db, type, CurrentMcId);
                    }

                    // Create a shape object and save all attributes to this object. 
                    /** NOTE
                     * Message: 
                     * I set geometry to null because I can't convert OGR geometry to NetTopologySuite geometry.
                     * The geometry is updated using WKT text through query afterwards.
                     * 
                     * By: Shawn
                     * Date: 2019-09-12
                     */
                    BaseItem outObj = this.GetDBObject(type, f.Value, CurrentMcId, null);
                    currentId++;
                    outObj.Id = currentId;

                    // Save object to database
                    db.Add(outObj);
                    //if (!isMC) db.SaveChanges();

                    // update ModelId in ModelComponent
                    if (isMC)
                    {
                        mc.ModelId = f.Key;
                        mc.Name = $"{mc.Name}({mc.ModelId})";
                        mc.Description = mc.Name;
                        //db.SaveChanges();
                    }

                    // Add to output dictionary
                    output.Add(Convert.ToInt16(f.Key), outObj);

                    // Add update statement to string builder
                    sb.AppendLine(this.GetUpdateGeometrySQL(outObj, f.Value[VectorFeatureToObjectConverterOCR.GEOMETRY_KEY]));
                }

                // Save to db
                db.SaveChanges();

                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            // Update all geometries in database
            this.UpdateGeometry(sb);

            return output;
        }

        public Dictionary<string, int> CurrentId { get; set; } = new Dictionary<string, int>();

        private int GetCurrentId(AgBMPToolContext db, Type type)
        {
            if (CurrentId.ContainsKey(type.Name))
            {
                return CurrentId[type.Name];
            }

            try
            {
                if (this.IsValidModelComponentType(type))
                {
                    int maxId =
                            (from mc in db.ModelComponents.AsQueryable()
                             join mct in db.ModelComponentTypes.AsQueryable() on mc.ModelComponentTypeId equals mct.Id
                             where mct.Name == type.Name
                             select mc.ModelId
                             ).Max();
                    CurrentId[type.Name] = maxId;
                    return maxId;
                }
                else if (type == typeof(ModelComponent))
                {
                    int maxId = db.ModelComponents.Select(o => o.Id).Max();
                    CurrentId[type.Name] = maxId;
                    return maxId;
                }
                else if (type == typeof(Watershed))
                {
                    int maxId = db.Watersheds.Select(o => o.Id).Max();
                    CurrentId[type.Name] = maxId;
                    return maxId;
                }
                else if (type == typeof(Farm))
                {
                    int maxId = db.Farms.Select(o => o.Id).Max();
                    CurrentId[type.Name] = maxId;
                    return maxId;
                }
                else if (type == typeof(Parcel))
                {
                    int maxId = db.Parcels.Select(o => o.Id).Max();
                    CurrentId[type.Name] = maxId;
                    return maxId;
                }
                else if (type == typeof(LegalSubDivision))
                {
                    int maxId = db.LegalSubDivisions.Select(o => o.Id).Max();
                    CurrentId[type.Name] = maxId;
                    return maxId;
                }
                else if (type == typeof(Municipality))
                {
                    int maxId = db.Municipalities.Select(o => o.Id).Max();
                    CurrentId[type.Name] = maxId;
                    return maxId;
                }
                else if (type == typeof(SubWatershed))
                {
                    int maxId = db.SubWatersheds.Select(o => o.Id).Max();
                    CurrentId[type.Name] = maxId;
                    return maxId;
                }
                else if (type == typeof(Subbasin))
                {
                    int maxId = db.Subbasins.Select(o => o.Id).Max();
                    CurrentId[type.Name] = maxId;
                    return maxId;
                }
            }
            catch (Exception)
            {
                CurrentId[type.Name] = 0;
                return 0;
            }

            throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                            // following is the message send to console
                            $"Object type error!");
        }

        internal int GetWatershedId()
        {
            return ImportedFeatures == null ? 1 : ((Watershed)ImportedFeatures["Watershed"].Values.First()).Id;
        }

        private Dictionary<int, object> ImportOneVectorNTS(Type type, VectorFile vectorFile)
        {
            // Convert shapefile features to objects
            Dictionary<int, VectorObject> feaToObj = new VectorFeatureToObjectConverterNTS().ToNTSObjects(vectorFile);

            // if no object returned
            if (feaToObj == null) return null;

            // Create a dictionary to save all imported objects
            Dictionary<int, object> output = new Dictionary<int, object>();

            // Create a stringbuilder for updating geometry
            StringBuilder sb = new StringBuilder();

            // Flag if this is a model component object
            bool isMC = this.IsValidModelComponentType(type);

            // Loop through all feature objects
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                db.ChangeTracker.AutoDetectChangesEnabled = false;

                // Get currentId
                int currentId = isMC ? this.GetCurrentId(db, type) : 0;

                foreach (var f in feaToObj)
                {
                    // define MCId
                    int mcId = Program.INVALID_VALUE;

                    // If this object is MC
                    if (isMC)
                    {
                        CurrentMcId++;
                        mcId = CurrentMcId;
                        AddModelComponent(db, type, CurrentMcId);
                    }

                    // Create a shape object and save all attributes to this object. 
                    /** NOTE
                     * Message: 
                     * I set geometry to null because I can't convert OGR geometry to NetTopologySuite geometry.
                     * The geometry is updated using WKT text through query afterwards.
                     * 
                     * By: Shawn
                     * Date: 2019-09-12
                     */
                    Geometry geom = f.Value.Geometry;
                    GeometryFactory factory = new GeometryFactory();

                    if (geom.GeometryType.Equals("Polygon"))
                    {
                        geom = (MultiPolygon)factory.CreateMultiPolygon(new Polygon[] { (Polygon)geom });
                    }
                    else if (geom.GeometryType.Equals("Point"))
                    {
                        geom = (MultiPoint)factory.CreateMultiPoint(new Point[] { (Point)geom });
                    }
                    else if (geom.GeometryType.Equals("LineString"))
                    {
                        geom = (MultiLineString)factory.CreateMultiLineString(new LineString[] { (LineString)geom });
                    }

                    object outObj = this.GetDBObject(type, f.Value.Attributes, mcId, geom);

                    // Save object to database
                    db.Add(outObj);
                    db.SaveChanges();

                    // update ModelId in ModelComponent
                    if (isMC)
                    {
                        ModelComponent mc = db.ModelComponents.Find(mcId);
                        mc.ModelId = ((BaseItem)outObj).Id;
                        mc.Name = $"{mc.Name}({mc.ModelId})";
                        mc.Description = mc.Name;
                        db.SaveChanges();
                    }

                    // Add to output dictionary
                    output.Add(Convert.ToInt16(f.Key), outObj);

                    db.ChangeTracker.AutoDetectChangesEnabled = true;
                }
            }

            return output;
        }

        private ModelComponent AddModelComponent(AgBMPToolContext db, Type type, int currentMcId)
        {
            // Get model component type id based on object
            int mctId = this.GetModelComponentTypeId(type);

            // Check if object is a valid model component type
            if (mctId != Program.INVALID_VALUE)
            {
                // Get current watershed id
                Watershed w = (Watershed)ImportedFeatures["Watershed"].Values.First();

                ModelComponent mc = new ModelComponent
                {
                    Id = currentMcId,
                    Name = type.Name,
                    ModelComponentTypeId = mctId,
                    WatershedId = w.Id,
                };

                db.ModelComponents.Add(mc);

                return mc;

                //using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
                //{

                //    //db.SaveChanges();

                //    //return mc.Id;
                //}
            }

            return null;
            //return Program.INVALID_VALUE;
        }

        // Update geometry based on WKT extracted from shapefile feature
        public int UpdateGeometry(StringBuilder sb)
        {
            return new EsatDbWriter().ExecuteNonQuery(sb);
        }

        // Returns model component type id based on object type
        public int GetModelComponentTypeId(Type type)
        {
            try
            {
                using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
                {
                    int id = db.ModelComponentTypes
                                .Where(m => m.Name == type.Name)
                                .FirstOrDefault().Id;
                    return id;
                }
            }
            catch (System.Exception)
            {
                return Program.INVALID_VALUE;
            }
        }

        // Returns true if object type has model component type in ESAT
        private bool IsValidModelComponentType(Type type)
        {
            return this.GetModelComponentTypeId(type) != Program.INVALID_VALUE;
        }

        public BaseItem GetDBObject(Type type, Dictionary<string, string> feature, int mcId, Geometry geom)
        {
            BaseItem outObj = new BaseItem();

            // creating objecte
            if (type == typeof(Watershed))
            {
                outObj = new Watershed
                {
                    Alias = feature["alias"],
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Description = feature["descriptio"],
                    Geometry = (MultiPolygon)geom,
                    Modified = new DateTimeOffset(DateTime.Now),
                    Name = feature["name"],
                    OutletReachId = Convert.ToInt16(feature["outletreac"])
                };
            }
            else if (type == typeof(Farm))
            {
                outObj = new Farm
                {
                    Geometry = (MultiPolygon)geom,
                    Name = feature["name"],
                    OwnerId = Convert.ToInt16(feature["ownerid"])
                };
            }
            else if (type == typeof(Parcel))
            {
                outObj = new Parcel
                {
                    FullDescription = feature["fulldescri"],
                    OwnerId = Convert.ToInt16(feature["ownerid"]),
                    Geometry = (MultiPolygon)geom
                };
            }
            else if (type == typeof(LegalSubDivision))
            {
                outObj = new LegalSubDivision
                {
                    FullDescription = feature["fulldescri"],
                    Geometry = (MultiPolygon)geom,
                    LSD = Convert.ToInt16(feature["lsd"]),
                    Meridian = Convert.ToInt16(feature["meridian"]),
                    Quarter = feature["quarter"],
                    Range = Convert.ToInt16(feature["range"]),
                    Section = Convert.ToInt16(feature["section"]),
                    Township = Convert.ToInt16(feature["township"])
                };
            }
            else if (type == typeof(Municipality))
            {
                outObj = new Municipality
                {
                    Geometry = (MultiPolygon)geom,
                    Name = feature["name"],
                    Region = feature["region"]
                };
            }
            else if (type == typeof(SubWatershed))
            {
                outObj = new SubWatershed
                {
                    Alias = feature["alias"],
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Description = feature["descriptio"],
                    Geometry = (MultiPolygon)geom,
                    Modified = new DateTimeOffset(DateTime.Now),
                    Name = feature["name"],
                    WatershedId = ((Watershed)ImportedFeatures["Watershed"][Convert.ToInt32(feature["watershedi"])]).Id
                };
            }
            else if (type == typeof(Subbasin))
            {
                outObj = new Subbasin
                {
                    SubWatershedId = ((SubWatershed)ImportedFeatures["SubWatershed"][Convert.ToInt32(feature["subwatersh"])]).Id
                };
            }
            else if (type == typeof(Reach))
            {
                outObj = new Reach
                {
                    Geometry = (MultiLineString)geom,
                    ModelComponentId = mcId,
                    SubbasinId = ((Subbasin)ImportedFeatures["Subbasin"][Convert.ToInt32(feature["subbasinid"])]).Id
                };
            }
            else if (type == typeof(SubArea))
            {
                outObj = new SubArea
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Elevation = (Decimal)Convert.ToSingle(feature["elevation"]),
                    Geometry = (MultiPolygon)geom,
                    LandUse = feature["landuse"],
                    LegalSubDivisionId = ((LegalSubDivision)ImportedFeatures["LegalSubDivision"][Convert.ToInt32(feature["legalsubdi"])]).Id,
                    ModelComponentId = mcId,
                    ParcelId = ((Parcel)ImportedFeatures["Parcel"][Convert.ToInt32(feature["parcelid"])]).Id,
                    Slope = (Decimal)Convert.ToSingle(feature["slope"]),
                    SoilTexture = feature["soiltextur"],
                    SubbasinId = ((Subbasin)ImportedFeatures["Subbasin"][Convert.ToInt32(feature["subbasinid"])]).Id
                };
            }
            else if (type == typeof(IsolatedWetland))
            {
                outObj = new IsolatedWetland
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Geometry = (MultiPolygon)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id,
                    Volume = (Decimal)Convert.ToSingle(feature["volume"])
                };
            }
            else if (type == typeof(RiparianWetland))
            {
                outObj = new RiparianWetland
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Geometry = (MultiPolygon)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id,
                    Volume = (Decimal)Convert.ToSingle(feature["volume"])
                };
            }
            else if (type == typeof(Lake))
            {
                outObj = new Lake
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Geometry = (MultiPolygon)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id,
                    Volume = (Decimal)Convert.ToSingle(feature["volume"])
                };
            }
            else if (type == typeof(VegetativeFilterStrip))
            {
                outObj = new VegetativeFilterStrip
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    AreaRatio = (Decimal)Convert.ToSingle(feature["arearatio"]),
                    DrainageArea = null,
                    Geometry = (MultiPolygon)geom,
                    //Length = (Decimal)Convert.ToSingle(feature["length"]),
                    //Width = (Decimal)Convert.ToSingle(feature["width"]),
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id
                };
            }
            else if (type == typeof(RiparianBuffer))
            {
                outObj = new RiparianBuffer
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    AreaRatio = (Decimal)Convert.ToSingle(feature["arearatio"]),
                    DrainageArea = null,
                    Geometry = (MultiPolygon)geom,
                    //Length = (Decimal)Convert.ToSingle(feature["length"]),
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id
                };
            }
            else if (type == typeof(GrassedWaterway))
            {
                outObj = new GrassedWaterway
                {
                    Geometry = (MultiPolygon)geom,
                    Length = (Decimal)Convert.ToSingle(feature["length"]),
                    Width = (Decimal)Convert.ToSingle(feature["width"]),
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id
                };
            }
            else if (type == typeof(FlowDiversion))
            {
                outObj = new FlowDiversion
                {
                    Geometry = (MultiPoint)geom,
                    Length = (Decimal)Convert.ToSingle(feature["length"]),
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id
                };
            }
            else if (type == typeof(Reservoir))
            {
                outObj = new Reservoir
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Geometry = (MultiPolygon)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id,
                    Volume = (Decimal)Convert.ToSingle(feature["volume"])
                };
            }
            else if (type == typeof(SmallDam))
            {
                outObj = new SmallDam
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Geometry = (MultiPolygon)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id,
                    Volume = (Decimal)Convert.ToSingle(feature["volume"])
                };
            }
            else if (type == typeof(Wascob))
            {
                outObj = new Wascob
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Geometry = (MultiPolygon)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id,
                    Volume = (Decimal)Convert.ToSingle(feature["volume"])
                };
            }
            else if (type == typeof(Dugout))
            {
                outObj = new Dugout
                {
                    AnimalTypeId = Convert.ToInt32(feature["animaltype"]),
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Geometry = (MultiPolygon)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id,
                    Volume = (Decimal)Convert.ToSingle(feature["volume"])
                };
            }
            else if (type == typeof(CatchBasin))
            {
                outObj = new CatchBasin
                {
                    Area = (Decimal)Convert.ToSingle(feature["shape_area"]) / 10000,
                    Geometry = (MultiPolygon)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id,
                    Volume = (Decimal)Convert.ToSingle(feature["volume"])
                };
            }
            else if (type == typeof(Feedlot))
            {
                outObj = new Feedlot
                {
                    AnimalAdultRatio = (Decimal)Convert.ToSingle(feature["animaladul"]),
                    AnimalNumber = Convert.ToInt32(feature["animalnumb"]),
                    AnimalTypeId = Convert.ToInt32(feature["animaltype"]),
                    Geometry = (MultiPolygon)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id
                };
            }
            else if (type == typeof(ManureStorage))
            {
                outObj = new ManureStorage
                {
                    Area = (Decimal)Convert.ToSingle(feature["area"]),
                    Geometry = (MultiPoint)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id,
                    Volume = (Decimal)Convert.ToSingle(feature["volume"])
                };
            }
            else if (type == typeof(RockChute))
            {
                outObj = new RockChute
                {
                    Geometry = (MultiPoint)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id
                };
            }
            else if (type == typeof(PointSource))
            {
                outObj = new PointSource
                {
                    Geometry = (MultiPoint)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id
                };
            }
            else if (type == typeof(ClosedDrain))
            {
                outObj = new ClosedDrain
                {
                    Geometry = (MultiPoint)geom,
                    ModelComponentId = mcId,
                    Name = null,
                    ReachId = ((Reach)ImportedFeatures["Reach"][Convert.ToInt32(feature["reachid"])]).Id,
                    SubAreaId = ((SubArea)ImportedFeatures["SubArea"][Convert.ToInt32(feature["subareaid"])]).Id
                };
            }

            return outObj;
        }

        public string GetUpdateGeometrySQL(object obj, string geomWKT)
        {
            if (this.ValidTypes.Contains(obj.GetType()))
            {
                return $"UPDATE public.\"{obj.GetType().Name}\" SET \"Geometry\" = {geomWKT} WHERE \"Id\" = {((BaseItem)obj).Id};";
            }
            return "";
        }

        private void UpdateWatershedOutletReachId()
        {
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                var watershed = db.Watersheds.Find(ImportedFeatures["Watershed"].FirstOrDefault().Key);
                int newReachId = ((Reach)ImportedFeatures["Reach"][watershed.OutletReachId]).Id;
                watershed.OutletReachId = newReachId;
                db.SaveChanges();
            }
        }

        public Dictionary<string, Dictionary<int, object>> ImportedFeatures { get; set; }
        public HashSet<Type> ValidTypes { get; } = new HashSet<Type> { new Watershed().GetType(), new Parcel().GetType(), new LegalSubDivision().GetType(), new Farm().GetType(),
                    new Municipality().GetType(), new SubWatershed().GetType(), new Subbasin().GetType(), new Reach().GetType(), new SubArea().GetType(),
                    new IsolatedWetland().GetType(),new RiparianWetland().GetType(),new Lake().GetType(),new VegetativeFilterStrip().GetType(),
                    new RiparianBuffer().GetType(),new GrassedWaterway().GetType(),new FlowDiversion().GetType(),new Reservoir().GetType(),
                    new SmallDam().GetType(),new Wascob().GetType(),new Dugout().GetType(), new CatchBasin().GetType(),new Feedlot().GetType(),
                    new ManureStorage().GetType(),new RockChute().GetType(),new PointSource().GetType(),new ClosedDrain().GetType(),new SolutionParcels().GetType(),
                    new OptimizationWeights().GetType(),new OptimizationConstraints().GetType(),new OptimizationParcels().GetType(),new OptimizationLegalSubDivisions().GetType(),
                    new SolutionLegalSubDivisions().GetType() };
    }
}
