using System.Windows.Media.Imaging;

public class FtfImageHelper
{
	public FtfImage ftfImage = new FtfImage();
	public int imagei;

	public FtfImageHelper()
	{
		imagei = 0;
	}

	public BitmapSource createImage()
	{
		FtfImageData imageData;
		int nimages = 2;
		int width = 1000;
		int height = 1000;
		double v;
		double a;

		ftfImage.clear();

		for (int i = 0; i < nimages; i++)
		{
			imageData = new FtfImageData();
			imageData.width = width;
			imageData.height = height;
			imageData.fileFormat.setColor(true);
			imageData.initData();

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					v = (double)(x + y * width) / (width * height);
					a = (double)x / width;
					if (i > 0)
					{
						a = 1 - a;
					}
					imageData.rData[y, x] = 1 - v;
					imageData.gData[y, x] = v;
					imageData.bData[y, x] = 0;
					imageData.aData[y, x] = a;
				}
			}

			ftfImage.imageDatas.Add(imageData);
		}

		imagei = 0;
		ChannelFormat channelFormat = new ChannelFormat();
		channelFormat.setColorScreen(true);
		return ftfImage.getBitmapSource(channelFormat, imagei);
	}

	public BitmapSource readImage(string filename)
	{
		BitmapSource bitmap = null;

		if (ftfImage.readImage(filename))
		{
			imagei = 0;
			ChannelFormat channelFormat = new ChannelFormat();
			channelFormat.setColorScreen(true);
			bitmap = ftfImage.getBitmapSource(channelFormat, imagei);
		}
		return bitmap;
	}

	public bool writeImage(string filename)
	{
		return ftfImage.writeImage(filename);
	}

	public string getErrorMessage()
	{
		return ftfImage.getErrorMessage();
	}

}
