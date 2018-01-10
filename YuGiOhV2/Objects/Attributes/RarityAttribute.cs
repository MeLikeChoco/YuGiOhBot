using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RarityAttribute : Attribute
    {

        public string Rarity { get; set; }
        public string PropertyName { get; set; }

        public RarityAttribute(string rarity, [CallerMemberName]string propertyName = null)
        {

            Rarity = rarity;
            PropertyName = propertyName;

        }

    }
}
