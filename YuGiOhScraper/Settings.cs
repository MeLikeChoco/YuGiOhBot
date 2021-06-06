using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper
{
    public static class Settings
    {

        private static bool _isSubProcess = false;
        private static bool _sqlite = true;
        private static bool _json = false;
        private static int _cardAmount = -1;
        private static int _boosterPackAmount = -1;

        public static bool IsSubProcess => _isSubProcess;
        public static bool Sqlite => _sqlite;
        public static bool Json => _json;
        public static int CardAmount => _cardAmount;
        public static int BoosterPackAmount => _boosterPackAmount;
        public static string PipeName { get; private set; }
        public static bool NeedsPipe => !string.IsNullOrEmpty(PipeName);

        public static void Initialize(string[] args)
        {


            if (args.Length > 0)
            {

                if (args.Contains("-p"))
                    SetBoolProperty("-p", ref _isSubProcess);

                if (args.Contains("-s"))
                    SetBoolProperty("-s", ref _sqlite);

                if (args.Contains("-j"))
                    SetBoolProperty("-j", ref _json);

                if (args.Contains("-ca"))
                    SetIntProperty("-ca", ref _cardAmount);

                if (args.Contains("-bpa"))
                    SetIntProperty("-bpa", ref _boosterPackAmount);

                if (args.Contains("--pipe"))
                    PipeName = GetSetting("-pipe");

                void SetBoolProperty(string option, ref bool property)
                {

                    var argHeaderIndex = Array.IndexOf(args, option);

                    if (argHeaderIndex >= 0 
                        && argHeaderIndex + 1 < args.Length 
                        && int.TryParse(args[argHeaderIndex + 1], out var result))
                        property = result == 1;

                }

                void SetIntProperty(string option, ref int property)
                {

                    var argHeaderIndex = Array.IndexOf(args, option);

                    if (argHeaderIndex >= 0
                        && argHeaderIndex + 1 < args.Length
                        && int.TryParse(args[argHeaderIndex + 1], out var result)
                        && result > -1)
                        property = result;

                }

                string GetSetting(string option)
                {

                    var argHeaderIndex = Array.IndexOf(args, option);

                    if (argHeaderIndex > 0 && argHeaderIndex + 1 < args.Length)
                        return args[argHeaderIndex + 1];
                    else
                        return null;

                }

            }

        }

    }
}
