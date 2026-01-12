using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UnityProjectManager.Models
{
    public enum BoardTaskStatus
    {
        Todo,
        Doing,
        Done
    }

    public partial class BoardTask : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [ObservableProperty]
        private string _title = string.Empty;
        
        [ObservableProperty]
        private string _description = string.Empty;
        
        [ObservableProperty]
        private BoardTaskStatus _status = BoardTaskStatus.Todo;
        
        [ObservableProperty]
        private string _color = GetRandomColor();

        private static string GetRandomColor()
        {
            var colors = new[] { "#F2C94C", "#27AE60", "#2F80ED", "#9B51E0", "#EB5757", "#F2994A" };
            return colors[new Random().Next(colors.Length)];
        }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public partial class ProjectBoard : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [ObservableProperty]
        private string _name = "New Board";
        
        public List<BoardTask> Tasks { get; set; } = new List<BoardTask>();
    }
}
