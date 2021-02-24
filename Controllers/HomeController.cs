using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FavoriteMovies.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace FavoriteMovies.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyContext _context;

        public HomeController(MyContext context)
        {
            _context = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
        //process registration form
        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            //check for validation errors, ensure no two users have same email
            if(_context.Users.Any(u => u.Email == newUser.Email))
            {
                // email already exists in database
                ModelState.AddModelError("Email", "Email address is already in use.");
            }
            if(ModelState.IsValid)
            {
                //hash the password
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                //add user to db
                _context.Add(newUser);
                _context.SaveChanges();

                //store id in session
                HttpContext.Session.SetInt32("UserId", newUser.UserId);

                // redirect to dashboard
                return RedirectToAction("Dashboard");
            }
            
            return View("Index");
            
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser userToLogin)
        {
            // check if user exists in db
            var foundUser = _context.Users
                .FirstOrDefault(u => u.Email == userToLogin.LoginEmail);

            if(foundUser == null)
            {
                ModelState.AddModelError("LoginEmail", "Please check your email and password.");
                return View("Index");
            }
            // check password match
            var hasher = new PasswordHasher<LoginUser>();
            var result = hasher.VerifyHashedPassword(userToLogin, foundUser.Password, userToLogin.LoginPassword);

            if(result == 0)
            {
                ModelState.AddModelError("LoginEmail", "Please check your email and password.");
                return View("Index");
            }

            HttpContext.Session.SetInt32("UserId", foundUser.UserId);
            return RedirectToAction("Dashboard");

            // check validation
        }
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            //get the user's info from session
            var CurrentUser = GetCurrentUser();

            //check to ensure logged in
            if(CurrentUser == null)
            {
                return RedirectToAction("Index");
            }
            // pass user info to the view
            ViewBag.CurrentUser = CurrentUser;
            ViewBag.AllMovies = _context.Movies
                .Include(movie => movie.PostedBy)
                .Include(Movie => Movie.Likes)
                .OrderByDescending(Movie => Movie.Likes.Count)
                .ToList();
            return View();
        }

        [HttpGet("movies/new")]
        public IActionResult NewMoviePage()
        {
            return View();
        }

        //process the form
        [HttpPost("movies")]
        public IActionResult CreateMovie(Movie movieFromForm)
        {
            if(movieFromForm.ReleaseDate >= DateTime.Now)
            {
                //add error
                ModelState.AddModelError("ReleaseDate", "Please ensure the date is in the past.");
            }
            if(ModelState.IsValid)
            {
                movieFromForm.UserId = (int)HttpContext.Session.GetInt32("UserId");
                _context.Add(movieFromForm);
                // add user to the like list

                _context.SaveChanges();
                return Redirect($"/movies/{movieFromForm.MovieId}");
            }
            return View("NewMoviePage");
        }

        [HttpGet("movies/{num}")]
        public IActionResult SingleMoviePage(int num)
        {
            ViewBag.Movie = _context.Movies
                .Include(m => m.PostedBy)
                .Include(m => m.Likes) // pull all the likes
                .ThenInclude(like => like.UserWhoLikes) //within each like, give the user
                .First(m => m.MovieId == num);
            return View();
        }

        [HttpPost("movies/{MovieId}/delete")]
        public IActionResult DeleteMovie(int MovieId)
        {
            var MovieToDelete = _context.Movies
                .First(m => m.MovieId == MovieId);

            _context.Remove(MovieToDelete);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        //handle unliking
        [HttpPost("/movies/{MovieId}/likes/delete")]
        public IActionResult DeleteLike(int MovieId)
        {
            var CurrentUser = GetCurrentUser();
            // get the like
            var likeToDelete = _context.Likes
                .First(like => like.MovieId == MovieId && like.UserId == CurrentUser.UserId);

            _context.Remove(likeToDelete);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        //handle liking
        [HttpPost("/movies/{movieId}/likes")]
        public IActionResult AddLike(int MovieId)
        {
            var CurrentUser = GetCurrentUser();

            var LikeToAdd = new Like {
                UserId = GetCurrentUser().UserId,
                MovieId = MovieId
            };

            _context.Add(LikeToAdd);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }
        public User GetCurrentUser()
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if(UserId == null)
            {
                return null;
            }
            User CurrentUser = _context.Users.First(u => u.UserId == UserId);
            return CurrentUser;
        }
    }
}
