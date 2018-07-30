using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MongoDbMultiTablesOneCollection.Entities
{
	public class DbDocument : IDbDocument
	{
		public string Id { get; set; }

		/// <summary>
		/// Create a new DbDocument with Id
		/// </summary>
		/// <param name="id">The Id</param>
		public DbDocument(string id)
		{
			Id = id;
		}

		/// <summary>
		/// Create a new DbDocument with an auto-generated Id
		/// </summary>
		public DbDocument()
		{
			Id = Guid.NewGuid().ToString().ToUpper();
		}
	}
}