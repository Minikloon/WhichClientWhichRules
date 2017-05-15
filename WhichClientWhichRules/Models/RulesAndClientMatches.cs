using System.Collections.Generic;
using Auth0.ManagementApi.Models;

namespace WhichClientWhichRules.Models
{
    public class RulesAndClientMatches
    {
		public IList<ClientAndMatchedRules> ClientAndRules { get; set; }
		public IList<UnmatchedRule> UnmatchedRules { get; set; }
    }

	public class ClientAndMatchedRules
	{
		public Client Client { get; set; }
		public ISet<Rule> MatchedRules { get; set; }
	}

	public class UnmatchedRule
	{
		public string RuleName { get; set; }
		public string Reason { get; set; }
	}
}
