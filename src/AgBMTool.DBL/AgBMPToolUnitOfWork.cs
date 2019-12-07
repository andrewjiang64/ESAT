using System;
using System.Collections.Generic;
using System.Text;
using AgBMPTool.DBModel;
using AgBMTool.DBL.Interface;
using AgBMPTool.DBModel.Model;

namespace AgBMTool.DBL
{
    public class AgBMPToolUnitOfWork : IUnitOfWork
    {
        private AgBMPToolContext Context { get; }

        public AgBMPToolUnitOfWork(AgBMPToolContext context)
        {
            Context = context;
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            return new AgBMPToolRepository<T>(Context);
        }
        public void Commit()
        {
            Context.SaveChanges();
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public List<T> ExecuteProcedure<T>(string sqlQuery)
        {
            return Context.ExecuteProcedure<T>(sqlQuery);
        }

        public int ExecuteProcedure(string sqlQuery)
        {
            return Context.ExecuteProcedure(sqlQuery);
        }

        #region ISqlQueryProvider
        /* class SqlQueryProvider : ISqlQueryProvider
         {
             public AgriProfitContext _dbContext { get; }
             internal SqlQueryProvider(AgriProfitContext dbContext)
             {
                 _dbContext = dbContext;
             }*/

        /*    public IQueryable<T> GetQuery<T>() where T : class
            {
                DbQuery<T> query = null;
                try
                {
                    query = (DbQuery<T>)_dbContext.Set<T>();
                }
                catch (InvalidCastException ex)
                {
                    throw new ApplicationException("Failed to get DbQuery from data context", ex);
                }
                return query.AsNoTracking();
            }
            public TT ExecuteScalar<TT>(string command, params object[] parameters)
            {
                return _dbContext.Database.ExecuteSqlCommand(command, parameters);
            }
            public IEnumerable<TT> ExecuteEntityProcedure<TT>(string command, params object[] parameters)
            {
                var rt = _dbContext.ExecuteQuery<TT>(command, parameters);
                return _dbContext.ExecuteQuery<TT>(command, parameters);
            }
            public IEnumerable<TT> ExecuteQuery<TT>(string commandName, params object[] parameters)
            {
                return _dbContext.ExecuteQuery<TT>(commandName, parameters).AsEnumerable();
            }
            public IEnumerable<TT> ExecuteProcedure<TT>(string sqlSProcName, params SqlParameter[] parameters)
            {
                string paramText = "";
                string commandText = "";
                foreach (var param in parameters)
                {
                    paramText += String.Format(" {0},", param.ParameterName);
                }
                paramText = paramText.TrimEnd(',');
                commandText = String.Format("EXEC {0} {1}", sqlSProcName, paramText);
                return _dbContext.ExecuteQuery<TT>(commandText, parameters).AsEnumerable();
            }
        }*/
        #endregion ISqlQueryProvider
    }
}
