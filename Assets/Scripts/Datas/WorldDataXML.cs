using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot("LevelsCollection")]
public class WorldDataXML {
	public string testString;

	[XmlArray("Levels")]
	[XmlArrayItem("Level")]
	public List<LevelDataXML> levelDataXMLs = new List<LevelDataXML>();
}
