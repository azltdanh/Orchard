using System;
using System.Collections.Generic;
using System.Linq;

namespace RealEstate.Helpers
{
    public class CsvParseException : Exception
    {
        public CsvParseException(string message)
            : base(message)
        {
        }
    }

    public static class Exceptions
    {
        public static string[] CsvSplit(this String source)
        {
            var splitString = new List<string>();
            var slashesToRemove = new List<int>();
            var state = State.AtBeginningOfToken;
            char[] sourceCharArray = source.ToCharArray();
            int tokenStart = 0;
            int len = sourceCharArray.Length;
            for (int i = 0; i < len; ++i)
            {
                switch (state)
                {
                    case State.AtBeginningOfToken:
                        if (sourceCharArray[i] == '"')
                        {
                            state = State.InQuotedToken;
                            slashesToRemove = new List<int>();
                            continue;
                        }
                        if (sourceCharArray[i] == ',')
                        {
                            splitString.Add("");
                            tokenStart = i + 1;
                            continue;
                        }
                        state = State.InNonQuotedToken;
                        continue;
                    case State.InNonQuotedToken:
                        if (sourceCharArray[i] == ',')
                        {
                            splitString.Add(
                                source.Substring(tokenStart, i - tokenStart));
                            state = State.AtBeginningOfToken;
                            tokenStart = i + 1;
                        }
                        continue;
                    case State.InQuotedToken:
                        if (sourceCharArray[i] == '"')
                        {
                            state = State.ExpectingComma;
                            continue;
                        }
                        if (sourceCharArray[i] == '\\')
                        {
                            state = State.InEscapedCharacter;
                            slashesToRemove.Add(i - tokenStart);
                        }
                        continue;
                    case State.ExpectingComma:
                        if (sourceCharArray[i] != ',')
                            throw new CsvParseException("Expecting comma");
                        string stringWithSlashes =
                            source.Substring(tokenStart, i - tokenStart);
                        stringWithSlashes = slashesToRemove.Reverse<int>().Aggregate(stringWithSlashes, (current, item) => current.Remove(item, 1));
                        splitString.Add(
                            stringWithSlashes.Substring(1,
                                stringWithSlashes.Length - 2));
                        state = State.AtBeginningOfToken;
                        tokenStart = i + 1;
                        continue;
                    case State.InEscapedCharacter:
                        state = State.InQuotedToken;
                        continue;
                }
            }
            switch (state)
            {
                case State.AtBeginningOfToken:
                    splitString.Add("");
                    return splitString.ToArray();
                case State.InNonQuotedToken:
                    splitString.Add(
                        source.Substring(tokenStart,
                            source.Length - tokenStart));
                    return splitString.ToArray();
                case State.InQuotedToken:
                    throw new CsvParseException("Expecting ending quote");
                case State.ExpectingComma:
                    string stringWithSlashes =
                        source.Substring(tokenStart, source.Length - tokenStart);
                    stringWithSlashes = slashesToRemove.Reverse<int>().Aggregate(stringWithSlashes, (current, item) => current.Remove(item, 1));
                    splitString.Add(
                        stringWithSlashes.Substring(1,
                            stringWithSlashes.Length - 2));
                    return splitString.ToArray();
                case State.InEscapedCharacter:
                    throw new CsvParseException("Expecting escaped character");
            }
            throw new CsvParseException("Unexpected error");
        }

        private enum State
        {
            AtBeginningOfToken,
            InNonQuotedToken,
            InQuotedToken,
            ExpectingComma,
            InEscapedCharacter
        };
    }
}