using Havit.Blazor.Components.Web;
using KeywordFighting;
using KeywordFighting.Configuration;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .Configure<AppOptions>(builder.Configuration)
    .AddKeywordFightingApplication()
    .AddKeywordFighting()
    .AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddHxServices();

await builder.Build().RunAsync();
