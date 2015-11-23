using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionalParser
{
    public delegate List<Tuple<T, String>> Parser<T>(String inp);

    public static class Parsers
    {
        public static Parser<T> Return<T>(T v)
        {
            return inp => new List<Tuple<T, string>> { Tuple.Create(v, inp) };
        }

        public static Parser<T> Failure<T>()
        {
            return inp => new List<Tuple<T, string>>();
        }

        public static readonly Parser<char> Item = 
            inp => String.IsNullOrEmpty(inp)
                ? Failure<char>()(inp)
                : Return(inp[0])(inp.Substring(1));

        public static List<Tuple<T, string>> Parse<T>(this string input, Parser<T> p)
        {
            return p(input);
        }

        public static Parser<T2> Bind<T1, T2>(this Parser<T1> p, Func<T1, Parser<T2>> f)
        {
            return inp =>
            {
                var result = inp.Parse(p);
                return !result.Any() 
                    ? Failure<T2>()(inp)
                    : result.First().Item2.Parse(f(result.First().Item1));
            };
        }

        public static Parser<TResult> Select<TSource, TResult>(this Parser<TSource> source, Func<TSource, TResult> selector)
        {
            return inp =>
            {
                var result = inp.Parse(source);
                return !result.Any()
                    ? Failure<TResult>()(inp)
                    : Return(selector(result.First().Item1))(result.First().Item2);
            };
        }

        public static Parser<TResult> SelectMany<TSource, TValue, TResult>(this Parser<TSource> source, Func<TSource, Parser<TValue>> valueSelector, Func<TSource, TValue, TResult> resultSelector)
        {
            return source.Bind(s => valueSelector(s).Select(v => resultSelector(s, v)));
        }

        public static Parser<T> Choice<T>(this Parser<T> p, Parser<T> q)
        {
            return inp =>
            {
                var result = inp.Parse(p);
                return !result.Any()
                    ? inp.Parse(q)
                    : result;
            };
        }

        public static Parser<char> Sat(Predicate<char> p)
        {
            return Item.Bind(c => p(c) ? Return(c) : Failure<char>());
        }

        public static readonly Parser<char> Digit = Sat(Char.IsDigit);

        public static readonly Parser<char> Lower = Sat(Char.IsLower);

        public static readonly Parser<char> Upper = Sat(Char.IsUpper);

        public static readonly Parser<char> Letter = Sat(Char.IsLetter);

        public static readonly Parser<char> AlphaNum = Sat(Char.IsLetterOrDigit);

        public static Parser<char> CharP(char c)
        {
            return Sat(x => x == c);
        }

        public static Parser<string> StringP(string str)
        {
            return inp => !inp.StartsWith(str)
                ? Failure<string>()(inp)
                : Return(str)(inp.Substring(str.Length));
        }

        // Recursive definition can cause StackOverflowException
        public static Parser<string> StringRec(string str)
        {
            return String.IsNullOrEmpty(str)
                ? Return(String.Empty)
                : from a in CharP(str[0])
                  from b in StringP(str.Substring(1))
                  from result in Return(str)
                  select result;
        }

        public static Parser<List<T>> Many<T>(this Parser<T> p)
        {
            return inp =>
            {
                var state = Tuple.Create(new List<T>(), inp);

                while (true)
                {
                    var newState = p(state.Item2);
                    if (!newState.Any())
                        break;
                    state = Tuple.Create(state.Item1.Concat(new[] { newState.First().Item1 }).ToList(), newState.First().Item2);
                }

                return Return(state.Item1)(state.Item2);
            };
        }

        // Mutually recursive definition can cause StackOverflowException
        public static Parser<List<T>> ManyMutRec<T>(this Parser<T> p)
        {
            return p.Many1().Choice(Return(new List<T>()));
        }

        public static Parser<List<T>> Many1<T>(this Parser<T> p)
        {
            return p.Bind(v => p.Many().Bind(vs => Return(new List<T> { v }.Concat(vs).ToList())));
        }

        public static readonly Parser<int> Nat = Digit.Many1().Bind(n => Return(Int32.Parse(String.Concat(n))));

        public static readonly Parser<string> Ident =
            from c in Lower
            from cs in AlphaNum.Many()
            from result in Return(String.Concat(new[] {c}.Concat(cs)))
            select result;

        public static readonly Parser<Unit> Space = Sat(Char.IsWhiteSpace).Many().Select(x => Unit.Value);

        public static Parser<T> Token<T>(this Parser<T> p)
        {
            return
                from before in Space
                from t in p
                from after in Space
                from result in Return(t)
                select result;
        }

        public static readonly Parser<string> Identifier = Ident.Token();

        public static readonly Parser<int> Natural = Nat.Token();

        public static Parser<string> Symbol(string xs)
        {
            return inp => inp.Parse(StringP(xs).Token());
        }
    }

    public class Unit
    {
        private Unit() { }
        public static readonly Unit Value = new Unit();
        public override string ToString()
        {
            return "unit";
        }
    }
}
