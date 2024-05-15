using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlantifyApp.Core.Models
{
    public class Likes
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
