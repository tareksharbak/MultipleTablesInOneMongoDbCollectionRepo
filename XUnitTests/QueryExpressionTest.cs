using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using MongoDbMultiTablesOneCollection;
using MongoDbMultiTablesOneCollection.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTests
{
	public class QueryExpressionTest
	{
		[Fact]
		public async Task Where_QueryingWithIdEquality_ReturnsTheCorrectAbstractedEntity()
		{
			//Arrange
			var students = new List<Student>
			{
				new Student("John Smith") { GPA = 3.75 },
				new Student("Tim Smith") { GPA = 3.65 }
			};

			var collection = new MongoDbCollectionMock<Student>();
			IRepository<Student> repo = new MongoDbRepo<Student>(collection);
			foreach (var student in students)
			{
				await repo.InsertOrUpdateAsync(student);
			}

			//Act
			var studentsFromDb = await repo.WhereAsync(a => a.Id == "John Smith");

			//Assert
			Assert.Single(studentsFromDb);
			Assert.Equal("John Smith", studentsFromDb.Single().Id);
		}

		[Fact]
		public async Task Where_QueryingWithIdStartsWith_ReturnsTheCorrectAbstractedEntity()
		{
			//Arrange
			var students = new List<Student>
			{
				new Student("John Smith") { GPA = 3.75 },
				new Student("Tim Smith") { GPA = 3.65 }
			};

			var collection = new MongoDbCollectionMock<Student>();
			IRepository<Student> repo = new MongoDbRepo<Student>(collection);
			foreach (var student in students)
			{
				await repo.InsertOrUpdateAsync(student);
			}

			//Act
			var studentsFromDb = await repo.WhereAsync(a => a.Id.StartsWith("John"));

			//Assert
			Assert.Single(studentsFromDb);
			Assert.Equal("John Smith", studentsFromDb.Single().Id);
		}

		[Fact]
		public async Task Where_QueryingWithIdEndsWith_ReturnsTheCorrectAbstractedEntity()
		{
			//Arrange
			var students = new List<Student>
			{
				new Student("John Smith") { GPA = 3.75 },
				new Student("Tim Smith") { GPA = 3.65 }
			};

			var collection = new MongoDbCollectionMock<Student>();
			IRepository<Student> repo = new MongoDbRepo<Student>(collection);
			foreach (var student in students)
			{
				await repo.InsertOrUpdateAsync(student);
			}

			//Act
			var studentsFromDb = await repo.WhereAsync(a => a.Id.EndsWith("Smith"));

			//Assert
			Assert.Equal(2, studentsFromDb.Count);
		}

		[Fact]
		public async Task Where_QueryingWithIdCompareTo_ReturnsTheCorrectAbstractedEntity()
		{
			//Arrange
			var students = new List<Student>
			{
				new Student("John Smith") { GPA = 3.75 },
				new Student("Tim Smith") { GPA = 3.65 }
			};

			var collection = new MongoDbCollectionMock<Student>();
			IRepository<Student> repo = new MongoDbRepo<Student>(collection);
			foreach (var student in students)
			{
				await repo.InsertOrUpdateAsync(student);
			}

			//Act
			var studentsFromDb = await repo.WhereAsync(a => a.Id.CompareTo("T") >= 0);

			//Assert
			Assert.Single(studentsFromDb);
			Assert.Equal("Tim Smith", studentsFromDb.Single().Id);
		}
	}
}
