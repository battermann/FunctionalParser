using System;
using System.Linq;
using FunctionalParser;
using NFluent;
using NUnit.Framework;

namespace FunctionalParserTests
{
    [TestFixture]
    public class ParsersTests
    {
        [TestCase("Hello", 'H', "ello")]
        [TestCase("H", 'H', "")]
        [TestCase("123", '1', "23")]
        public void Input_should_succeed(string inp, char v, string output)
        {
            var result = inp.Parse(Parsers.Item);

            var r = Parsers.Item(inp);

            Check.That(result.First().Item1).IsEqualTo(v);
            Check.That(result.First().Item2).IsEqualTo(output);
        }

        [Test]
        public void Input_should_fail()
        {
            var result = String.Empty.Parse(Parsers.Item);
            Check.That(result).IsEmpty();
        }

        [TestCase("foo", 1)]
        [TestCase("foo", 'x')]
        [TestCase("foo", "bar")]
        [TestCase("", "bar")]
        public void Return_should_succeed<T>(string inp, T v)
        {
            var result = inp.Parse(Parsers.Return(v));

            Check.That(result.First().Item1).IsEqualTo(v);
            Check.That(result.First().Item2).IsEqualTo(inp);
        }

        [Test]
        public void Failure_should_fail()
        {
            Check.That("123".Parse(Parsers.Failure<char>())).IsEmpty();
            Check.That(String.Empty.Parse(Parsers.Failure<char>())).IsEmpty();
        }

        [TestCase("123", '1', '3', "")]
        [TestCase("abcd", 'a', 'c', "d")]
        public void Bind_should_succeed(string inp, char c1, char c2, string output)
        {
            var p = Parsers.Item.Bind(
                    v1 => Parsers.Item.Bind(
                        v2 => Parsers.Item.Bind(
                            v3 => Parsers.Return(Tuple.Create(v1, v3)))));

            var result = inp.Parse(p);

            Check.That(result.First().Item1).IsEqualTo(Tuple.Create(c1, c2));
            Check.That(result.First().Item2).IsEqualTo(output);
        }

        [TestCase("ab")]
        [TestCase("")]
        public void Bind_should_fail(string inp)
        {
            var p = Parsers.Item.Bind(
                    v1 => Parsers.Item.Bind(
                        v2 => Parsers.Item.Bind(
                            v3 => Parsers.Return(Tuple.Create(v1, v3)))));

            var result = inp.Parse(p);

            Check.That(result).IsEmpty();
        }

        [Test]
        public void Choice_test()
        {
            var p = Parsers.Item.Choice(Parsers.Return('x'));

            var result1 = "foo".Parse(p);
            Check.That(result1.First().Item1).IsEqualTo('f');

            var result2 = String.Empty.Parse(p);
            Check.That(result2.First().Item1).IsEqualTo('x');
        }

        [Test]
        public void Digit_test()
        {
            Check.That("123abc".Parse(Parsers.Digit)).ContainsExactly(Tuple.Create('1', "23abc"));
            Check.That("abc123".Parse(Parsers.Digit)).IsEmpty();
        }

        [Test]
        public void Many_test()
        {
            var result = "abc".Parse(Parsers.Item.Many());
            Check.That(result.First().Item1).ContainsExactly('a', 'b', 'c');

            var result2 = "a".Parse(Parsers.Item.Many());
            Check.That(result2.First().Item1).ContainsExactly('a');

            var result3 = "".Parse(Parsers.Item.Many());
            Check.That(result3.First().Item1).IsEmpty();
        }

        [Test]
        public void ManyMutRec_test()
        {
            var result = "abc".Parse(Parsers.Item.ManyMutRec());
            Check.That(result.First().Item1).ContainsExactly('a', 'b', 'c');

            var result2 = "a".Parse(Parsers.Item.ManyMutRec());
            Check.That(result2.First().Item1).ContainsExactly('a');

            var result3 = "".Parse(Parsers.Item.ManyMutRec());
            Check.That(result3.First().Item1).IsEmpty();
        }

        [Test]
        public void Aggregate_test()
        {
            var p = Parsers.Digit.Aggregate(0, (i, c) => i + (c - 48));

            var result = "1234567890".Parse(p);

            Check.That(result.First().Item1).IsEqualTo(45);

            var result2 = "foo".Parse(p);

            Check.That(result2.First().Item1).IsEqualTo(0);
        }

        [Test]
        public void Many1_test()
        {
            var result = "abc".Parse(Parsers.Item.Many1());
            Check.That(result.First().Item1).ContainsExactly('a', 'b', 'c');

            var result2 = "a".Parse(Parsers.Item.Many1());
            Check.That(result2.First().Item1).ContainsExactly('a');

            var result3 = "".Parse(Parsers.Item.Many1());
            Check.That(result3).IsEmpty();

            var result4 = "123a".Parse(Parsers.Digit.Many1());
            Check.That(result4.First().Item1).ContainsExactly('1', '2', '3');
        }

        [Test]
        public void Many1_large_input_should_succeed()
        {
            const int count = 5000;

            var str = String.Concat(Enumerable.Repeat(1, count).Select(x => x.ToString()));
            var inp = str + "XXX";

            var result = inp.Parse(Parsers.Digit.Many1());
            Check.That(result.First().Item1.Count()).IsEqualTo(count);
        }

        [Test]
        public void Nat_test()
        {
            Check.That("123".Parse(Parsers.Nat)).ContainsExactly(Tuple.Create(123, String.Empty));
        }

        [Test]
        public void Linq_query_syntax_test()
        {
            var p =
                from x in Parsers.Item
                from _ in Parsers.Item
                from z in Parsers.Item
                select new string(new[] { x, z });

            var result = "abc".Parse(p);

            Check.That(result).ContainsExactly(Tuple.Create("ac", String.Empty));
        }

        [Test]
        public void StringP_test()
        {
            var r1 = "123def".Parse(Parsers.StringP("123"));
            Check.That(r1.First()).IsEqualTo(Tuple.Create("123", "def"));

            var r2 = "abc123def".Parse(Parsers.StringP("123"));
            Check.That(r2).IsEmpty();

            var r3 = String.Empty.Parse(Parsers.StringP("123"));
            Check.That(r3).IsEmpty();

            var r4 = "123".Parse(Parsers.StringP("123"));
            Check.That(r4.First()).IsEqualTo(Tuple.Create("123", String.Empty));
        }

        [Test]
        public void StringRec_test()
        {
            var r1 = "123def".Parse(Parsers.StringRec("123"));
            Check.That(r1.First()).IsEqualTo(Tuple.Create("123", "def"));

            var r2 = "abc123def".Parse(Parsers.StringRec("123"));
            Check.That(r2).IsEmpty();

            var r3 = String.Empty.Parse(Parsers.StringRec("123"));
            Check.That(r3).IsEmpty();

            var r4 = "123".Parse(Parsers.StringRec("123"));
            Check.That(r4.First()).IsEqualTo(Tuple.Create("123", String.Empty));
        }

        [Test]
        public void StringP_large_input_should_succeed()
        {
            var str = String.Concat(Enumerable.Range(0, 1000000).Select(x => x.ToString()));

            var inp = str + "XXX";

            var result = inp.Parse(Parsers.StringP(str));
            Check.That(result).ContainsExactly(Tuple.Create(str, "XXX"));
        }

        [Test]
        public void Ident_test()
        {
            var result = "a123 foobar".Parse(Parsers.Ident);

            Check.That(result).ContainsExactly(Tuple.Create("a123", " foobar"));
        }

        [Test]
        public void Symbol_test()
        {
            Check.That("       foo    bar".Parse(Parsers.Symbol("foo"))).ContainsExactly(Tuple.Create("foo", "bar"));
        }

        [Test]
        public void Natural_should_succeed()
        {
            var r = "123".Parse(Parsers.Natural);

            Check.That(r.First().Item1).IsEqualTo(123);
        }

        [Test]
        public void Parsing_lists()
        {
            var p =
                from ign1 in Parsers.Symbol("[")
                from n in Parsers.Natural
                from ns in
                    (from ign2 in Parsers.Symbol(",") from n2 in Parsers.Natural select n2)
                           .Many()
                from ign3 in Parsers.Symbol("]")
                select (new[] { n }.Concat(ns));

            var result = "[ 1,  2    , 3,  4 ,   5   ]".Parse(p);

            Check.That(result.First().Item1).ContainsExactly(1, 2, 3, 4, 5);

            // this shoudld fail
            var result2 = "[1,2,]".Parse(p);
            Check.That(result2).IsEmpty();
        }

        [TestCase("42", 42)]
        [TestCase("(((((42)))))", 42)]
        [TestCase("1+1", 2)]
        [TestCase("(1+1)", 2)]
        [TestCase("1*1", 1)]
        [TestCase("1*2", 2)]
        [TestCase("(1*2)", 2)]
        [TestCase("2*3+4", 10)]
        [TestCase("2*(3+4)", 14)]
        [TestCase("2 * 3 +  4", 10)]
        [TestCase("2*(     3+ 4)  ", 14)]
        [TestCase("2*3-4", 6)] // leaves "-4" unconsumed
        [TestCase("((1))*(2+(((3)))*(4+(((5))+6))*(((7*8)))+9)", 2531)]
        public void ArithmeticExpressions_should_succeed(string inp, int expected)
        {
            var result = inp.Parse(ArithmeticExpressions.Expr());
            Check.That(result.First().Item1).IsEqualTo(expected);
        }

        [TestCase("-1")]
        [TestCase("()")]
        [TestCase("(5")]
        [TestCase("(1+2")]
        [TestCase("(1+2()")]
        public void ArithmeticExpressions_should_fail(string inp)
        {
            var result = inp.Parse(ArithmeticExpressions.Expr());
            Check.That(result).IsEmpty();
        }

        [TestCase("I", 1)]
        [TestCase("III", 3)]
        [TestCase("IX", 9)]
        [TestCase("MLXVI", 1066)]
        [TestCase("MCMLXXXIX", 1989)]
        [TestCase("MMMMMMM", 7000)]
        public void RomanNumeralsNoSyntaxCheck_should_succeed(string rn, int an)
        {
            var numnber = rn.Parse(RomanNumerals.RomanNumeralNoSyntaxCheck);

            Check.That(numnber.First().Item1).IsEqualTo(an);
        }

        [TestCase("I", 1)]
        [TestCase("III", 3)]
        [TestCase("IX", 9)]
        [TestCase("MLXVI", 1066)]
        [TestCase("VI", 6)]
        [TestCase("MCMLXXXIX", 1989)]
        [TestCase("MMMMMMM", 7000)]
        public void RomanNumerals_should_succeed(string rn, int an)
        {
            var numnber = rn.Parse(RomanNumerals.RomanNumeral);

            Check.That(numnber.First().Item1).IsEqualTo(an);
        }

        [TestCase("IIII")]
        [TestCase("VX")]
        [TestCase("IVX")]
        [TestCase("MDLVX")]
        [TestCase("sdafasd")]
        [TestCase("")]
        public void RomanNumerals_should_fail(string rn)
        {
            var result = rn.Parse(RomanNumerals.RomanNumeral);

            Check.That(result).IsEmpty();
        }
    }
}
