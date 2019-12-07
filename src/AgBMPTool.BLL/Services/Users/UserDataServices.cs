using AgBMPTool.BLL.Services.Users;
using AgBMPTool.DBModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using AgBMPTool.BLL.Models.Shared;
using AgBMTool.DBL.Interface;
using AgBMPTool.DBModel.Model.User;
using AgBMPTool.DBModel.Model.ModelComponent;
using NetTopologySuite.Geometries;
using AgBMPTool.DBModel.Model.Boundary;
using Microsoft.EntityFrameworkCore;

namespace AgBMPTool.BLL.Services.Users
{
    public class UserDataServices: IUserDataServices
    {
        private readonly IUnitOfWork _uow;
        public UserDataServices(IUnitOfWork _iUnitOfWork)
        {
            this._uow = _iUnitOfWork;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public UserDetailsDTO GetUserDetailsByUserEmail(string userEmail)
        {
            UserDetailsDTO userDetails = new UserDetailsDTO();
            try
            {
                userDetails = _uow.GetRepository<User>().Get(x => x.Email == userEmail).Select(x=> UserDetailsDTO.Map(x)).SingleOrDefault();
            }
            catch (Exception e)
            {
                
            }

            return userDetails;
        }

        /// <summary>
        /// function to test Db connection
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public List<int> GetAllUsersId()
        {
            List<int> userIds = new List<int>();
            try
            {
                userIds = _uow.GetRepository<User>().Get().Select(x => x.Id).ToList();
            }
            catch (Exception e)
            {

            }

            return userIds;
        }

        public List<BaseItemDTO> getUserScenarioTypes(int userId) {
            List<BaseItemDTO> res = new List<BaseItemDTO>();
            var userType = _uow.GetRepository<User>().Get(x => x.Id == userId).Select(x => x.UserType).FirstOrDefault();
            if (userType != null) {
                switch (userType.Name)
                {
                    case "Municipality Manager":
                        Console.WriteLine("Case 1");
                        break;
                    case "Admin":
                        Console.WriteLine("Case 2");
                        break;
                    case "Watershed Manager":
                        Console.WriteLine("Case 2");
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }
            return res;
        }

        private Geometry getFilterGeometry(int municipalityId, int waterShedId, int subWaterShedId) {
            if (subWaterShedId == 0) 
                subWaterShedId = -1;
            if (municipalityId != -1){
                return _uow.GetRepository<Municipality>().Get(x => x.Id == municipalityId).FirstOrDefault().Geometry;
            }
            else if (waterShedId != -1) {
                return _uow.GetRepository<Watershed>().Get(x => x.Id == waterShedId).FirstOrDefault().Geometry;
            }
            else if (subWaterShedId != -1) {
                return _uow.GetRepository<SubWatershed>().Get(x => x.Id == subWaterShedId).FirstOrDefault().Geometry;
            }

            return null;
        }

        private Geometry getUserBoundary(int userId)
        {
            var userType = _uow.GetRepository<User>().Query().Include(x => x.UserType).Where(x => x.Id == userId).Select(x => x.UserType).FirstOrDefault();
            if (userType.Id == 1)
            {
                var municipalities = _uow.GetRepository<UserMunicipalities>().Query().Include(x => x.Municipality).Where(x => x.UserId == userId).Select(x => x.Municipality).ToList();
                Geometry geometry = null;
                foreach (var municipality in municipalities)
                {
                    if (geometry == null)
                    {
                        geometry = municipality.Geometry;
                    }
                    else
                    {
                        geometry = (Geometry)geometry.Union(municipality.Geometry);
                    }
                }
                return geometry;
            }
            else if (userType.Id == 2)
            {
                var watersheds = _uow.GetRepository<UserWatersheds>().Query().Include(x => x.Watershed).Where(x => x.UserId == userId).Select(x => x.Watershed).ToList();
                Geometry geometry = null;
                foreach (var watershed in watersheds)
                {
                    if (geometry == null)
                    {
                        geometry = watershed.Geometry;
                    }
                    else
                    {
                        geometry = (Geometry)geometry.Union(watershed.Geometry);
                    }
                }
                return geometry;
            }
            return null;
        }

        public ExtentModel getUserExtent(int userId)
        {
            var boundary = getUserBoundary(userId);
            if (boundary != null)
            {
                var extent = boundary.EnvelopeInternal;
                extent.ExpandBy(extent.Width * 0.5, extent.Height * 0.5);

                return new ExtentModel
                {
                    xmin = extent.MinX,
                    ymin = extent.MinY,
                    xmax = extent.MaxX,
                    ymax = extent.MaxY,
                    spatialReference = new SpatialReferenceModel() { wkid = 4326 }
                };
            }
            else
                return null;
        }

        public Double[] getUserMapCenter(int userId){
            var boundary = getUserBoundary(userId);
            if(boundary != null)
                return new Double[] { boundary.Centroid.X, boundary.Centroid.Y };
            else
                return new Double[] { };
        }

        public GeometryLayerStyle getGeometryLayerStyle(String layername) {
            return _uow.GetRepository<GeometryLayerStyle>().Get(x => x.layername == layername).FirstOrDefault();
        }

        private IEnumerable<Geometry> getUserGeometryBoundaries(int userId) {
            var userType = _uow.GetRepository<User>().Query().Include(x => x.UserType).Where(x => x.Id == userId).Select(x => x.UserType).FirstOrDefault();
            if (userType.Id == 1)
            {
                var municipalities = getMunicitpilitiesByUserId(userId);
                 return municipalities.Select(x => x.Geometry);
            }
            else
            {
                var watersheds = getMunicitpilitiesByUserId(userId);
                return watersheds.Select(x => x.Geometry);
            }
        }
        public List<MutiPolyGon> getUserLSD(int userId, int municipalityId, int waterShedId, int subWaterShedId)
        {
            var usergeometryboundary = getUserGeometryBoundaries(userId);
            var filterGeometry = getFilterGeometry(municipalityId, waterShedId, subWaterShedId);
            var geometrylayerstyle = getGeometryLayerStyle("LSD");
            var lsds = _uow.GetRepository<LegalSubDivision>().Query();
            var filteredlsd = lsds.Where(x => usergeometryboundary.Any(y => y.Intersects(x.Geometry))
            && (filterGeometry == null || (filterGeometry != null && filterGeometry.Intersects(x.Geometry))));
            return filteredlsd.Select(x =>
                new MutiPolyGon
                {
                    coordinates = MutiPolyGon.convertPolygonString(x.Geometry),
                    attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(x.Id) },
                    style = new PolyGonStyle(geometrylayerstyle)
                }
          ).ToList();
        }
        public List<PolyLine> getUserReach(int userId, int municipalityId, int waterShedId, int subWaterShedId) {
            var res = new List<PolyLine>();
            var filterGeometry = getFilterGeometry(municipalityId, waterShedId, subWaterShedId);
            var watersheids = getWatershedByUserId(userId).Select(x => x.Id).ToList();

            var modelComponentIds = _uow.GetRepository<ModelComponent>()
                .Get(x => watersheids.Contains(x.WatershedId)).Select(x => x.Id).ToList();

            var lsds = _uow.GetRepository<Reach>().Get(x => modelComponentIds.Contains(x.ModelComponentId));
            var geometrylayerstyle = getGeometryLayerStyle("Reach");
            foreach (var lsd in lsds) {
                var polylinegeometry = lsd.Geometry;
                if (filterGeometry != null && !filterGeometry.Intersects(polylinegeometry)) {
                    continue;
                }
                var coordinates = PolyLine.convertPolygonString(polylinegeometry);
                PolyLine polyline = new PolyLine();
                polyline.coordinates = coordinates;
                polyline.attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(lsd.Id) };
                polyline.style = new SimpleLineStyle(geometrylayerstyle);
                res.Add(polyline);

            }
            return res;
        }

        public List<MutiPolyGon> getUserParcel(int userId, int municipalityId, int waterShedId, int subWaterShedId)
        {
            var usergeometryboundary = getUserGeometryBoundaries(userId);
            var filterGeometry = getFilterGeometry(municipalityId, waterShedId, subWaterShedId);
            var geometrylayerstyle = getGeometryLayerStyle("Parcel");
            var parcels = _uow.GetRepository<Parcel>().Query();
            var filteredparcel = parcels.Where(x =>
            usergeometryboundary.Any(y => y.Intersects(x.Geometry)) && 
            (filterGeometry == null || (filterGeometry != null && 
            filterGeometry.Intersects(x.Geometry))));
            var res = filteredparcel.Select(x =>
                new MutiPolyGon
                {
                    coordinates = MutiPolyGon.convertPolygonString(x.Geometry),
                    attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(x.Id) },
                    style = new PolyGonStyle(geometrylayerstyle)
                }
          ).ToList();
            return res;
        }


        public List<MutiPolyGon> getUserFarm(int userId, int municipalityId, int waterShedId, int subWaterShedId)
        {
            var usergeometryboundary = getUserGeometryBoundaries(userId);
            var filterGeometry = getFilterGeometry(municipalityId, waterShedId, subWaterShedId);
            var geometrylayerstyle = getGeometryLayerStyle("Farm");
            var farms = _uow.GetRepository<Farm>().Query();
            var filteredfarm = farms.Where(x =>
            usergeometryboundary.Any(y => y.Intersects(x.Geometry)) && (filterGeometry == null ||  (filterGeometry != null && filterGeometry.Intersects(x.Geometry))));
            return filteredfarm.Select(x =>
                new MutiPolyGon
                {
                    coordinates = MutiPolyGon.convertPolygonString(x.Geometry),
                    attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(x.Id) },
                    style = new PolyGonStyle(geometrylayerstyle)
                }
          ).ToList();
        }

        public List<Municipality> getMunicitpilitiesByUserId(int userId) {
            var userType = _uow.GetRepository<User>().Query().Include(x => x.UserType).Where(x => x.Id == userId).Select(x => x.UserType).FirstOrDefault();
            if (userType.Id == 1)
            {
                return _uow.GetRepository<UserMunicipalities>()
                    .Query()
                    .Include(x => x.Municipality)
                    .Where(x => x.UserId == userId)
                    .Select(x => x.Municipality).ToList();

            }
            else {
                var watershedgeometries = _uow.GetRepository<UserWatersheds>()
                    .Query()
                    .Include(x => x.Watershed)
                    .Where(x => x.UserId == userId)
                    .Select(x => x.Watershed.Geometry);
                var municipalities = _uow.GetRepository<Municipality>().Query().ToList();
                return municipalities.Where(
                    x => 
                    watershedgeometries.Any(
                        y => y.Intersects(x.Geometry)))
                    .ToList();
            }
        }

        public List<Watershed> getWatershedByUserId(int userId)
        {
            var userType = _uow.GetRepository<User>().Query().Include(x => x.UserType).Where(x => x.Id == userId).Select(x => x.UserType).FirstOrDefault();
            if (userType.Id == 1)
            {
                var municitpilitiesGeometries = _uow.GetRepository<UserMunicipalities>().Query().Include(
                    x => x.Municipality)
                    .Where(x => x.UserId == userId)
                    .Select(x => x.Municipality.Geometry);
                var watersheds = _uow.GetRepository<Watershed>().Query().ToList();
                return watersheds.Where(
                    x =>
                    (municitpilitiesGeometries.Any(
                        y => y.Intersects(x.Geometry))))
                    .ToList();

            }
            else
            {
                return _uow.GetRepository<UserWatersheds>().Query()
                    .Include(x => x.Watershed)
                    .Where(x => x.UserId == userId)
                    .Select(x => x.Watershed).ToList();
            }
        }

        public List<MutiPolyGon> getUserMunicipalities(int userId, int municipalityId, int waterShedId, int subWaterShedId)
        {
            var res = new List<MutiPolyGon>();
            var filterGeometry = getFilterGeometry(municipalityId, waterShedId, subWaterShedId);
            var municipalities = getMunicitpilitiesByUserId(userId);
            var geometrylayerstyle = getGeometryLayerStyle("Municipality");
            var filteredmunicipalities = municipalities.Where(x => (municipalityId == -1 || (municipalityId != -1 && x.Id == municipalityId))).Where(x =>
             filterGeometry == null || (filterGeometry != null && filterGeometry.Intersects(x.Geometry)));
            return filteredmunicipalities.Select(x =>
                new MutiPolyGon
                {
                    coordinates = MutiPolyGon.convertPolygonString(x.Geometry),
                    attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(x.Id) },
                    style = new PolyGonStyle(geometrylayerstyle)
                }
          ).ToList();
        }
        public List<MutiPolyGon> getUserWaterShed(int userId, int municipalityId, int waterShedId, int subWaterShedId)
        {
            var res = new List<MutiPolyGon>();
            var filterGeometry = getFilterGeometry(municipalityId, waterShedId, subWaterShedId);
            var geometrylayerstyle = getGeometryLayerStyle("WaterShed");
            var watersheds = getWatershedByUserId(userId);
            var filteredwatersheds = watersheds.Where(x => (waterShedId == -1 || (waterShedId != -1 && x.Id == waterShedId))).Where(x =>
            filterGeometry == null || (filterGeometry != null && filterGeometry.Intersects(x.Geometry)));
            return filteredwatersheds.Select(x =>
                new MutiPolyGon
                {
                    coordinates = MutiPolyGon.convertPolygonString(x.Geometry),
                    attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(x.Id) },
                    style = new PolyGonStyle(geometrylayerstyle)
                }
          ).ToList();
        }

        public List<MutiPolyGon> getUserSubWaterShed(int userId, int municipalityId, int waterShedId, int subWaterShedId)
        {
            var res = new List<MutiPolyGon>();
            var filterGeometry = getFilterGeometry(municipalityId, waterShedId, subWaterShedId);
            var geometrylayerstyle = getGeometryLayerStyle("SubWaterShed");
            var watershedIds = getWatershedByUserId(userId).Select(x => x.Id);
            var subWatersheds = _uow.GetRepository<SubWatershed>().Get(x => watershedIds.Contains(x.WatershedId)).Where(x => (subWaterShedId == -1 || (subWaterShedId != -1 && x.Id == subWaterShedId))).ToList();
            foreach (var subWatershed in subWatersheds)
            {
                var subWatershedGeometry = subWatershed.Geometry;
                if (filterGeometry != null && !filterGeometry.Intersects(subWatershedGeometry))
                {
                    continue;
                }
                var coordinates = MutiPolyGon.convertPolygonString(subWatershedGeometry);
                MutiPolyGon mutiPolyGon = new MutiPolyGon();
                mutiPolyGon.coordinates = coordinates;
                mutiPolyGon.attributes = new List<GeometryAttriBute>() { GeometryAttriBute.getGeometryIdAttribue(subWatershed.Id) };
                mutiPolyGon.style = new PolyGonStyle(geometrylayerstyle);
                res.Add(mutiPolyGon);
            }
            return res;
        }
    }
}
