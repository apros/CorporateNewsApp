using CorporateNews.Web.Services;
using CorporateNews.Web.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add HttpClient
builder.Services.AddHttpClient();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Register NewsPlugin as a singleton, injecting IHttpClientFactory and IConfiguration
builder.Services.AddSingleton<NewsPlugin>(sp =>
    new NewsPlugin(
        sp.GetRequiredService<IHttpClientFactory>(),
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<ILogger<NewsPlugin>>()
    )
);

// Register SemanticKernelService as a singleton, injecting IConfiguration and NewsPlugin
builder.Services.AddSingleton<SemanticKernelService>(sp =>
    new SemanticKernelService(
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<NewsPlugin>(),
        sp.GetRequiredService<ILogger<SemanticKernelService>>()
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
