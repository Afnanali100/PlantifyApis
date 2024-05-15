using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantifyApp.Core.Models
{
    public class Posts
    {
        [Key]
        public int? post_id {  get; set; }   

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
