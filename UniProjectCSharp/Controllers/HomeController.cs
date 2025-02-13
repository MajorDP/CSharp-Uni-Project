using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using UniProjectCSharp.Models;
using UniProjectCSharp.Services;
using System.Text.RegularExpressions;

namespace UniProjectCSharp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GameService _gameService;

        public HomeController(ILogger<HomeController> logger, GameService gameService)
        {
            _logger = logger;
            _gameService = gameService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserCookie = Request.Cookies["currentUser"];

            if (currentUserCookie != null)
            {
                //IF THERE IS A LOGGED IN USER, SET VIEWDATA FOR NAVIGATION FOR LOGGED IN USERS
                ViewData["IsLoggedIn"] = true;
            }
            else
            {
                ViewData["IsLoggedIn"] = false;
            }
            try
            {

                var currentIndex = Convert.ToInt32(HttpContext.Request.Query["currentIndex"].FirstOrDefault());
                if (currentIndex == null)
                {

                    var games = await _gameService.GetGames();
                    var length = games.Count;

                    Random rand = new Random();
                    var randomGames = new List<Games>();

                    while (randomGames.Count < 3)
                    {
                        int randomIndex = rand.Next(games.Count);  
                        if (!randomGames.Contains(games[randomIndex]))  
                        {
                            randomGames.Add(games[randomIndex]);
                        }
                    }

                    return View(randomGames);
                }
                else
                {
                    var games = await _gameService.GetGames();
                    return View(games);
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Game()
        {
            // SEARCHING COOKIES TO SEE IF THERE IS A LOGGED IN USER, IF THERE IS NOT, WE'RE GOING STRAIGHT TO LOGIN
            var currentUserCookie = JsonConvert.DeserializeObject<User>(Request.Cookies["currentUser"]);

            if (currentUserCookie == null)
            {
                return RedirectToAction("Login");
            }

            //IF THERE IS A LOGGED IN USER, SET VIEWDATA FOR NAVIGATION FOR LOGGED IN USERS
            ViewData["IsLoggedIn"] = true;
            ViewData["savedGames"] = currentUserCookie.SavedGames;

            try
            {
                var id = Convert.ToInt32(HttpContext.Request.Query["Id"].FirstOrDefault());
                var games = await _gameService.GetGames();
                var game = games.Find(el => el.Id == id);

                return View(game); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching a game in controller: {ex.Message}");
                return View(new List<dynamic>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Games()
        {

            // SEARCHING COOKIES TO SEE IF THERE IS A LOGGED IN USER, IF THERE IS NOT, WE'RE GOING STRAIGHT TO LOGIN
            var currentUserCookie = Request.Cookies["currentUser"];

            if (currentUserCookie == null)
            {
                return RedirectToAction("Login");
            }

            //IF THERE IS A LOGGED IN USER, SET VIEWDATA FOR NAVIGATION FOR LOGGED IN USERS
            ViewData["IsLoggedIn"] = true;

            try
            {
                var games = await _gameService.GetGames();
                return View(games);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching games in controller: {ex.Message}");
                return View(new List<dynamic>()); 
            }
        }

        public async Task<IActionResult> UpdateRating(int id, string vote) {
            Console.WriteLine(id.ToString(), vote);

            // SEARCHING COOKIES TO SEE IF THERE IS A LOGGED IN USER, IF THERE IS NOT, WE'RE GOING STRAIGHT TO LOGIN
            var currentUserCookie = Request.Cookies["currentUser"];

            if (currentUserCookie == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                var games = await _gameService.GetGames();
                var game = games.Find(el => el.Id == id);
                int index = games.FindIndex(game => game.Id == id);
                if (game == null)
                {
                    Console.WriteLine("aaaaaaaaaaaa");
                    return NotFound();
                }

                if (vote == "upvote")
                {
                    game.Rating = Math.Round(game.Rating + 0.1, 1);  // Increase rating by 0.1
                }
                else if (vote == "downvote")
                {
                    game.Rating = Math.Round(game.Rating - 0.1, 1);  // Decrease rating by 0.1
                }

                games[index] = game;

                await _gameService.UpdateGames(games);

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating rating: {ex.Message}");
                return View("Games");
            }
        }


        public async Task<IActionResult> AddGame(int id)
        {

            // SEARCHING COOKIES TO SEE IF THERE IS A LOGGED IN USER, IF THERE IS NOT, WE'RE GOING STRAIGHT TO LOGIN
            var currentUserCookie = JsonConvert.DeserializeObject<User>(Request.Cookies["currentUser"]);

            if (currentUserCookie == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                var games = await _gameService.GetGames();


                currentUserCookie.SavedGames.Add(id);
                var updatedUserCookie = JsonConvert.SerializeObject(currentUserCookie);
                Response.Cookies.Append("currentUser", updatedUserCookie);
                await _gameService.UpdateProfile(currentUserCookie);




                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating rating: {ex.Message}");
                return View("Games");
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> OnLogin(User data)
        {
            if (data == null)
            {
                return RedirectToAction("Login", new { error = "null data." });
            }

            if (data.Email == null || data.Password == null)
            {
                return RedirectToAction("Login", new { error = "Please enter all credentials." });
            }

            var userData = await _gameService.Login(data);

            if (userData == null)
            {
                return RedirectToAction("Login", new { error = "Error retrieving user data from the service." });
            }

            var currentUser = userData.Find(x => x.Email == data.Email && x.Password == data.Password);

            if (currentUser == null)
            {
                return RedirectToAction("Login", new { error = "User not found." });
            }

            var currentUserJson = JsonConvert.SerializeObject(currentUser);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,  // Make sure it's not accessible via JavaScript
                Secure = true,    // Use this for production when using HTTPS
                Expires = DateTime.Now.AddHours(1) // Set expiration for the cookie
            };

            // Step 8: Set the cookie in the response
            Response.Cookies.Append("currentUser", currentUserJson, cookieOptions);

            // Step 9: Redirect to the index page
            return RedirectToAction("index");
        }


        public async Task<IActionResult> OnRegister(User data)
        {

            if (data == null)
            {
                return RedirectToAction("Register", new { error = "Null data." });
            }

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(data.Email, emailPattern))
            {
                return RedirectToAction("Register", new { error = "Invalid email format." });
            }

            if(data.Password.Length < 6)
            {
                return RedirectToAction("Register", new { error = "Password must be at least 6 characters." });
            }

            if (data.Email == null || data.Password == null || data.RepeatPassword == null)
            {
                return RedirectToAction("Register", new { error = "Please enter all credentials." });
            }

            if (data.Password != data.RepeatPassword)
            {
                return RedirectToAction("Register", new { error = "Password and Repeat Password don't match." });
            }

            var userData = await _gameService.Login(data);
            var currentUser = userData.Find(x => x.Email == data.Email);


            if (currentUser != null)
            {
                return RedirectToAction("Register", new { error = "User with same email already exists." });
            }
            var newUser = new User
            {
                Email = data.Email,
                Password = data.Password,
                IsAdmin = false,
                SavedGames = new List<int> { 1 }
            };


            await _gameService.CreateUser(newUser);

            var currentUserJson = JsonConvert.SerializeObject(newUser);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Make sure it's not accessible via JavaScript
                Expires = DateTime.Now.AddHours(1) // Set expiration for the cookie
            };

            // Set the cookie in the response
            Response.Cookies.Append("currentUser", currentUserJson, cookieOptions);
            return RedirectToAction("index");
        }

        public IActionResult Register()
        {
            return View();
        }
        public async Task<IActionResult> Profile()
        {

            // SEARCHING COOKIES TO SEE IF THERE IS A LOGGED IN USER, IF THERE IS NOT, WE'RE GOING STRAIGHT TO LOGIN
            var currentUserCookie = JsonConvert.DeserializeObject < User > (Request.Cookies["currentUser"]);

            if (currentUserCookie == null)
            {
                return RedirectToAction("Login");
            }

            //IF THERE IS A LOGGED IN USER, SET VIEWDATA FOR NAVIGATION FOR LOGGED IN USERS
            ViewData["userEmail"] = currentUserCookie.Email;
            ViewData["userRegistrationDate"] = DateTime.Now.ToString("dd.MM.yyyy"); //FIX IN DATABASE
            ViewData["savedGames"] = currentUserCookie.SavedGames;
            ViewData["IsLoggedIn"] = true;


            try
            {
                var games = await _gameService.GetGames();
                return View(games);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching games in controller: {ex.Message}");
                return View(new List<dynamic>()); 
            }
        }


        public IActionResult Logout()
        {
            Response.Cookies.Delete("currentUser");
            return View("Login");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
