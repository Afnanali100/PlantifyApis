using PlantifyApp.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PlantifyApp.Apis.Dtos
{
    public class PostDto
    {
        [Key]
        public int? post_id { get; set; }

        public string? user_id { get; set; }

        public string? current_user_id { get; set; }


        public string? user_name { get; set; }

        public string? user_image { get; set; }
        public string? description { get; set; }

        public string? image_name { get; set; }

        public string? video_name { get; set; }

        public DateTime? creation_date { get; set; }

        public List<CommentDto>? Comments { get; set; }

        public List<LikeDto>? Likes { get; set; } 


        public int? LikesCount { get; set; } 

        public bool? IsLiked { get; set; } = false;

    }
}
