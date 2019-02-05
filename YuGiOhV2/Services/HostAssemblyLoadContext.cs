//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.Loader;
//using System.Text;
//using System.Threading.Tasks;

//namespace YuGiOhV2.Services
//{
//    public class HostAssemblyLoadContext : AssemblyLoadContext
//    {

//        private string _module;

//        public HostAssemblyLoadContext(string module)
//        {

//            _module = module;

//        }

//        protected override Assembly Load(AssemblyName assemblyName)
//        {

//            var path = Path.Combine(Directory.GetCurrentDirectory(), "Modules", _module);

//        }

//    }
//}
