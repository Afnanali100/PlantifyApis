using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantifyApp.Core.Models
{
    public class Plants
    {
        public int? Id { get; set; }

        public string? user_id { get; set; }
        public string? common_name { get; set; }

        public string? scientific_name { get; set; }

        public string? other_name { get; set; }

        public string? medium_url { get; set; }

        public string? small_url { get; set; }
    }
}
