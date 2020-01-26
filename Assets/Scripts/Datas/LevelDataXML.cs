using System.Xml;
using System.Xml.Serialization;

public class LevelDataXML {
	[XmlAttribute("isBonus")] public bool isBonus;
	[XmlAttribute("parMoves")] public int parMoves;
	[XmlAttribute("name")] public string name;
	[XmlAttribute("desc")] public string desc;
	[XmlAttribute("layout")] public string layout;
}