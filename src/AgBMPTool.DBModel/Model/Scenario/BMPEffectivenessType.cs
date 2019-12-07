using AgBMPTool.DBModel.Model.ScenarioModelResult;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Scenario
{
    public class BMPEffectivenessType : Type.BaseType
    {
        [ForeignKey(nameof(ScenarioModelResultType))]
        public int? ScenarioModelResultTypeId { get; set; }

        /// <summary>
        /// The corresponding scenario model result type. Could be null. 
        /// </summary>
        public ScenarioModelResultType ScenarioModelResultType { get; set; }

        [ForeignKey(nameof(UnitType))]
        public int UnitTypeId { get; set; }

        public Type.UnitType UnitType { get; set; }

        [ForeignKey(nameof(ScenarioModelResultVariableType))]
        public int? ScenarioModelResultVariableTypeId { get; set; }

        public ScenarioModelResultVariableType ScenarioModelResultVariableType { get; set; }

        /// <summary>
        /// defalut weight value
        /// </summary>
        public int DefaultWeight { get; set; } 

        [ForeignKey(nameof(DefaultConstraintType))]
        public int? DefaultConstraintTypeId { get; set; }

        public Optimization.OptimizationConstraintValueType DefaultConstraintType { get; set; }

        public decimal? DefaultConstraint { get; set; }

        [ForeignKey(nameof(BMPEffectivenessLocationType))]
        public int BMPEffectivenessLocationTypeId { get; set; }

        public BMPEffectivenessLocationType BMPEffectivenessLocationType { get; set; }

        [ForeignKey(nameof(UserEditableConstraintBoundType))]
        public int UserEditableConstraintBoundTypeId { get; set; }

        public Optimization.OptimizationConstraintBoundType UserEditableConstraintBoundType { get; set; }

        [ForeignKey(nameof(UserNotEditableConstraintValueType))]
        public int UserNotEditableConstraintValueTypeId { get; set; }

        public Optimization.OptimizationConstraintValueType UserNotEditableConstraintValueType { get; set; }

        public decimal UserNotEditableConstraintBoundValue { get; set; }        
    }
}
