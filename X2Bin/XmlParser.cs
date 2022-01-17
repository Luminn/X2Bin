using System.Xml;
using System.Xml.Linq;

namespace X2Bin; 

public static class XmlParser {

    public static void ParseNode(XElement? xmlNode, SchemaNode schemaNode, BinaryWriter outStream) {
        if (schemaNode.WriteExist) {
            outStream.Write(xmlNode != null);
        }
        if (xmlNode == null) {
            if (schemaNode.WriteExist) return;
            if (schemaNode.DefaultValue == null) {
                ErrorReport.ReportError($"Required XML node {schemaNode.Name} missing.");
            }
            foreach (var i in schemaNode.DefaultValue!) {
                outStream.Write(i);
            }
            return;
        }
        ErrorReport.Element = xmlNode.Name.NamespaceName;
        ErrorReport.LineNumber = ((IXmlLineInfo) xmlNode).LineNumber;
        if (schemaNode.Type is "LOCAL" or "LOCAL_TRIM" or "LOCAL_NOTRIM") {
            ParseLocalization(xmlNode, schemaNode, outStream);
            return;
        } else if (schemaNode.IsTextNode) {
            outStream.Write(schemaNode.Serialize(xmlNode.Value));
            return;
        }
        foreach (var node in schemaNode.Childs) {
            var exclude = new List<XObject?>();
            if (node.IsNameNode) {
                outStream.Write(node.Serialize(xmlNode.Name.LocalName));
            } else if (node.IsAttribute && node.IsAny) {
                var attrs = xmlNode.Attributes().Where(x => !exclude.Contains(x)).ToArray();
                outStream.Write(Literal.ArrayIndex(attrs.Length));
                foreach (var attr in attrs) {
                    exclude.Add(attr);
                    outStream.Write(schemaNode.Serialize(attr.Value));
                }
            } else if (node.IsAttribute) {
                var attr = xmlNode.GetAttribute(node.Name!);
                exclude.Add(attr);
                outStream.Write(node.Serialize(attr?.Value));
            } else if (node.IsInnerTextNode) {
                outStream.Write(node.Serialize(xmlNode.Value));
            } else if (node.IsAny) {
                var childs = xmlNode.Elements().Where(x => !exclude.Contains(x)).ToArray();
                outStream.Write(Literal.ArrayIndex(childs.Length));
                foreach (var elem in childs) {
                    exclude.Add(elem);
                    ParseNode(elem, node, outStream);
                }
            } else if (node.IsArray) {
                var childs = xmlNode.GetElements(node.Name!).Where(x => !exclude.Contains(x)).ToArray();
                outStream.Write(Literal.ArrayIndex(childs.Length));
                foreach (var elem in childs) {
                    exclude.Add(elem);
                    ParseNode(elem, node, outStream);
                }
            } else {
                var elem = xmlNode.GetElement(node.Name!);
                exclude.Add(elem);
                ParseNode(elem, node, outStream);
            }
        }
    }

    private static void ParseLocalization(XElement xmlNode, SchemaNode schemaNode, BinaryWriter outStream) {
        LocalizedString localizedString;
        if (xmlNode.HasElements) {
            localizedString = LocalizedString.Create(xmlNode.Elements().Select(x => (x.Name.LocalName, x.Value)), schemaNode.Type!);
        } else {
            if (!string.IsNullOrWhiteSpace(xmlNode.Value)) {
                localizedString = new LocalizedString(xmlNode.Value);
            } else {
                localizedString = new LocalizedString(schemaNode.DefaultValue?[0].StringValue ?? "");
            }
        }
        outStream.Write(localizedString);
    }

    public static void ParseFile(string fileName, SchemaNode root, BinaryWriter outStream, bool singleton, ref int count) {
        using var file = File.OpenRead(fileName);
        XDocument? document;
        try {
            document = XDocument.Load(file);
        } catch (InvalidCastException) {
            ErrorReport.ReportWarning($"Invalid xml file {fileName}.", false);
            return;
        }
        var rootElem = document.Root!;
        if (rootElem.Name.LocalName == root.Name) {
            ErrorReport.FileName = fileName;
            ParseNode(rootElem, root, outStream);
            count += 1;
        } else {
            foreach (var elem in rootElem.Elements("ParseNodes")) {
                ErrorReport.FileName = fileName;
                if (elem.Name.LocalName != root.Name) continue;
                ParseNode(elem, root, outStream);
                count += 1;
                if (singleton) { return; }
            }
        }
    }

    public static void ParseDirectory(string fileName, string extension, SchemaNode root, BinaryWriter outStream, bool singleton, ref int count) {
        if (!File.GetAttributes(fileName).HasFlag(FileAttributes.Directory)){
            ParseFile(fileName, root, outStream, singleton, ref count);
            return;
        }
        if (singleton) {
            ErrorReport.ReportWarning("Pasrsing directory with singleton.");
        }
        var files = Directory.GetFiles(fileName, $"*{extension}.xml", SearchOption.AllDirectories);
        foreach (var file in files) {
            ParseFile(file, root, outStream, singleton, ref count);
            if (singleton && count > 0) {
                return;
            }
        }
    }
}