using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaPractice.Controllers
{
    public class TestController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;

        public TestController(IRepository<Movie> movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<IActionResult> Test()
        {
            try
            {
                // Спроба додати новий фільм
                var movie = new Movie
                {
                    Title = "Test Movie",
                    Description = "Test Description",
                    DurationMinutes = 120,
                    ReleaseDate = DateTime.Now,
                    TrailerUrl = "test-trailer.mp4",
                    Rating = 6.7f,
                    PosterUrl = "test-poster.jpg",
                };

                await _movieRepository.AddAsync(movie);

                // Спроба отримати всі фільми
                var movies = await _movieRepository.GetAllAsync();

                return Json(new { success = true, message = "Repository works!", moviesCount = movies.Count() });
            }
            catch (Exception ex)
            {
                // Отримуємо більш детальну інформацію про помилку
                var innerException = ex.InnerException?.Message ?? "No inner exception";
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    innerError = innerException
                });
            }
        }
    }
}
