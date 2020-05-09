using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using MongoDB.Bson;

namespace Sanatana.MongoDb.Repository
{
    public class MongoDbRepository<T> : IRepository<T> 
        where T : class
    {
        //fields
        protected IMongoCollection<T> _collection;


        //ctor
        public MongoDbRepository()
        {

        }
        public MongoDbRepository(IMongoCollection<T> collection)
        {
            _collection = collection;
        }



        //shared methods
        protected virtual FilterDefinition<T> ToFilter(Expression<Func<T, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            var visitedCondition = (Expression<Func<T, bool>>)visitor.Visit(filterConditions);
            return Builders<T>.Filter.Where(visitedCondition);
        }

        protected virtual UpdateDefinition<T> ToUpdateDefinition(Updates<T> updates)
        {
            UpdateDefinition<T> updateDefinition = Builders<T>.Update.Combine();
            if(updates.Sets != null)
                foreach (Update<T> update in updates.Sets)
                {
                    updateDefinition = updateDefinition.Set(update.PropertyExpression, update.Value);
                }
            if (updates.Increments != null)
                foreach (Update<T> update in updates.Increments)
                {
                    updateDefinition = updateDefinition.Inc(update.PropertyExpression, update.Value);
                }
            if (updates.SetOnInserts != null)
                foreach (Update<T> update in updates.SetOnInserts)
                {
                    updateDefinition = updateDefinition.SetOnInsert(update.PropertyExpression, update.Value);
                }
            if (updates.Pushes != null)
                foreach (UpdateCollection<T> update in updates.Pushes)
                {
                    var property = new ExpressionFieldDefinition<T>(update.PropertyExpression);
                    updateDefinition = updateDefinition.Push(property, update.Value);
                }
            if (updates.Pulls != null)
                foreach (UpdateCollection<T> update in updates.Pulls)
                {
                    var property = new ExpressionFieldDefinition<T>(update.PropertyExpression);
                    updateDefinition = updateDefinition.Pull(property, update.Value);
                }

            return updateDefinition;
        }



        //methods
        public virtual Task InsertOne(T entity, CancellationToken token = default)
        {
            var options = new InsertOneOptions();
            return _collection.InsertOneAsync(entity, options, cancellationToken: token);
        }

        public virtual async Task<bool> InsertOneHandleDuplicate(T entity, CancellationToken token = default)
        {
            try
            {
                var options = new InsertOneOptions();
                await _collection.InsertOneAsync(entity, options, cancellationToken: token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (MongoDbExceptionInspector.IsDuplicateException(ex))
                {
                    return true;
                };

                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return false;
        }

        public virtual Task InsertMany(IEnumerable<T> entities, CancellationToken token = default)
        {
            return _collection.InsertManyAsync(entities, cancellationToken: token);
        }

        /// <summary>
        /// Count and return the number of results that match a query.
        /// Method without a query predicate the method returns results based on the collection’s metadata, which may result in an approximate count.
        /// On a sharded cluster, the resulting count will not correctly filter out orphaned documents.
        /// After an unclean shutdown, the count may be incorrect.
        /// Run validate on each collection on the mongod to restore the correct statistics after an unclean shutdown.
        /// </summary>
        /// <param name="filterConditions"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual Task<long> CountDocuments(Expression<Func<T, bool>> filterConditions = null, CancellationToken token = default)
        {
            FilterDefinition<T> filter = filterConditions == null
                ? ToFilter(x => true)
                : ToFilter(filterConditions);

            return _collection.CountDocumentsAsync(filter, cancellationToken: token);
        }

        public virtual Task<T> FindOne(Expression<Func<T, bool>> filterConditions, CancellationToken token = default)
        {
            FilterDefinition<T> filter = ToFilter(filterConditions);
            return _collection.Find(filter)
                .Limit(1)
                .FirstOrDefaultAsync(token);
        }

        public virtual Task<T> FindOneAndUpdate(Expression<Func<T, bool>> filterConditions, Updates<T> updates, 
            ReturnDocument returnDocument = ReturnDocument.Before, CancellationToken token = default)
        {
            FilterDefinition<T> filter = ToFilter(filterConditions);
            UpdateDefinition<T> updateDefinition = ToUpdateDefinition(updates);
            var options = new FindOneAndUpdateOptions<T>
            {
                ReturnDocument = returnDocument
            };
            return _collection.FindOneAndUpdateAsync(filter, updateDefinition, options, cancellationToken: token);
        }

        public virtual Task<T> FindOneAndReplace(T entity, bool isUpsert, ReturnDocument returnDocument = ReturnDocument.Before, CancellationToken token = default)
        {
            Func<object, object> idGetter = FieldDefinitions.GetIdFieldGetter(typeof(T));
            ObjectId entityId = (ObjectId)idGetter.Invoke(entity);
            var filter = Builders<T>.Filter.Eq("_id", entityId);

            var options = new FindOneAndReplaceOptions<T>
            {
                ReturnDocument = returnDocument,
                IsUpsert = isUpsert
            };
            return _collection.FindOneAndReplaceAsync(filter, entity, options, cancellationToken: token);
        }

        public virtual Task<T> FindOneAndDelete(Expression<Func<T, bool>> filterConditions, CancellationToken token = default)
        {
            FilterDefinition<T> filter = ToFilter(filterConditions);
            return _collection.FindOneAndDeleteAsync(filter, cancellationToken: token);
        }

        /// <summary>
        /// Select multiple items for specified page using a filter.
        /// </summary>
        /// <param name="filterConditions">Filtering expression</param>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize">Number of items on page. Should be greater than 0.</param>
        /// <param name="orderDescending">Order direction</param>
        /// <param name="orderExpression">Property expression that will be sorted by</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public virtual Task<List<T>> FindMany(Expression<Func<T, bool>> filterConditions, int pageIndex, int pageSize,
            bool orderDescending = false, Expression<Func<T, object>> orderExpression = null, CancellationToken token = default)
        {
            FilterDefinition<T> filter = ToFilter(filterConditions);
            var options = new FindOptions();
            var query = _collection.Find(filter, options);

            if (orderExpression != null)
            {
                if (orderDescending)
                {
                    query = query.SortByDescending(orderExpression);
                }
                else
                {
                    query = query.SortBy(orderExpression);
                }
            }

            int skipItems = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);
            query = query.Skip(skipItems).Limit(pageSize);

            return query.ToListAsync(cancellationToken: token);
        }

        public virtual Task<List<T>> FindAll(Expression<Func<T, bool>> filterConditions = null, CancellationToken token = default)
        {
            var options = new FindOptions();
            var query = filterConditions == null
                ? _collection.Find(x => true, options)
                : _collection.Find(filterConditions, options);
            return query.ToListAsync(cancellationToken: token);
        }

        public virtual async Task<long> UpdateOne(Expression<Func<T, bool>> filterConditions, Updates<T> updates, CancellationToken token = default)
        {
            var filter = Builders<T>.Filter.Where(filterConditions);
            UpdateDefinition<T> update = ToUpdateDefinition(updates);
            UpdateResult result = await _collection.UpdateOneAsync(filter, update, cancellationToken: token)
                .ConfigureAwait(false);
            return result.ModifiedCount;
        }

        public virtual async Task<long> UpdateMany(Expression<Func<T, bool>> filterConditions, Updates<T> updates, CancellationToken token = default)
        {
            FilterDefinition<T> filter = ToFilter(filterConditions);
            UpdateDefinition<T> updateDefinition = ToUpdateDefinition(updates);

            UpdateResult result = await _collection
                .UpdateManyAsync(filter, updateDefinition, cancellationToken: token)
                .ConfigureAwait(false);
            return result.ModifiedCount;
        }

        public virtual async Task<long> ReplaceOne(T entity, bool isUpsert, CancellationToken token = default)
        {
            Func<object, object> idGetter = FieldDefinitions.GetIdFieldGetter(typeof(T));
            ObjectId entityId = (ObjectId)idGetter.Invoke(entity);
            var filter = Builders<T>.Filter.Eq("_id", entityId);

            var options = new ReplaceOptions
            {
                IsUpsert = isUpsert
            };
            ReplaceOneResult result = await _collection.ReplaceOneAsync(filter, entity, options, cancellationToken: token)
                .ConfigureAwait(false);
            return result.ModifiedCount;
        }

        public virtual async Task<long> ReplaceMany(IEnumerable<T> entities, bool isUpsert, CancellationToken token = default)
        {
            if (entities.Count() == 0)
            {
                return 0;
            }

            var requests = new List<WriteModel<T>>();

            Func<object, object> idGetter = FieldDefinitions.GetIdFieldGetter(typeof(T));
            foreach (T entity in entities)
            {
                ObjectId entityId = (ObjectId)idGetter.Invoke(entity);
                var filter = Builders<T>.Filter.Eq("_id", entityId);
                requests.Add(new ReplaceOneModel<T>(filter, entity)
                {
                    IsUpsert = isUpsert
                });
            }

            // BulkWrite
            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult<T> bulkResult = await _collection
                .BulkWriteAsync(requests, options, cancellationToken: token)
                .ConfigureAwait(false);

            return bulkResult.Upserts.Count + bulkResult.ModifiedCount;
        }

        public virtual async Task<long> DeleteOne(Expression<Func<T, bool>> filterConditions, CancellationToken token = default)
        {
            FilterDefinition<T> filter = ToFilter(filterConditions);
            DeleteResult result = await _collection.DeleteOneAsync(filter, cancellationToken: token)
                .ConfigureAwait(false);
            return result.DeletedCount;
        }

        public virtual async Task<long> DeleteMany(Expression<Func<T, bool>> filterConditions, CancellationToken token = default)
        {
            FilterDefinition<T> filter = ToFilter(filterConditions);
            DeleteResult result = await _collection.DeleteManyAsync(filter, cancellationToken: token)
                .ConfigureAwait(false);
            return result.DeletedCount;
        }


    }
}
