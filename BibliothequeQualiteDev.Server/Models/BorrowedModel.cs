using BibliothequeQualiteDev.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class BorrowedModel
{
    public int id_borrow { get; set; }
    public int user_id { get; set; }
    public int book_id { get; set; }
    public DateTime date_start { get; set; }
    public DateTime date_end { get; set; }
    public bool is_returned { get; set; }

    // IMPORTANT : Ajouter [ForeignKey] pour éviter la création de colonnes fantômes
    [ForeignKey("book_id")]
    public BookModel? Book { get; set; }

    [ForeignKey("user_id")]
    public UsersModel? User { get; set; }
}