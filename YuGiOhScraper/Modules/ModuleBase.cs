using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;

namespace YuGiOhScraper.Modules
{
    public abstract class ModuleBase
    {

        public string ModuleName { get; set; }
        public string DatabaseName { get; set; }

        protected string DatabasePath => DatabaseName;
        protected string ConnectionString => $"Data Source = {DatabaseName}";

        protected IDictionary<string, string> TcgCards, OcgCards, TcgBoosters, OcgBoosters;

        public ModuleBase(string moduleName, string databaseName)
        {

            ModuleName = moduleName;
            DatabaseName = databaseName;

        }

        public async Task RunAsync()
        {

            Console.WriteLine($"{ModuleName} module has began running...");

            var cardLinks = await GetCardLinks();
            var boosterPackLinks = await GetBoosterPackLinks();

            Console.WriteLine($"There are {cardLinks.Count} cards and {boosterPackLinks.Count} booster packs.");

            await DoBeforeParse(cardLinks, boosterPackLinks);

            var cards = ParseCards(cardLinks, out var errors);
            var boosterPacks = ParseBoosterPacks(boosterPackLinks, out var errors2);
            errors = errors.Concat(errors2);

            await DoBeforeSaveToDb(cards, boosterPacks, errors);
            await SaveToDatabase(cards, boosterPacks, errors);

            Console.WriteLine($"{ModuleName} module has finished running.");

        }

        protected abstract Task<IDictionary<string, string>> GetCardLinks();
        protected abstract Task<IDictionary<string, string>> GetBoosterPackLinks();
        protected abstract Task DoBeforeParse(IDictionary<string, string> cardLinks, IDictionary<string, string> boosterPackLinks);
        protected abstract IEnumerable<Card> ParseCards(IDictionary<string, string> cardLinks, out IEnumerable<Error> errors);
        protected abstract IEnumerable<BoosterPack> ParseBoosterPacks(IDictionary<string, string> boosterPackLinks, out IEnumerable<Error> errors);
        protected abstract Task DoBeforeSaveToDb(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors);
        protected abstract Task SaveToDatabase(IEnumerable<Card> cards, IEnumerable<BoosterPack> boosterPacks, IEnumerable<Error> errors);

        //private  object _inlineLock = new object();
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void InlineWrite(string message)
        {

            //lock (_inlineLock)
            //{

            //    int currentLineCursor = Console.CursorTop;
            //    Console.SetCursorPosition(0, Console.CursorTop);
            //    Console.Write(new string(' ', Console.WindowWidth));
            //    Console.SetCursorPosition(0, currentLineCursor);
            //    //Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            //    Console.Write(message);

            //}

            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
            //Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            Console.Write(message);


        }

    }
}
