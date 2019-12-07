using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class SolutionDTO : SolutionOptimizationBase
    {
        public List<UnitSolution> SolutionLegalSubDivisions
        {
            get => UnitSolutions.FindAll(x => x.LocationType == Enumerators.Enumerators.OptimizationSolutionLocationTypeEnum.LegalSubDivision);
        }

        public List<UnitSolution> SolutionParcels
        {
            get => UnitSolutions.FindAll(x => x.LocationType == Enumerators.Enumerators.OptimizationSolutionLocationTypeEnum.Parcel);
        }

        public List<UnitSolution> SolutionModelComponents
        {
            get => UnitSolutions.FindAll(x => x.LocationType == Enumerators.Enumerators.OptimizationSolutionLocationTypeEnum.ReachBMP);
        }
    }
}
