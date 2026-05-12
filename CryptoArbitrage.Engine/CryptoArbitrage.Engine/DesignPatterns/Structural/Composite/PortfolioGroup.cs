using System.Text;

namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Composite;

/// <summary>
/// Composite object that contains portfolio components and treats them as one.
/// </summary>
public class PortfolioGroup : IPortfolioComponent
{
    private readonly List<IPortfolioComponent> _children = new();

    public PortfolioGroup(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public void Add(IPortfolioComponent component)
    {
        _children.Add(component);
    }

    public void Remove(IPortfolioComponent component)
    {
        _children.Remove(component);
    }

    public IReadOnlyList<IPortfolioComponent> GetChildren()
    {
        return _children;
    }

    public string Execute()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Composite Group -> {Name}");

        foreach (var child in _children)
        {
            builder.AppendLine(child.Execute());
        }

        return builder.ToString().TrimEnd();
    }
}
