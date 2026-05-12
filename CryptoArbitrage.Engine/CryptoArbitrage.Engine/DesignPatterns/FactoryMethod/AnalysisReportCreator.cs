namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;

/// <summary>
/// Creator - Abstract base class that declares the factory method
/// </summary>
public abstract class AnalysisReportCreator
{
    /// <summary>
    /// Some operation that uses the factory method - equivalent to someOperation() from diagram
    /// </summary>
    public string ProcessAnalysis()
    {
        // Create the product using factory method
        IAnalysisReport report = CreateReport();
        
        // Use the product
        var reportContent = report.GenerateReport();
        
        // Additional processing
        return $"Analysis processed successfully.\nReport Type: {report.ReportType}\n{reportContent}";
    }

    /// <summary>
    /// Factory Method - equivalent to createProduct(): Product from diagram
    /// This method must be implemented by concrete creators
    /// </summary>
    public abstract IAnalysisReport CreateReport();
}