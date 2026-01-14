using CommunityToolkit.Mvvm.ComponentModel;

namespace UnityProjectManager.Models
{
    public partial class InspirationItem : ObservableObject
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = ""; // Can be local path or web URL
        
        [ObservableProperty]
        private object? _image; // Stores the loaded Bitmap
        
        public string Source { get; set; } = "Unity"; // Twitter, Facebook, Unity, Reddit
        public string Author { get; set; } = "";
        public string Link { get; set; } = "";
        public string Tags { get; set; } = ""; // e.g. #2D #HDRP
        public int Likes { get; set; } = 0;
    }
}
