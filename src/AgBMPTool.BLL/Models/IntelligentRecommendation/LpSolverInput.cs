using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;
using static AgBMPTool.BLL.Services.Utilities.LpSolve;

namespace AgBMPTool.BLL.Models.IntelligentRecommendation
{
    public class LpSolverInput
    {
        public int ColNumber { get; set; }
        public List<double> ObjectiveFnCoefficients { get; set; } = new List<double>() { 0 };
        public List<Constraint> EqualConstraints { get; set; } = new List<Constraint>();
        public List<Constraint> LessOrEqualConstraints { get; set; } = new List<Constraint>();
        public List<Constraint> GreaterOrEqualConstraints { get; set; } = new List<Constraint>();
        public List<Column> Columns { get; set; } = new List<Column>();
        public List<Row> Rows { get; set; } = new List<Row>();
        public class Constraint
        {
            public List<double> LHS { get; set; }
            public lpsolve_constr_types Type { get; set; }
            public double RHS { get; set; }
        }

        public class Column
        {
            public int LocationId { get; set; } = 0;
            public OptimizationSolutionLocationTypeEnum LocationType { get; set; } = OptimizationSolutionLocationTypeEnum.ReachBMP;
            public string BMPCombinationTypeName { get; set; } = "";
            public int BMPCombinationTypeId { get; set; } = 0;
            public string ColumnName { get => LocationId > 0 ? $"{LocationId}-{BMPCombinationTypeName}" : "NoBMP"; }
        }

        public class Row
        {
            public int LocationId { get; set; } = 0;
            public OptimizationSolutionLocationTypeEnum LocationType { get; set; } = OptimizationSolutionLocationTypeEnum.ReachBMP;
            public string BMPTypeName { get; set; } = "";
            public int BMPTypeId { get; set; } = 0;
            public string RowName { get => LocationId > 0 ? $"{BMPTypeName}-{LocationId}" : "NaN"; }
        }

    }
}
