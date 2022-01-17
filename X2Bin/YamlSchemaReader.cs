using YamlDotNet.RepresentationModel;

namespace X2Bin; 

public static class YamlSchemaReader {

    public static Literal[] ParseDefault(YamlNode node, string? type=null) {
        if (node is YamlScalarNode scalarNode) {
            return new [] {Literal.Parse(scalarNode.Value!, type)};
        } else if (node is YamlSequenceNode sequenceNode) {
            var list = new List<Literal>();
            foreach (var n in sequenceNode) {
                list.Add(Literal.Parse(((YamlScalarNode)n).Value!));
            }
            return list.ToArray();
        }
        throw new Exception("Mapping node is not a valid default value.");
    }

    public static SchemaNode Read(YamlScalarNode name, YamlNode value) {
        var tagName = name.Value;
        return value switch {
            YamlScalarNode scalarNode => SchemaNode.From(tagName!, scalarNode.Value!, null),
            YamlSequenceNode sequenceNode => ParseArray(sequenceNode, tagName),
            YamlMappingNode mappingNode => ParseChilds(mappingNode, tagName),
            _ => throw new Exception($"Unknown YAML node {value}!")
        };
    }

    private static SchemaNode ParseChilds(YamlMappingNode mappingNode, string? tagName) {
        var childs = new List<SchemaNode>();
        var defaultVal = null as Literal[];
        foreach (var (n, v) in mappingNode.Children) {
            if (((YamlScalarNode) n).Value == "$default") {
                defaultVal = ParseDefault(v);
            }
        }
        var node = SchemaNode.Complex(tagName!, defaultVal);
        foreach (var (n, v) in mappingNode.Children) {
            if (((YamlScalarNode) n).Value != "$default") {
                childs.Add(Read((YamlScalarNode) n, v));
            }
        }
        node.Childs = childs;
        return node;
    }

    private static SchemaNode ParseArray(YamlSequenceNode sequenceNode, string? tagName) {
        var type = ((YamlScalarNode) sequenceNode[0]).Value!.ToUpper();
        if (type == "ENUM") {
            var className = ((YamlScalarNode) sequenceNode[1]).Value!;
            if (sequenceNode.Count() < 3) return SchemaNode.Enum(type, className);
            var defaultValue = ((YamlScalarNode) sequenceNode[2]).Value!;
            return SchemaNode.Enum(type, className, defaultValue);
        } else {
            var defaultValue = ParseDefault(sequenceNode[1], type);
            return SchemaNode.From(tagName!, type, defaultValue);
        }
    }

    public static SchemaNode Read(string document, bool asLib=false) {
        ErrorReport.FileName = document;
        using var input = File.OpenRead(document);
        var reader = new StreamReader(input);
        var yaml = new YamlStream();
        yaml.Load(reader);
        var root = (YamlMappingNode)yaml.Documents[0].RootNode;
        foreach (var (k, value) in root.Children) {
            var key = (YamlScalarNode) k;
            if (key.Value!.StartsWith("$$")) { // function node
                Read(key, value);
            } else if (key.Value == "$$include") {
                var fileName = ((YamlScalarNode) value).Value!;
                Read(fileName, true);
            } else {
                return Read(key, value);
            }
        }
        if (!asLib) {
            ErrorReport.ReportError($"No root node fount in {document}!");
        }
        return new SchemaNode(); // discard
    }
}