using System;
using System.Collections.Generic;
using System.Linq;
using WhichClientWhichRules.Utils;

namespace WhichClientWhichRules.ScriptParser
{
    public class VariableScriptParser : IScriptParser
    {
	    private readonly string _variableName;

	    public VariableScriptParser(string variableName)
	    {
		    _variableName = variableName;
	    }

	    public IList<IApplicableClient> ParseApplicableClients(string script)
	    {
			int indexOfClientsVariable = script.IndexOf(_variableName, StringComparison.Ordinal);
		    if (indexOfClientsVariable == -1)
			    return null;
		    var scriptFromVariable = script.Substring(indexOfClientsVariable);

		    var commaSeparatedList = scriptFromVariable.GetFirstEnclosed('[', ']');
		    if (commaSeparatedList == null)
			    return null;

		    var clientNameList = commaSeparatedList.Split(',').Select(
			    jsString => jsString.Trim(' ', '\'', '"')
		    );

		    return clientNameList
				.Select(clientName => (IApplicableClient) new ApplicableByName(clientName))
				.ToList();
		}
    }
}
