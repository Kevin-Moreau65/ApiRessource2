namespace ApiRessource2.Models
{
    public class PostResource
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategorieId { get; set; }
        public string Path { get; set; } = "";
    }
}
