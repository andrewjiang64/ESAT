using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    /// <summary>
    /// subwatershed is part of watershed
    /// </summary>
    public class SubWatershed : BaseItemWithBoundary
    {
        public string Name { get; set; }

        public string Alias { get; set; }

        public string Description { get; set; }

        public decimal Area { get; set; }

        public DateTimeOffset Modified { get; set; }

        [ForeignKey(nameof(Watershed))]
        public int WatershedId { get; set; }

        public Watershed Watershed { get; set; }

        public List<Subbasin> Subbasins { get; set; }
    }
}
