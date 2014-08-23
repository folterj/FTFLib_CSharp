using System;
using System.Collections.Generic;
using System.IO;

public class FtfHeader
{
	public ulong tagsSize;
	public Dictionary<string, string> tags;
	public ulong contentSize;

	private ulong position;

	public FtfHeader()
	{
		tags = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		tagsSize = 0;
		contentSize = 0;
		position = 0;
	}

	public void clearTags()
	{
		tags.Clear();
	}

	public void setTag(string label, string content)
	{
		if (tags.ContainsKey(label))
		{
			tags[label] = content;
		}
		else
		{
			tags.Add(label, content);
		}
	}

	public FtfTag getTag(string label)
	{
		FtfTag tag = null;
		string content = "";
    
		if (tags.ContainsKey(label))
		{
			tags.TryGetValue(label, out content);
			tag = new FtfTag(label, content);
		}
		return tag;
	}

	public string getTags()
	{
		string s = "";
    
		foreach(KeyValuePair<string, string> tagKeyValue in tags)
		{
			if (s != "")
			{
				s += "\n";
			}
			s += string.Format("{0} : {1}",tagKeyValue.Key, tagKeyValue.Value);
		}
		return s;
	}

	public bool read(Stream stream, bool reverse)
	{
		string label;
		string content;
    
		try
		{
			tagsSize = Util.readUInt64(stream, reverse);
    
			position = 0;
			while (position < tagsSize)
			{
				// read tags until tags size is reached
				label = readString(stream, tagsSize);
				content = readString(stream, tagsSize);
				if (label != "")
				{
					setTag(label, content);
				}
			}
    
			contentSize = Util.readUInt64(stream, reverse);
    
			return true;
		}
		catch(Exception)
		{
		}
		return false;
	}

	public bool write(Stream stream, ulong contentSize)
	{
		this.contentSize = contentSize;
		tagsSize = 0;
		foreach (KeyValuePair<string, string> tag in tags)
		{
			tagsSize += (ulong)tag.Key.Length + 1 + (ulong)tag.Value.Length + 1;
		}
    
		try
		{
			stream.Write(BitConverter.GetBytes(tagsSize), 0, sizeof(ulong));
    
			foreach (KeyValuePair<string, string> tag in tags)
			{
				stream.Write(Util.getBytes(tag.Key), 0, tag.Key.Length + 1);
				stream.Write(Util.getBytes(tag.Value), 0, tag.Value.Length + 1);
			}
    
			stream.Write(BitConverter.GetBytes(contentSize), 0, sizeof(ulong));
    
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	private string readString(Stream stream, ulong maxPosition)
	{
		string s = "";
		Char c = ' ';
    
		while (c != 0 && position < maxPosition)
		{
			c = (Char)stream.ReadByte();
			position++;
			if (c != 0)
			{
				s += c;
			}
		}
		return s;
	}

}
