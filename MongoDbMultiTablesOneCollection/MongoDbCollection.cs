using MongoDB.Driver;
using MongoDbMultiTablesOneCollection.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbMultiTablesOneCollection
{
	//A wrapper to make unit testing the repository possible.
	public class MongoDbCollection<T> : IMongoDbCollection<T>
	{
		private readonly IMongoCollection<T> collection;

		public MongoDbCollection(string connectionString, string dbName)
		{
			//Create a MongoClient from the connectionString
			var mongoClient = new MongoClient(connectionString);

			//Use the database name for both the database name and collection name
			//Get the database by name, or create one if it doesn't already exist
			var mongoDatabase = mongoClient.GetDatabase(dbName);
			//Get the collection by name, or create one if it doesn't already exist
			collection = mongoDatabase.GetCollection<T>(dbName);
		}

		public MongoDbCollection(IMongoCollection<T> collection)
		{
			this.collection = collection;
		}

		public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter)
		{
			return (await collection.FindAsync(filter)).ToList();
		}

		public async Task InsertOneAsync(T document)
		{
			await collection.InsertOneAsync(document);
		}

		public async Task ReplaceOneAsync(Expression<Func<T, bool>> filter, T replacement)
		{
			await collection.ReplaceOneAsync(filter, replacement);
		}

		public async Task<bool> DeleteOneAsync(Expression<Func<T, bool>> filter)
		{
			var result = await collection.DeleteOneAsync(filter);

			return result.IsAcknowledged & result.DeletedCount == 1;
		}

		public async Task<long> DeleteManyAsync(Expression<Func<T, bool>> filter)
		{
			var result = await collection.DeleteManyAsync(filter);

			return result.DeletedCount;
		}
	}
}
