using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization.Internal;
using WhichClientWhichRules.Models;

namespace WhichClientWhichRules.Controllers
{
	public class AccountController : Controller
	{
		public IActionResult Login(string returnUrl = "/")
		{
			return new ChallengeResult("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
		}

		[Authorize]
		public async Task Logout()
		{
			await HttpContext.Authentication.SignOutAsync("Auth0", new AuthenticationProperties
			{
				// Indicate here where Auth0 should redirect the user after a logout.
				// Note that the resulting absolute Uri must be whitelisted in the 
				// **Allowed Logout URLs** settings for the client.
				RedirectUri = Url.Action("Index", "Home")
			});
			await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		}
	}
}
