using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WhichClientWhichRules.Models;
using WhichClientWhichRules.ScriptParser;

namespace WhichClientWhichRules.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly IScriptParser _scriptParser = new ConditionBlockScriptParser();
		private readonly Auth0Settings _auth0;

		public HomeController(IOptions<Auth0Settings> auth0Settings)
		{
			_auth0 = auth0Settings.Value;
		}

		public async Task<IActionResult> Index()
		{
	        var authClient = new AuthenticationApiClient(new Uri($"https://{_auth0.Domain}"));
	        var token = await authClient.GetTokenAsync(new ClientCredentialsTokenRequest()
	        {
		        Audience = $"https://{_auth0.Domain}/api/v2/",
				ClientId = _auth0.ClientId,
				ClientSecret = _auth0.ClientSecret
			});

	        var mgmtClient = new ManagementApiClient(token.AccessToken, new Uri($"https://{_auth0.Domain}/api/v2"));

			var rules = await mgmtClient.Rules.GetAllAsync(fields: "name,script,order");
			var clients = await mgmtClient.Clients.GetAllAsync(fields: "name,client_id");

			var rulesByClient = clients.Where(c => c.Name != "All Applications").ToDictionary(
				client => client, 
				client => new ClientAndMatchedRules()
				{
					Client = client,
					MatchedRules = new HashSet<Rule>()
				}
			);
			var unmatchedRules = new List<UnmatchedRule>();

			foreach (var rule in rules)
			{
				var applicableClients = _scriptParser.ParseApplicableClients(rule.Script);
				if (applicableClients == null || applicableClients.Count == 0)
				{
					unmatchedRules.Add(new UnmatchedRule()
					{
						RuleName = rule.Name,
						Reason = "Couldn't detect clients within script"
					});
					continue;
				}
				foreach (var applicable in applicableClients)
				{
					Client matchingClient;
					if (!applicable.TryGetMatchingClient(clients, out matchingClient))
					{
						unmatchedRules.Add(new UnmatchedRule()
						{
							RuleName = rule.Name,
							Reason = $"Couldn't find matching client {applicable.GetHumanReadableId()}"
						});
						continue;
					}

					ClientAndMatchedRules clientAndMatchedRules = rulesByClient[matchingClient];
					clientAndMatchedRules.MatchedRules.Add(rule);
				}
			}

			return View(new RulesAndClientMatches()
			{
				ClientAndRules = rulesByClient.Values.ToList(),
				UnmatchedRules = unmatchedRules
			});
        }
    }
}
