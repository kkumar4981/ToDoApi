using ToDoApi.Models;
using ToDoApi.Models.Auth;

namespace ToDoApi.Services;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
