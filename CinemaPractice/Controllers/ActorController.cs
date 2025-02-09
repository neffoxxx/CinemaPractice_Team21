using Microsoft.AspNetCore.Mvc;
using Infrastructure.Entities; // Залежно від вашої структури проекту
using AppCore.Interfaces;
using System.Threading.Tasks;

namespace CinemaPractice.Controllers
{
    public class ActorController : Controller
    {
        private readonly IActorService _actorService; // Ваш сервіс для роботи з акторами

        public ActorController(IActorService actorService)
        {
            _actorService = actorService;
        }

        // Дія для перегляду деталей актора
        public async Task<IActionResult> Details(int id, int movieId)
        {
            var actorDto = await _actorService.GetActorByIdAsync(id); // Отримати актора за ідентифікатором
            if (actorDto == null)
            {
                return NotFound(); // Повернути 404, якщо актора не знайдено
            }

            ViewData["MovieId"] = movieId; // Зберегти movieId у ViewData для використання в представленні
            return View(actorDto); // Повернути представлення з даними актора
        }

        // Дія для відображення списку акторів
        public async Task<IActionResult> Index()
        {
            var actors = await _actorService.GetAllActorsAsync(); // Отримати всіх акторів
            return View(actors); // Повернути представлення з даними акторів
        }

        // Додайте інші дії, якщо потрібно
    }
} 