namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Command;

/// <summary>
/// Command interface that declares execute and undo operations.
/// </summary>
public interface ICommand
{
    string Name { get; }

    string Execute();

    string Undo();
}
