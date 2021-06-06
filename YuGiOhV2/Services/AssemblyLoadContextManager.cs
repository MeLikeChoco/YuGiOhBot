using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Services
{
    public class AssemblyLoadContextManager
    {

        public readonly Dictionary<string, AssemblyLoadContext> Contexts;

        public AssemblyLoadContextManager()
        {

            Contexts = new Dictionary<string, AssemblyLoadContext>();

            foreach (var assemblyName in File.ReadAllLines("assemblies.txt"))
            {

                

            }

        }



    }
}
