public class FtfImageData
{
	public int width, height;
	public ChannelFormat fileFormat;
	public double[,] rData, gData, bData, iData, aData;

	public FtfImageData()
	{
		reset();
	}

	~FtfImageData()
	{
		reset();
	}

	public void reset()
	{
		fileFormat = new ChannelFormat();
		width = 0;
		height = 0;

		if (rData != null)
		{
			rData = null;
		}
		if (gData != null)
		{
			gData = null;
		}
		if (bData != null)
		{
			bData = null;
		}
		if (iData != null)
		{
			iData = null;
		}
		if (aData != null)
		{
			aData = null;
		}
	}

	public void initData()
	{
		if (!fileFormat.grayscaleMode)
		{
			// color
			rData = new double[height, width];
			gData = new double[height, width];
			bData = new double[height, width];

			iData = null;
		}
		else
		{
			// grayscale
			iData = new double[height, width];

			rData = null;
			gData = null;
			bData = null;
		}

		if (fileFormat.hasAlpha)
		{
			aData = new double[height, width];
		}
		else
		{
			aData = null;
		}
	}

}
