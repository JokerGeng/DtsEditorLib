using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class ParseException : Exception
    {
        public int Line { get; }

        public ParseException(string message, int line = 0) : base(message)
        {
            Line = line;
            base.Source="Parse in "+ Line.ToString();
        }
    }
}
