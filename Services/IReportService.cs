using CMCS.web2.Models;

namespace CMCS.web2.Services
{
    public interface IReportService
    {
        byte[] GenerateClaimsReport(List<Claim> claims, DateTime fromDate, DateTime toDate);
        byte[] GenerateUserReport(List<User> users);
    }
}