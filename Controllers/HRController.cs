using Microsoft.AspNetCore.Mvc;
using CMCS.web2.Models;
using CMCS.web2.Services;

namespace CMCS.web2.Controllers
{
    public class HRController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IReportService _reportService;
        private readonly ILogger<HRController> _logger;

        public HRController(IUserRepository userRepository, IClaimRepository claimRepository,
                          IReportService reportService, ILogger<HRController> logger)
        {
            _userRepository = userRepository;
            _claimRepository = claimRepository;
            _reportService = reportService;
            _logger = logger;
        }

        // GET: /HR/Users
        public IActionResult Users()
        {
            if (!IsHRUser()) return RedirectToAction("AccessDenied", "Home");

            var users = _userRepository.GetAllUsers();
            return View(users);
        }

        // GET: /HR/CreateUser
        public IActionResult CreateUser()
        {
            if (!IsHRUser()) return RedirectToAction("AccessDenied", "Home");
            return View();
        }

        // POST: /HR/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User user)
        {
            if (!IsHRUser()) return RedirectToAction("AccessDenied", "Home");

            if (ModelState.IsValid)
            {
                _userRepository.AddUser(user);
                TempData["Success"] = "User created successfully";
                return RedirectToAction("Users");
            }

            return View(user);
        }

        // GET: /HR/EditUser/{id}
        public IActionResult EditUser(Guid id)
        {
            if (!IsHRUser()) return RedirectToAction("AccessDenied", "Home");

            var user = _userRepository.GetUserById(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /HR/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User user)
        {
            if (!IsHRUser()) return RedirectToAction("AccessDenied", "Home");

            if (ModelState.IsValid)
            {
                _userRepository.UpdateUser(user);
                TempData["Success"] = "User updated successfully";
                return RedirectToAction("Users");
            }

            return View(user);
        }

        // GET: /HR/Reports
        public IActionResult Reports()
        {
            if (!IsHRUser()) return RedirectToAction("AccessDenied", "Home");
            return View();
        }

        // POST: /HR/GenerateClaimsReport
        [HttpPost]
        public IActionResult GenerateClaimsReport(DateTime fromDate, DateTime toDate)
        {
            if (!IsHRUser()) return RedirectToAction("AccessDenied", "Home");

            var claims = _claimRepository.GetAllClaims();
            var reportBytes = _reportService.GenerateClaimsReport((List<Claim>)claims, fromDate, toDate);

            return File(reportBytes, "text/plain", $"ClaimsReport_{DateTime.Now:yyyyMMdd}.txt");
        }

        // POST: /HR/GenerateUserReport
        [HttpPost]
        public IActionResult GenerateUserReport()
        {
            if (!IsHRUser()) return RedirectToAction("AccessDenied", "Home");

            var users = _userRepository.GetAllUsers();
            var reportBytes = _reportService.GenerateUserReport(users);

            return File(reportBytes, "text/plain", $"UserReport_{DateTime.Now:yyyyMMdd}.txt");
        }

        private bool IsHRUser()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == UserRole.HR.ToString();
        }
    }
}