using MongoDbMultiTablesOneCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace XUnitTests
{
	public class MongoDbCollectionMock<T> : IMongoDbCollection<T>
	{
		private readonly List<T> entities = new List<T>();

		public async Task<long> DeleteManyAsync(Expression<Func<T, bool>> filter)
		{
			var items = await FindAsync(filter);
			foreach (var item in items)
			{
				entities.Remove(item);
			}
			return items.Count;
		}

		public async Task<bool> DeleteOneAsync(Expression<Func<T, bool>> filter)
		{
			var items = await FindAsync(filter);
			var item = items.SingleOrDefault();
			if (item != null)
			{
				entities.Remove(item);
				return true;
			}
			return false;
		}

		public Task<List<T>> FindAsync(Expression<Func<T, bool>> filter)
		{
			//Copy the entities to make sure that the changes that are applied to the objects are not affected in the list of entities
			//This only works in the cotext of this test
			List<T> results = new List<T>();
			var items = entities.Where(filter.Compile()).ToList();
			foreach (var item in items)
			{
				var type = typeof(T);
				var constructor = type.GetConstructor(new Type[] { typeof(string) });
				var newDocument = (T)constructor.Invoke(new object[] { "" });

				foreach (var property in type.GetProperties())
				{
					property.SetValue(newDocument, property.GetValue(item));
				}
				results.Add(newDocument);
			}
			return Task.FromResult(results);
		}

		public Task InsertOneAsync(T document)
		{
			//Copy the entity to make sure that the changes that are applied to the object are not affected in the list of entities
			//This only works in the cotext of this test
			var type = typeof(T);
			var constructor = type.GetConstructor(new Type[] { typeof(string) });
			var newDocument = (T)constructor.Invoke(new object[] { "" });

			foreach (var property in type.GetProperties())
			{
				property.SetValue(newDocument, property.GetValue(document));
			}

			entities.Add(newDocument);
			return Task.CompletedTask;
		}

		public async Task ReplaceOneAsync(Expression<Func<T, bool>> filter, T replacement)
		{
			var entities = await FindAsync(filter);
			var item = entities.SingleOrDefault();
			entities.Remove(item);
			await InsertOneAsync(replacement);
		}
	}
}
