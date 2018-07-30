using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoDbMultiTablesOneCollection
{
	public interface IRepository<T> where T : IDbDocument
	{
		/// <summary>
		/// Gets the entity by the Id
		/// </summary>
		/// <param name="id">The Id of the entity</param>
		/// <returns></returns>
		Task<T> GetByIdAsync(string id);
		/// <summary>
		/// Gets all entities in the table
		/// </summary>
		/// <returns></returns>
		Task<List<T>> GetAllAsync();
		/// <summary>
		/// Gets all entities that match the expression
		/// </summary>
		/// <param name="exp">The query expression</param>
		/// <returns></returns>
		Task<List<T>> WhereAsync(Expression<Func<T, bool>> exp);
		/// <summary>
		/// Inserts or updates an item.
		/// The item will be updated if there already exists an item with the same Id.
		/// </summary>
		/// <param name="newItem">The item to insert or update</param>
		/// <returns></returns>
		Task<T> InsertOrUpdateAsync(T newItem);
		/// <summary>
		/// Deletes an entity by its Id
		/// </summary>
		/// <param name="id">The Id of the entity to delete</param>
		/// <returns></returns>
		Task<bool> DeleteAsync(string id);
		/// <summary>
		/// Deletes the entity
		/// </summary>
		/// <param name="item">The entity to be deleted</param>
		/// <returns></returns>
		Task<bool> DeleteAsync(T item);
		/// <summary>
		/// Deletes all entities in the table
		/// </summary>
		/// <returns>The number of deleted entities</returns>
		Task<long> DeleteAllAsync();
	}
}