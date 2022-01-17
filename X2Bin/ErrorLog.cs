namespace X2Bin; 

public class ReportedError: Exception {
    public ReportedError(string? message) : base(message) { }
}

public static class ErrorReport {

    static ErrorReport() {
        BaseColor = Console.ForegroundColor;
    }

    public static ConsoleColor BaseColor;
    public static string FileName { get; set; } = string.Empty;
    public static int LineNumber { get; set; }
    public static string Element { get; set; } = string.Empty;

    public static void ReportError(string message, bool includeLineData=true) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.Write("Error: ");
        Console.ForegroundColor = BaseColor;
        Console.Error.Write($"{message}");
        Console.Error.WriteLine(includeLineData ? $" at {Element}, {FileName} Line {LineNumber}." : ".");
        throw new ReportedError($"Error: {message}.");
    }
    
    public static void ReportWarning(string message, bool includeLineData=true) {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Error.Write("Warning: ");
        Console.ForegroundColor = BaseColor;
        Console.Error.Write($"{message}");
        Console.Error.WriteLine(includeLineData ? $" at {Element}, {FileName} Line {LineNumber}." : ".");
    }
}