using NitroBrew.Attributes;
using NitroBrew.Attributes.StoredProcActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingConsole
{
    [GetStoredProc("SelectTestModel2")]
    internal class TestModel2
    {
        [EntityKey]
        public int TestModel2Id { get; set; }

        [IdForEntity(nameof(TestModel3))]
        public int TestModel3Id { get; set; }

        //public List<TestModel> TestModels { get; set; }

        public TestModel3 TestModel3 { get; set; }

        public string Name { get; set; }
    }
}
