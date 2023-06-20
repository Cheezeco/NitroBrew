﻿using NitroBrew.Attributes;
using NitroBrew.Attributes.StoredProcActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingConsole
{
    [GetStoredProc("SelectTestModel3")]
    internal class TestModel3
    {
        [EntityKey]
        public int TestModel3Id { get; set; }

        public string Name { get; set; }
    }
}
