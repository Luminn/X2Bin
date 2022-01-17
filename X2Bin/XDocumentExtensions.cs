using System.Xml.Linq;

namespace X2Bin; 

public static class XDocumentExtensions {

    public static XAttribute? GetAttribute(this XElement element, string names) {
        return (from name in names.Split('|') where element.Attribute(name) != null 
            select element.Attribute(name)).FirstOrDefault();
    }
    
    public static XAttribute[] GetAttributes(this XElement element, string names) {
        return (from name in names.Split('|') select element.Attributes(name))
            .SelectMany(x => x).ToArray();
    }
    
    public static XElement? GetElement(this XElement element, string names) {
        return (from name in names.Split('|') where element.Element(name) != null 
            select element.Element(name)).FirstOrDefault();
    }
    
    public static XElement[] GetElements(this XElement element, string names) {
        return (from name in names.Split('|') select element.Elements(name))
            .SelectMany(x => x).ToArray();
    }
}