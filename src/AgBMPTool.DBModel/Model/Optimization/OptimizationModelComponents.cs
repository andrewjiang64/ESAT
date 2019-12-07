﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Optimization
{
    /// <summary>
    /// The selected structure location for optimization. All locations (LSD or parcels) are selected by default.
    /// </summary>
    public class OptimizationModelComponents : BaseItem
    {
        [ForeignKey(nameof(Optimization))]
        public int OptimizationId { get; set; }

        public Optimization Optimization { get; set; }

        [ForeignKey(nameof(BMPType))]
        public int BMPTypeId { get; set; }

        public Scenario.BMPType BMPType { get; set; }

        [ForeignKey(nameof(ModelComponent))]
        public int ModelComponentId { get; set; }

        public ModelComponent.ModelComponent ModelComponent { get; set; }

        /// <summary>
        /// If the parcel or lsd is selected
        /// </summary>
        public bool IsSelected { get; set; }
    }
}