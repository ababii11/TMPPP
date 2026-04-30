using ArbitrageProject.Interfaces;
using ArbitrageProject.Services;
using ArbitrageProject.Engines;
using ArbitrageProject.Strategies;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Observer;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Command;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;
using CryptoArbitrage.Web.Services.Persistence;

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
builder.Services.AddSingleton<CryptoPriceService>(sp =>
    new CryptoPriceService(sp.GetServices<IExchangeService>(), seed: 42));
builder.Services.AddSingleton<TradingBotInvoker>();
builder.Services.AddSingleton<ArbitrageBotStateOriginator>();
builder.Services.AddSingleton<StateManager>();
builder.Services.AddSingleton<IBotStateStore, SqlServerBotStateStore>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var persistence = scope.ServiceProvider.GetRequiredService<IBotStateStore>();
    var originator = scope.ServiceProvider.GetRequiredService<ArbitrageBotStateOriginator>();
    var stateManager = scope.ServiceProvider.GetRequiredService<StateManager>();
    persistence.InitializeAsync(originator, stateManager).GetAwaiter().GetResult();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();