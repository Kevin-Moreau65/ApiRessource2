using System.ComponentModel.DataAnnotations.Schema;

namespace ApiRessource2.Models
{
    public class Consultation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public int ResourceId { get; set; }
        public virtual Resource Resource { get; set; }
        public virtual User User { get; set; }
    }
}