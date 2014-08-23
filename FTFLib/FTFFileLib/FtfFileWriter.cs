using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

public class FtfFileWriter : FtfFile
{
	private FtfElement nextElement;

	private bool writeHash()
	{
		byte[] hashBytes;
    
		try
		{
			if (useHash && !hashDone)
			{
				cryptoStream.FlushFinalBlock();
				hashBytes = hashAlgorythm.Hash;
				ioStream.Write(hashBytes, 0, hashBytes.Length);
				hashDone = true;
			}
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public bool open(string filename)
	{
		this.filename = filename;
    
		try
		{
			fileStream = File.Create(filename);
			bufferedStream = new BufferedStream(fileStream, bufferSize);
			ioStream = bufferedStream;
    
			ftfContent.writeHeader(ioStream);
			nextElement = new FtfElement();
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public bool open(string filename, HashAlgorithm hashAlgorythm)
	{
		string hashtype;
    
		this.filename = filename;
		this.hashAlgorythm = hashAlgorythm;
		useHash = true;
    
		try
		{
			fileStream = File.Create(filename);
			bufferedStream = new BufferedStream(fileStream, bufferSize);
			cryptoStream = new CryptoStream(bufferedStream, hashAlgorythm, CryptoStreamMode.Write);
			ioStream = cryptoStream;
    
			if (hashAlgorythm.GetType() == typeof(SHA512Managed))
			{
				hashtype = "SHA512";
			}
			else if (hashAlgorythm.GetType() == typeof(SHA384Managed))
			{
				hashtype = "SHA384";
			}
			else if (hashAlgorythm.GetType() == typeof(SHA256Managed))
			{
				hashtype = "SHA256";
			}
			else if (hashAlgorythm.GetType() == typeof(SHA1Managed))
			{
				hashtype = "SHA1";
			}
			else
			{
				string s = hashAlgorythm.ToString();
				string[] parts = s.Split('.');
				hashtype = parts[parts.Length - 1];
			}
    
			ftfContent.setGlobalTag(FtfTag.hashtypeLabel, hashtype);
    
			ftfContent.setGlobalTag(FtfTag.hashsizeLabel, (hashAlgorythm.HashSize / 8).ToString());
    
			ftfContent.writeHeader(ioStream);
			nextElement = new FtfElement();
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public void setGlobalTag(string label, string content)
	{
		ftfContent.setGlobalTag(label, content);
	}

	public void setElementTag(string label, string content)
	{
		nextElement.setTag(label, content);
	}

	public bool writeElement(byte[] content)
	{
		return writeElement(content, FtfCompression.NONE);
	}

	public bool writeElement(byte[] content, FtfCompression compression)
	{
		bool ok;
    
		if (compression != FtfCompression.NONE)
		{
			nextElement.setTag(FtfTag.compressionLabel, compression.ToString());
		}
    
		if (compression == FtfCompression.DEFLATE)
		{
			// context for MemoryStream
			{
				MemoryStream compressedStream = new MemoryStream();
    
				// context for DeflateStream
				{
					DeflateStream zipStream = new DeflateStream(compressedStream, CompressionMode.Compress);
    
					zipStream.Write(content, 0, content.Length);
					zipStream.Close();
					nextElement.content = compressedStream.ToArray();
				}
			}
		}
		else if (compression == FtfCompression.GZIP)
		{
			// context for MemoryStream
			{
				MemoryStream compressedStream = new MemoryStream();
    
				// context for GZipStream
				{
					GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
    
					zipStream.Write(content, 0, content.Length);
					zipStream.Close();
					nextElement.content = compressedStream.ToArray();
				}
			}
		}
		else
		{
			nextElement.content = content;
		}
    
		ok = ftfContent.writeElement(ioStream, nextElement);
		nextElement = new FtfElement();
    
		return ok;
	}

	public bool close()
	{
		bool ok = true;
    
		try
		{
			ok = writeHash();
    
			if (ioStream != null)
			{
				ioStream.Flush();
			}
			if (bufferedStream != null)
			{
				bufferedStream.Flush();
			}
			if (fileStream != null)
			{
				fileStream.Flush();
				fileStream.Close();
			}
		}
		catch (Exception)
		{
		}
		return ok;
	}

}
