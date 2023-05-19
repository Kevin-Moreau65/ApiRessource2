namespace ApiRessource2.Models
{
    public class Favoris
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ResourceId { get; set; }

        public virtual Resource Resource { get; set; }
        public virtual User User { get; set; }
    }
}
