using System.Collections.Generic;

namespace APLOIP
{
    public class Quiz
    {
        public string UniqueTitle { get; set; }
        public string DisplayTitle { get; set; }
        public List<IEntity> Entities { get; set; } = new List<IEntity>();
    }
}
