namespace X2Bin; 

public partial record Color(byte R, byte G, byte B, byte A) {
    
    public static Color FromString(string str){
        if (string.IsNullOrWhiteSpace(str)) {
            return new Color(0, 0, 0, 0);
        }
        if (BaseColors.ContainsKey(str.ToLower())) {
            return BaseColors[str.ToLower()];
        }
        if (str[0] == '#') {
            str = str[1..];
        }
        if (str.Length != 6) {
            ErrorReport.ReportError($"Unable to parse Color {str}.");
        }
        var r = Convert.ToByte(str[..2], 16);
        var g = Convert.ToByte(str[2..4], 16);
        var b = Convert.ToByte(str[4..6], 16);
        var a = Convert.ToByte(str[6..], 16);
        return new Color(r, g, b, a);
    }
}