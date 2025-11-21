using CMCS.web2.Models;
using System.Text;

namespace CMCS.web2.Services
{
    public class ReportService : IReportService
    {
        public byte[] GenerateClaimsReport(List<Claim> claims, DateTime fromDate, DateTime toDate)
        {
            var filteredClaims = claims.Where(c => c.SubmittedDate >= fromDate && c.SubmittedDate <= toDate).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Claims Report");
            sb.AppendLine($"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
            sb.AppendLine("==========================================");
            sb.AppendLine("Date | Email | Hours | Rate | Amount | Status");
            sb.AppendLine("------------------------------------------");

            foreach (var claim in filteredClaims)
            {
                sb.AppendLine($"{claim.SubmittedDate:yyyy-MM-dd} | {claim.LecturerEmail} | {claim.HoursWorked} | €{claim.HourlyRate} | €{claim.TotalAmount} | {claim.Status}");
            }

            sb.AppendLine("==========================================");
            sb.AppendLine($"Total Claims: {filteredClaims.Count}");
            sb.AppendLine($"Total Amount: €{filteredClaims.Sum(c => c.TotalAmount):N2}");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public byte[] GenerateUserReport(List<User> users)
        {
            var sb = new StringBuilder();
            sb.AppendLine("User Report");
            sb.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            sb.AppendLine("==========================================");
            sb.AppendLine("Name | Email | Role | Hourly Rate | Status");
            sb.AppendLine("------------------------------------------");

            foreach (var user in users)
            {
                sb.AppendLine($"{user.Name} {user.Surname} | {user.Email} | {user.Role} | €{user.HourlyRate} | {(user.IsActive ? "Active" : "Inactive")}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}