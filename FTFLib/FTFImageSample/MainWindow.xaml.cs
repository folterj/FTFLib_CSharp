using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace FTFImageSample
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		FtfImageHelper ftfImageHelper = new FtfImageHelper();
		BitmapSource bitmap;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void createMenuItem_Click(object sender, RoutedEventArgs e)
		{
			mainImage.Source = ftfImageHelper.createImage();
		}

		private void openMenuItem_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			if (openFileDialog.ShowDialog() == true)
			{
				bitmap = ftfImageHelper.readImage(openFileDialog.FileName);
				if (bitmap != null)
				{
					mainImage.Source = bitmap;
				}
				else
				{
					MessageBox.Show(ftfImageHelper.getErrorMessage(), "Read error");
				}
			}
		}

		private void saveMenuItem_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();

			if (saveFileDialog.ShowDialog() == true)
			{
				if (!ftfImageHelper.writeImage(saveFileDialog.FileName))
				{
					MessageBox.Show("Error writing image", "Write error");
				}
			}
		}

	}
}
