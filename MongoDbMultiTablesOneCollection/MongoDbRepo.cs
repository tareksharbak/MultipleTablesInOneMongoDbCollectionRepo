///Author: Tarek Sharbak

using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

namespace MongoDbMultiTablesOneCollection
{
	public class MongoDbRepo<T> : IRepository<T> where T : IDbDocument
	{
		private readonly IMongoDbCollection<T> collection;

		//The name of the table to prefix all entities
		private static readonly string tablePrefix = typeof(T).Name + "|";
		//Adds 1 to the integer value of the last character of tablePrefix, use this to be able to query the full range of the table
		private static readonly string tablePrefixUpperBound = tablePrefix.Remove(tablePrefix.Length - 1, 1) + (char)(tablePrefix[tablePrefix.Length - 1] + 1);

		public MongoDbRepo(IMongoDbCollection<T> collection)
		{
			this.collection = collection;
		}

		/// <summary>
		/// Gets the entity by the Id
		/// </summary>
		/// <param name="id">The Id of the entity</param>
		/// <returns></returns>
		public async Task<T> GetByIdAsync(string id)
		{
			//First append the table name to the Id to find the corresponding entity
			id = AppendTableNameToId(id);

			//Find the item by Id
			var result = await collection.FindAsync(a => a.Id == id);
			var item = result.SingleOrDefault();

			if (item != null)
			{
				//We have to remove the table name from the Id of the returned item to keep the abstraction.
				item.Id = RemoveTableNameFromId(item.Id);
			}

			return item;
		}

		/// <summary>
		/// Gets all entities in the table
		/// </summary>
		/// <returns></returns>
		public async Task<List<T>> GetAllAsync()
		{
			//Query all entities that are prefixed by the table name
			var result = await collection.FindAsync(a => a.Id.CompareTo(tablePrefix) >= 0 && a.Id.CompareTo(tablePrefixUpperBound) < 0);

			//We have to remove the table name from the Ids of the returned items to keep the abstraction.
			result.ForEach(a => a.Id = RemoveTableNameFromId(a.Id));

			return result;
		}

		/// <summary>
		/// Gets all entities that match the expression
		/// </summary>
		/// <param name="exp">The query expression</param>
		/// <returns></returns>
		public async Task<List<T>> WhereAsync(Expression<Func<T, bool>> exp)
		{
			//Adds a filter to only query the entities that belong to this table.
			//In case the user included some queries on the Id field of the entities, they will also be modified to include the table prefix.
			//Since all entities from all tables live on the same collection, we don't want to run the query over the entire collection.
			var newExp = AddGlobalTableFilters(exp);

			var result = await collection.FindAsync(newExp);

			//We have to remove the table name from the Ids of the returned items to keep the abstraction.
			result.ForEach(a => a.Id = RemoveTableNameFromId(a.Id));

			return result;
		}

		/// <summary>
		/// Inserts or updates an item.
		/// The item will be updated if there already exists an item with the same Id.
		/// </summary>
		/// <param name="newItem">The item to insert or update</param>
		/// <returns></returns>
		public async Task<T> InsertOrUpdateAsync(T newItem)
		{
			//First append the table name to the Id.
			newItem.Id = AppendTableNameToId(newItem.Id);

			T item = await GetByIdAsync(newItem.Id);
			//If there are no items in the collection with the same Id, insert the new item.
			if (item == null)
			{
				await collection.InsertOneAsync(newItem);
				//We have to remove the table name that we added from the Id of the returned item to keep the abstraction.
				newItem.Id = RemoveTableNameFromId(newItem.Id);
				return newItem;
			}
			//Otherwise, replace the current item.
			else
			{
				await collection.ReplaceOneAsync(a => a.Id == newItem.Id, newItem);
				//We have to remove the table name that we added from the Id of the returned item to keep the abstraction.
				newItem.Id = RemoveTableNameFromId(newItem.Id);
				return newItem;
			}
		}

		/// <summary>
		/// Deletes an entity by its Id
		/// </summary>
		/// <param name="id">The Id of the entity to delete</param>
		/// <returns></returns>
		public async Task<bool> DeleteAsync(string id)
		{
			//First append the table name to the key to find the corresponding entity
			id = AppendTableNameToId(id);

			//Deletes the entity that matches the expression
			return await collection.DeleteOneAsync(a => a.Id == id);
		}

		/// <summary>
		/// Deletes the entity
		/// </summary>
		/// <param name="item">The entity to be deleted</param>
		/// <returns></returns>
		public async Task<bool> DeleteAsync(T item)
		{
			//First append the table name to the Id to find the corresponding entity
			var id = AppendTableNameToId(item.Id);

			return await DeleteAsync(id);
		}

		/// <summary>
		/// Deletes all entities in the table
		/// </summary>
		/// <returns>The number of deleted entities</returns>
		public async Task<long> DeleteAllAsync()
		{
			//Deletes all the entities that are prefixed with the table name.
			return await collection.DeleteManyAsync(a => a.Id.CompareTo(tablePrefix) >= 0 && a.Id.CompareTo(tablePrefixUpperBound) < 0);
		}

		private string AppendTableNameToId(string id)
		{
			//Only append the table name if it doesn't already exist.
			//In case the method is called twice.
			if (!id.StartsWith(tablePrefix))
				id = tablePrefix + id;

			return id;
		}

		private string RemoveTableNameFromId(string id)
		{
			//Remove the table prefix if it exists.
			if (id.StartsWith(tablePrefix))
				id = id.Remove(0, tablePrefix.Length);

			return id;
		}

		private Expression<Func<T, bool>> AddGlobalTableFilters(Expression<Func<T, bool>> exp)
		{
			//Checks if the expression has queries over the Id field, and modifies them by adding the table prefix to the values.
			var idModifierVisitor = new ModifyIdVisitor();
			exp = idModifierVisitor.Visit(exp) as Expression<Func<T, bool>>;
			
			//Creates a new expression that restrict the query to only run over the entities that belong to this table
			Expression<Func<T, bool>> newExp = a => a.Id.CompareTo(tablePrefix) >= 0 && a.Id.CompareTo(tablePrefixUpperBound) < 0;

			//Updates the new expression to match the parameter of the original expression, so that they can be added together later.
			var visitor = new ParameterUpdateVisitor(newExp.Parameters.First(), exp.Parameters.First());
			newExp = visitor.Visit(newExp) as Expression<Func<T, bool>>;

			//Logical AND between the new expression and the old expression.
			var binExp = Expression.And(newExp.Body, exp.Body);
			
			//Returns the composit expression.
			return Expression.Lambda<Func<T, bool>>(binExp, newExp.Parameters);
		}

		
		/// <summary>
		/// Replaces the old parameter with a new one in an expression
		/// </summary>
		private class ParameterUpdateVisitor : ExpressionVisitor
		{
			private readonly ParameterExpression oldParameter;
			private readonly ParameterExpression newParameter;

			public ParameterUpdateVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
			{
				this.oldParameter = oldParameter;
				this.newParameter = newParameter;
			}

			/// <summary>
			/// Will be called for each parameter in the expression.
			/// Will replace all old parameters with a new parameters
			/// </summary>
			/// <param name="node"></param>
			/// <returns></returns>
			protected override Expression VisitParameter(ParameterExpression node)
			{
				if (ReferenceEquals(node, oldParameter))
					return newParameter;

				return base.VisitParameter(node);
			}
		}

		/// <summary>
		/// Checks the expressions where the left side is the member 'Id' to pass it to the IdValueUpdateVisitor
		/// </summary>
		private class ModifyIdVisitor : ExpressionVisitor
		{
			/// <summary>
			/// Will be called when the expression is a binary expression. E.g. Id == "value" or Id.CompareTo("value") > 0
			/// </summary>
			/// <param name="node"></param>
			/// <returns></returns>
			protected override Expression VisitBinary(BinaryExpression node)
			{
				MemberExpression propertyExpression = null;

				//E.g. Id.CompareTo("value") > 0
				if (node.Left is MethodCallExpression methodCallExpression)
				{
					propertyExpression = methodCallExpression.Object as MemberExpression;
					
				}
				//E.g. Id == "value" 
				else if (node.Left is MemberExpression)
				{
					propertyExpression = node.Left as MemberExpression;
				}

				//Checks if the member of the expression is the field 'Id'
				if (propertyExpression?.Member?.Name == "Id")
				{
					//Pass the expression the IdValueUpdateVisitor to add the table prefix to the expression constant
					var visitor = new IdValueUpdateVisitor();
					node = visitor.Visit(node) as BinaryExpression;
				}

				return base.VisitBinary(node);
			}

			/// <summary>
			/// Will be called when the expression is a method call (A method that returns a boolean in our particular case)
			/// E.g. Id.StartsWith("value") or Id.Contains("value")
			/// </summary>
			/// <param name="node"></param>
			/// <returns></returns>
			protected override Expression VisitMethodCall(MethodCallExpression node)
			{
				//We only want to modify the expression value if the method is 'StartsWith',
				//because the entities in the collection are all prefixed with the table name
				if(node.Method.Name == "StartsWith")
				{
					var visitor = new IdValueUpdateVisitor();
					node = visitor.Visit(node) as MethodCallExpression;
				}

				return base.VisitMethodCall(node);
			}
		}

		/// <summary>
		/// Adds the table prefix to the constant of the expression
		/// </summary>
		private class IdValueUpdateVisitor : ExpressionVisitor
		{
			protected override Expression VisitConstant(ConstantExpression node)
			{
				//Since the Id is always a string, we only want to change the constant of type 'string'.
				if (node.Type == typeof(string))
				{
					//Appends the table prefix if it doesn't already exist.
					var newConstant = node.Value.ToString();
					if (!newConstant.StartsWith(tablePrefix))
						newConstant = tablePrefix + newConstant;

					//Creates a new constant expression with the new value.
					node = Expression.Constant(newConstant);
				}

				return base.VisitConstant(node);
			}
		}
	}
}