using System;
using System.IO;

public class FtfElement
{
	public FtfHeader header = new FtfHeader();
	public byte[] content;

	public FtfElement()
	{
	}

	public FtfElement(Stream stream, bool reverse, bool readContent)
	{
		read(stream, reverse, readContent);
	}

	public FtfTag getTag(string label)
	{
		return header.getTag(label);
	}

	public void setTag(string label, string content)
	{
		header.setTag(label, content);
	}

	public bool read(Stream stream, bool reverse, bool readContent)
	{
		bool ok = header.read(stream, reverse);
    
		if (readContent && ok)
		{
			ok = readElementContent(stream);
		}
		return ok;
	}

	public bool readElementContent(Stream stream)
	{
		try
		{
			content = new byte[(int)header.contentSize];
			stream.Read(content, 0, (int)header.contentSize);
			return true;
		}
		catch(Exception)
		{
		}
		return false;
	}

	public bool skipElementContent(Stream stream)
	{
		try
		{
			stream.Seek((long)header.contentSize, SeekOrigin.Current);
			return true;
		}
		catch(Exception)
		{
		}
		return false;
	}

	public bool write(Stream stream)
	{
		try
		{
			header.write(stream, (ulong)content.Length);
			stream.Write(content, 0, content.Length);
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

}
