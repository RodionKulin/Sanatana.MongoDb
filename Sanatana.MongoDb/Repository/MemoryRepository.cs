using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using Sanatana.MongoDb.Repository.Memory;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sanatana.MongoDb.Repository
{
    /// <summary>
    /// Memory collection repository for writing tests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MemoryRepository<T> : IRepository<T>
        where T : class
    {
        //fields
        public List<T> Collection { get; set; } = new List<T>();


        //ctor
        public MemoryRepository()
        {

        }
        public MemoryRepository(List<T> collection)
        {
            Collection = collection;
        }


        //shared methods
        protected virtual void UpdateField(Updates<T> updates, T entity, bool wasInserted)
        {
            if (updates.Sets != null)
                foreach (Update<T> update in updates.Sets)
                {
                    PropertyInfo propertyInfo = ExpressionReflection.GetPropertyInfo(update.PropertyExpression.Body);
                    propertyInfo.SetValue(entity, update.Value);
                }
            if (updates.Increments != null)
                foreach (Update<T> update in updates.Increments)
                {
                    PropertyInfo propertyInfo = ExpressionReflection.GetPropertyInfo(update.PropertyExpression.Body);
                    int propertyValue = (int)propertyInfo.GetValue(entity);
                    propertyValue += (int)update.Value;
                    propertyInfo.SetValue(entity, propertyValue);
                }
            if (updates.SetOnInserts != null && wasInserted)
                foreach (Update<T> update in updates.SetOnInserts)
                {
                    PropertyInfo propertyInfo = ExpressionReflection.GetPropertyInfo(update.PropertyExpression.Body);
                    propertyInfo.SetValue(entity, update.Value);
                }
            if (updates.Pushes != null)
                foreach (UpdateCollection<T> update in updates.Pushes)
                {
                    PropertyInfo propertyInfo = ExpressionReflection.GetPropertyInfo(update.PropertyExpression.Body);
                    IEnumerable propertyValue = (IEnumerable)propertyInfo.GetValue(entity);
                    ExpressionReflection.AddToEnumerable(propertyValue, update.Value);
                }
            if (updates.Pulls != null)
                foreach (UpdateCollection<T> update in updates.Pulls)
                {
                    PropertyInfo propertyInfo = ExpressionReflection.GetPropertyInfo(update.PropertyExpression.Body);
                    IEnumerable propertyValue = (IEnumerable)propertyInfo.GetValue(entity);
                    ExpressionReflection.RemoveFromEnumerable(propertyValue, update.Value);
                }
        }


        //methods
        public virtual Task InsertMany(IEnumerable<T> entities, CancellationToken token = default)
        {
            Collection.AddRange(entities);
            return Task.CompletedTask;
        }

        public virtual Task InsertOne(T entity, CancellationToken token = default)
        {
            Collection.Add(entity);
            return Task.CompletedTask;
        }

        public virtual Task<long> Count(Expression<Func<T, bool>> filterConditions, CancellationToken token = default)
        {
            return Task.FromResult<long>(Collection.Count);
        }

        public virtual Task<List<T>> FindMany(Expression<Func<T, bool>> filterConditions, int pageIndex, int pageSize, bool orderDescending = false, Expression<Func<T, object>> orderExpression = null, CancellationToken token = default)
        {
            IEnumerable<T> query = Collection.Where(x => filterConditions.Compile().Invoke(x));
            if (orderExpression != null)
            {
                Func<T, object> orderFunc = orderExpression.Compile();
                if (orderDescending)
                {
                    query = query.OrderByDescending(orderFunc);
                }
                else
                {
                    query = query.OrderBy(orderFunc);
                }
            }

            int skip = pageIndex * pageSize;
            List<T> list = query.Skip(skip)
                .Take(pageSize)
                .ToList();
            return Task.FromResult(list);
        }

        public virtual Task<T> FindOne(Expression<Func<T, bool>> filterConditions, CancellationToken token = default)
        {
            T entity = Collection.Where(x => filterConditions.Compile().Invoke(x)).FirstOrDefault();
            return Task.FromResult(entity);
        }

        public virtual Task<T> FindOneAndDelete(Expression<Func<T, bool>> filterConditions, CancellationToken token = default)
        {
            T entity = Collection.Where(x => filterConditions.Compile().Invoke(x)).FirstOrDefault();
            Collection.Remove(entity);
            return Task.FromResult(entity);
        }

        public virtual Task<T> FindOneAndUpdate(Expression<Func<T, bool>> filterConditions, Updates<T> updates, 
            ReturnDocument returnDocument = ReturnDocument.Before, CancellationToken token = default)
        {
            T entity = Collection.Where(x => filterConditions.Compile().Invoke(x)).FirstOrDefault();

            T returnedEntity = entity;
            if (returnDocument == ReturnDocument.Before)
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, entity);
                    ms.Position = 0;
                    returnedEntity = (T)formatter.Deserialize(ms);
                }
            }

            UpdateField(updates, entity, false);
            return Task.FromResult(returnedEntity);
        }

        public virtual Task<long> UpdateOne(Expression<Func<T, bool>> filterConditions, Updates<T> updates, CancellationToken token = default)
        {
            T entity = Collection.Where(x => filterConditions.Compile().Invoke(x)).FirstOrDefault();
            if(entity != null)
            {
                UpdateField(updates, entity, false);
            }
            long updatedCount = entity == null ? 0 : 1;
            return Task.FromResult<long>(updatedCount);
        }

        public virtual Task<long> UpdateMany(Expression<Func<T, bool>> filterConditions, Updates<T> updates, CancellationToken token = default)
        {
            List<T> entitiesToUpdate = Collection.Where(x => filterConditions.Compile().Invoke(x)).ToList();
            foreach (T entity in entitiesToUpdate)
            {
                UpdateField(updates, entity, false);
            }
            return Task.FromResult<long>(entitiesToUpdate.Count);
        }

        public virtual Task<long> UpdateOne(T entity, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual Task<long> UpdateMany(IEnumerable<T> entities, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual Task<long> UpsertMany(IEnumerable<T> entities, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual Task<long> DeleteMany(Expression<Func<T, bool>> filterConditions, CancellationToken token = default)
        {
            List<T> entitiesToDelete = Collection.Where(x => filterConditions.Compile().Invoke(x)).ToList();
            foreach (var entityToDelete in entitiesToDelete)
            {
                Collection.Remove(entityToDelete);
            }

            return Task.FromResult<long>(entitiesToDelete.Count);
        }

        public virtual Task<long> DeleteOne(Expression<Func<T, bool>> filterConditions, CancellationToken token = default)
        {
            T entity = Collection.Where(x => filterConditions.Compile().Invoke(x)).FirstOrDefault();
            if (entity != null)
            {
                Collection.Remove(entity);
            }
            long deletedCount = entity == null ? 0 : 1;
            return Task.FromResult<long>(deletedCount);
        }

    }
}
