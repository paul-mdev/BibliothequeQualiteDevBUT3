using System.Reflection.Metadata;

public class BookModel
{
    public int book_id { get; set; }
    public string book_name { get; set; } = string.Empty;
    public string book_author{ get; set; }
    public string book_editor { get; set; }

    public DateTime book_date { get; set; }
   //public Blob book_picture { get; set; }
    public string? book_image_ext { get; set; }

}

