using ArbitrageProject.Interfaces;
using ArbitrageProject.Services;
using ArbitrageProject.Engines;
using ArbitrageProject.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Exchange services (stub)
builder.Services.AddTransient<IExchangeService, BinanceService>();
builder.Services.AddTransient<IExchangeService, KrakenService>();

// Core logic
builder.Services.AddTransient<IPriceProvider, PriceProvider>();
builder.Services.AddTransient<IFeeCalculator>(_ => new FeeCalculator(0.1m));
builder.Services.AddTransient<IArbitrageStrategy>(_ => new SimpleArbitrageStrategy(0.5m));
builder.Services.AddTransient<ArbitrageEngine>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();