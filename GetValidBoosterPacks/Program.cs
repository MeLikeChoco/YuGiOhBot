using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.XPath;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;

namespace GetValidBoosterPacks
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var url = "https://yugipedia.com/wiki/Template:Booster_Packs";
            string[] boosterPacks;

            using (var dom = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(url))
            {

                var body = dom.Body.GetElementsByClassName("nowraplinks collapsible autocollapse navbox-inner").First();
                IEnumerable<IElement> boosterPackRows = body.GetElementsByTagName("ul");
                boosterPackRows = boosterPackRows.Take(boosterPackRows.Count() - 1).Skip(1);
                boosterPacks = boosterPackRows.SelectMany(element => element.Children).Select(element => element.TextContent.Trim()).ToArray();

            }

            using (var pipe = new NamedPipeServerStream("GetValidBoosterPacks.Pipe", PipeDirection.Out))
            {
                
                pipe.WaitForConnection();
                Serializer.Serialize(pipe, boosterPacks);
                pipe.WaitForPipeDrain();
                pipe.Disconnect();
                pipe.Close();

            }


        }
    }
}
