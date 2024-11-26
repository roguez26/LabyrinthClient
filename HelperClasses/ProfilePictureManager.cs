using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;



namespace HelperClasses
{
    public class ProfilePictureManager
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
            catch (InvalidOperationException ex)
            {

            }
            catch (ArgumentOutOfRangeException ex)
            {

            } 
            return bitmapImage;
        }
    }
}
