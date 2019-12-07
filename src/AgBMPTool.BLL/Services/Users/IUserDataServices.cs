using AgBMPTool.BLL.Models.Shared;
using System;
using System.Collections.Generic;
using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Boundary;

namespace AgBMPTool.BLL.Services.Users
{
    public interface IUserDataServices
    {
        List<int> GetAllUsersId();
        UserDetailsDTO GetUserDetailsByUserEmail(string userEmail);

        List<BaseItemDTO> getUserScenarioTypes(int userId);
        List<MutiPolyGon> getUserLSD(int userId, int municipalityId, int waterShedId, int subWaterShedId);
        List<PolyLine> getUserReach(int userId, int municipalityId, int waterShedId, int subWaterShedId);
        List<MutiPolyGon> getUserParcel(int userId, int municipalityId, int waterShedId, int subWaterShedId);
        List<MutiPolyGon> getUserFarm(int userId, int municipalityId, int waterShedId, int subWaterShedId);
        List<MutiPolyGon> getUserMunicipalities(int userId, int municipalityId, int waterShedId, int subWaterShedId);
        List<MutiPolyGon> getUserWaterShed(int userId, int municipalityId, int waterShedId, int subWaterShedId);
        List<MutiPolyGon> getUserSubWaterShed(int userId, int municipalityId, int waterShedId, int subWaterShedId);
        Double[] getUserMapCenter(int userId);
        ExtentModel getUserExtent(int userId);
        List<Municipality> getMunicitpilitiesByUserId(int userId);
        List<Watershed> getWatershedByUserId(int userId);
        GeometryLayerStyle getGeometryLayerStyle(String layername);
    }
}
