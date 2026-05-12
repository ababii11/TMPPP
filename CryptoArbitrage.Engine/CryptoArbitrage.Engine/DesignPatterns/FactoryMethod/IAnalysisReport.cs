namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;

/// <summary>
/// Product interface - defines the common interface for all analysis reports
/// </summary>
public interface IAnalysisReport
{
    /// <summary>
    /// Equivalent to doStuff() from the diagram - generates and returns the report
    /// </summary>
    string GenerateReport();
    
    /// <summary>
    /// Gets the report type for identification
    /// </summary>
    string ReportType { get; }
}