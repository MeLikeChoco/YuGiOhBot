using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Services
{
    public class ServiceObserver
    {

        private const string ServicesDirectory = "Services";

        private string[] _services => Directory.GetDirectories(ServicesDirectory);
        

    }
}
