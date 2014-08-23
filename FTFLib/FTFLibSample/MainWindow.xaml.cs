using System.Windows;
using Microsoft.Win32;

namespace FTFLibSample
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void openMenuItem_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			byte[] content;
			string s = "File content:\n";

			if (openFileDialog.ShowDialog() == true)
			{
				content = FtfHelper.readFile(openFileDialog.FileName);
				if (content.Length > 0)
				{
					if (content.Length < 1000)
					{
						foreach (byte x in content)
						{
							s += string.Format("{0:X2} ", x);
						}
						MessageBox.Show(s, "FTF File");
					}
					else
					{
						MessageBox.Show(string.Format("Content length : {0}", content.Length), "FTF File");
					}
				}
			}
		}

		private void saveMenuItem_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			byte[] content = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			if (saveFileDialog.ShowDialog() == true)
			{
				if (FtfHelper.writeFile(saveFileDialog.FileName, content))
				{
					MessageBox.Show("File content written ok", "FTF File");
				}
			}
		}

	}
}
