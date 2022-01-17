using System.Reflection;

namespace X2Bin; 

public static class EnumParser {
    public static Dictionary<(string, string), int> Parser = new();
    
    public static void Setup(string assemblyFile) {
        var assembly = Assembly.LoadFile(assemblyFile);
        foreach (var type in assembly.GetTypes().Where(x => x.IsEnum && x.IsPublic)) {
            var names = Enum.GetNames(type);
            var values = (Enum[])Enum.GetValues(type);
            for (var i = 0; i < names.Length; i++) {
                Parser[(type.Name, names[i])] = Convert.ToInt32(values[i]);
            }
        }
    }

    public static int Parse(string className, string enumName) {
        return Parser[(className, enumName)];
    }

}