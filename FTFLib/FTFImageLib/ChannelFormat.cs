using System.Windows.Media;

public enum ColorChannelsFormat
{
	None,
	I,
	IA,
	AI,
	RGB,
	BGR,
	ARGB,
	RGBA,
	BGRA
}

public class ChannelFormat
{
	public bool grayscaleMode;
	public bool floatMode;
	public ColorChannelsFormat colorChannelsFormat;
	public bool hasAlpha;
	public bool compressed;
	public int channels;
	public int bytesPerChannel;
	public int bitsPerChannel;
	public int bytesPerPixel;
	public int bitsPerPixel;
	public PixelFormat pixelFormat = new PixelFormat();

	public ChannelFormat()
	{
		floatMode = false;
		grayscaleMode = false;
		colorChannelsFormat = ColorChannelsFormat.RGB;
		hasAlpha = false;
		compressed = true;

		setColor(hasAlpha);
	}

	public ChannelFormat(ChannelFormat format)
	{
		grayscaleMode = format.grayscaleMode;
		floatMode = format.floatMode;
		colorChannelsFormat = format.colorChannelsFormat;
		hasAlpha = format.hasAlpha;
		compressed = format.compressed;
		channels = format.channels;
		bytesPerChannel = format.bytesPerChannel;
		bitsPerChannel = format.bitsPerChannel;
		bytesPerPixel = format.bytesPerPixel;
		bitsPerPixel = format.bitsPerPixel;
		pixelFormat = format.pixelFormat;
	}

	public void setColor(bool alpha)
	{
		floatMode = true;
		grayscaleMode = false;
		hasAlpha = alpha;
		compressed = true;
		if (alpha)
		{
			colorChannelsFormat = ColorChannelsFormat.RGBA;
			channels = 4;
		}
		else
		{
			colorChannelsFormat = ColorChannelsFormat.RGB;
			channels = 3;
		}
		bytesPerChannel = 4;
		bitsPerChannel = bytesPerChannel * 8;
		bytesPerPixel = channels * bytesPerChannel;
		bitsPerPixel = bytesPerPixel * 8;
	}

	public void setGrayscale(bool alpha)
	{
		floatMode = true;
		grayscaleMode = true;
		hasAlpha = alpha;
		compressed = true;
		if (alpha)
		{
			colorChannelsFormat = ColorChannelsFormat.IA;
			channels = 2;
		}
		else
		{
			colorChannelsFormat = ColorChannelsFormat.I;
			channels = 1;
		}
		bytesPerChannel = 4;
		bitsPerChannel = bytesPerChannel * 8;
		bytesPerPixel = channels * bytesPerChannel;
		bitsPerPixel = bytesPerPixel * 8;
	}

	public void setColorScreen(bool alpha)
	{
		if (alpha)
		{
			init(PixelFormats.Bgra32);
		}
		else
		{
			init(PixelFormats.Bgr24);
		}
	}

	public void setGrayscaleScreen()
	{
		init(PixelFormats.Gray8); // Format16bppGrayScale not supported for display
	}

	public void init(PixelFormat pixelFormat)
	{
		this.pixelFormat = pixelFormat;

		grayscaleMode = (pixelFormat == PixelFormats.Gray8 ||
						pixelFormat == PixelFormats.Gray16 ||
						pixelFormat == PixelFormats.Gray32Float);

		hasAlpha = (pixelFormat == PixelFormats.Bgra32 ||
					pixelFormat == PixelFormats.Pbgra32 ||
					pixelFormat == PixelFormats.Prgba128Float ||
					pixelFormat == PixelFormats.Prgba64 ||
					pixelFormat == PixelFormats.Rgba128Float ||
					pixelFormat == PixelFormats.Rgba64);

		floatMode = (pixelFormat == PixelFormats.Gray32Float ||
					pixelFormat == PixelFormats.Prgba128Float ||
					pixelFormat == PixelFormats.Rgb128Float ||
					pixelFormat == PixelFormats.Rgba128Float);

		if (pixelFormat == PixelFormats.Gray32Float ||
			pixelFormat == PixelFormats.Prgba128Float ||
			pixelFormat == PixelFormats.Rgb128Float ||
			pixelFormat == PixelFormats.Rgba128Float)
		{
			bitsPerChannel = 32;
		}
		else if (pixelFormat == PixelFormats.Prgba64 ||
				pixelFormat == PixelFormats.Rgba64 ||
				pixelFormat == PixelFormats.Gray16)
		{
			bitsPerChannel = 16;
		}
		else
		{
			bitsPerChannel = 8;
		}

		compressed = true;

		if (grayscaleMode)
		{
			if (hasAlpha)
			{
				colorChannelsFormat = ColorChannelsFormat.AI;
				channels = 2;
			}
			else
			{
				colorChannelsFormat = ColorChannelsFormat.I;
				channels = 1;
			}
		}
		else
		{
			if (hasAlpha)
			{
				colorChannelsFormat = ColorChannelsFormat.ARGB;
				channels = 4;
			}
			else
			{
				colorChannelsFormat = ColorChannelsFormat.RGB;
				channels = 3;
			}
		}

		bytesPerChannel = bitsPerChannel / 8;
		bitsPerPixel = bitsPerChannel * channels;
		bytesPerPixel = bitsPerPixel / 8;
	}

}
