namespace BackgroundProcessor.Console
{
    using System;

    using BackgroundProcessor.Logic;

    class Program
    {
        static void Main(string[] args)
        {
            var timedWord = WordAnalyzer.AnalyseUserInputTimed("qwerthjklmnbvcxz", "poiuylkjhnmgfxcv");
            System.Console.WriteLine(
                "{0} lettered word {1} took {2} millisec or {3}! Result is {4}",
                timedWord.Word.Length,
                timedWord.Word,
                timedWord.MilliSecond,
                GetTime(timedWord.MilliSecond),
                timedWord.Result);

            System.Console.ReadKey();
        }

        private static string GetTime(long millisec)
        {
            var t = TimeSpan.FromMilliseconds(millisec);
            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
        }
    }
}
