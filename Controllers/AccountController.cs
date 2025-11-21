using Microsoft.AspNetCore.Mvc;
using CMCS.web2.Models;
using CMCS.web2.Services;

namespace CMCS.web2.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserRepository userRepository, ILogger<AccountController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // GET: /Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            // If user is already logged in, redirect to home
            if (IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = _userRepository.AuthenticateUser(model.Email, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    _logger.LogWarning("Failed login attempt for email: {Email}", model.Email);
                    return View(model);
                }

                // Set session
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", $"{user.Name} {user.Surname}");
                HttpContext.Session.SetString("UserRole", user.Role.ToString());

                _logger.LogInformation("User {Email} logged in successfully", user.Email);

                // Redirect based on role or returnUrl
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            HttpContext.Session.Clear();
            _logger.LogInformation("User {Email} logged out", userEmail);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        private bool IsUserAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }
    }
}