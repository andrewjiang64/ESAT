using AgBMPTool.BLL.Models.Project;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData
{
    public class IntelligentRecommendationOptimizationDTO : SolutionOptimizationBase
    {
        public List<UnitSolution> OptimizationLegalSubDivisions
        {
            get => UnitSolutions.FindAll(x => x.LocationType == Enumerators.Enumerators.OptimizationSolutionLocationTypeEnum.LegalSubDivision);
        }

        public List<UnitSolution> OptimizationParcels
        {
            get => UnitSolutions.FindAll(x => x.LocationType == Enumerators.Enumerators.OptimizationSolutionLocationTypeEnum.Parcel);
        }

        public List<UnitSolution> OptimizationModelComponents
        {
            get => UnitSolutions.FindAll(x => x.LocationType == Enumerators.Enumerators.OptimizationSolutionLocationTypeEnum.ReachBMP);
        }
    }
}
