using System.Collections.Generic;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Command;

/// <summary>
/// Invoker that executes commands and stores history for undo operations.
/// </summary>
public class TradingBotInvoker
{
    private readonly Stack<ICommand> _history = new();

    public string ExecuteCommand(ICommand command)
    {
        var result = command.Execute();
        _history.Push(command);
        return result;
    }

    public string UndoLastCommand()
    {
        if (_history.Count == 0)
        {
            return "[TradingBotInvoker] No command in history to undo.";
        }

        var command = _history.Pop();
        return command.Undo();
    }

    public int HistoryCount() => _history.Count;

    public void ClearHistory()
    {
        _history.Clear();
    }
}
