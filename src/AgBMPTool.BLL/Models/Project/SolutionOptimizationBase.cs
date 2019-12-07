using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class SolutionOptimizationBase
    {
        public int ProjectId { get; set; }
        public List<UnitSolution> UnitSolutions { get; set; } = new List<UnitSolution>();

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            SolutionOptimizationBase s = (SolutionOptimizationBase)obj;
            if (s.ProjectId != this.ProjectId) return false;

            if (UnitSolutions.Count != s.UnitSolutions.Count) return false;

            foreach (var us in UnitSolutions)
            {
                if (s.UnitSolutions.Find(x => x.LocationType == us.LocationType && x.LocationId == us.LocationId && x.BMPTypeId == us.BMPTypeId) == null) return false;
            }

            return true;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            throw new NotImplementedException();
            return base.GetHashCode();
        }
    }
}
