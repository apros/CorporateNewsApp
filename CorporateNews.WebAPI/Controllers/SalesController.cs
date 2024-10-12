using Microsoft.AspNetCore.Mvc;
using System.ServiceModel.Syndication;
using System.Xml;
using CorporateNews.Core.Services;


namespace CorporateNews.WebAPI.Controllers
{
    

    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly SalesService _salesService;

        public SalesController(SalesService salesService)
        {
            _salesService = salesService;
        }

        [HttpGet("rss")]
        public IActionResult GetRssFeed()
        {
            var quarterlySales = _salesService.GenerateQuarterlySales();
            var feed = new SyndicationFeed(
                "Quarterly Sales",
                "Quarterly sales reports",
                new Uri("https://example.com/sales"),
                "SalesFeedID",
                DateTime.Now);

            var items = quarterlySales.Select(s => new SyndicationItem(
                $"Q{s.Quarter} {s.Date.Year} Sales Report",
                $"{s.Date:d} - In quarter {s.Quarter} we increased our sales from ${s.PreviousSales:N2} to ${s.CurrentSales:N2}.",
                new Uri("https://example.com/sales"),
                s.Date.ToString("yyyyMMddHHmmss"),
                s.Date
            )).ToList();

            feed.Items = items;

            var settings = new XmlWriterSettings
            {
                Encoding = System.Text.Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true
            };

            using (var stream = new MemoryStream())
            using (var xmlWriter = XmlWriter.Create(stream, settings))
            {
                var rssFormatter = new Rss20FeedFormatter(feed, false);
                rssFormatter.WriteTo(xmlWriter);
                xmlWriter.Flush();
                return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
            }
        }
    }
}
