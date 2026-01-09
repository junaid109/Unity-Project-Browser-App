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
                    // This is a web URL. 
                    // Note: Synchronous fetch in a converter is generally bad for performance.
                    // But for this simple manager, we'll do it or use a Task.
                    // For now, let's try a simple trick or just return a placeholder for web images 
                    // and handle it in the ViewModel? 
                    // Actually, let's try to load it.
                    return Task.Run(() => LoadFromUrl(path)).Result;
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
