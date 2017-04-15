using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Mvc;
using TwitterApiClient;

namespace TwitterHashtag.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.DataPoints = JsonConvert.SerializeObject(new List<DataPoint>());
            return View();
        }

        [HttpPost]
        public ActionResult Index(Search search)
        {
            var hashtag = search.Hashtag;
            var linqToTwitter = new LinqClient();
            var dataPoints = linqToTwitter.GetTweetsByHour(hashtag, 0);
            
            ViewBag.Hashtag = "Hashtag Timeline for: " + hashtag; ;
            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}