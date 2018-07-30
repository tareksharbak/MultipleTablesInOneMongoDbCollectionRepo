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
	public class RepositoryTest
	{
		[Fact]
		public async Task InsertDocument_InsertsThePrefixedId()
		{
			//Arrange
			string fullName = "John Smith";
			double gpa = 3.75;

			var student = new Student(fullName) { GPA = gpa };

			string insertedId = null;

			var collectionMock = new Mock<IMongoDbCollection<Student>>();

			collectionMock.Setup(a => a.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
				.ReturnsAsync(new List<Student>());

			collectionMock.Setup(a => a.InsertOneAsync(It.IsAny<Student>()))
				.Callback<Student>(s => insertedId = s.Id)
				.Returns(Task.CompletedTask);

			IRepository<Student> repo = new MongoDbRepo<Student>(collectionMock.Object);

			//Act
			await repo.InsertOrUpdateAsync(student);

			//Assert
			collectionMock.Verify(a => a.InsertOneAsync(It.IsAny<Student>()));
			Assert.Equal(nameof(Student) + "|" + fullName, insertedId);
		}

		[Fact]
		public async Task InsertDocument_ReturnsAbstractedEntityWithoutPrefix()
		{
			//Arrange
			string fullName = "John Smith";
			double gpa = 3.75;

			var student = new Student(fullName) { GPA = gpa };

			var collectionMock = new Mock<IMongoDbCollection<Student>>();

			collectionMock.Setup(a => a.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
				.ReturnsAsync(new List<Student>());

			IRepository<Student> repo = new MongoDbRepo<Student>(collectionMock.Object);

			//Act
			var inserted = await repo.InsertOrUpdateAsync(student);

			//Assert
			collectionMock.Verify(a => a.InsertOneAsync(It.IsAny<Student>()));
			Assert.Equal(fullName, inserted.Id);
		}

		[Fact]
		public async Task ReplaceDocument_ReplacesTheEntityWithThePrefixedId()
		{
			//Arrange
			string fullName = "John Smith";
			double gpa = 3.75;

			var student = new Student(fullName) { GPA = gpa };

			Student updatedStudent = null;
			string updatedStudentId = null;

			var collectionMock = new Mock<IMongoDbCollection<Student>>();

			collectionMock.Setup(a => a.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
				.ReturnsAsync(new List<Student> { new Student(fullName) { GPA = gpa, Id = nameof(Student) + "|" + fullName } });

			collectionMock.Setup(a => a.ReplaceOneAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<Student>()))
				.Callback<Expression<Func<Student, bool>>, Student>((f, r) =>
				{
					updatedStudent = r;
					updatedStudentId = r.Id;
				})
				.Returns(Task.CompletedTask);

			IRepository<Student> repo = new MongoDbRepo<Student>(collectionMock.Object);

			//Act
			await repo.InsertOrUpdateAsync(student);

			//Assert
			collectionMock.Verify(a => a.ReplaceOneAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<Student>()));
			Assert.Equal(gpa, updatedStudent.GPA);
			Assert.Equal(nameof(Student) + "|" + fullName, updatedStudentId);
		}

		[Fact]
		public async Task ReplaceDocument_ReturnsAbstractedEntityWithoutPrefix()
		{
			//Arrange
			string fullName = "John Smith";
			double gpa = 3.75;

			var student = new Student(fullName) { GPA = gpa };

			var collectionMock = new Mock<IMongoDbCollection<Student>>();

			collectionMock.Setup(a => a.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
				.ReturnsAsync(new List<Student> { new Student(fullName) { GPA = gpa, Id = nameof(Student) + "|" + fullName } });

			IRepository<Student> repo = new MongoDbRepo<Student>(collectionMock.Object);

			//Act
			var updated = await repo.InsertOrUpdateAsync(student);

			//Assert
			collectionMock.Verify(a => a.ReplaceOneAsync(It.IsAny<Expression<Func<Student, bool>>>(), It.IsAny<Student>()));
			Assert.Equal(fullName, updated.Id);
		}

		[Fact]
		public async Task GetDocumentById_GetsTheDocument()
		{
			//Arrange
			string fullName = "John Smith";
			double gpa = 3.75;

			var student = new Student(fullName) { GPA = gpa };

			var collectionMock = new Mock<IMongoDbCollection<Student>>();

			collectionMock.Setup(a => a.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
				.ReturnsAsync(new List<Student> { new Student(fullName) { GPA = gpa, Id = nameof(Student) + "|" + fullName } });

			IRepository<Student> repo = new MongoDbRepo<Student>(collectionMock.Object);

			//Act
			var studentFromDb = await repo.GetByIdAsync(fullName);

			//Assert
			Assert.NotNull(studentFromDb);
			Assert.Equal(student.Id, studentFromDb.Id);
		}

		[Fact]
		public async Task GetDocumentById_ReturnsAbstractedEntityWithoutPrefix()
		{
			//Arrange
			string fullName = "John Smith";
			double gpa = 3.75;

			var student = new Student(fullName) { GPA = gpa };

			var collectionMock = new Mock<IMongoDbCollection<Student>>();

			collectionMock.Setup(a => a.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
				.ReturnsAsync(new List<Student> { new Student(fullName) { GPA = gpa, Id = nameof(Student) + "|" + fullName } });

			IRepository<Student> repo = new MongoDbRepo<Student>(collectionMock.Object);

			//Act
			var studentFromDb = await repo.GetByIdAsync(fullName);

			//Assert
			Assert.NotNull(studentFromDb);
			Assert.Equal(fullName, studentFromDb.Id);
		}

		[Fact]
		public async Task GetDocumentById_ReturnsNullIfEntityNotFound()
		{
			//Arrange
			string fullName = "John Smith";
			double gpa = 3.75;

			var student = new Student(fullName) { GPA = gpa };

			var mongoDbCollectionMock = new Mock<IMongoDbCollection<Student>>();

			mongoDbCollectionMock.Setup(a => a.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
				.ReturnsAsync(new List<Student>());

			IRepository<Student> repo = new MongoDbRepo<Student>(mongoDbCollectionMock.Object);

			//Act
			var studentFromDb = await repo.GetByIdAsync(fullName);

			//Assert
			Assert.Null(studentFromDb);
		}

		[Fact]
		public async Task GetAllDocuments_GetsAllTheDocuments()
		{
			//Arrange
			var students = new List<Student>
			{
				new Student("John Smith") { GPA = 3.75, Id = nameof(Student) + "|" + "John Smith" },
				new Student("Tim Smith") { GPA = 3.65, Id = nameof(Student) + "|" + "Tim Smith" }
			};

			var mongoDbCollectionMock = new Mock<IMongoDbCollection<Student>>();

			mongoDbCollectionMock.Setup(a => a.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
				.ReturnsAsync(students);

			IRepository<Student> repo = new MongoDbRepo<Student>(mongoDbCollectionMock.Object);

			//Act
			var studentsFromDb = await repo.GetAllAsync();

			//Assert
			Assert.Equal(2, studentsFromDb.Count);
		}

		[Fact]
		public async Task GetAllDocuments_ReturnsAbstractedEntitiesWithoutTablePrefix()
		{
			//Arrange
			var students = new List<Student>
			{
				new Student("John Smith") { GPA = 3.75, Id = nameof(Student) + "|" + "John Smith" },
				new Student("Tim Smith") { GPA = 3.65, Id = nameof(Student) + "|" + "Tim Smith" }
			};

			var mongoDbCollectionMock = new Mock<IMongoDbCollection<Student>>();

			mongoDbCollectionMock.Setup(a => a.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
				.ReturnsAsync(students);

			IRepository<Student> repo = new MongoDbRepo<Student>(mongoDbCollectionMock.Object);

			//Act
			var studentsFromDb = await repo.GetAllAsync();

			//Assert
			Assert.Equal(2, studentsFromDb.Count);
			Assert.True(studentsFromDb.All(a => !a.Id.StartsWith(nameof(Student))));
		}

		[Fact]
		public async Task Delete_DeletesTheEntity()
		{
			//Arrange
			var student = new Student("John Smith") { GPA = 3.75 };
			var collectionMock = new Mock<IMongoDbCollection<Student>>();
			IRepository<Student> repo = new MongoDbRepo<Student>(collectionMock.Object);

			//Act
			var isDeleted = await repo.DeleteAsync(student);

			//Assert
			collectionMock.Verify(a => a.DeleteOneAsync(It.IsAny<Expression<Func<Student, bool>>>()));
		}
	}
}
