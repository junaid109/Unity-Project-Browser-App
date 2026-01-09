using System;

namespace UnityProjectManager.Models
{
    public class LearnContent
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Url { get; set; }
        public int Duration { get; set; }
        public string? SkillLevel { get; set; }
        public string? Type { get; set; } // tutorial, project, course, pathway
    }
}
