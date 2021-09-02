using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Common.Models.YuGiOh
{
    public class Error
    {

        public string Name { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Url { get; set; }
        public Type Type { get; set; }

    }

    public enum Type
    {
        Card,
        Booster
    }
}
