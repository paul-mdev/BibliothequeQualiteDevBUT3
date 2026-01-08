public class BookModel
{
    public int book_id { get; set; }
    public string book_name { get; set; } = string.Empty;
    public string book_author { get; set; } = string.Empty; // ← espace ajouté
    public string book_editor { get; set; } = string.Empty;
    public DateTime book_date { get; set; }
    public string? book_image_ext { get; set; }

    // Navigation inverse recommandée pour cascade propre
    public virtual ICollection<BorrowedModel> Borrowed { get; set; } = new List<BorrowedModel>();
}