using System;
using System.IO;

public class FtfHelper
{
	public static byte[] readFile(string filename)
	{
		byte[] content = null;
		FtfContent ftfContent = new FtfContent();
		FileStream fileStream;

		try
		{
			fileStream = new FileStream(filename, FileMode.Open);

			ftfContent.readHeader(fileStream);
			ftfContent.readElement(fileStream, true);

			content = ftfContent.getCurrentElementContent();

			fileStream.Close();
		}
		catch (Exception)
		{
		}
		return content;
	}

	public static bool writeFile(string filename, byte[] content)
	{
		bool ok = false;
		FtfContent ftfContent = new FtfContent();
		FtfElement ftfElement = new FtfElement();
		FileStream fileStream;

		try
		{
			fileStream = File.Create(filename);

			ftfContent.setGlobalTag(FtfTag.applicationLabel, "FTFLib");
			ftfContent.writeHeader(fileStream);
			ftfElement.content = content;
			ok = ftfContent.writeElement(fileStream, ftfElement);

			fileStream.Flush();
			fileStream.Close();
		}
		catch (Exception)
		{
		}
		return ok;
	}

}
