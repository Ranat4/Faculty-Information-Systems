using System.Security.Claims;

namespace FacultyInformationSystem_FIS_.Services
{
    public interface ICollegeScopeService
    {
        // null = unrestricted (Admin). A real CollegeId = restrict list
        // results to that college (Dean). Deans with no college assigned
        // get an impossible sentinel value so they see nothing, not
        // everything — never treat "no college" as "no restriction."
        Task<int?> GetScopeCollegeIdAsync(ClaimsPrincipal user);

        // Used to guard direct access to a single record by ID, so a
        // Dean can't bypass the list filtering just by typing a URL.
        Task<bool> CanAccessRecordAsync(ClaimsPrincipal user, int? submitterCollegeId);
    }
}