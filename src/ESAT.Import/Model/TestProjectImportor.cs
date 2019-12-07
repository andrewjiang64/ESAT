using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.User;
using AgBMTool.DBL.Interface;
using ESAT.Import.Utils;
using ESAT.Import.Utils.TextFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAT.Import.Model
{
    public class TestProjectImportor
    {
        private readonly IUnitOfWork _uow;

        public TestProjectImportor(IUnitOfWork _iUnitOfWork)
        {
            this._uow = _iUnitOfWork;
        }

        public void ImportTestProjectFromText(string textFilePath, int investorId, int watershedId)
        {
            // Import from text file
            new EsatDbWriter().ExecuteNonQuery(TextReader.GetText(textFilePath));

            // Add WatershedExistingBMPType
            WatershedExistingBMPTypeFactory webtFactory = new WatershedExistingBMPTypeFactory(_uow);

            foreach (var webt in webtFactory.BuildWatershedExistingBMPTypeDTOs(watershedId, 2, investorId))
            {
                _uow.GetRepository<WatershedExistingBMPType>().Add(
                    new WatershedExistingBMPType
                    {
                        ModelComponentId = webt.ModelComponentId,
                        BMPTypeId = webt.BMPTypeId,
                        ScenarioTypeId = webt.ScenarioTypeId,
                        InvestorId = webt.InvestorId
                    });
            }
            _uow.Commit();
        }
    }
}
