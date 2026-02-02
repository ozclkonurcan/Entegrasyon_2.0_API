using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace WEB.Middleware;

public class RoleClaimsTransformer : IClaimsTransformation
{
	public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
	{
		if (principal.Identity is ClaimsIdentity identity)
		{
			var roleClaim = identity.FindFirst(ClaimTypes.Role);
			if (roleClaim != null && roleClaim.Value == "2")
			{
				// Rolü yeniden ekliyoruz
				identity.RemoveClaim(roleClaim);
				identity.AddClaim(new Claim(ClaimTypes.Role, "SuperAdmin"));
			}
		}
		return Task.FromResult(principal);
	}
}