using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Enumerators
{
    public class Enumerators
    {


        public enum UserType
        {
            Admin = 1,
            WatershedManager = 2,
            MunicipalityManager = 3,
            Farmer = 4
        }


        /// <summary>
        /// Scenario Result Summarization Type
        /// </summary>
        public enum ScenarioResultSummarizationTypeEnum
        {
            LSD = 1,
            Parcel = 2,
            Farm = 3,
            Municipality = 4,
            Subwatershed = 5,
            Watershed = 6
        }


        public enum ScenarioTypeEnum
        {
            Conventional = 1,
            Existing = 2
        }

        public enum ProjectSpatialUnitTypeEnum
        {
            LSD = 1,
            Parcel = 2
        }


        public enum OptimizationTypeEnum
        {
            EcoService = 1,
            Budget = 2
        }

        public enum OptimizationSolutionLocationTypeEnum
        {
            LegalSubDivision = 1,
            Parcel = 2,
            ReachBMP = 3
        }

        public enum OptimizationObjectiveTypeEnum
        {
            MaximizeEcoService = 1,
            MinimizeBudget = 2
        }

        public enum OptimizationConstraintBoundTypeEnum
        {
            GreaterThan = 1,
            LessThan = 2
        }

        public enum OptimizationConstraintValueTypeEnum
        {
            Percent = 1,
            AbsoluteValue = 2
        }

        public enum BMPEffectivenessLocationTypeEnum
        {
            Onsite = 1,
            Offsite = 2
        }

        public enum ModelComponentTypeEnum
        {
            SubArea = 1,
            Reach = 2,
            IsolatedWetland = 3,
            RiparianWetland = 4,
            Lake = 5,
            VegetativeFilterStrip = 6,
            RiparianBuffer = 7,
            GrassedWaterway = 8,
            FlowDiversion = 9,
            Reservoir = 10,
            SmallDam = 11,
            Wascob = 12,
            Dugout = 13,
            CatchBasin = 14,
            Feedlot = 15,
            ManureStorage = 16,
            RockChute = 17,
            PointSource = 18,
            ClosedDrain = 19
        }
    }
}
