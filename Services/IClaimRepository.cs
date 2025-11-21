using CMCS.web2.Models;
using System;
using System.Collections.Generic;

namespace CMCS.web2.Services
{
    public interface IClaimRepository
    {
        IEnumerable<Claim> GetAllClaims();
        IEnumerable<Claim> GetClaimsForLecturer(Guid lecturerId);
        Claim? GetClaimById(Guid id);
        void AddClaim(Claim claim);
        void UpdateClaim(Claim claim);
    }
}
