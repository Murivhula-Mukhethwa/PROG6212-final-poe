using Microsoft.AspNetCore.Mvc;
using CMCS.web2.Models;
using CMCS.web2.Services;

namespace CMCS.web2.Controllers
{
    public class ClaimController : Controller
    {
        private readonly IClaimRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IFileEncryptionService _fileService;
        private readonly ILogger<ClaimController> _logger;

        public ClaimController(IClaimRepository repository, IUserRepository userRepository,
                             IFileEncryptionService fileService, ILogger<ClaimController> logger)
        {
            _repository = repository;
            _userRepository = userRepository;
            _fileService = fileService;
            _logger = logger;
        }

        // GET: /Claim/Create
        public IActionResult Create()
        {
            if (!IsLecturerUser()) return RedirectToAction("AccessDenied", "Home");

            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var model = new ClaimCreateViewModel
            {
                LecturerEmail = user.Email,
                LecturerName = $"{user.Name} {user.Surname}",
                HourlyRate = user.HourlyRate
            };

            return View(model);
        }

        // POST: /Claim/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaimCreateViewModel model)
        {
            if (!IsLecturerUser()) return RedirectToAction("AccessDenied", "Home");

            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            // Validate monthly hours (180 hours max)
            var currentMonthClaims = _repository.GetAllClaims()
                .Where(c => c.UserId == user.UserId &&
                           c.SubmittedDate.Month == DateTime.UtcNow.Month &&
                           c.SubmittedDate.Year == DateTime.UtcNow.Year)
                .Sum(c => c.HoursWorked);

            if (currentMonthClaims + model.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked",
                    $"Total hours for this month would exceed 180. You have already claimed {currentMonthClaims} hours.");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate user data
                model.LecturerEmail = user.Email;
                model.LecturerName = $"{user.Name} {user.Surname}";
                model.HourlyRate = user.HourlyRate;
                return View(model);
            }

            string? storedFileName = null;

            if (model.SupportingDocument != null)
            {
                if (!FileValidationService.IsValidFile(model.SupportingDocument, out string error))
                {
                    ModelState.AddModelError("SupportingDocument", error);
                    return View(model);
                }

                try
                {
                    storedFileName = await _fileService.EncryptAndSaveFileAsync(model.SupportingDocument);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "File encryption failed");
                    ModelState.AddModelError("SupportingDocument", "File upload failed. Please try again.");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("SupportingDocument", "Supporting document is required.");
                return View(model);
            }

            var claim = new Claim
            {
                UserId = user.UserId,
                LecturerEmail = user.Email,
                LecturerName = $"{user.Name} {user.Surname}",
                HoursWorked = model.HoursWorked,
                HourlyRate = user.HourlyRate, // From user profile
                Notes = model.Notes,
                OriginalFileName = model.SupportingDocument?.FileName,
                StoredFileName = storedFileName,
                Status = ClaimStatus.Pending,
                SubmittedDate = DateTime.UtcNow
            };

            _repository.AddClaim(claim);
            TempData["Success"] = "Claim submitted successfully!";
            return RedirectToAction("MyClaims");
        }

        // GET: /Claim/MyClaims
        public IActionResult MyClaims(string sortBy = "submitted", string sortOrder = "desc")
        {
            if (!IsLecturerUser()) return RedirectToAction("AccessDenied", "Home");

            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var claims = _repository.GetAllClaims()
                                    .Where(c => c.UserId == user.UserId);

            // Enhanced sorting
            claims = sortBy.ToLower() switch
            {
                "status" => sortOrder == "desc" ? claims.OrderByDescending(c => c.Status) : claims.OrderBy(c => c.Status),
                "amount" => sortOrder == "desc" ? claims.OrderByDescending(c => c.TotalAmount) : claims.OrderBy(c => c.TotalAmount),
                "hours" => sortOrder == "desc" ? claims.OrderByDescending(c => c.HoursWorked) : claims.OrderBy(c => c.HoursWorked),
                _ => sortOrder == "desc" ? claims.OrderByDescending(c => c.SubmittedDate) : claims.OrderBy(c => c.SubmittedDate)
            };

            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(claims.ToList());
        }

        // GET: /Claim/Download/{id}
        public async Task<IActionResult> Download(Guid id)
        {
            var claim = _repository.GetClaimById(id);
            if (claim == null || claim.StoredFileName == null)
                return NotFound();

            // Check if user owns the claim or is admin
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            if (claim.UserId != user.UserId && !IsAdminUser())
                return RedirectToAction("AccessDenied", "Home");

            try
            {
                var fileBytes = await _fileService.DecryptFileAsync(claim.StoredFileName);
                return File(fileBytes, "application/octet-stream", claim.OriginalFileName ?? "document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File decryption failed for claim {ClaimId}", id);
                return BadRequest("Error decrypting file.");
            }
        }

        private User? GetCurrentUser()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                return _userRepository.GetUserById(userId);
            }
            return null;
        }

        private bool IsLecturerUser()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == UserRole.Lecturer.ToString();
        }

        private bool IsAdminUser()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == UserRole.ProgrammeCoordinator.ToString() ||
                   userRole == UserRole.AcademicManager.ToString() ||
                   userRole == UserRole.HR.ToString();
        }
    }
}