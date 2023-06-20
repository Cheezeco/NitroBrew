using NitroBrew.Attributes;
using NitroBrew.Attributes.StoredProcActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingConsole
{
    [GetStoredProc("SelectTestModel")]
    [GetAllStoredProc("GetAllTestModel")]
    [InsertStoredProc("InsertTestModel")]
    [UpdateStoredProc("UpdateTestModel")]
    [DeleteStoredProc("DeleteTestModel")]
    internal class TestModel
    {
        [EntityKey]
        public int TestModelId { get; set; }

        //[IdForEntity(nameof(TestModel2))]
        //public int TestModel2Id { get; set; }

        //[BridgeTableProc("TestBridgeProc")]
        [OneToManyProc("TestModelOneToManyProc")]
        public List<TestModel2> TestModel2 { get; set; }

        [IdForEntity(nameof(TestModel4))]
        public int TestModel4Id { get; set; }

        public TestModel4 TestModel4 { get; set; }

        public string Name { get; set; }
    }

    public class Custom
    {

    }
}
