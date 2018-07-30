using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoDbMultiTablesOneCollection
{
	//A wrapper to make unit testing the repository possible.
	public interface IMongoDbCollection<T>
	{
		Task<long> DeleteManyAsync(Expression<Func<T, bool>> filter);
		Task<bool> DeleteOneAsync(Expression<Func<T, bool>> filter);
		Task<List<T>> FindAsync(Expression<Func<T, bool>> filter);
		Task InsertOneAsync(T document);
		Task ReplaceOneAsync(Expression<Func<T, bool>> filter, T replacement);
	}
}