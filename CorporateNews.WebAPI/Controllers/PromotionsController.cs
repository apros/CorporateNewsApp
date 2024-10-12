using Microsoft.AspNetCore.Mvc;
using System.ServiceModel.Syndication;
using System.Xml;
using CorporateNews.Core.Services;

namespace CorporateNews.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly PromotionService _promotionService;

        public PromotionsController(PromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet("rss")]
        public IActionResult GetRssFeed()
        {
            var promotions = _promotionService.GeneratePromotions();
            var feed = new SyndicationFeed(
                "Corporate Promotions",
                "Recent promotions in our company",
                new Uri("https://example.com/promotions"),
                "PromotionsFeedID",
                DateTime.Now);

            var items = promotions.Select(p => new SyndicationItem(
                $"{p.EmployeeName} promoted to {p.NewPosition}",
                $"On {p.Date:d}, {p.EmployeeName} was promoted to {p.NewPosition}.",
                new Uri("https://example.com/promotions"),
                p.Date.ToString("yyyyMMddHHmmss"),
                p.Date
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
