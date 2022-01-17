namespace X2Bin; 

public class Literal {

    protected static HashSet<string> FalseWords = new() {
        "false", "no", "none"
    };

    public static bool DefaultDouble = false;

    protected string? _Type; 
    public string Type {
        get => _Type!;
        init => _Type = value.ToUpper();
    }

    public long IntValue { get; init; }
    public double FloatValue { get; init; }
    public string StringValue { get; init; } = string.Empty;

    // since false is usually represented as no_exist
    // anything that's not spaces or "false" or "no" is true
    public static int ParseBool(string? str) {
        if (string.IsNullOrWhiteSpace(str)) return 0;
        if (FalseWords.Contains(str.ToLower())) return 0;
        return 1;
    }

    private static long? ParseInt(string obj) {
        if (string.IsNullOrWhiteSpace(obj)) {
            return null;
        }
        if (long.TryParse(obj, out var value)) {
            return value;
        }
        if (long.TryParse(obj[..^1], out value)){
            return value;
        } 
        return null;
    }

    private static double? ParseFloat(string obj) {
        if (string.IsNullOrWhiteSpace(obj)) {
            return null;
        }
        if (double.TryParse(obj, out var value)) {
            return value;
        }
        if (double.TryParse(obj[..^1], out value)){
            return value;
        } 
        return null;
    }
    
    public static Literal ArrayIndex(int value) {
        return new Literal {IntValue = value, Type = "INT"};
    }
    
    public static Literal ParseExist(string? str) {
        if (str == null || FalseWords.Contains(str)) {
            return new Literal {IntValue = 0, Type = "BOOL"};
        }
        return new Literal {IntValue = 1, Type = "BOOL"};
    }
    
    public static Literal ParseNoExist(string? str) {
        if (str == null || FalseWords.Contains(str)) {
            return new Literal {IntValue = 1, Type = "BOOL"};
        }
        return new Literal {IntValue = 0, Type = "BOOL"};
    }

    public static Literal[] Default(string type) {
        if (type.StartsWith("TUPLE") || type.StartsWith("CSV")) {
            var num = type[^1] - '0';
            var result = new Literal[num];
            for (var i = 0; i < num; i++) {
                result[i] = new Literal {Type = "INT", IntValue = 0};
            }
            return result;
        } else if (type == "PAIR") {
            return new[] {
                new Literal {Type = "INT", IntValue = 0},
                new Literal {Type = "INT", IntValue = 0}
            };
        } else if (type == "NO_EXIST") {
            return new[] {new Literal {Type = type, IntValue = 1}};
        }
        return new[] {
            new Literal {
                Type = type, IntValue = 0, FloatValue = 0f, StringValue = string.Empty
            }
        };
    }

    public static Literal Parse(string obj, string? typeSuggestion=null) {
        var floatValue = ParseFloat(obj);
        var intValue = ParseInt(obj);
        return typeSuggestion switch {
            "BOOL" => new Literal {IntValue = ParseBool(obj), Type = typeSuggestion},
            "INT" or "INT7" or "INT8" or "INT16" or "INT32" or "INT64" or "BYTE" or "SHORT" or "LONG" => 
                new Literal {IntValue = intValue!.Value, Type = typeSuggestion},
            "SINGLE" or "FLOAT" or "DOUBLE" or "HALF" or "ANGLE" => 
                new Literal {FloatValue = floatValue!.Value, Type = typeSuggestion},
            "STRING" or "STRING_TRIM" or "STRING_NOTRIM" or "RAW" or "RAW_TRIM" or "RAW_NOTRIM" 
                or "CODE" or "SCRIPT" or "LOCAL" or "COLOR" => 
                new Literal {StringValue = obj, Type = typeSuggestion},
            null when obj.ToLower() == "true" => new Literal {IntValue = 1, Type = "BOOL"},
            null when obj.ToLower() == "false" => new Literal {IntValue = 0, Type = "BOOL"},
            null when obj[^1] is 'H' or 'h' && floatValue != null => 
                new Literal{FloatValue = floatValue.Value, Type = "HALF"},
            null when obj[^1] is 'F' or 'f' && floatValue != null => 
                new Literal{FloatValue = floatValue.Value, Type = "FLOAT"},
            null when obj[^1] is 'D' or 'd' && floatValue != null => 
                new Literal{FloatValue = floatValue.Value, Type = "DOUBLE"},
            null when obj[^1] is 'B' or 'b' && intValue != null => 
                new Literal{IntValue = intValue.Value, Type = "INT8"},
            null when obj[^1] is 'S' or 's' && intValue != null => 
                new Literal{IntValue = intValue.Value, Type = "INT16"},
            null when obj[^1] is 'I' or 'i' && intValue != null => 
                new Literal{IntValue = intValue.Value, Type = "INT32"},
            null when obj[^1] is 'L' or 'l' && intValue != null => 
                new Literal{IntValue = intValue.Value, Type = "INT64"},
            null when intValue != null =>
                new Literal{IntValue = intValue.Value, Type = "INT"},
            null when floatValue != null =>
                new Literal{FloatValue = floatValue.Value, Type = DefaultDouble ? "DOUBLE": "FLOAT"},
            null =>
                new Literal {StringValue = obj, Type = "STRING"},
            _ => throw new Exception($"Error: Unknown literal type {typeSuggestion}.")

        };
    }
}
