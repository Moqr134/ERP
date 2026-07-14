using ERP_Clint;
using ERP_Clint.Service;
using ERP_Clint.Service.InventoryService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PRMS_Clint.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICatigoryService, CatigoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISuppliersService, SuppliersService>();
builder.Services.AddScoped<CostumAuth>();
builder.Services.AddScoped<IStockTransactionsService, StockTransactionsService>();
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7136/")

    });
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CostumAuth>());

await builder.Build().RunAsync();
