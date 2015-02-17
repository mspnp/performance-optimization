namespace BackgroundProcessor.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using BackgroundProcessor.WebRole.Models;

    public class WordAnalyzer
    {
        private static Random _random = new Random((int)DateTimeOffset.Now.Ticks);

        private static readonly string[] ComputerWords =
        {
            "ABLE", "TABLE", "STABLE", "SITABLE", "SUITABLE", "SUITABLER",
            "REACTFULOP", "REACTFULOPS", "IMCOPULANTER"
        };

        private const int MinLength = 4;

        private const int MaxLength = 12;

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

        private static Task<long> AnalyzeTestStringsAsync(IEnumerable<string> inStrings)
        {
            var sw = Stopwatch.StartNew();
            inStrings.ToList().ForEach(
                genStr =>
                {
                    // Ensure we're not running into index out of bounds
                    Debug.Assert(genStr.Length >= MinLength && genStr.Length <= MaxLength);

                    // Discard results as they're not relevant
                    AnalyseUserInput(genStr, ComputerWords[genStr.Length - MinLength]);
                });
            sw.Stop();

            return Task.FromResult(sw.ElapsedMilliseconds);
        }

        public static Task<IDictionary<LetterWordCount, long>> GenerateAnalyzeTestStringsAsync(ICollection<LetterWordCount> lwcList)
        {
            var results = new Dictionary<LetterWordCount, long>();
            foreach (var lwCnt in lwcList)
            {
                results[lwCnt] = AnalyzeTestStringsAsync(GenerateRandomStrings(lwCnt).Result).Result;
            }

            return Task.FromResult<IDictionary<LetterWordCount,long>>(results);
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

        private static Task<IEnumerable<string>> GenerateRandomStrings(LetterWordCount lwCnt)
        {
            var wordList = new List<string>(lwCnt.WordCount);
            for (var i = 0; i < lwCnt.WordCount; i++)
            {
                wordList.Add(GetRandomString(lwCnt.LetterCount).Result);
            }

            return Task.FromResult<IEnumerable<string>>(wordList);
        }

        private static Task<string> GetRandomString(int numLetters)
        {
            var sb = new StringBuilder();
            while (sb.Length<numLetters)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));

                if (sb.ToString().IndexOf(ch) < 0) sb.Append(ch);
            }

            return Task.FromResult(sb.ToString());
        }

        public static Task<string> GetPrintableTime(long millisec)
        {
            var t = TimeSpan.FromMilliseconds(millisec);
            return Task.FromResult(string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes, t.Seconds, t.Milliseconds));
        }
    }
}
