using System.Text;
using System.Text.RegularExpressions;

namespace X2Bin; 

public static class Parsers {

    public static Encoding StringEncoding;

    static Parsers() {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        StringEncoding = Encoding.UTF8;
    }
    public static void Int7Serializer(BinaryWriter writer, int value) {
        writer.Write7BitEncodedInt(value);
    }
    
    public static void Int8Serializer(BinaryWriter writer, int value) {
        writer.Write((byte) value);
    }
    
    public static void Int16Serializer(BinaryWriter writer, int value) {
        writer.Write((short) value);
    }
    
    public static void Int32Serializer(BinaryWriter writer, int value) {
        writer.Write(value);
    }
    public static void Int64Serializer(BinaryWriter writer, int value) {
        writer.Write((long) value);
    }
    
    public static void Int7Encoder(BinaryWriter writer, string value) {
        if (ReferenceEquals(StringEncoding, Encoding.UTF8)) {
            writer.Write(value);
        } else {
            var buffer = StringEncoding.GetBytes(value);
            writer.Write7BitEncodedInt(buffer.Length);
            writer.Write(buffer);
        }
    }
    
    public static void Int8Encoder(BinaryWriter writer, string value) {
        var buffer = StringEncoding.GetBytes(value);
        writer.Write((byte) buffer.Length);
        writer.Write(buffer);
    }
    
    public static void Int16Encoder(BinaryWriter writer, string value) {
        var buffer = StringEncoding.GetBytes(value);
        writer.Write((short) buffer.Length);
        writer.Write(buffer);
    }
    
    public static void Int32Encoder(BinaryWriter writer, string value) {
        var buffer = StringEncoding.GetBytes(value);
        writer.Write(buffer.Length);
        writer.Write(buffer);
    }
    
    public static void Int64Encoder(BinaryWriter writer, string value) {
        var buffer = StringEncoding.GetBytes(value);
        writer.Write((long) buffer.Length);
        writer.Write(buffer);
    }
    
    public static void FloatSerializer(BinaryWriter writer, double value) {
        writer.Write((float) value);
    }
    
    public static void DoubleSerializer(BinaryWriter writer, double value) {
        writer.Write(value);
    }

    public static void NullTerminatedEncoder(BinaryWriter writer, string value) {
        var buffer = StringEncoding.GetBytes(value);
        writer.Write((long) buffer.Length);
        writer.Write(buffer);
    }

    public static string AggressiveTrimmer(string text) {
        if (string.IsNullOrWhiteSpace(text)) {
            return "";
        }
        StringBuilder builder = new();
        foreach (var line in text.Trim().Split("\n")) {
            builder.Append(line.Trim());
            builder.Append('\n');
        }
        return builder.ToString().Trim();
    }
    
    public static string PassiveTrimmer(string text) {
        if (string.IsNullOrWhiteSpace(text)) {
            return "";
        }
        return text.Trim();
    }

    public static string NoTrimmer(string text) {
        return text;
    }

    public static string CCodeTrimmer(string text) {
        text = Regex.Replace(text, @"\\\\.*$", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\\\*.*\*\\", "", RegexOptions.Singleline);
        text = Regex.Replace(text, @"\s+", " ");
        text = Regex.Replace(text, @"\s*([{}\[\]()*<>=;!^&|+\-%?.:])\s*", @"$1");
        return text.Trim();
    }
    
    
}