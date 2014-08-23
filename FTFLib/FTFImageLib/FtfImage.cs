using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Imaging;

public class FtfImage
{
	public List<FtfImageData> imageDatas;

	public bool readError;
	public string errorMessage;

	public static double gammaCorrection = 2.2;

	public FtfImage()
	{
		imageDatas = new List<FtfImageData>();
		clear();
	}

	public void clear()
	{
		imageDatas.Clear();
		readError = false;
		errorMessage = "";
	}

	public bool readImage(string filename)
	{
		bool hashOk = true;
		FtfFileReader reader = new FtfFileReader();
		FtfImageData imageData;
		byte[] content;
		string value = "";
		int elements = 1;
		FtfColorModel colormodel = FtfColorModel.NONE;
		FtfColorChannelsFormat colorChannelsFormat = FtfColorChannelsFormat.NONE;
		FtfComponentFormat componentformat = FtfComponentFormat.NONE;
		int componentbits = 0;
		int channels;
		bool hasAlpha;
		bool compressed = false;
		ChannelFormat fileFormat;

		clear();
		if (reader.open(filename))
		{
			// Required tags

			elements = getIntTag(reader, FtfTag.elementsLabel, false);

			for (int elementi = 0; elementi < elements; elementi++)
			{
				imageData = new FtfImageData();
				fileFormat = imageData.fileFormat;

				content = reader.readElement();

				imageData.width = getIntTag(reader, FtfTag.widthLabel, true);

				imageData.height = getIntTag(reader, FtfTag.heightLabel, true);

				channels = getIntTag(reader, FtfTag.channelsLabel, false);

				value = getTag(reader, FtfTag.colormodelLabel, true);
				if (value != null)
				{
					if (Enum.TryParse(value, true, out colormodel))
					{
						if (colormodel == FtfColorModel.GRAYSCALE)
						{
							hasAlpha = (channels > 1);
							fileFormat.setGrayscale(hasAlpha);
						}
						else if (colormodel == FtfColorModel.RGB)
						{
							hasAlpha = (channels > 3);
							fileFormat.setColor(hasAlpha);
						}
						else
						{
							readError = true;
							errorMessage += string.Format("Unrecognised tag value {0}:{1}\n", FtfTag.colormodelLabel, value);
						}
					}
					else
					{
						readError = true;
						errorMessage += string.Format("Unrecognised tag value {0}:{1}\n", FtfTag.colormodelLabel, value);
					}
				}

				value = getTag(reader, FtfTag.componentformatLabel, true);
				if (value != null)
				{
					if (Enum.TryParse(value, true, out componentformat))
					{
						if (componentformat == FtfComponentFormat.FP)
						{
							fileFormat.floatMode = true;
						}
						else if (componentformat == FtfComponentFormat.INT)
						{
							fileFormat.floatMode = false;
						}
						else
						{
							readError = true;
							errorMessage += string.Format("Unrecognised tag value {0}:{1}\n", FtfTag.componentformatLabel, value);
						}
					}
					else
					{
						readError = true;
						errorMessage += string.Format("Unrecognised tag value {0}:{1}\n", FtfTag.componentformatLabel, value);
					}
				}

				componentbits = getIntTag(reader, FtfTag.componentbitsLabel, true);

				// Optional tags

				compressed = reader.elementCompressed();

				// color channels format
				value = getTag(reader, FtfTag.colorchannelsformatLabel, false);
				if (value != null)
				{
					if (Enum.TryParse(value, true, out colorChannelsFormat))
					{
						switch (colorChannelsFormat)
						{
							case FtfColorChannelsFormat.I:
								fileFormat.colorChannelsFormat = ColorChannelsFormat.I;
								break;
							case FtfColorChannelsFormat.IA:
								fileFormat.colorChannelsFormat = ColorChannelsFormat.IA;
								break;
							case FtfColorChannelsFormat.AI:
								fileFormat.colorChannelsFormat = ColorChannelsFormat.AI;
								break;
							case FtfColorChannelsFormat.RGB:
								fileFormat.colorChannelsFormat = ColorChannelsFormat.RGB;
								break;
							case FtfColorChannelsFormat.BGR:
								fileFormat.colorChannelsFormat = ColorChannelsFormat.BGR;
								break;
							case FtfColorChannelsFormat.ARGB:
								fileFormat.colorChannelsFormat = ColorChannelsFormat.ARGB;
								break;
							case FtfColorChannelsFormat.RGBA:
								fileFormat.colorChannelsFormat = ColorChannelsFormat.RGBA;
								break;
							case FtfColorChannelsFormat.BGRA:
								fileFormat.colorChannelsFormat = ColorChannelsFormat.BGRA;
								break;
							default:
								fileFormat.colorChannelsFormat = ColorChannelsFormat.None;
								break;
						}
					}
					else
					{
						readError = true;
						errorMessage += string.Format("Unrecognised tag value {0}:{1}\n", FtfTag.colorchannelsformatLabel, value);
					}
				}

				if (componentbits != 0)
				{
					fileFormat.bitsPerChannel = componentbits;
					fileFormat.bytesPerChannel = fileFormat.bitsPerChannel / 8;
					fileFormat.bitsPerPixel = fileFormat.bitsPerChannel * fileFormat.channels;
					fileFormat.bytesPerPixel = fileFormat.bitsPerPixel / 8;
					fileFormat.compressed = compressed;
				}
				if (!readError)
				{
					bytesToBuffer(content, imageData, false, false);
					imageDatas.Add(imageData);
				}
				if (content != null)
				{
					content = null;
				}
			}

			hashOk = reader.readHash();
			if (!hashOk)
			{
				readError = true;
				errorMessage += "Hash bad\n";
			}
		}
		reader.close();

		return (!readError && hashOk);
	}

	public bool writeImage(string filename)
	{
		return writeImage(filename, null);
	}

	public bool writeImage(string filename, ChannelFormat outputFormat)
	{
		bool ok = false;
		FtfFileWriter writer = new FtfFileWriter();
		byte[] content;
		FtfImageData imageData;
		ChannelFormat channelFormat;
		FtfColorChannelsFormat colorChannelsFormat = new FtfColorChannelsFormat();
		int elements;

		elements = imageDatas.Count;

		// Global tags

		// elements
		writer.setGlobalTag(FtfTag.elementsLabel, elements.ToString());

		// application
		writer.setGlobalTag(FtfTag.applicationLabel, "FTFImageLib");

		if (writer.open(filename, new SHA512Managed()))
		{
			for (int elementi = 0; elementi < elements; elementi++)
			{
				imageData = imageDatas[elementi];

				if (outputFormat != null)
				{
					channelFormat = outputFormat;
				}
				else
				{
					channelFormat = imageData.fileFormat;
				}

				content = bufferToBytes(imageData, channelFormat, false, false);

				// width
				writer.setElementTag(FtfTag.widthLabel, imageData.width.ToString());

				// height
				writer.setElementTag(FtfTag.heightLabel, imageData.height.ToString());

				// channels
				writer.setElementTag(FtfTag.channelsLabel, channelFormat.channels.ToString());

				// color model
				if (channelFormat.grayscaleMode)
				{
					writer.setElementTag(FtfTag.colormodelLabel, FtfColorModel.GRAYSCALE.ToString());
				}
				else
				{
					writer.setElementTag(FtfTag.colormodelLabel, FtfColorModel.RGB.ToString());
				}

				// color channels format
				switch (channelFormat.colorChannelsFormat)
				{
					case ColorChannelsFormat.I:
						colorChannelsFormat = FtfColorChannelsFormat.I;
						break;
					case ColorChannelsFormat.IA:
						colorChannelsFormat = FtfColorChannelsFormat.IA;
						break;
					case ColorChannelsFormat.AI:
						colorChannelsFormat = FtfColorChannelsFormat.AI;
						break;
					case ColorChannelsFormat.RGB:
						colorChannelsFormat = FtfColorChannelsFormat.RGB;
						break;
					case ColorChannelsFormat.BGR:
						colorChannelsFormat = FtfColorChannelsFormat.BGR;
						break;
					case ColorChannelsFormat.ARGB:
						colorChannelsFormat = FtfColorChannelsFormat.ARGB;
						break;
					case ColorChannelsFormat.RGBA:
						colorChannelsFormat = FtfColorChannelsFormat.RGBA;
						break;
					case ColorChannelsFormat.BGRA:
						colorChannelsFormat = FtfColorChannelsFormat.BGRA;
						break;
					default:
						colorChannelsFormat = FtfColorChannelsFormat.NONE;
						break;
				}
				if (colorChannelsFormat != FtfColorChannelsFormat.NONE)
				{
					writer.setElementTag(FtfTag.colorchannelsformatLabel, colorChannelsFormat.ToString());
				}

				// dimensions
				writer.setElementTag(FtfTag.dimensionsLabel, string.Format("{0} {1} {2}", imageData.height, imageData.width, channelFormat.channels));

				// component format
				if (channelFormat.floatMode)
				{
					writer.setElementTag(FtfTag.componentformatLabel, FtfComponentFormat.FP.ToString());
				}
				else
				{
					writer.setElementTag(FtfTag.componentformatLabel, FtfComponentFormat.INT.ToString());
				}

				// component bits
				writer.setElementTag(FtfTag.componentbitsLabel, channelFormat.bitsPerChannel.ToString());

				ok = true;
				writer.setElementTag(FtfTag.elementLabel, elementi.ToString());
				if (channelFormat.compressed)
				{
					ok &= writer.writeElement(content, FtfCompression.DEFLATE);
				}
				else
				{
					ok &= writer.writeElement(content);
				}
				content = null;
			}
		}
		writer.close();

		return ok;
	}

	public string getErrorMessage()
	{
		return errorMessage;
	}

	public BitmapSource getBitmapSource(ChannelFormat channelFormat, int imagei)
	{
		BitmapSource bitmap;
		FtfImageData imageData = null;
		byte[] byteData;

		if (imagei >= 0 && imagei < imageDatas.Count)
		{
			imageData = imageDatas[imagei];
		}
		byteData = bufferToBytes(imageData, channelFormat, false, true);
		bitmap = bytesToBitmapSource(byteData, imageData.width, imageData.height, channelFormat);
		byteData = null;
		return bitmap;
	}

	public byte[] bufferToBytes(FtfImageData imageData, ChannelFormat channelFormat, bool floatGammaCorrection, bool reverse)
	{
		byte[] byteData;
		byte[] pixelData;
		int pos = 0;
		int pos0;
		double i;
		double r;
		double g;
		double b;
		double a = 1;
		double c1 = 0;
		double c2 = 0;
		double c3 = 0;
		double c4 = 0;
		bool floatMode = channelFormat.floatMode;
		bool grayscaleMode = channelFormat.grayscaleMode;
		ColorChannelsFormat colorChannelsFormat = channelFormat.colorChannelsFormat;
		bool hasAlpha = channelFormat.hasAlpha;
		int channels = channelFormat.channels;
		int bytesPerChannel = channelFormat.bytesPerChannel;
		int bytesPerPixel = channelFormat.bytesPerPixel;
		ChannelFormat fileFormat = imageData.fileFormat;
		int width = imageData.width;
		int height = imageData.height;

		byteData = new byte[height * width * channelFormat.bytesPerPixel];
		pixelData = new byte[bytesPerPixel];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (hasAlpha && fileFormat.hasAlpha)
				{
					a = imageData.aData[y, x];
				}

				if (grayscaleMode)
				{
					// grayscale
					if (fileFormat.grayscaleMode)
					{
						i = imageData.iData[y, x];
					}
					else
					{
						// convert color to grayscale
						i = (imageData.rData[y, x] + imageData.gData[y, x] + imageData.bData[y, x]) / 3;
					}

					if (floatMode && floatGammaCorrection)
					{
						i = (float)Math.Pow(i, gammaCorrection);
					}

					switch (colorChannelsFormat)
					{
						case ColorChannelsFormat.IA:
							c1 = i;
							c2 = a;
							break;
						case ColorChannelsFormat.AI:
							c1 = a;
							c2 = i;
							break;
						default: // I
							c1 = i;
							break;
					}
				}
				else
				{
					// color
					if (!fileFormat.grayscaleMode)
					{
						r = imageData.rData[y, x];
						g = imageData.gData[y, x];
						b = imageData.bData[y, x];
					}
					else
					{
						// convert grayscale to color
						i = imageData.iData[y, x];
						r = i;
						g = i;
						b = i;
					}

					if (floatMode && floatGammaCorrection)
					{
						r = (float)Math.Pow(r, gammaCorrection);
						g = (float)Math.Pow(g, gammaCorrection);
						b = (float)Math.Pow(b, gammaCorrection);
					}

					switch (colorChannelsFormat)
					{
						case ColorChannelsFormat.BGR:
							c1 = b;
							c2 = g;
							c3 = r;
							break;
						case ColorChannelsFormat.ARGB:
							c1 = a;
							c2 = r;
							c3 = g;
							c4 = b;
							break;
						case ColorChannelsFormat.RGBA:
							c1 = r;
							c2 = g;
							c3 = b;
							c4 = a;
							break;
						case ColorChannelsFormat.BGRA:
							c1 = b;
							c2 = g;
							c3 = r;
							c4 = a;
							break;
						default: // RGB
							c1 = r;
							c2 = g;
							c3 = b;
							break;
					}
				}
				pos0 = 0;
				Util.copyBytes(pixelData, c1, floatMode, pos0, bytesPerChannel);
				if (channels >= 2)
				{
					pos0 += bytesPerChannel;
					Util.copyBytes(pixelData, c2, floatMode, pos0, bytesPerChannel);
				}
				if (channels >= 3)
				{
					pos0 += bytesPerChannel;
					Util.copyBytes(pixelData, c3, floatMode, pos0, bytesPerChannel);
				}
				if (channels >= 4)
				{
					pos0 += bytesPerChannel;
					Util.copyBytes(pixelData, c4, floatMode, pos0, bytesPerChannel);
				}
				if (reverse)
				{
					Array.Reverse(pixelData);
				}
				Array.Copy(pixelData, 0, byteData, pos, bytesPerPixel);
				pos += bytesPerPixel;
			}
		}

		return byteData;
	}

	public BitmapSource bytesToBitmapSource(byte[] byteData, int width, int height, ChannelFormat channelFormat)
	{
		WriteableBitmap bitmap = null;
		int bytesPerPixel;

		if (width != 0 && height != 0)
		{
			bytesPerPixel = channelFormat.pixelFormat.BitsPerPixel / 8;
			bitmap = new WriteableBitmap(width, height, 96, 96, channelFormat.pixelFormat, null);
			bitmap.WritePixels(new Int32Rect(0, 0, width, height), byteData, width * bytesPerPixel, 0, 0);
		}
		return bitmap;
	}

	public void bytesToBuffer(byte[] byteData, FtfImageData imageData, bool floatGammaCorrection, bool reverse)
	{
		byte[] pixelData;
		double i;
		double r;
		double g;
		double b;
		double a = 1;
		double c1 = 0;
		double c2 = 0;
		double c3 = 0;
		double c4 = 0;
		int pos = 0;
		int pos0;
		bool floatMode = imageData.fileFormat.floatMode;
		bool grayscaleMode = imageData.fileFormat.grayscaleMode;
		ColorChannelsFormat colorChannelsFormat = imageData.fileFormat.colorChannelsFormat;
		bool hasAlpha = imageData.fileFormat.hasAlpha;
		int channels = imageData.fileFormat.channels;
		int bytesPerChannel = imageData.fileFormat.bytesPerChannel;
		int bytesPerPixel = imageData.fileFormat.bytesPerPixel;
		int width = imageData.width;
		int height = imageData.height;

		imageData.initData();
		pixelData = new byte[bytesPerPixel];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Array.Copy(byteData, pos, pixelData, 0, bytesPerPixel);
				if (reverse)
				{
					Array.Reverse(pixelData);
				}

				pos0 = 0;
				c1 = Util.getDouble(pixelData, pos0, floatMode, bytesPerChannel);
				if (channels >= 2)
				{
					pos0 += bytesPerChannel;
					c2 = Util.getDouble(pixelData, pos0, floatMode, bytesPerChannel);
				}
				if (channels >= 3)
				{
					pos0 += bytesPerChannel;
					c3 = Util.getDouble(pixelData, pos0, floatMode, bytesPerChannel);
				}
				if (channels >= 4)
				{
					pos0 += bytesPerChannel;
					c4 = Util.getDouble(pixelData, pos0, floatMode, bytesPerChannel);
				}

				if (grayscaleMode)
				{
					// grayscale
					switch (colorChannelsFormat)
					{
						case ColorChannelsFormat.IA:
							i = c1;
							a = c2;
							break;
						case ColorChannelsFormat.AI:
							a = c1;
							i = c2;
							break;
						default: // I
							i = c1;
							break;
					}

					if (floatMode && floatGammaCorrection)
					{
						i = (float)Math.Pow(i, 1 / gammaCorrection);
					}
					imageData.iData[y, x] = i;
				}
				else
				{
					// color
					switch (colorChannelsFormat)
					{
						case ColorChannelsFormat.BGR:
							b = c1;
							g = c2;
							r = c3;
							break;
						case ColorChannelsFormat.ARGB:
							a = c1;
							r = c2;
							g = c3;
							b = c4;
							break;
						case ColorChannelsFormat.RGBA:
							r = c1;
							g = c2;
							b = c3;
							a = c4;
							break;
						case ColorChannelsFormat.BGRA:
							b = c1;
							g = c2;
							r = c3;
							a = c4;
							break;
						default: // RGB
							r = c1;
							g = c2;
							b = c3;
							break;
					}

					if (floatMode && floatGammaCorrection)
					{
						r = (float)Math.Pow(r, 1 / gammaCorrection);
						g = (float)Math.Pow(g, 1 / gammaCorrection);
						b = (float)Math.Pow(b, 1 / gammaCorrection);
					}
					imageData.rData[y, x] = r;
					imageData.gData[y, x] = g;
					imageData.bData[y, x] = b;
				}
				if (hasAlpha)
				{
					imageData.aData[y, x] = a;
				}
				pos += bytesPerPixel;
			}
		}
	}

	public string getTag(FtfFileReader reader, string label, bool required)
	{
		string s;

		s = reader.getTag(label);
		if ((s == null || s == "") && required)
		{
			readError = true;
			errorMessage += string.Format("Required tag missing: {0}\n", label);
		}
		return s;
	}

	public int getIntTag(FtfFileReader reader, string label, bool required)
	{
		int x = 0;

		string s = getTag(reader, label, required);
		if (s != "")
		{
			int.TryParse(s, out x);
		}
		return x;
	}

}
