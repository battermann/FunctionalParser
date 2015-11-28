using System.Collections.Generic;
using System.Linq;

namespace FunctionalParser
{
    public static class RomanNumerals
    {
        public static Parser<int> RomanNumeral()
        {
            var dict = new Dictionary<string, int>
            {
                {"M", 1000}, {"D", 500}, {"C", 100}, {"L", 50}, {"X", 10}, {"V", 5}, {"I", 1}, {"CM", 900}, {"CD", 400}, {"XC", 90}, {"XL", 40}, {"IX", 9}, {"Iv", 4},
            };

            return
                from ns in
                    (from n in Parsers.StringP("IV")
                        .Choice(Parsers.StringP("IX"))
                        .Choice(Parsers.StringP("XL"))
                        .Choice(Parsers.StringP("XC"))
                        .Choice(Parsers.StringP("CD"))
                        .Choice(Parsers.StringP("CM"))
                        .Choice(Parsers.StringP("I"))
                        .Choice(Parsers.StringP("V"))
                        .Choice(Parsers.StringP("X"))
                        .Choice(Parsers.StringP("L"))
                        .Choice(Parsers.StringP("C"))
                        .Choice(Parsers.StringP("D"))
                        .Choice(Parsers.StringP("M"))
                        from r in Parsers.Return(dict[n])
                        select r).Many()
                from r in Parsers.Return(ns.Sum())
                select r;
        }
    }
}
