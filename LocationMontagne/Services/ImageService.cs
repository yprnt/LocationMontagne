using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace LocationMontagne.Services
{
    public static class ImageService
    {
        public static readonly string IMAGE_DIRECTORY = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
        public static readonly string DEFAULT_IMAGE = "placeholder.jpg";

        static ImageService()
        {
            if (!Directory.Exists(IMAGE_DIRECTORY))
            {
                Directory.CreateDirectory(IMAGE_DIRECTORY);
            }
        }

        public static string SaveImage(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath)) return null;

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}";
            string destinationPath = Path.Combine(IMAGE_DIRECTORY, fileName);

            File.Copy(sourcePath, destinationPath, true);
            return fileName;
        }

        public static BitmapImage LoadImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = DEFAULT_IMAGE;
            }

            string imagePath = Path.Combine(IMAGE_DIRECTORY, fileName);

            if (!File.Exists(imagePath))
            {
                return new BitmapImage(new Uri("/Assets/placeholder.jpg", UriKind.Relative));
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(imagePath);
            image.EndInit();
            return image;
        }

        public static void DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            string imagePath = Path.Combine(IMAGE_DIRECTORY, fileName);
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }
}
