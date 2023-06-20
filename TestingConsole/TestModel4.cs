using NitroBrew.Attributes;
using NitroBrew.Attributes.StoredProcActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingConsole
{
    [GetStoredProc("SelectTestModel4")]
    internal class TestModel4
    {
        [EntityKey]
        public int TestModel4Id { get; set; }

        public string Name { get; set; }
    }
}
