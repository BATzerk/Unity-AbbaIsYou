using System.Xml;
using System.Xml.Serialization;

public class LevelDataXML {
	[XmlAttribute("desc")] public string desc;
	[XmlAttribute("fueID")] public string fueID;
    
	[XmlAttribute("layout")] public string layout;
    
    [XmlAttribute("zoom")] public float zoom=1f;
}