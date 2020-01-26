using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.MongoDb.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<long> CountDocuments(Expression<Func<T, bool>> filterConditions, CancellationToken token = default);
        Task<long> EstimatedDocumentCount(CancellationToken token = default);
        Task<long> DeleteMany(Expression<Func<T, bool>> filterConditions, CancellationToken token = default);
        Task<long> DeleteOne(Expression<Func<T, bool>> filterConditions, CancellationToken token = default);
        Task<List<T>> FindMany(Expression<Func<T, bool>> filterConditions, int pageIndex, int pageSize, bool orderDescending = false, Expression<Func<T, object>> orderExpression = null, CancellationToken token = default);
        Task<T> FindOne(Expression<Func<T, bool>> filterConditions, CancellationToken token = default);
        Task<T> FindOneAndDelete(Expression<Func<T, bool>> filterConditions, CancellationToken token = default);
        Task<T> FindOneAndUpdate(Expression<Func<T, bool>> filterConditions, Updates<T> updates, ReturnDocument returnDocument = ReturnDocument.Before, CancellationToken token = default);
        Task<T> FindOneAndReplace(T entity, bool isUpsert, ReturnDocument returnDocument = ReturnDocument.Before, CancellationToken token = default);
        Task InsertMany(IEnumerable<T> entities, CancellationToken token = default);
        Task InsertOne(T entity, CancellationToken token = default);
        Task<long> UpdateOne(Expression<Func<T, bool>> filterConditions, Updates<T> updates, CancellationToken token = default);
        Task<long> UpdateMany(Expression<Func<T, bool>> filterConditions, Updates<T> updates, CancellationToken token = default);
        Task<long> ReplaceOne(T entity, bool isUpsert, CancellationToken token = default);
        Task<long> ReplaceMany(IEnumerable<T> entities, bool isUpsert, CancellationToken token = default);
    }
}