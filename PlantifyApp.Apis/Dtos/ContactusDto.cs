namespace PlantifyApp.Apis.Dtos
{
    public class ContactusDto
    {
        public int? id { get; set; }

        public string? name { get; set; }

        public string? email { get; set; }

        public string? message { get; set; }

        public bool? is_replied { get; set; } = false;

        public DateTime? created_at { get; set; } = DateTime.Now;
    }
}
