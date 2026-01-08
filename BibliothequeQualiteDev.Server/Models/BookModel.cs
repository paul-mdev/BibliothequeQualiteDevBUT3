using System.Reflection.Metadata;

public class BookModel
{
    public int book_id { get; set; }              // Clé primaire
    public string book_name { get; set; } = string.Empty;
    public string book_author { get; set; }
    public string book_editor { get; set; }

    public DateTime book_date { get; set; }       // Date de publication
    public string? book_image_ext { get; set; }   // Extension de l'image (ex: .jpg) → null si pas d'image

    // Navigation inverse recommandée pour cascade propre
    public virtual ICollection<BorrowedModel> Borrowed { get; set; } = new List<BorrowedModel>();
}
