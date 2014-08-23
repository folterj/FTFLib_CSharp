using System;
using System.IO;
using System.Collections;

public class FtfContent
{
	public static byte[] fileHeader = new byte[] { 0x46, 0x54, 0x46, 0x4F, 0x52, 0x4D, 0x41, 0x54 };
	public static ulong endianness = 0x454E4449414E4553;

	public FtfHeader globalHeader = new FtfHeader();

	public FtfElement currentElement;

	public bool reverse;

	public FtfContent()
	{
		reverse = false;
	}

	public FtfTag getTag(string label)
	{
		FtfTag tag = null;

		if (currentElement != null)
		{
			// look in current element header first
			tag = currentElement.getTag(label);
		}
		if (tag == null)
		{
			// if not found; look in global header
			tag = globalHeader.getTag(label);
		}
		return tag;
	}

	public void setGlobalTag(string label, string content)
	{
		globalHeader.setTag(label, content);
	}

	public void setTag(string label, string content)
	{
		if (currentElement == null)
		{
			currentElement.setTag(label, content);
		}
	}

	public bool readHeader(Stream stream)
	{
		byte[] fileHeader0 = new byte[fileHeader.Length];
		ulong endianness0;

		stream.Read(fileHeader0, 0, fileHeader.Length);
		// check if matching header
		if (StructuralComparisons.StructuralEqualityComparer.Equals(fileHeader, fileHeader0))
		{
			// check if size fields need reversing
			endianness0 = Util.readUInt64(stream, false);
			reverse = (endianness0 != endianness);

			globalHeader.read(stream, reverse);

			return true;
		}
		return false;
	}

	public bool readElement(Stream stream, bool readContent)
	{
		currentElement = new FtfElement(stream, reverse, readContent);
		if (currentElement != null)
		{
			return true;
		}
		return false;
	}

	public bool writeHeader(Stream stream)
	{
		try
		{
			stream.Write(fileHeader, 0, fileHeader.Length);
			stream.Write(BitConverter.GetBytes(endianness), 0, sizeof(ulong));
			return globalHeader.write(stream, 0);
		}
		catch (Exception)
		{
		}
		return false;
	}

	public bool writeElement(Stream stream, FtfElement element)
	{
		try
		{
			element.write(stream);
			currentElement = element;
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public FtfElement findElement(Stream stream, string label)
	{
		FtfElement foundElement = null;
		FtfElement element = new FtfElement();
		FtfTag tag;

		while (stream.Position < stream.Length)
		{
			element.read(stream, reverse, true);
			tag = element.getTag(FtfTag.labelLabel);
			if (tag != null)
			{
				foundElement = element;
				break;
			}
		}
		return foundElement;
	}

	public FtfElement findElement(Stream stream, string label, string content)
	{
		FtfElement foundElement = null;
		FtfElement element = new FtfElement();
		FtfTag tag;

		while (stream.Position < stream.Length)
		{
			element.read(stream, reverse, true);
			tag = element.getTag(FtfTag.labelLabel);
			if (tag != null)
			{
				if (tag.content == content)
				{
					foundElement = element;
					break;
				}
			}
		}
		return foundElement;
	}

	public byte[] getCurrentElementContent()
	{
		return currentElement.content;
	}

}
