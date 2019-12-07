
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AgBMTool.DBL.Interface
{
    public interface ISqlQueryProvider
    {
        // IEnumerable<TT> runStoredProcedure(string connectionstring, )
        /// <summary>
        /// Runs a command directly against the db.
        /// </summary>
        /// <typeparam name="TT">Return type expected</typeparam>
        /// <param name="command">SQL command text</param>
        /// <param name="parameters">SQL command parameters</param>
        /// <returns></returns>
        TT ExecuteScalar<TT>(string command, params object[] parameters);

        /// <summary>
        /// Runs a query directly against the db, returning results
        /// </summary>
        /// <typeparam name="TT">Return type expected</typeparam>
        /// <param name="command">SQL command text</param>
        /// <param name="parameters">SQL command parameters</param>
        /// <returns></returns>
        IEnumerable<TT> ExecuteQuery<TT>(string commandName, params object[] parameters);

        /// <summary>
        /// Runs a query directly against the db, returning results
        /// </summary>
        /// <typeparam name="TT">Return type expected</typeparam>
        /// <param name="procName">SQL stored procedure name</param>
        /// <param name="parameters">SQL command parameters</param>
        /// <returns></returns>
        IEnumerable<TT> ExecuteProcedure<TT>(string procName, params SqlParameter[] parameters);

        IEnumerable<TT> ExecuteEntityProcedure<TT>(string command, params object[] parameters);


        IQueryable<T> GetQuery<T>() where T : class;
    }
}
