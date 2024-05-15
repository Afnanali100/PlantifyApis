using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using PlantifyApp.Core.Models;
using System.Text.Json.Serialization;

namespace PlantifyApp.Apis.Dtos
{
    public class LikeDto
    {
        [Key]
        public int? like_id { get; set; }

        [ForeignKey("Post")]
        public int? post_id { get; set; }

        public string? user_id { get; set; }
        [JsonIgnore]
        public Posts? Post { get; set; } // Navigation property


        public DateTime? creation_date { get; set; }

    }
}
