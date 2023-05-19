using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Eventing.Reader;

namespace ApiRessource2.Models
{
    public class Resource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public string Path { get; set; }
        public bool IsDeleted { get; set; }
        public int UpVote { get; set; }
        public int DownVote { get; set; }
        public int CategorieId { get; set; }
        public virtual Categorie Categorie { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("ResourceId")]
        public virtual ICollection<Comment> Comments { get; set; }

        public virtual Voted Voted { get; set; }

        public virtual Favoris Favoris { get; set; }

    }
}
