namespace FunctionalParser
{
    public static class ArithmeticExpressions
    {
        public static Parser<int> Expr()
        {       
            return     
                from t in Term()
                from r in
                   (from _ in Parsers.Symbol("+")
                    from e in Expr()
                    from r in Parsers.Return(t+e)
                    select r)
                        .Choice(Parsers.Return(t))
                select r;
        }

        public static Parser<int> Term()
        {
            return
                from f in Factor()
                from r in
                   (from _ in Parsers.Symbol("*")
                    from t in Term()
                    from r in Parsers.Return(f*t)
                    select r)
                        .Choice(Parsers.Return(f))
                select r;
        }

        public static Parser<int> Factor()
        {
            return
               (from ign1 in Parsers.Symbol("(")
                from e in Expr()
                from ign2 in Parsers.Symbol(")")
                from r in Parsers.Return(e)
                select r)
                    .Choice(Parsers.Natural);
        }
    }
}