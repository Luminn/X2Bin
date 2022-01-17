using System.Text;

namespace X2Bin;

public delegate void IntSerializer(BinaryWriter writer, int value);
public delegate void StringSerializer(BinaryWriter writer, string value);
public delegate string StringProcessor(string input);

public static class Serialize {
    
    public static readonly Codex Dictionary = new ();
    public static readonly Codex Scripts = new ();

    public static IntSerializer IntSerializer = Parsers.Int7Serializer;
    public static StringSerializer StringIndexSerializer = Parsers.Int7Encoder;

    public static StringProcessor StringProcessor = Parsers.AggressiveTrimmer;
    public static StringProcessor CodeProcessor = Parsers.CCodeTrimmer;

    public static void SetIntSerializer(string arg) {
        IntSerializer = arg.ToLower() switch {
            "7bit" or "int7" => Parsers.Int7Serializer,
            "int8" => Parsers.Int8Serializer,
            "int16" => Parsers.Int16Serializer,
            "int32" => Parsers.Int32Serializer,
            "int64" => Parsers.Int64Serializer,
            _ => throw new Exception($"Error: Unknown INT serializer option {arg}.")
        };
    }
    
    public static void SetStringIndexSerializer(string arg) {
        StringIndexSerializer = arg.ToLower() switch {
            "7bit" or "int7" => Parsers.Int7Encoder,
            "int8" => Parsers.Int8Encoder,
            "int16" => Parsers.Int16Encoder,
            "int32" => Parsers.Int32Encoder,
            "int64" => Parsers.Int64Encoder,
            "nullterm" => Parsers.NullTerminatedEncoder,
            _ => throw new Exception($"Error: Unknown string index serializer option {arg}.")
        };
    }
    
    public static void SetStringProcessor(string arg) {
        StringProcessor = arg.ToLower() switch {
            "aggressive" => Parsers.AggressiveTrimmer,
            "passive" => Parsers.PassiveTrimmer,
            "no" => Parsers.NoTrimmer,
            _ => throw new Exception($"Error: Unknown STRING processor option {arg}.")
        };
    }
    
    public static void SetScriptProcessor(string arg) {
        CodeProcessor = arg.ToLower() switch {
            "cstyle" => Parsers.CCodeTrimmer,
            "aggressive" => Parsers.AggressiveTrimmer,
            "passive" => Parsers.PassiveTrimmer,
            "no" => Parsers.NoTrimmer,
            _ => throw new Exception($"Error: Unknown SCRIPT processor option {arg}.")
        };
    }

    public static void SetEncoding(string encoding) {
        if (int.TryParse(encoding, out var codePage)) {
            Parsers.StringEncoding = Encoding.GetEncoding(codePage);
        } else {
            Parsers.StringEncoding = Encoding.GetEncoding(encoding);
        }
    }


    public static void WriteInt(this BinaryWriter writer, int literal) {
        IntSerializer(writer, literal);
    }
    
    public static void WriteString(this BinaryWriter writer, string literal) {
        StringIndexSerializer(writer, literal);
    }

    public static void Write(this BinaryWriter writer, Literal literal) {
        unchecked {
            switch (literal.Type) {
                case "INT":
                    writer.WriteInt((int) literal.IntValue);
                    break;
                case "INT8" or "BYTE":
                    writer.Write((byte) literal.IntValue);
                    break;
                case "INT16" or "SHORT":
                    writer.Write((short) literal.IntValue);
                    break;
                case "INT32":
                    writer.Write((int) literal.IntValue);
                    break;
                case "INT64" or "LONG":
                    writer.Write(literal.IntValue);
                    break;
                case "INT7": 
                    writer.Write7BitEncodedInt((int) literal.IntValue);
                    break;
                case "LONG7": 
                    writer.Write7BitEncodedInt64(literal.IntValue);
                    break;
                case "BOOL" or "EXIST" or "NO_EXIST":
                    writer.Write(literal.IntValue != 0);
                    break;
                case "HALF":
                    writer.Write((Half) literal.FloatValue);
                    break;
                case "FLOAT" or "SINGLE":
                    writer.Write((float)literal.FloatValue);
                    break;
                case "DOUBLE":
                    writer.Write(literal.FloatValue);
                    break;
                case "ANGLE":
                    if (Literal.DefaultDouble) {
                        writer.Write(literal.FloatValue * Math.PI / 180f);
                    } else {
                        writer.Write((float)literal.FloatValue * MathF.PI / 180f);
                    }
                    break;
                case "STRING" or "LOCAL":
                    writer.WriteInt(Dictionary.InsertOrGet(StringProcessor(literal.StringValue)));
                    break;
                case "STRING_TRIM":
                    writer.WriteInt(Dictionary.InsertOrGet(Parsers.AggressiveTrimmer(literal.StringValue)));
                    break;
                case "STRING_NOTRIM":
                    writer.WriteInt(Dictionary.InsertOrGet(literal.StringValue));
                    break;
                case "RAW":
                    writer.WriteString(StringProcessor(literal.StringValue));
                    break;
                case "RAW_TRIM":
                    writer.WriteString(Parsers.AggressiveTrimmer(literal.StringValue));
                    break;
                case "RAW_NOTRIM":
                    writer.WriteString(literal.StringValue);
                    break;
                case "CODE" or "SCRIPT":
                    writer.WriteInt(Scripts.InsertOrGet(CodeProcessor(literal.StringValue)));
                    break;
                case "COLOR":
                    writer.Write(Color.FromString(literal.StringValue));
                    break;
                default:
                    throw new Exception($"Error: Unknown literal type {literal.Type}.");
            }
        }
    }
    
    public static void Write(this BinaryWriter writer, Color color) {
        writer.Write(color.R);
        writer.Write(color.G);
        writer.Write(color.B);
        writer.Write(color.A);
    }

    public static void Write(this BinaryWriter writer, IEnumerable<Literal> literals) {
        foreach (var l in literals) {
            writer.Write(l);
        }
    }
    
    public static void Write(this BinaryWriter writer, LocalizedString localizations) {
        writer.WriteInt(Dictionary.InsertOrGet(localizations));
    }

}