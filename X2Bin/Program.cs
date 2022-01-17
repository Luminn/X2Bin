using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace X2Bin;

partial class Program {
    
    private static string? Compression;

    public static void MissingArguments() {
        Console.WriteLine(Help);
        Environment.Exit(1);
    }

    private static string? GetArgumentValue(List<string> args, string argName) {
        var i = args.IndexOf(argName);
        if (i < 0 || i >= args.Count - 1) {
            return null;
        }
        return args[i + 1];
    }
    
    private static bool HasArgumentValue(List<string> args, string argName) {
        return args.Contains(argName);
    }
    
    public static void Main(string[] argv) {
        var arguments = argv.ToList();
        if (HasArgumentValue(arguments, "--help")) {
            Console.WriteLine(Help);
            return;
        }
        if (HasArgumentValue(arguments, "--commands")) {
            Console.WriteLine(Commands);
            return;
        }
        if (HasArgumentValue(arguments, "--schema-help")) {
            Console.WriteLine(Schema);
            return;
        }
        var output = GetArgumentValue(arguments, "--output");
        if (output == null) {
            Console.Error.Write("Required inputs Missing!");
            MissingArguments();
            Environment.Exit(1);
        }
        var buildFile = GetArgumentValue(arguments, "--build");
        if (buildFile != null) {
            ParseBuild(buildFile);
        } else {
            ParseSingle(arguments);
        }
        var outputFolder = Path.GetDirectoryName(output);
        WriteDictionary(outputFolder, Compression);
        WriteScripts(outputFolder, Compression);
    }

    public static void ParseBuild(string argsFile) {
        var arguments = File.ReadAllText(argsFile);
        const StringSplitOptions opt = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        foreach (var line in arguments.Split('\n', opt)) {
            var args = line.Split(' ', opt).ToList();
            ParseSingle(args);
        }
    }

    private static void ParseSingle(List<string> arguments) {
        var schema = GetArgumentValue(arguments, "--schema");
        var input = GetArgumentValue(arguments, "--input");
        var output = GetArgumentValue(arguments, "--output");
        var mode = GetArgumentValue(arguments, "--mode");
        var singleton = HasArgumentValue(arguments, "--singleton");
        var extension = GetArgumentValue(arguments, "--extension");
        Compression = GetArgumentValue(arguments, "--compression") ?? Compression;
        if (schema == null || input == null || output == null) {
            Console.Error.Write("Required inputs Missing!");
            MissingArguments();
            Environment.Exit(1);
        }
        var (xml, json, yaml) = ParseMode(mode, input);
        Setup(arguments);
        var schemaRoot = YamlSchemaReader.Read(schema);
        var count = 0;
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
        if (singleton) {
            if (xml) {
                XmlParser.ParseDirectory(input, extension ?? "", schemaRoot, writer, singleton, ref count);
            } else if (json) {
                throw new NotSupportedException("JSON not supported.");
            } else if (yaml) {
                throw new NotSupportedException("YAML not supported.");
            }
        } else {
            if (xml) {
                XmlParser.ParseDirectory(input, extension ?? "", schemaRoot, writer, singleton, ref count);
            }
            if (json) {
                throw new NotSupportedException("JSON not supported.");
            }
            if (yaml) {
                throw new NotSupportedException("YAML not supported.");
            }
        }
        WriteMain(output, Compression, singleton, count, stream);
    }

    private static void WriteMain(string output, string? compression, bool singleton, int count, MemoryStream stream) {
        using var outMain = File.Open(output, FileMode.Create);
        DeflaterOutputStream? deflateStream = null;
        var mainWriter = new BinaryWriter(outMain);
        if (compression == "zlib") {
            deflateStream = new DeflaterOutputStream(outMain, new Deflater(Deflater.BEST_COMPRESSION));
            mainWriter = new BinaryWriter(deflateStream);
        }
        if (!singleton) {
            mainWriter.Write(Literal.ArrayIndex(count));
        }
        mainWriter.Write(stream.GetBuffer(), 0, (int) stream.Position);
        deflateStream?.Close();
    }

    private static void WriteDictionary(string? outputFolder, string? compression) {
        if (LocalizedString.Localizations.Count == 0) {
            using var outDict = File.Open(Path.Join(outputFolder, "dict.xbin"), FileMode.Create);
            var writer = new BinaryWriter(outDict);
            DeflaterOutputStream? deflateStream = null;
            if (compression == "zlib") {
                deflateStream = new DeflaterOutputStream(outDict, new Deflater(Deflater.BEST_COMPRESSION));
                writer = new BinaryWriter(deflateStream);
            }
            writer.Write(Literal.ArrayIndex(Serialize.Dictionary.Count));
            foreach (var str in Serialize.Dictionary.Enumerate()) {
                writer.WriteString(str);
            }
            deflateStream?.Close();
        } else {
            foreach (var localization in LocalizedString.Localizations) {
                using var outDict = File.Open(Path.Join(outputFolder, $"{localization}.xbin"), FileMode.Create);
                var writer = new BinaryWriter(outDict);
                DeflaterOutputStream? deflateStream = null;
                if (compression == "zlib") {
                    deflateStream = new DeflaterOutputStream(outDict, new Deflater(Deflater.BEST_COMPRESSION));
                    writer = new BinaryWriter(deflateStream);
                }
                writer.Write(Literal.ArrayIndex(Serialize.Dictionary.Count));
                foreach (var str in Serialize.Dictionary.Enumerate(localization)) {
                    writer.WriteString(str);
                }
                deflateStream?.Close();
            }
        }
    }
    
    private static void WriteScripts(string? outputFolder, string? compression) {
        using var outScripts = File.Open(Path.Join(outputFolder, "scripts.xbin"), FileMode.Create);
        var writer = new BinaryWriter(outScripts);
        DeflaterOutputStream? deflateStream = null;
        if (compression == "zlib") {
            deflateStream = new DeflaterOutputStream(outScripts, new Deflater(Deflater.BEST_COMPRESSION));
            writer = new BinaryWriter(deflateStream);
        }
        writer.Write(Literal.ArrayIndex(Serialize.Scripts.Count));
        foreach (var str in Serialize.Scripts.Enumerate()) {
            if (CSharpScriptCompiler.DoCompile) {
                CSharpScriptCompiler.Compile(writer, str);
            } else {
                writer.WriteString(str);
            }
        }
        deflateStream?.Close();
    }
}



