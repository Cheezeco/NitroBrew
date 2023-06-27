using Microsoft.Data.SqlClient;
using NitroBrew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cache = new Cache();

            var repository =
                new Repository(
                    "Data Source=localhost\\SQLExpress;Database=testdb;Integrated Security=sspi;Trust Server Certificate=True",
                    cache);

            //TestGet(repository);
            //TestInsert(repository);
            //TestUpdate(repository);
            //TestGetAll(repository);
            //TestGetAllInclude(repository);
            //TestDelete(repository);
            // TestLoadIncludes(repository);
            TestIncludeEnumerable();

            Console.ReadKey();
        }

        private static void TestGet(Repository repository)
        {
            var includeBuilder = new IncludeBuilder<TestModel>();

            includeBuilder
                .Include(x => x.TestModel4)
                .Include(x => x.TestModel2);

            var result = repository.Get(1, includeBuilder);

            Console.ReadKey();
        }

        private static void TestInsert(Repository repository)
        {
            var testModel = new TestModel()
            {
                Name = "TestingInsert"
            };

            repository.Insert(testModel);
        }

        private static void TestUpdate(Repository repository)
        {
            var entity = repository.Get<TestModel>(3);

            entity.Name = "RenamedTestModel";

            repository.Update(entity);
        }

        private static void TestDelete(Repository repository)
        {
            repository.Delete<TestModel>(3);
        }

        private static void TestGetAll(Repository repository)
        {
            var result = repository.GetAll<TestModel>();

            Console.ReadKey();
        }

        private static void TestGetAllInclude(Repository repository)
        {
            var includeBuilder = new IncludeBuilder<TestModel>();

            includeBuilder
                .Include(x => x.TestModel4)
                .Include(x => x.TestModel2);
            var result = repository.GetAll<TestModel>(includeBuilder);

            Console.ReadKey();
        }

        private static void TestLoadIncludes(Repository repository)
        {
            var entity = repository.Get<TestModel>(1);

            var includeBuilder = new IncludeBuilder<TestModel>();

            includeBuilder
                .Include(x => x.TestModel4)
                .Include(x => x.TestModel2);

            repository.LoadIncludes(entity, includeBuilder);

            Console.ReadKey();
        }

        private static void TestIncludeEnumerable()
        {
            var cache = new Cache
            {
                ItemLifeSpan = TimeSpan.MaxValue
            };

            var enumerable = new List<TestModel>();

            cache.Add(0, enumerable);

            var test = cache.GetEnumerable<TestModel>(0);

            Console.ReadKey();
        }
    }
}