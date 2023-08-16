using System.Reflection;
using System.Runtime.Loader;

namespace YuGiOh.Bot.Models
{
    public class ModuleAssemblyLoadContext : AssemblyLoadContext
    {

        private AssemblyDependencyResolver _dependencyResolver;

        public ModuleAssemblyLoadContext(string assemblyName) : base(assemblyName)
        {

            _dependencyResolver = new AssemblyDependencyResolver(assemblyName);

        }

        protected override Assembly Load(AssemblyName assemblyName)
        {

            string assemblyPath = _dependencyResolver.ResolveAssemblyToPath(assemblyName);

            if (assemblyPath is not null)
                return LoadFromAssemblyPath(assemblyPath);

            return null;

        }

    }
}