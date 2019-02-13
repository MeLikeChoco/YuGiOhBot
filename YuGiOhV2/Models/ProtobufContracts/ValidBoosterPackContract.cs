using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.ProtobufContracts
{
    [ProtoContract]
    public class ValidBoosterPackContract
    {

        [ProtoMember(1)]
        public string[] BoosterPacks { get; set; }

    }
}
