using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    /// <summary>
    /// A location where a bmp could be applied. It could be any component in the watershed.
    /// </summary>
    public class ModelComponent : BaseItem
    {
        /// <summary>
        /// The id in the model
        /// </summary>
        public int ModelId { get; set; }

        /// <summary>
        /// Name of the model component
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of model component
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Watershed Id
        /// </summary>
        [ForeignKey(nameof(Watershed))]
        public int WatershedId { get; set; }

        /// <summary>
        /// Watershed
        /// </summary>
        public Watershed Watershed { get; set; }

        [ForeignKey(nameof(ModelComponentType))]
        public int ModelComponentTypeId { get; set; }

        /// <summary>
        /// The type of model component
        /// </summary>
        public ModelComponentType ModelComponentType { get; set; }

        /// <summary>
        /// All bmp types that could be applied.
        /// </summary>
        public List<ModelComponentBMPTypes> ModelComponentBMPTypes { get; set; }

        public CatchBasin CatchBasin { get; set; }

        public Dugout Dugout { get; set; }

        public Feedlot Feedlot { get; set; }

        public FlowDiversion FlowDiversion { get; set; }

        public GrassedWaterway GrassedWaterway { get; set; }

        public IsolatedWetland IsolatedWetland { get; set; }

        public Lake Lake { get; set; }

        public ManureStorage ManureStorage { get; set; }

        public PointSource PointSource { get; set; }

        public Reach Reach { get; set; }

        public Reservoir Reservoir { get; set; }

        public RiparianBuffer RiparianBuffer { get; set; }

        public RiparianWetland GetRiparianWetland { get; set; }

        public RockChute RockChute { get; set; }

        public SmallDam SmallDam { get; set; }

        public SubArea SubArea { get; set; }

        public VegetativeFilterStrip VegetativeFilterStrip { get; set; }

        public Wascob Wascob { get; set; }
    }
}
