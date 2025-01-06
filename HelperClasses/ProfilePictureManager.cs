using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;



namespace HelperClasses
{
    public static class ProfilePictureManager
    {
        public static BitmapImage ByteArrayToBitmapImage(byte[] imageData)
        {
            BitmapImage bitmapImage = new BitmapImage();
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    using (MemoryStream memoryStream = new MemoryStream(imageData))
                    {
                        memoryStream.Position = 0;
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                System.Windows.MessageBox.Show("An error occurred while loading the image. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                System.Windows.MessageBox.Show("The image data provided is invalid. Please check the file.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bitmapImage;
        }
    }
}
