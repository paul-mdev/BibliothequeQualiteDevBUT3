using System.Reflection.Metadata;

public class BookModel
{
    public int Id { get; set; }
    public string BookName { get; set; } = string.Empty;
    public string BookAuthor{ get; set; }
    public string BookEditor { get; set; }

    public DateTime BookDate { get; set; }
  //  public Blob BookPicture { get; set; }

}

