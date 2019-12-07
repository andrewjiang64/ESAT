using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Model
{
    interface IWatershedExistingBMPTypeFactory
    {
        List<WatershedExistingBMPTypeDTO> BuildWatershedExistingBMPTypeDTOs(int watershedId, int scenarioTypeId, int investorId);
    }
}
