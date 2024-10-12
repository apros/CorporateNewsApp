using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace CorporateNews.Web.Plugins
{
    public class NewsPlugin : KernelPlugin
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly Dictionary<string, KernelFunction> _functions;
        private readonly ILogger<NewsPlugin> _logger;
        public override int FunctionCount => _functions.Count;

        public NewsPlugin(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<NewsPlugin> logger) : base("NewsPlugin")
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiBaseUrl"];
            _functions = new Dictionary<string, KernelFunction>();
            _logger = logger;
            InitializeFunctions();
        }        

        private void InitializeFunctions()
        {
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<KernelFunctionAttribute>() != null);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<KernelFunctionAttribute>();
                if (attribute != null)
                {
                    var function = KernelFunctionFactory.CreateFromMethod(method, this);
                    _functions[attribute.Name ?? method.Name] = function;
                }
            }
        }

        [KernelFunction("get_promotions")]
        [Description("Gets the latest company promotions.")]
        public async Task<string> GetPromotions()
        {
            _logger.LogInformation("GetPromotions function called");
            return await GetRssContent("promotions");
        }

        [KernelFunction("get_sales")]
        [Description("Gets the latest sales information.")]
        public async Task<string> GetSales()
        {
            _logger.LogInformation("GetSales function called");
            return await GetRssContent("sales");
        }

        private async Task<string> GetRssContent(string category)
        {
            try
            {
                _logger.LogInformation($"Fetching RSS content for {category}");
                var rssContent = await _httpClient.GetStringAsync($"{_apiBaseUrl}/api/{category}/rss");
                var formattedContent = FormatRssItems(rssContent, category);
                _logger.LogInformation($"Successfully fetched and formatted RSS content for {category}");
                return formattedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching {category} information");
                return $"Error fetching {category} information: {ex.Message}";
            }
        }

        private string FormatRssItems(string rssContent, string category)
        {
            using (var reader = XmlReader.Create(new StringReader(rssContent)))
            {
                var feed = SyndicationFeed.Load(reader);
                var items = feed.Items.Select(item => $"- {item.Title.Text}: {item.Summary.Text}");
                return $"{category.ToUpperFirst()} information:\n" + string.Join("\n", items);
            }
        }

        public override bool TryGetFunction(string name, [NotNullWhen(true)] out KernelFunction? function)
        {
            return _functions.TryGetValue(name, out function);
        }

        public override IEnumerator<KernelFunction> GetEnumerator()
        {
            return _functions.Values.GetEnumerator();
        }
    }

    public static class StringExtensions
    {
        public static string ToUpperFirst(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }
    }
}