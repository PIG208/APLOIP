﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APLOIP
{
    public class Entry
    {
        public string DisplayTitle { get; set; }
        public string UniqueTitle { get; set; }
        public string PageContent { get; set; }
        public int BasicClassID { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ModificationTime { get; set; }
    }
}
