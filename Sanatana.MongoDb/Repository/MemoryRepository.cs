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
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

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

        public virtual Task<bool> InsertOneHandleDuplicate(T entity, CancellationToken token = default)
        {
            bool isDuplicate = Collection.Contains(entity);
            if (!isDuplicate)
            {
                Collection.Add(entity);
            };
            return Task.FromResult(isDuplicate);

        }

        public virtual Task<long> CountDocuments(Expression<Func<T, bool>> filterConditions = null, CancellationToken token = default)
        {
            if(filterConditions == null)
            {
                filterConditions = x => true;
            }

            long count = Collection
                .Where(filterConditions.Compile())
                .LongCount();
            return Task.FromResult<long>(count);
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

        public virtual Task<List<T>> FindAll(Expression<Func<T, bool>> filterConditions = null, CancellationToken token = default)
        {
            return Task.FromResult(Collection.ToList());
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
                //Could clone it here if required with AutoMapper or BinarySerializer.
                //However BinarySerializer does not serialize ObjectId.
                returnedEntity = entity;
            }

            UpdateField(updates, entity, false);
            return Task.FromResult(returnedEntity);
        }

        public async Task<T> FindOneAndReplace(T entity, bool isUpsert, ReturnDocument returnDocument = ReturnDocument.Before, CancellationToken token = default)
        {
            Func<object, object> idGetter = FieldDefinitions.GetIdFieldGetter(typeof(T));
            ObjectId entityId = (ObjectId)idGetter.Invoke(entity);
            T collectionEntity = Collection
                .Where(x => (ObjectId)idGetter.Invoke(x) == entityId)
                .FirstOrDefault();

            T returnedEntity = collectionEntity;
            if (returnDocument == ReturnDocument.Before)
            {
                //Could clone it here if required with AutoMapper or BinarySerializer.
                //However BinarySerializer does not serialize ObjectId.
                returnedEntity = collectionEntity;
            }

            await ReplaceOne(entity, isUpsert, token);

            return returnedEntity;
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

        public Task<long> ReplaceOne(T entity, bool isUpsert, CancellationToken token = default)
        {
            Func<object, object> idGetter = FieldDefinitions.GetIdFieldGetter(typeof(T));
            ObjectId entityId = (ObjectId)idGetter.Invoke(entity);

            T entityToReplace = Collection.Find(x => (ObjectId)idGetter.Invoke(x) == entityId);

            long replaced = 0;
            bool doReplace = isUpsert || entityToReplace != null;
            if (doReplace)
            {
                replaced = Collection.RemoveAll(x => (ObjectId)idGetter.Invoke(x) == entityId);
                Collection.Add(entity);
            }

            return Task.FromResult(replaced);
        }

        public async Task<long> ReplaceMany(IEnumerable<T> entities, bool isUpsert, CancellationToken token = default)
        {
            long replaced = 0;
            foreach (T item in entities)
            {
                replaced += await ReplaceOne(item, isUpsert);
            }

            return replaced;
        }

    }
}
