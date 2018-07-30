using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbMultiTablesOneCollection.Entities
{
	public class Student : DbDocument
	{
		//The id of the student will be the full name
		public Student(string fullName) : base(fullName)
		{
			FullName = fullName;
		}

		public string FullName { get; set; }
		public double GPA { get; set; }
	}
}
