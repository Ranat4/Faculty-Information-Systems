using FacultyInformationSystem_FIS_.Data;
using System.Security.Claims;

namespace FacultyInformationSystem_FIS_.Services
{
    public class CollegeScopeService : ICollegeScopeService
    {
        private const int NoCollegeSentinel = -1;
        private readonly ApplicationDbContext _context;

        public CollegeScopeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetScopeCollegeIdAsync(ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
            {
                return null;
            }

            if (user.IsInRole("Dean"))
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var dean = await _context.Users.FindAsync(userId);
                return dean?.CollegeId ?? NoCollegeSentinel;
            }

            return NoCollegeSentinel;
        }

        public async Task<bool> CanAccessRecordAsync(ClaimsPrincipal user, int? submitterCollegeId)
        {
            if (user.IsInRole("Admin"))
            {
                return true;
            }

            if (user.IsInRole("Dean"))
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var dean = await _context.Users.FindAsync(userId);
                return dean?.CollegeId != null && dean.CollegeId == submitterCollegeId;
            }

            return false;
        }
    }
}