namespace PlantifyApp.Apis.Dtos
{
    public class PlantDto
    {
        public string? user_id { get; set; }
        public int? Id { get; set; }
        public string? common_name { get; set; }
        public string?[] scientific_name { get; set; }
        public string?[] other_name { get; set; }
        public DefaultImageDto default_image { get; set; }
    }

    public class DefaultImageDto
    {
        public string? medium_url { get; set; }
        public string? small_url { get; set; }
    }
}
