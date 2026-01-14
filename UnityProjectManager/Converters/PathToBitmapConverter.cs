using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace UnityProjectManager.Converters
{
    public class PathToBitmapConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Web URL: Not supported by this synchronous converter anymore.
                    // Use View Model loading for web images.
                    return null;
                }

                if (File.Exists(path))
                {
                    try
                    {
                        return new Bitmap(path);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        private Bitmap? LoadFromUrl(string url)
        {
            try
            {
                var data = _httpClient.GetByteArrayAsync(url).Result;
                using var ms = new MemoryStream(data);
                return new Bitmap(ms);
            }
            catch
            {
                return null;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
