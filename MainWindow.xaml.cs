using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.UserProfile;
using Windows.Foundation;
using Windows.Storage.Streams;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Reflection;
using System.Diagnostics;
using System.Security.Policy;
using Windows.Networking.BackgroundTransfer;
using Microsoft.Win32;
using System.Runtime.InteropServices;


namespace SpotyLi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<string> files = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void dropBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                dropBoxLabel.Text = "Отпустите изображение сюда";
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void dropBox_DragLeave(object sender, DragEventArgs e)
        {
            dropBoxLabel.Text = "Перетащите изображение в область";
        }

        private void dropBox_DragDrop(object sender, DragEventArgs e)
        {
            dropBoxLabel.Text = "Перетащите изображение в область";
            files.Clear();
            foreach (string obj in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                if (Regex.IsMatch(Path.GetExtension(obj), @"(jpg|jpeg|png|gif)$"))
                {
                    files.Add(obj);
                    fileName.Text = files[0];
                }
            }
            fileName.Text = $"{Path.GetFileName(files[0])} \nуспешно загружен";
            image_view_Loaded();
        }
        public void image_view_Loaded()
        {
            BitmapImage img = new BitmapImage(new Uri(files[0]));
            image_view.Source = img;
        }

        private async void setImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var uri = (files[0]);
                IStorageFile file = await StorageFile.GetFileFromPathAsync(uri);
                await Task.Run((Action)(() =>
                {
                    IAsyncAction asyncAction = LockScreen.SetImageFileAsync(file);
                }));
                MessageBox.Show("Установлено на экран блокировки", "Успех");
            }
            catch(Exception ex) { MessageBox.Show(ex.Message, "Ты еблан?"); }
        }

        public static class WinAPI
        {
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        }

        private void setImageDesctop_Click(object sender, RoutedEventArgs e)
        {
            const int SPIF_SENDWININICHANGE = 0x02;
            const int SPI_SETDESKWALLPAPER = 20;
            const int SPIF_UPDATEINIFILE = 0x01; 
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                key.SetValue("Wallpaper", Path.GetFullPath(files[0]));
                key.SetValue("WallpaperStyle", "2"); // 0 - застявка, 1 - плитка, 2 - растянуть, 3 - подогнать, 4 - центрировать
                key.SetValue("TileWallpaper", "0"); // 0 - нет, 1 - да
                key.Close();
                MessageBox.Show("Обои установлены на рабочий стол");
                WinAPI.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, null, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
            catch
            {
                MessageBox.Show("Обои не установлены на рабочий стол", "Ошибка");
            }
        }
    }
}

