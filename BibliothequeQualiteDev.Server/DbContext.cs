using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {

        Console.WriteLine("teest");
        BookRepository.Add(new BookModel());

    }

    public DbSet<BookModel> BookRepository => Set<BookModel>();





}
