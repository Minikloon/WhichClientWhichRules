using System.Collections.Generic;

namespace WhichClientWhichRules.ScriptParser
{
	interface IScriptParser
	{
		IList<IApplicableClient> ParseApplicableClients(string script);
	}
}
