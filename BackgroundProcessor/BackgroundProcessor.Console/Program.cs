namespace BackgroundProcessor.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using BackgroundProcessor.Logic;
    using BackgroundProcessor.WebRole.Models;

    class Program
    {
        static void Main(string[] args)
        {
            TimedWordAnalysis();
        }

        static void TimedWordAnalysis()
        {
            var lwcList = new List<LetterWordCount>();
            for (var i = 4; i <= 12; i++)
            {
                lwcList.Add(new LetterWordCount { LetterCount = i, WordCount = 100000 });
            }

            var retMap = WordAnalyzer.GenerateAnalyzeTestStringsAsync(lwcList);

            Console.WriteLine("LetterCount          WordCount           Time");   
            Console.WriteLine();
            foreach (var kvp in retMap.Result)
            {
                Console.WriteLine(
                    "{0}            {1}            {2}",
                    kvp.Key.LetterCount,
                    kvp.Key.WordCount,
                    WordAnalyzer.GetPrintableTime(kvp.Value).Result);
            }

            Console.WriteLine();
            Console.WriteLine(
                "Total time taken is: {0}",
                WordAnalyzer.GetPrintableTime(retMap.Result.Sum(kvp => kvp.Value)).Result);
            Console.ReadKey();
        }
    }
}
