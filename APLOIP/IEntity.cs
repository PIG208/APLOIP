using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace APLOIP
{
    public interface IEntity
    {
        string Info { get; set; }
        string Answer { get; set; }
        string Type { get; set; }
        int Index { get; set; }
        string SerializeEntity();
    }

    public class McqEntity : IEntity
    {
        public string Info { get; set; }
        public string Answer { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }
        public string SerializeEntity()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<string> values = Info.Split('|').ToList();
            if(values.Count > 0)
            {
                result.Add("title", values?[0]);
                result.Add("content", values?[1]);
                result.Add("choices", values.Skip(2));
            }
            values = Answer.Split('|').ToList();
            result.Add("answer", int.Parse(values[0]));
            return JsonConvert.SerializeObject(result);
        }
    }
    public class DragEntity : IEntity
    {
        public string Info { get; set; }
        public string Answer { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }
        public string SerializeEntity()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<string> values = Info.Split('|').ToList();
            if (values.Count > 0)
            {
                result.Add("title", values?[0]);
                result.Add("content", values?[1]);
                result.Add("choices", values.Skip(2));
            }
            List<int> answers = new List<int>();
            Answer.Split('|').ToList().ForEach(element => {
                if (int.TryParse(element, out int value))
                    answers.Add(value);
            });
            result.Add("answer", answers);
            return JsonConvert.SerializeObject(result);
        }
    }
}
