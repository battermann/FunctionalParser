using System;
using System.Linq;

namespace FunctionalParser
{
    public static class RomanNumerals
    {
        public readonly static Parser<int> RomanNumeralNoSyntaxCheck =
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
                .Many1()
                .Select(rns => rns.Sum());

        public static readonly Parser<int> RomanNumeral =
            Parsers.StringP("IV").Select(x => 4)
                .Choice(Parsers.StringP("IX").Select(x => 9))
                .Choice(Parsers.StringP("XL").Select(x => 40))
                .Choice(Parsers.StringP("XC").Select(x => 90))
                .Choice(Parsers.StringP("CD").Select(x => 400))
                .Choice(Parsers.StringP("CM").Select(x => 900))
                .Choice(Parsers.CharP('I').Select(x => 1)
                    .Many1()
                    .Select(cs => cs.Sum())
                    .Sat(sum => sum <= 3))
                .Choice(Parsers.CharP('X').Select(x => 10)
                    .Many1()
                    .Select(cs => cs.Sum())
                    .Sat(sum => sum <= 30))
                .Choice(Parsers.CharP('C').Select(x => 100)
                    .Many1()
                    .Select(cs => cs.Sum())
                    .Sat(sum => sum <= 300))
                .Choice(Parsers.CharP('M').Select(x => 1000)
                    .Many1()
                    .Select(x => x.Sum()))
                .Choice(Parsers.CharP('V').Select(x => 5))
                .Choice(Parsers.CharP('L').Select(x => 50))
                .Choice(Parsers.CharP('D').Select(x => 500))
                .Many1()
                .Sat(ns => ns.Zip(ns.Skip(1), (a,b) => a > b).All(b => b))
                //.Sat(ns => ns.Zip(ns.Skip(1), Tuple.Create).All(t => t.Item1 > t.Item2))
                .Select(ns => ns.Sum());
    }
}
