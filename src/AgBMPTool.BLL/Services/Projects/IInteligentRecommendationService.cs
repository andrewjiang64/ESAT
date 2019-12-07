using AgBMPTool.BLL.Models.IntelligentRecommendation;

namespace AgBMPTool.BLL.Services.Projects
{
    public interface IInteligentRecommendationService
    {
        /// <summary>
        /// Create a recommended solution subject to a list of environmental constraints or budget target.
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <returns>Returns TRUE if generated successfully, otherwise returns FALSE</returns>
        bool BuildRecommendedSolution(int projectId, bool isPrintLp);

        /// <summary>
        /// Get constraint upper and lower bounds in percentage and absolute value
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="bmpEffectivenessTypeId">BMP effectiveness id</param>
        /// <returns>EffectivenessBoundDTO contains BMP effectiveness id, upper and lower bounds in both percentage and absolute value</returns>
        EffectivenessBoundDTO GetConstraintBound(int projectId, int bmpEffectivenessTypeId);

        /// <summary>
        /// Get constraint upper and lower budget bounds
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <returns>BudgetBoundDTO contains upper and lower budget bounds in $/year</returns>
        BudgetBoundDTO GetBudgetBound(int projectId);

        /// <summary>
        /// Save optimization settings
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="settingDTO">OptimizationTypeEnum, List of EcoServiceValueWeight, List of ConstraintInput</param>
        /// <returns>True if saving successfully, otherwise returns false</returns>
        bool SaveOptimizationSettings(int projectId, OptimizationSettingDTO settingDTO);
    }
}
