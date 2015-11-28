using System.Collections.Generic;
using System.Linq;

namespace FunctionalParser
{
    public static class RomanNumerals
    {
        public readonly static Parser<int> RomanNumeral =
            Parsers.StringP("IV").Select(x => 4)
                .Choice(Parsers.StringP("IX").Select(x => 9))
                .Choice(Parsers.StringP("XL").Select(x => 40))
                .Choice(Parsers.StringP("XC").Select(x => 90))
                .Choice(Parsers.StringP("CD").Select(x => 400))
                .Choice(Parsers.StringP("CM").Select(x => 900))
                .Choice(Parsers.StringP("I").Select(x => 1))
                .Choice(Parsers.StringP("V").Select(x => 5))
                .Choice(Parsers.StringP("X").Select(x => 10))
                .Choice(Parsers.StringP("L").Select(x => 50))
                .Choice(Parsers.StringP("C").Select(x => 100))
                .Choice(Parsers.StringP("D").Select(x => 500))
                .Choice(Parsers.StringP("M").Select(x => 1000))
                .Many()
                .Select(rns => rns.Sum());
    }
}
