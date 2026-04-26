using System;
using CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Command;

/// <summary>
/// Client code that creates commands and submits them to the invoker.
/// </summary>
public static class CommandDemo
{
    public static void Run()
    {
        IOrderTool receiver = new BinanceOrderTool();
        var invoker = new TradingBotInvoker();

        ICommand buyBtc = new ExecuteBuyOrderCommand(receiver, "BTC/USDT", 0.15m, 64820m);
        ICommand sellEth = new ExecuteSellOrderCommand(receiver, "ETH/USDT", 1.25m, 3295m);

        Console.WriteLine(invoker.ExecuteCommand(buyBtc));
        Console.WriteLine();

        Console.WriteLine(invoker.ExecuteCommand(sellEth));
        Console.WriteLine();

        Console.WriteLine($"History count: {invoker.HistoryCount()}");
        Console.WriteLine();

        Console.WriteLine("Undo last command:");
        Console.WriteLine(invoker.UndoLastCommand());
        Console.WriteLine();

        Console.WriteLine("Undo previous command:");
        Console.WriteLine(invoker.UndoLastCommand());
        Console.WriteLine();

        Console.WriteLine($"History count: {invoker.HistoryCount()}");
    }
}
