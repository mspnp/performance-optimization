namespace BackgroundProcessor.WebRole.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using BackgroundProcessor.Logic;
    using BackgroundProcessor.WebRole.Models;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Bulls and Cows Test Bed Home Page";

            return View();
        }

        [HttpPost]
        private async Task<ActionResult> SendWorkToBackground(IDictionary<byte,ICollection<string>> wordMap)
        {
            //await ServiceBusQueueHandler
            return View("Index");
        }

        public ActionResult DoWorkInNewThread()
        {
            //new Thread(GetPrimes).Start();

            return this.View("Index");
        }

        private static IDictionary<LetterWordCount, ICollection<string>> StoreWorkLoadInAMap(FormCollection form)
        {
            var map = new Dictionary<LetterWordCount, ICollection<string>>();
            const string Format = "txtCnt{0}Letter";
            var lb = int.Parse(form["lowerBound"]);
            var ub = int.Parse(form["upperBound"]);

            for (var index = lb; index <= ub; index++)
            {
                var lwCnt = new LetterWordCount
                            {
                                LetterCount = index,
                                WordCount = int.Parse(form[string.Format(Format, index)])
                            };
                map[lwCnt] = new List<string>();
            }

            return map;
        }


        public async Task<ActionResult> AnalyzeDataAsync(FormCollection form)
        {
            // Herein you should store the workload in a map and offload to a queue
            var map = StoreWorkLoadInAMap(form);
            WordAnalyzer.RandomizeMapWithWords(map);
            await
                ServiceBusQueueHandler.AddWorkLoadToQueueAsync(
                    WebApiApplication.QueueClient,
                    WebApiApplication.QueueName,
                    map);
            return this.View("Index");
        }

        //[HttpGet]
        public async Task<ActionResult> SetDataModelAsync(int lowerBound, int upperBound)
        {
            var tbModel = new TextBoxGenerationModel { LowerBound = lowerBound, UpperBound = upperBound };
            return this.PartialView("TextBoxesForWordAnalyzer", tbModel);
        }
    }
}
