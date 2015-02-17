namespace BackgroundProcessor.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using BackgroundProcessor.WebRole.Models;

    public class WordAnalyzer
    {
        private static Random _random = new Random((int)DateTimeOffset.Now.Ticks);
        public static string AnalyseUserInput(string userWord, string gameWord)
        {
            userWord = userWord.ToLower();
            gameWord = gameWord.ToLower();
            var gameWordArr = gameWord.ToCharArray();

            var phrase = new StringBuilder();

            // Cow checker
            var result = gameWordArr.Where(x => userWord.IndexOf(x)>=0 && userWord.IndexOf(x) != gameWord.IndexOf(x)).Count();
            phrase.Append(string.Empty.PadLeft(result, '1'));

            // Bull checker
            result = gameWordArr.Where(x => userWord.IndexOf(x) >= 0 && userWord.IndexOf(x) == gameWord.IndexOf(x)).Count();
            phrase.Append(string.Empty.PadLeft(result, '0'));

            return phrase.ToString();
        }

        public static TimedWord AnalyseUserInputTimed(string userWord, string gameWord)
        {
            userWord = userWord.ToLower();
            gameWord = gameWord.ToLower();
            var gameWordArr = gameWord.ToCharArray();

            var phrase = new StringBuilder();

            var sw = Stopwatch.StartNew();
            // Cow checker
            var result = gameWordArr.Where(x => userWord.IndexOf(x) >= 0 && userWord.IndexOf(x) != gameWord.IndexOf(x)).Count();
            phrase.Append(string.Empty.PadLeft(result, '1'));

            // Bull checker
            result = gameWordArr.Where(x => userWord.IndexOf(x) >= 0 && userWord.IndexOf(x) == gameWord.IndexOf(x)).Count();
            phrase.Append(string.Empty.PadLeft(result, '0'));

            sw.Stop();

            return new TimedWord { Word = userWord, MilliSecond = sw.ElapsedMilliseconds, Result = phrase.ToString() };
        }


        public static void RandomizeMapWithWords(IDictionary<LetterWordCount, ICollection<string>> map)
        {
            foreach (var kvp in map)
            {
                var lwCnt = kvp.Key;
                var list = kvp.Value;
                for (var index = 0; index < lwCnt.WordCount; index++)
                {
                    list.Add(GetRandomString(lwCnt.LetterCount));
                }
            }
        }

        private static string GetRandomString(int numLetters)
        {
            var sb = new StringBuilder();
            while (sb.Length<=numLetters)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));

                if (sb.ToString().IndexOf(ch) < 0) sb.Append(ch);
            }

            return sb.ToString();
        }
    }
}
