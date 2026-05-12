using System.Text.RegularExpressions;
using CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Command;

/// <summary>
/// ConcreteCommand for executing sell orders via an existing exchange receiver.
/// </summary>
public class ExecuteSellOrderCommand : ICommand
{
    private readonly IOrderTool _receiver;
    private readonly string _pair;
    private readonly decimal _amount;
    private readonly decimal _price;
    private string? _lastOrderId;

    public string Name => "ExecuteSellOrderCommand";

    public ExecuteSellOrderCommand(IOrderTool receiver, string pair, decimal amount, decimal price)
    {
        _receiver = receiver;
        _pair = pair;
        _amount = amount;
        _price = price;
    }

    public string Execute()
    {
        var result = _receiver.PlaceSellOrder(_pair, _amount, _price);
        _lastOrderId = ExtractOrderId(result);
        return result;
    }

    public string Undo()
    {
        if (string.IsNullOrWhiteSpace(_lastOrderId))
        {
            return $"[{Name}] Undo skipped: no executed order to cancel.";
        }

        return _receiver.CancelOrder(_lastOrderId);
    }

    private static string? ExtractOrderId(string text)
    {
        var match = Regex.Match(text, @"Order ID:\s*(\S+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }
}
