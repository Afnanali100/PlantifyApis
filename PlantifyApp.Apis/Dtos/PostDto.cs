using PlantifyApp.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PlantifyApp.Apis.Dtos
{
    public class PostDto
    {
        [Key]
        public int? post_id { get; set; }

        public string? user_id { get; set; }

        public string? description { get; set; }

        public string? image_name { get; set; }

        public string? video_name { get; set; }

        public DateTime? creation_date { get; set; }

        public List<Comments>? Comments { get; set; }

        public List<Likes>? Likes { get; set; } 

        public int? LikesCount { get; set; } 



    }
}
