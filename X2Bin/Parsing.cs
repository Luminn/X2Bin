namespace X2Bin;

partial class Program {
    private static (bool, bool, bool) ParseMode(string? mode, string input) {
        var xml = mode?.ToLower().Contains("xml") == true;
        var json = mode?.ToLower().Contains("json") == true;
        var yaml = mode?.ToLower().Contains("yaml") == true;
        if (input.EndsWith(".xml")) { xml = true; }
        if (input.EndsWith(".json")) { json = true; }
        if (input.EndsWith(".yaml") || input.EndsWith(".yml")) { yaml = true; }
        if ((xml || json || yaml) == false) {
            Console.Error.Write("Mode missing, specify a file type");
            MissingArguments();
            Environment.Exit(1);
        }
        return (xml, json, yaml);
    }

    private static void Setup(List<string> arguments) {
        var intSerializer = GetArgumentValue(arguments, "--int");
        var floatSerializer = GetArgumentValue(arguments, "--float");
        var stringSerializer = GetArgumentValue(arguments, "--string");
        var encoding = GetArgumentValue(arguments, "--encoding");
        var stringProcessor = GetArgumentValue(arguments, "--trim-string");
        var scriptProcessor = GetArgumentValue(arguments, "--trim-code");
        var enumPath = GetArgumentValue(arguments, "--enum");
        var cSharpScriptPath = GetArgumentValue(arguments, "--csharpscript");
        if (intSerializer != null) {
            Serialize.SetIntSerializer(intSerializer);
        }
        if (floatSerializer != null) {
            Literal.DefaultDouble = floatSerializer == "double";
        }
        if (stringSerializer != null) {
            Serialize.SetStringIndexSerializer(stringSerializer);
        }
        if (encoding != null) {
            Serialize.SetEncoding(encoding);
        }
        if (stringProcessor != null) {
            Serialize.SetStringProcessor(stringProcessor);
        }
        if (scriptProcessor != null) {
            Serialize.SetScriptProcessor(scriptProcessor);
        }
        if (enumPath != null) {
            EnumParser.Setup(enumPath);
        }
        if (cSharpScriptPath != null) {
            CSharpScriptCompiler.Setup(cSharpScriptPath);
        }
    }
}