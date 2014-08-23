using System;
using System.IO;
using System.Security.Cryptography;


public class FtfFile
{
	public FtfContent ftfContent;
	public string filename;
	public HashAlgorithm hashAlgorythm;
	public bool useHash;
	public bool hashDone;
	public int hashSize;

	public static string fileExtension = ".ftf";

	public static int bufferSize = 1000000;

	public FileStream fileStream;
	public BufferedStream bufferedStream;
	public CryptoStream cryptoStream;
	public Stream ioStream;

	public FtfFile()
	{
		reset();
	}
	public void reset()
	{
		ftfContent = new FtfContent();
		filename = "";
		hashAlgorythm = null;
		useHash = false;
		hashDone = false;
		hashSize = 0;
	}
}
