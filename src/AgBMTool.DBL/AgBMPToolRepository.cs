using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AgBMTool.DBL.Interface;
using AgBMPTool.DBModel;
using AgBMPTool.DBModel.Model;

namespace AgBMTool.DBL
{
    public class AgBMPToolRepository<T> : IRepository<T> where T : class
    {
        private readonly AgBMPToolContext db;

        public AgBMPToolRepository(AgBMPToolContext db)
        {
                this.db = db;
        }
        public void Add(T entity)
        {
            db.Set<T>().Add(entity);
        }

        public void Delete(T entity)
        {
                db.Set<T>().Remove(entity);
        }

        public IEnumerable<T> Get()
        {
            return db.Set<T>().AsEnumerable<T>();
        }

        public IEnumerable<T> Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return db.Set<T>().Where(predicate).AsEnumerable<T>();
        }

        public void Update(T entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.Set<T>().Update(entity);
        }

        public void UpdateRange(List<T> entity)
        {
            db.Set<T>().UpdateRange(entity);
        }

        public IQueryable<T> Query()
        {
            return db.Set<T>();
        }
    }
}
