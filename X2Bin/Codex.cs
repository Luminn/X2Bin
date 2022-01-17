namespace X2Bin;

public class LocalizedString {

    public static readonly HashSet<string> Localizations = new ();

    public readonly string NotLocalized;
    public readonly Dictionary<string, string>? Localized;

    public static readonly LocalizedString Empty = new ("");

    public LocalizedString(string str) {
        NotLocalized = str;
    }

    public static LocalizedString Create(IEnumerable<(string, string)> localized, string type) {
        switch (type) {
            case "LOCAL":
                var str1 = localized
                    .Select(x => (x.Item1.ToUpper(), Serialize.StringProcessor(x.Item2))).ToArray();
                return new LocalizedString(str1);
            case "LOCAL_TRIM":
                var str2 = localized
                    .Select(x => (x.Item1.ToUpper(), Parsers.AggressiveTrimmer(x.Item2))).ToArray();
                return new LocalizedString(str2);
            case "LOCAL_NOTRIM":
                var str3 = localized
                    .Select(x => (x.Item1.ToUpper(), x.Item2)).ToArray();
                return new LocalizedString(str3);
            default:
                throw new Exception($"Unknown localized string type {type}.");
        }
    }
    
    protected LocalizedString((string, string)[] localized) {
        Localized = new Dictionary<string, string>();
        NotLocalized = localized[0].Item2;
        foreach (var (lang, str) in localized) {
            Localizations.Add(lang);
            Localized[lang] = str;
        }
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is LocalizedString localizedString) {
            if (Localized is null && NotLocalized == localizedString.NotLocalized) {
                return true;
            }
        }
        return false;
    }

    public override int GetHashCode() {
        return  Localized?.GetHashCode() ?? NotLocalized.GetHashCode();
    }
    
}

public class Codex {

    public readonly Dictionary<LocalizedString, int> Dict = new() {[LocalizedString.Empty]=0};
    public readonly List<LocalizedString> Ordering = new() {LocalizedString.Empty};
    public int Count => Ordering.Count;

    public int InsertOrGet(string key) {
        return InsertOrGet(new LocalizedString(key));
    }
    
    public int InsertOrGet(LocalizedString key) {
        if (Dict.ContainsKey(key)) {
            return Dict[key];
        }
        Dict[key] = Count;
        Ordering.Add(key);
        return Count - 1;
    }

    public IEnumerable<string> Enumerate() {
        return Ordering.Select(x => x.NotLocalized);
    }
    
    public IEnumerable<string> Enumerate(string localization) {
        return Ordering.Select(x => x.Localized?[localization] ?? x.NotLocalized);
    }

}
