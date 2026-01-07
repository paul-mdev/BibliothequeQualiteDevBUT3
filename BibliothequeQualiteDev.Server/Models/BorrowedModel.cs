using BibliothequeQualiteDev.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BorrowedModel
{
    [Key]
    public int id_borrow { get; set; }

    public int user_id { get; set; }
    public int book_id { get; set; }

    public DateTime date_start { get; set; }
    public DateTime date_end { get; set; }

    public bool is_returned { get; set; } = false;

    // Propriétés de navigation ESSENTIELLES
    [ForeignKey("user_id")]
    public virtual UsersModel User { get; set; } = null!;

    [ForeignKey("book_id")]
    public virtual BookModel Book { get; set; } = null!;
}