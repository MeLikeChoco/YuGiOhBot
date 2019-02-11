using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Services
{
    public class ServiceObserver
    {

        private readonly NamedPipeServerStream _pipe;

        public ServiceObserver()
        {

            _pipe = new NamedPipeServerStream("Services.Pipe", PipeDirection.In);
            

        }

    }
}
