using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

public class FtfFileReader : FtfFile
{
	public bool open(string filename)
	{
		FtfTag tag;
		string hashtype;
    
		this.filename = filename;
    
		try
		{
			fileStream = new FileStream(filename, FileMode.Open);
			bufferedStream = new BufferedStream(fileStream, bufferSize);
			ioStream = bufferedStream;
			ftfContent.readHeader(ioStream);
    
			tag = ftfContent.getTag(FtfTag.hashtypeLabel);
			if (tag != null)
			{
				useHash = true;
				hashtype = tag.content;
    
				if (hashtype == "SHA512")
				{
					hashAlgorythm = new SHA512Managed();
				}
				else if (hashtype == "SHA384")
				{
					hashAlgorythm = new SHA384Managed();
				}
				else if (hashtype == "SHA256")
				{
					hashAlgorythm = new SHA256Managed();
				}
				else if (hashtype == "SHA1")
				{
					hashAlgorythm = new SHA1Managed();
				}
    
				tag = ftfContent.getTag(FtfTag.hashsizeLabel);
				if (tag != null)
				{
					int.TryParse(tag.content, out hashSize);
				}
			}
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public string getTag(string label)
	{
		string content = null;
    
		FtfTag tag = ftfContent.getTag(label);
		if (tag != null)
		{
			content = tag.content;
		}
		return content;
	}

	public FtfCompression elementCompression()
	{
		FtfCompression compression = FtfCompression.NONE;
		FtfTag tag;
    
		tag = ftfContent.getTag(FtfTag.compressionLabel);
		if (tag != null)
		{
			Enum.TryParse(tag.content, true, out compression);
		}
		return compression;
	}

	public bool elementCompressed()
	{
		return (elementCompression() != FtfCompression.NONE);
	}

	public byte[] readElement()
	{
		byte[] outcontent = null;
		byte[] content;
		FtfCompression compression = FtfCompression.NONE;
    
		if (ftfContent.readElement(ioStream, true))
		{
			content = ftfContent.getCurrentElementContent();
    
			compression = elementCompression();
    
			if (compression == FtfCompression.DEFLATE)
			{
				// context for MemoryStream
				{
					MemoryStream compressedStream = new MemoryStream(content);
    
					// context for DeflateStream
					{
						DeflateStream zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
    
						// context for MemoryStream
						{
							MemoryStream resultStream = new MemoryStream();
    
							zipStream.CopyTo(resultStream);
							outcontent = resultStream.ToArray();
						}
					}
				}
			}
			else if (compression == FtfCompression.GZIP)
			{
				// context for MemoryStream
				{
					MemoryStream compressedStream = new MemoryStream(content);
    
					// context for DeflateStream
					{
						DeflateStream zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
    
						// context for MemoryStream
						{
							MemoryStream resultStream = new MemoryStream();
    
							zipStream.CopyTo(resultStream);
							outcontent = resultStream.ToArray();
						}
					}
				}
			}
			else
			{
				outcontent = content;
			}
		}
    
		return outcontent;
	}

	public bool readHash()
	{
		byte[] calcHashBytes;
		byte[] fileHashBytes;
		bool hashOk = true;
		byte[] data;
		UInt64 dataLen;
    
		try
		{
			if (useHash && !hashDone && hashAlgorythm != null)
			{
				// calculate data length
				dataLen = (ulong)fileStream.Length - (ulong)hashSize;
				data = new byte[(int)dataLen];
				// reset both streams
				fileStream.Seek(0, SeekOrigin.Begin);
				bufferedStream.Seek(0, SeekOrigin.Begin);
				// create crypto stream on top
				cryptoStream = new CryptoStream(bufferedStream, hashAlgorythm, CryptoStreamMode.Read);
				// re-read until position
				cryptoStream.Read(data, 0, (int)dataLen);
    
				// can only check hash if all elements are read
				cryptoStream.FlushFinalBlock();
    
				calcHashBytes = hashAlgorythm.Hash;
				fileHashBytes = new byte[calcHashBytes.Length];
				if (bufferedStream.Read(fileHashBytes, 0, hashSize) > 0)
				{
					hashOk = StructuralComparisons.StructuralEqualityComparer.Equals(calcHashBytes, fileHashBytes);
				}
				hashDone = true;
			}
			return hashOk;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public bool close()
	{
		bool ok = true;
    
		try
		{
			ok = readHash();
    
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
		catch (Exception)
		{
		}
		return ok;
	}

}
