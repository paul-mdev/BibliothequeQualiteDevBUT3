using System.Reflection.Metadata;

public class BorrowedModel
{
    public int id_borrow { get; set; }
    public int user_id { get; set; }
    public int book_id { get; set; }  // Nouveau

    public DateTime date_start { get; set; }
    public DateTime date_end { get; set; }
    public bool is_returned { get; set; }
}
