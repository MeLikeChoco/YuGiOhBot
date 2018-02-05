using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Comparers
{
    public class IgnoreCaseComparer : IEqualityComparer<string>
    {

        public bool Equals(string x, string y)
            => x.Equals(y, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode(string obj)
            => obj.GetHashCode();

    }
}
