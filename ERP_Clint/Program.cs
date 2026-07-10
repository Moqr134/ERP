using ERP_Clint;
using ERP_Clint.Service;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PRMS_Clint.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<CostumAuth>();
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7136/")

    });
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CostumAuth>();

await builder.Build().RunAsync();
