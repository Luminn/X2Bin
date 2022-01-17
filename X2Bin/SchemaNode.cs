namespace X2Bin; 

public class SchemaNode {
    
    public static Literal[] ParseTuple(string str, int length, bool isCsv) {
        const StringSplitOptions opt = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        var splits = str.Split(isCsv ? ',' : ' ', opt);
        if (splits.Length != length) {
            ErrorReport.ReportError($"{str.Trim()} has {splits.Length} items, doesn't match TUPLE{length}.");
        }
        var result = new Literal[length];
        for (var i = 0; i < length; i++) {
            if (int.TryParse(splits[i], out var intVal)) {
                result[i] = new Literal{IntValue = intVal, Type = "INT"};
            } else {
                result[i] = new Literal {StringValue = splits[i], Type = "STRING"};
            }
        }
        return result;
    }
    
    private static readonly Literal[] ArrayIndex0 = {Literal.ArrayIndex(0)};

    public static readonly Dictionary<string, SchemaNode> RecursiveNodes = new();

    public string? Type;
    public Literal[]? DefaultValue;
    public string? Name;       
    public bool IsAttribute;
    public bool IsTextNode;
    public bool IsEnum;
    
    
    public string? EnumType;
    public bool IsNameNode;
    public bool IsArray;
    public bool WriteExist;
    public List<SchemaNode> Childs = new();

    public SchemaNode CopyAs(string name) {
        var realName = name.Trim("*?$+~".ToCharArray());
        if (name == "$any") { realName = null; }
        return new SchemaNode {
            Name = realName, DefaultValue = DefaultValue,
            IsArray = IsArray || name.EndsWith('*'),
            WriteExist = WriteExist || name.EndsWith('?'),
            Childs = Childs
        };
    }

    public int TupleLength {
        get {
            if (Type == null) return 0;
            if (Type.StartsWith("TUPLE") && Type.Length == 6 && Type[^1] <= '9' && Type[^1] >= '2') {
                return Type[^1] - '0';
            }
            if (Type.StartsWith("CSV") && Type.Length == 4 && Type[^1] <= '9' && Type[^1] >= '2') {
                return Type[^1] - '0';
            }
            if (Type == "PAIR") {
                return 2;
            }
            return 0;
        }
    }

    public bool IsCSV => Type!.StartsWith("CSV");
    public bool IsInnerTextNode => IsTextNode && Name == null;
    public bool IsAny => Name == null && IsArray;

    public Literal[] Serialize(string? str) {
        if (Type == "EXIST") {
            return new[] {Literal.ParseExist(str)};
        }
        if (Type == "NO_EXIST") {
            return new[] {Literal.ParseNoExist(str)};
        }
        if (string.IsNullOrEmpty(str)) {
            return new[] {DefaultValue![0]};
        }
        if (IsEnum) {
            return new[] {Literal.ArrayIndex(EnumParser.Parse(EnumType!, str))};
        }
        if (TupleLength >= 2) {
            return ParseTuple(str, TupleLength, IsCSV);
        }
        return new[] {Literal.Parse(str, Type)};
    }

    public static SchemaNode Complex(string name, Literal[]? defaultValue) {
        var realName = name.Trim("*?$+~".ToCharArray());
        if (name == "$name") {
            return new SchemaNode {
                IsNameNode = true, DefaultValue = defaultValue
            };
        } else if (name == "$value") {
            return new SchemaNode {
                IsTextNode = true, DefaultValue = defaultValue
            };
        } else if (name == "$default") {
            ErrorReport.ReportError("$default shouldn't be called!");
            return new SchemaNode();
        } else if (name == "$any") {
            return new SchemaNode {
                IsArray = true, DefaultValue = defaultValue
            };
        } else if (name == "$attrs") {
            return new SchemaNode {
                IsArray = true, DefaultValue = defaultValue, IsAttribute = true
            };
        } else {
            var node = new SchemaNode {
                Name = realName,
                IsAttribute = name.StartsWith("~"),
                IsArray = name.EndsWith("*"),
                WriteExist = name.EndsWith("?"),
                DefaultValue = defaultValue ?? (name.EndsWith("+") ? ArrayIndex0 : null)
            };
            if (name.StartsWith("$")) {
                RecursiveNodes[realName] = node;
            }
            return node;
        }
    }

    
    public static SchemaNode From(string name, string type, Literal[]? defaultValue) {
        var realName = name.Trim("*?$+~!".ToCharArray());
        defaultValue ??= Literal.Default(type);
        if (name == "$name") {
            return new SchemaNode {
                IsNameNode = true, Type = type, DefaultValue = defaultValue
            };
        } else if (name == "$value") {
            return new SchemaNode {
                IsTextNode = true, Type = type, DefaultValue = defaultValue
            };
        } else if (name == "$default") {
            ErrorReport.ReportError("$default shouldn't be called!");
            return new SchemaNode();
        } else if (name == "$any") {
            return new SchemaNode {
                IsArray = true, Type = type, DefaultValue = defaultValue, IsTextNode = true
            };
        } else if (name == "$attrs") {
            return new SchemaNode {
                IsArray = true, Type = type, DefaultValue = defaultValue, IsTextNode = true, IsAttribute = true
            };
        } else if (type.StartsWith("$")) {
            return RecursiveNodes[type[1..]].CopyAs(name);
        } else {
            var node = new SchemaNode {
                Name = realName, 
                Type = type, 
                DefaultValue = name.EndsWith("!") ? null : defaultValue,
                IsAttribute = name.StartsWith("~"),
                IsArray = name.EndsWith("*"), 
                IsTextNode = true,
            };
            if (name.StartsWith("$")) {
                ErrorReport.ReportWarning($"Trying to declare a recursive literal node {name} {type}.");
            }
            return node;
        }
    }
    
    public static SchemaNode Enum(string name, string className, string? defaultValue=null) {
        var realName = name.Trim("*?$+~!".ToCharArray());
        Literal[] defaultVal;
        if (defaultValue == null) {
            defaultVal = new[] {Literal.ArrayIndex(0)};
        } else {
            defaultVal = new[] {Literal.ArrayIndex(EnumParser.Parse(className, defaultValue))};
        }
        return new SchemaNode {
            Name = realName,
            Type = "INT",
            EnumType = className,
            DefaultValue = name.EndsWith("!") ? null : defaultVal,
            IsEnum = true,
            IsTextNode = true,
            IsAttribute = name.StartsWith("~"),
            IsArray = name.EndsWith("*"),
        };
    }
}