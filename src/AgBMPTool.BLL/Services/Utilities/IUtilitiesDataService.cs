using AgBMPTool.BLL.Models.Shared;
using AgBMPTool.BLL.Models.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Services.Utilities
{
    public interface IUtilitiesDataService
    {
        List<DropdownDTO> GetScenarioTypeOptions();

        List<DropdownDTO> GetSummarizationTypeOptions();

        List<DropdownDTO> GetSummarizationTypeOptionBySummerizationLevelId(int summerizationLevelId);

        List<BMPEffectivenessTypeDTO> GetBMPEffectivenessType();

        List<DropdownDTO> GetBMPEffectivenessLocationType();

        List<DropdownDTO> GetScenarioModelResultVariableTypes();
    }
}
