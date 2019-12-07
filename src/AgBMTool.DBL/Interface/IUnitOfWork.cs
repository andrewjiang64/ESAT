using System;
using System.Collections.Generic;
using System.Text;
using AgBMPTool.DBModel.Model;

namespace AgBMTool.DBL.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>() where T : class;

        List<T> ExecuteProcedure<T>(string sqlQuery);

        int ExecuteProcedure(string sqlQuery);
        void Commit();
    }
}
