using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Util
{
	public static byte[] getBytes(string s)
	{
		Encoding encoding = UTF8Encoding.Default;
		byte[] bytes = encoding.GetBytes(s);
		byte[] bytes2 = new byte[bytes.Length + 1];
    
		Array.Copy(bytes, bytes2, bytes.Length);
		bytes2[bytes2.Length - 1] = 0;
    
		return bytes2;
	}

	public static ulong readUInt64(Stream stream, bool reverse)
	{
		byte[] sizeBuffer = new byte[sizeof(ulong)];
    
		stream.Read(sizeBuffer, 0, sizeBuffer.Length);
    
		if (reverse)
		{
			Array.Reverse(sizeBuffer);
		}
		return BitConverter.ToUInt64(sizeBuffer, 0);
	}

	public static string tagListToString(List<FtfTag> tags)
	{
		string s = "";
    
		foreach (FtfTag tag in tags)
		{
			if (s != "")
			{
				s += "\n";
			}
			s += tag.ToString();
		}
		return s;
	}

	public static double getDouble(byte[] data, int pos, bool floatMode, int nbytes)
	{
		double x;
		ulong maxValue = (ulong)Math.Pow(2, nbytes * 8) - 1;
    
		if (floatMode)
		{
			// float mode
			if (nbytes == 8)
			{
				x = BitConverter.ToDouble(data, pos);
			}
			else
			{
				x = BitConverter.ToSingle(data, pos);
			}
		}
		else
		{
			// int mode
			switch (nbytes)
			{
				case 8:
					x = (double)BitConverter.ToUInt64(data, pos) / maxValue;
					break;
				case 4:
					x = (double)BitConverter.ToUInt32(data, pos) / maxValue;
					break;
				case 2:
					x = (double)BitConverter.ToUInt16(data, pos) / maxValue;
					break;
				default:
					x = (double)data[pos] / maxValue;
					break;
			}
		}
		return x;
	}

	public static byte[] getBytes(double x, bool floatMode, int nbytes)
	{
		byte[] data;
		ulong maxValue = (ulong)Math.Pow(2, nbytes * 8) - 1;
    
		if (floatMode)
		{
			// float mode
			if (nbytes == 8)
			{
				data = BitConverter.GetBytes(x);
			}
			else
			{
				data = BitConverter.GetBytes((float)x);
			}
		}
		else
		{
			// int mode
			switch (nbytes)
			{
				case 8:
					data = BitConverter.GetBytes((ulong)(x * maxValue));
					break;
				case 4:
					data = BitConverter.GetBytes((UInt32)(x * maxValue));
					break;
				case 2:
					data = BitConverter.GetBytes((UInt16)(x * maxValue));
					break;
				default:
					data = BitConverter.GetBytes((byte)(x * maxValue));
					break;
			}
		}
		return data;
	}

	public static void copyBytes(byte[] dest, double x, bool floatMode, int index, int nbytes)
	{
		Array.Copy(Util.getBytes(x, floatMode, nbytes), 0, dest, index, nbytes);
	}

}
