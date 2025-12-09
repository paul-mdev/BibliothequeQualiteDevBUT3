using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeQualiteDev.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly AppDbContext _db;

        public WeatherForecastController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IEnumerable<BookModel>> Get()
        {

            Console.WriteLine("ifjebvhjijfnvkjn");

            Console.WriteLine(_db.BOOK.Count());

           // return new[] {new BookModel(), new BookModel() };

            // Charge toutes les lignes de la table WeatherRecords
            return await _db.BOOK.ToListAsync();
        }



        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        //private readonly ILogger<WeatherForecastController> _logger;

        //public WeatherForecastController(ILogger<WeatherForecastController> logger)
        //{
        //    _logger = logger;
        //}

        //[HttpGet]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}
    }
}
