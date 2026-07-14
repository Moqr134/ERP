using ERP_Clint;
using ERP_Clint.Service;
using ERP_Clint.Service.InventoryService;
using ERP_Clint.Service.SalesService;
using ERP_Clint.Service.UserAdmin;
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
builder.Services.AddScoped<IStockTransactionsService, StockTransactionsService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();
builder.Services.AddScoped<IRoleAdminService, RoleAdminService>();
builder.Services.AddScoped<CostumAuth>();
builder.Services.AddScoped<AuthHttpMessageHandler>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7136/";
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthHttpMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CostumAuth>());

await builder.Build().RunAsync();
