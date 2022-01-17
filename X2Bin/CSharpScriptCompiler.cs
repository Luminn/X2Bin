using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace X2Bin; 

public class CSharpScriptCompiler {

    public static bool DoCompile;

    public static Type? Globals;
    public static ScriptOptions? Options;
    private const string GlobalsAttributeName = "X2BGlobals";
    private const string ScriptOptionsAttributeName = "X2BScriptOptions";
    
    private static void ParseGlobals(Assembly assembly) {
        if (Globals != null) return;
        foreach (var type in assembly.GetTypes()) {
            foreach (var attr in type.GetCustomAttributes()) {
                if (attr.GetType().Name == GlobalsAttributeName) {
                    Globals = type;
                    return;
                }
            }
        }
        throw new Exception($"Loading CSharpScript Failed: {GlobalsAttributeName} not found.");
    }
    
    private static void ParseOptions(Assembly assembly) {
        foreach (var type in assembly.GetTypes()) {
            foreach (var prop in type.GetProperties()) {
                if (prop.PropertyType.Name != "ScriptOptions") continue;
                foreach (var attr in prop.GetCustomAttributes()) {
                    if (attr.GetType().Name == ScriptOptionsAttributeName) {
                        Options = (ScriptOptions?) prop.GetValue(null, null);
                        return;
                    }
                }
            }
            foreach (var field in type.GetFields()) {
                if (field.FieldType.Name != "ScriptOptions") continue;
                foreach (var attr in field.GetCustomAttributes()) {
                    if (attr.GetType().Name == ScriptOptionsAttributeName) {
                        Options = (ScriptOptions?) field.GetValue(null);
                        return;
                    }
                }
            }
        }
        throw new Exception($"Loading CSharpScript Failed: {ScriptOptionsAttributeName} not found.");
    }

    public static void Setup(string assemblyFile) {
        var assembly = Assembly.LoadFrom(Path.GetFullPath(assemblyFile));
        ParseGlobals(assembly);
        ParseOptions(assembly);
        DoCompile = true;
    }
    
    public static void Compile(BinaryWriter writer, string script) {
        var compiled = CSharpScript.Create(script, Options, Globals).GetCompilation();
        var (peStream, pdbStream) = (new MemoryStream(), new MemoryStream());
        compiled.Emit(peStream, pdbStream);
        var peBuffer = peStream.GetBuffer();
        var pdbBuffer = pdbStream.GetBuffer();
        var r1 = (int)peStream.Position;
        var r2 = (int)pdbStream.Position;
        writer.Write(Literal.ArrayIndex(r1));
        writer.Write(peBuffer, 0, r1);
        writer.Write(Literal.ArrayIndex(r2));
        writer.Write(pdbBuffer, 0, r2);
    }
}