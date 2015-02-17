namespace BackgroundProcessor.WebRole.Controllers
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using BackgroundProcessor.Logic;
    using BackgroundProcessor.WebRole.Models;
    using BackgroundProcessor.WebRole.Attributes;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            this.ViewBag.Title = "Bulls and Cows Test Bed Home Page";

            return this.View();
        }

        private static ICollection<LetterWordCount> ParseWorkLoad(NameValueCollection form)
        {
            var lwcList = new List<LetterWordCount>();

            var lb = int.Parse(form["lowerBound"]);
            var ub = int.Parse(form["upperBound"]);

            // TODO: Bit of a hardcoding here to parse the textfields. Could do better!
            const string Format = "txtCnt{0}Letter";

            for (var index = lb; index <= ub; index++)
            {
                var lwCnt = new LetterWordCount
                            {
                                LetterCount = index,
                                WordCount = int.Parse(form[string.Format(Format, index)])
                            };
                
                lwcList.Add(lwCnt);
            }

            return lwcList;
        }

        [HttpPost]
        [MultipleSubmit(Name = "action", Argument = "SubmitBgWorkerAsync")]
        public async Task<ActionResult> SubmitBgWorkerAsync(FormCollection form)
        {
            // Herein you should store the workload in a map and offload to a queue
            var lwcList = ParseWorkLoad(form);
            await
                ServiceBusQueueHandler.AddWorkLoadToQueueAsync(
                    WebApiApplication.QueueClient,
                    WebApiApplication.QueueName,
                    lwcList);
            return this.View("Index");
        }

        [HttpPost]
        [MultipleSubmit(Name = "action", Argument = "SubmitNewThread")]
        public ActionResult SubmitNewThread(FormCollection form)
        {
            // Herein you should store the workload in a map and offload to a queue
            var lwcList = ParseWorkLoad(form);
            new Thread(() => WordAnalyzer.GenerateAnalyzeTestStringsAsync(lwcList)).Start();

            return this.View("Index");
        }

        public async Task<ActionResult> SetDataModelAsync(int lowerBound, int upperBound)
        {
            var tbModel = new TextBoxGenerationModel { LowerBound = lowerBound, UpperBound = upperBound };
            return this.PartialView("FormWordAnalyzer", tbModel);
        }
    }
}
