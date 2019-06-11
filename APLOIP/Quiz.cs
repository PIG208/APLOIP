using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APLOIP
{
    public class Quiz
    {
        public string UniqueTitle { get; set; }
        public string DisplayTitle { get; set; }
        public List<IEntity> Entities { get; set; } = new List<IEntity>();
    }
}
