using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class BMPCostAllEffectivenessDTO: BMPCostAreaDTO
    {
        public List<EffectivenessDTO> EffectivenessDTOs { get; set; } = new List<EffectivenessDTO>();
    }
}
