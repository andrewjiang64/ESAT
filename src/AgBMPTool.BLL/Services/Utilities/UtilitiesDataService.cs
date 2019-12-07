using AgBMPTool.BLL.Models.Utility;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMTool.DBL.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using AgBMPTool.BLL.Models.Shared;
using AgBMPTool.DBModel.Model.User;
using AgBMPTool.DBModel.Model.Boundary;
using AgBMPTool.DBModel.Model.ModelComponent;
using Microsoft.EntityFrameworkCore;

namespace AgBMPTool.BLL.Services.Utilities
{
    public class UtilitiesDataService : IUtilitiesDataService
    {
        private readonly IUnitOfWork _uow;

        public UtilitiesDataService(IUnitOfWork _iUnitOfWork)
        {
            this._uow = _iUnitOfWork;
        }

        /// <summary>
        /// Get ScenarioType option (Conventional & Existing)
        /// </summary>
        /// <returns>List of Scenario Type</returns>
        public List<DropdownDTO> GetScenarioTypeOptions()
        {
            var res = new List<DropdownDTO>();

            var scenarioType = _uow.GetRepository<ScenarioType>().Query().Where(x => x.IsBaseLine == true).ToList();

            foreach (var baseline in scenarioType)
            {
                res.Add(new DropdownDTO()
                {
                    ItemId = baseline.Id,
                    ItemName = baseline.Name,
                    Description = baseline.Description,
                    SortOrder = baseline.SortOrder,
                    IsDefault = baseline.IsDefault
                });
            }
            return res.OrderBy(x => x.SortOrder).ToList();
        }

        /// <summary>
        /// Get ScenarioType option (Conventional & Existing)
        /// </summary>
        /// <returns>List of Scenario Type</returns>
        public List<BMPEffectivenessTypeDTO> GetBMPEffectivenessType()
        {
            var res = new List<BMPEffectivenessTypeDTO>();

            res = _uow.GetRepository<BMPEffectivenessType>().Query().Include(x => x.ScenarioModelResultType.UnitType).
                Select(x => new BMPEffectivenessTypeDTO(x, x.ScenarioModelResultType.UnitType)).ToList();

            return res.OrderBy(x => x.sortOrder).ToList();
        }

        /// <summary>
        /// Get Scenario Result Summarization Types 
        /// </summary>
        /// <returns>List of Scenario Type</returns>
        public List<DropdownDTO> GetSummarizationTypeOptions()
        {
            var res = new List<DropdownDTO>();

            var summarizationTypes = _uow.GetRepository<ScenarioResultSummarizationType>().Query().ToList();

            foreach (var baseline in summarizationTypes)
            {
                res.Add(new DropdownDTO()
                {
                    ItemId = baseline.Id,
                    ItemName = baseline.Name,
                    Description = baseline.Description,
                    SortOrder = baseline.SortOrder,
                    IsDefault = baseline.IsDefault
                });
            }
            return res.OrderBy(x => x.SortOrder).ToList();
        }

        /// <summary>
        /// Get Scenario Result Summarization Types By SummerizationLevelId
        /// </summary>
        /// <returns>List of Scenario Type</returns>
        public List<DropdownDTO> GetSummarizationTypeOptionBySummerizationLevelId(int summerizationLevelId)
        {
            var res = new List<DropdownDTO>();

            var summarizationTypes = _uow.GetRepository<ScenarioResultSummarizationType>().Query().Where(x => x.Id == summerizationLevelId).Select(x => x).ToList();

            foreach (var baseline in summarizationTypes)
            {
                res.Add(new DropdownDTO()
                {
                    ItemId = baseline.Id,
                    ItemName = baseline.Name,
                    Description = baseline.Description,
                    SortOrder = baseline.SortOrder,
                    IsDefault = baseline.IsDefault
                });
            }
            return res.OrderBy(x => x.SortOrder).ToList();
        }

        /// <summary>
        /// get ScenarioModelResultVariableType 
        /// </summary>
        /// <returns></returns>
        public List<DropdownDTO> GetScenarioModelResultVariableTypes()
        {
            return _uow.GetRepository<ScenarioModelResultVariableType>().Get().Select(x => new DropdownDTO() { ItemId = x.Id, ItemName = x.Name, IsDefault = x.IsDefault }).OrderBy(x => x.SortOrder).ToList();
        }

        /// <summary>
        /// Get BMP Effectiveness Location Type (onsite/ offsite)
        /// </summary>
        /// <returns></returns>
        public List<DropdownDTO> GetBMPEffectivenessLocationType()
        {
            return _uow.GetRepository<BMPEffectivenessLocationType>().Get().Select(x => new DropdownDTO() { ItemId = x.Id, ItemName = x.Name, IsDefault = x.IsDefault }).OrderBy(x => x.SortOrder).ToList();
        }
    }
}
