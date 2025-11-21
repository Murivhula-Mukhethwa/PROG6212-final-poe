using Microsoft.AspNetCore.Mvc;
using CMCS.web2.Models;
using CMCS.web2.Services;

namespace CMCS.web2.Controllers
{
    public class AdminController : Controller
    {
        private readonly IClaimRepository _repository;
        private readonly IFileEncryptionService _fileService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IClaimRepository repository, IFileEncryptionService fileService, ILogger<AdminController> logger)
        {
            _repository = repository;
            _fileService = fileService;
            _logger = logger;
        }

        // GET: /Admin/CoordinatorView
        public IActionResult CoordinatorView(string search = "", string sortBy = "submitted", string sortOrder = "asc")
        {
            if (!IsCoordinatorUser()) return RedirectToAction("AccessDenied", "Home");

            var claims = _repository.GetAllClaims()
                .Where(c => c.Status == ClaimStatus.Pending);

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                claims = claims.Where(c => c.LecturerEmail.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         (c.Notes != null && c.Notes.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            // Sorting functionality
            claims = sortBy.ToLower() switch
            {
                "email" => sortOrder == "desc" ? claims.OrderByDescending(c => c.LecturerEmail) : claims.OrderBy(c => c.LecturerEmail),
                "amount" => sortOrder == "desc" ? claims.OrderByDescending(c => c.TotalAmount) : claims.OrderBy(c => c.TotalAmount),
                "hours" => sortOrder == "desc" ? claims.OrderByDescending(c => c.HoursWorked) : claims.OrderBy(c => c.HoursWorked),
                _ => sortOrder == "desc" ? claims.OrderByDescending(c => c.SubmittedDate) : claims.OrderBy(c => c.SubmittedDate)
            };

            ViewBag.Role = "Coordinator";
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View("ManageClaims", claims.ToList());
        }

        // GET: /Admin/ManagerView
        public IActionResult ManagerView(string search = "", string sortBy = "submitted", string sortOrder = "asc")
        {
            if (!IsManagerUser()) return RedirectToAction("AccessDenied", "Home");

            var claims = _repository.GetAllClaims()
                .Where(c => c.Status == ClaimStatus.Verified);

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                claims = claims.Where(c => c.LecturerEmail.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         (c.Notes != null && c.Notes.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            // Sorting functionality
            claims = sortBy.ToLower() switch
            {
                "email" => sortOrder == "desc" ? claims.OrderByDescending(c => c.LecturerEmail) : claims.OrderBy(c => c.LecturerEmail),
                "amount" => sortOrder == "desc" ? claims.OrderByDescending(c => c.TotalAmount) : claims.OrderBy(c => c.TotalAmount),
                "hours" => sortOrder == "desc" ? claims.OrderByDescending(c => c.HoursWorked) : claims.OrderBy(c => c.HoursWorked),
                _ => sortOrder == "desc" ? claims.OrderByDescending(c => c.SubmittedDate) : claims.OrderBy(c => c.SubmittedDate)
            };

            ViewBag.Role = "Manager";
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View("ManageClaims", claims.ToList());
        }

        [HttpPost]
        public IActionResult Verify(Guid id)
        {
            if (!IsCoordinatorUser()) return RedirectToAction("AccessDenied", "Home");

            var claim = _repository.GetClaimById(id);
            if (claim == null)
                return NotFound();

            claim.Status = ClaimStatus.Verified;
            claim.LastUpdated = DateTime.UtcNow;
            claim.LastUpdatedBy = GetCurrentUserName();
            _repository.UpdateClaim(claim);

            TempData["Success"] = "Claim verified successfully.";
            return RedirectToAction("CoordinatorView");
        }

        [HttpPost]
        public IActionResult Approve(Guid id)
        {
            if (!IsManagerUser()) return RedirectToAction("AccessDenied", "Home");

            var claim = _repository.GetClaimById(id);
            if (claim == null)
                return NotFound();

            claim.Status = ClaimStatus.Approved;
            claim.LastUpdated = DateTime.UtcNow;
            claim.LastUpdatedBy = GetCurrentUserName();
            _repository.UpdateClaim(claim);

            TempData["Success"] = "Claim approved successfully.";
            return RedirectToAction("ManagerView");
        }

        [HttpPost]
        public IActionResult Reject(Guid id, string? rejectionReason = null)
        {
            if (!IsAdminUser()) return RedirectToAction("AccessDenied", "Home");

            var claim = _repository.GetClaimById(id);
            if (claim == null)
                return NotFound();

            claim.Status = ClaimStatus.Rejected;
            claim.LastUpdated = DateTime.UtcNow;
            claim.LastUpdatedBy = GetCurrentUserName();

            if (!string.IsNullOrEmpty(rejectionReason))
            {
                claim.Notes += $"\n\n[REJECTION REASON]: {rejectionReason}";
            }

            _repository.UpdateClaim(claim);

            TempData["Error"] = $"Claim rejected.{(string.IsNullOrEmpty(rejectionReason) ? "" : " Reason: " + rejectionReason)}";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // GET: /Admin/Download/{id}
        public async Task<IActionResult> Download(Guid id)
        {
            if (!IsAdminUser()) return RedirectToAction("AccessDenied", "Home");

            var claim = _repository.GetClaimById(id);
            if (claim == null || claim.StoredFileName == null)
                return NotFound();

            try
            {
                var fileBytes = await _fileService.DecryptFileAsync(claim.StoredFileName);
                return File(fileBytes, "application/octet-stream", claim.OriginalFileName ?? "document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Decryption failed for file download.");
                return BadRequest("Error decrypting file.");
            }
        }

       
        private bool IsCoordinatorUser()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == UserRole.ProgrammeCoordinator.ToString() || userRole == UserRole.HR.ToString();
        }

        private bool IsManagerUser()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == UserRole.AcademicManager.ToString() || userRole == UserRole.HR.ToString();
        }

        private bool IsAdminUser()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == UserRole.ProgrammeCoordinator.ToString() ||
                   userRole == UserRole.AcademicManager.ToString() ||
                   userRole == UserRole.HR.ToString();
        }

        private string GetCurrentUserName()
        {
            return HttpContext.Session.GetString("UserName") ?? "Unknown";
        }
    }
}