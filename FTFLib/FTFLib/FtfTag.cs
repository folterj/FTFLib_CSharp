using System;

public class FtfTag
{
	public string label;
	public string content;

	// general tags (global)
	public static string elementsLabel = "ELEMENTS";
	public static string hashtypeLabel = "HASHTYPE";
	public static string hashsizeLabel = "HASHSIZE";
	public static string descriptionLabel = "DESCRIPTION";
	public static string applicationLabel = "APPLICATION";
	public static string authorLabel = "AUTHOR";
	public static string subjectLabel = "SUBJECT";

	// general tags (global / elements)
	public static string labelLabel = "LABEL";
	public static string elementLabel = "ELEMENT";
	public static string timestampLabel = "TIMESTAMP";
	public static string dimensionsLabel = "DIMENSIONS";

	// specific image tags:
	public static string widthLabel = "WIDTH";
	public static string heightLabel = "HEIGHT";
	public static string channelsLabel = "CHANNELS";
	public static string colormodelLabel = "COLORMODEL";
	public static string colorchannelsformatLabel = "COLORCHANNELSFORMAT";
	public static string componentformatLabel = "COMPONENTFORMAT";
	public static string componentbitsLabel = "COMPONENTBITS";
	public static string compressionLabel = "COMPRESSION";

	public FtfTag(string label, string content)
	{
		this.label = label;
		this.content = content;
	}

	public override string ToString()
	{
		string s = "";
    
		if (label != "")
		{
			s = label;
			if (content != "")
			{
				s += ":" + content;
			}
		}
		return s;
	}

}
