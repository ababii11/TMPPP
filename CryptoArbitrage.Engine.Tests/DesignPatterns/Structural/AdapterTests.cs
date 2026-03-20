using CryptoArbitrage.Engine.DesignPatterns.Structural.Adapter;

namespace CryptoArbitrage.Engine.Tests.DesignPatterns.Structural;

public class AdapterTests
{
    [Fact]
    public void ArbitrageTickerMonitor_ProcessesAllExchangeAdapters_ThroughCommonInterface()
    {
        var monitor = new ArbitrageTickerMonitor();
        var adapters = new IExchangeTickerClient[]
        {
            new BinanceTickerAdapter(new BinanceLegacyTickerApi()),
            new KrakenTickerAdapter(new KrakenLegacyTickerApi()),
            new BybitTickerAdapter(new BybitLegacyQuoteApi())
        };

        foreach (var adapter in adapters)
        {
            var snapshot = monitor.Process(adapter, "BTC/USDT");

            Assert.Equal(adapter.ExchangeName, snapshot.Exchange);
            Assert.Equal("BTC/USDT", snapshot.Pair);
            Assert.True(snapshot.Bid > 0);
            Assert.True(snapshot.Ask > snapshot.Bid);
        }
    }
}