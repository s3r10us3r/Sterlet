using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.gui
{
    public class TimerOptions
    {
        public enum Options
        {
            OneMinute, OnePlusOne, TwoPlusOne,
            ThreeMinutes, ThreePlusTwo, FiveMinutes,
            TenMinutes, FifteenPlusTen, ThirtyMinutes,
            NoTime
        }

        public Options Option { get; }
        internal int start;
        internal int increment = 0;

        public TimerOptions(Options option)
        {
            Option = option;
            switch (option)
            {
                case Options.OneMinute:
                    start = 60;
                    break;
                case Options.OnePlusOne:
                    start = 60;
                    increment = 1;
                    break;
                case Options.TwoPlusOne:
                    start = 120;
                    increment = 1;
                    break;
                case Options.ThreeMinutes:
                    start = 180;
                    break;
                case Options.ThreePlusTwo:
                    start = 180;
                    increment = 2;
                    break;
                case Options.FiveMinutes:
                    start = 300;
                    break;
                case Options.TenMinutes:
                    start = 600;
                    break;
                case Options.FifteenPlusTen:
                    start = 900;
                    increment = 10;
                    break;
                case Options.ThirtyMinutes:
                    start = 1800;
                    break;
                case Options.NoTime:
                    start = 0;
                    break;
            }
        }

        public string GetOptionString()
        {
            switch (Option)
            {
                case Options.OneMinute:
                    return "1 min.";
                case Options.OnePlusOne:
                    return "1 | 1";
                case Options.TwoPlusOne:
                    return "2 | 1";
                case Options.ThreeMinutes:
                    return "3 min.";
                case Options.ThreePlusTwo:
                    return "3 | 2";
                case Options.FiveMinutes:
                    return "5 min.";
                case Options.TenMinutes:
                    return "10 min.";
                case Options.FifteenPlusTen:
                    return "15 | 10";
                case Options.ThirtyMinutes:
                    return "30";
                case Options.NoTime:
                    return "No time";
                default:
                    throw new ArgumentException("Invalid argument!");
            }
        }
    }
}
