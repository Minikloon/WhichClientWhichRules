using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Jose;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WhichClientWhichRules.Models;

namespace WhichClientWhichRules.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
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
			var clients = await mgmtClient.Clients.GetAllAsync(fields: "name,global");

			var rulesByClient = clients.Where(c => c.Name != "All Applications").ToDictionary(
				client => client.Name, 
				client => new ClientAndMatchedRules()
				{
					ClientName = client.Name,
					MatchedRules = new List<Rule>()
				}
			);
			var unmatchedRules = new List<UnmatchedRule>();

			foreach (var rule in rules)
			{
				var clientNames = ParseClientsFromScript(rule.Script);
				if (clientNames == null)
				{
					unmatchedRules.Add(new UnmatchedRule()
					{
						RuleName = rule.Name,
						Reason = "Couldn't detect clients within script"
					});
					continue;
				}
				foreach (var detectedClient in clientNames)
				{
					ClientAndMatchedRules client;
					if (rulesByClient.TryGetValue(detectedClient, out client))
					{
						client.MatchedRules.Add(rule);
					}
					else
					{
						unmatchedRules.Add(new UnmatchedRule()
						{
							RuleName = rule.Name,
							Reason = $"Couldn't find matching client named \"{detectedClient}\""
						});
					}
				}
			}

			return View(new RulesAndClientMatches()
			{
				ClientAndRules = rulesByClient.Values.ToList(),
				UnmatchedRules = unmatchedRules
			});
        }

		private const string variableName = "clients";
		private IList<string> ParseClientsFromScript(string script)
		{
			int indexOfClientsVariable = script.IndexOf(variableName, StringComparison.Ordinal);
			if (indexOfClientsVariable == -1)
				return null;
			var scriptFromVariable = script.Substring(indexOfClientsVariable);

			var inBetweenFirstBrackets = scriptFromVariable.Split('[', ']');
			if (inBetweenFirstBrackets.Length < 2)
				return null;
			var commaSeparatedList = inBetweenFirstBrackets[1];

			var clientNameList = commaSeparatedList.Split(',').Select(
				jsString => jsString.Trim(' ', '\'', '"')
			);

			return clientNameList.ToList();
		}
    }
}
