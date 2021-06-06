using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ModuleAttribute : Attribute
    {

        public string Name { get; set; }

        public ModuleAttribute(string name)
            => Name = name;

        public ModuleAttribute() { }

    }
}
