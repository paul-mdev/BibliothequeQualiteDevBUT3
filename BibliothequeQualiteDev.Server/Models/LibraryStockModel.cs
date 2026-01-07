using System.Reflection.Metadata;

public class LibraryStockModel
{
    public int book_id { get; set; }
    public int total_stock { get; set; } = 1;
    public int borrowed_count { get; set; } = 0;

    public int AvailableCount => total_stock - borrowed_count;
}