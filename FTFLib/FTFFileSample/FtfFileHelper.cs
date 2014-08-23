using System;
using System.Security.Cryptography;

public class FtfFileHelper
{
	public static string readInfo(string filename)
	{
		string s = "";
		FtfFileReader reader = new FtfFileReader();
		Byte[] content;

		if (reader.open(filename))
		{
			s += "Global tags:\n" + reader.ftfContent.globalHeader.getTags();
			content = reader.readElement();
			s += "\n\nElement tags:\n" + reader.ftfContent.currentElement.header.getTags();
		}
		reader.close();

		return s;
	}

	public static Byte[] readFile(string filename)
	{
		FtfFileReader reader = new FtfFileReader();
		byte[] content = null;
		bool hashOk;

		if (reader.open(filename))
		{
			content = reader.readElement();
		}
		hashOk = reader.readHash();
		reader.close();

		return content;
	}

	public static bool writeFile(string filename, Byte[] content)
	{
		bool ok = false;

		FtfFileWriter writer = new FtfFileWriter();
		writer.setGlobalTag(FtfTag.applicationLabel, "FTFFileLib");
		writer.setGlobalTag(FtfTag.elementsLabel, "1");
		if (writer.open(filename, new SHA512Managed()))
		{
			ok = true;
			writer.setElementTag(FtfTag.elementLabel, "0");
			ok &= writer.writeElement(content);
		}
		ok &= writer.close();

		return ok;
	}

}
