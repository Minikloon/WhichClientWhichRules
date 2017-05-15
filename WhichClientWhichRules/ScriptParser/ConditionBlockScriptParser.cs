using System;
using System.Collections.Generic;
using System.Text;
using WhichClientWhichRules.Utils;

namespace WhichClientWhichRules.ScriptParser
{
    public class ConditionBlockScriptParser : IScriptParser
    {
	    public IList<IApplicableClient> ParseApplicableClients(string script)
	    {
		    var noWhitespace = RemoveWhitespaceExceptInsideQuotes(script);
		    var firstIfIndex = noWhitespace.IndexOf("if(context.", StringComparison.Ordinal);
		    if (firstIfIndex == -1)
			    return null;

		    var withinCondition = noWhitespace.Substring(firstIfIndex).GetFirstEnclosed('(', ')');

		    var applicableClients = new List<IApplicableClient>();
		    while (true)
		    {
				string enclosed = withinCondition.GetFirstEnclosed('\'', '\'');
				IApplicableClient applicable;
			    if (withinCondition.StartsWith("context.clientName"))
					applicable = new ApplicableByName(enclosed);
				else if (withinCondition.StartsWith("context.clientID"))
					applicable = new ApplicableById(enclosed);
			    else
				    break;

				applicableClients.Add(applicable);

			    var indexOfNextCheck = withinCondition.IndexOf("&&", StringComparison.Ordinal) + 2;
			    withinCondition = withinCondition.Substring(indexOfNextCheck);
		    }

		    return applicableClients;
	    }

	    private string RemoveWhitespaceExceptInsideQuotes(string str)
	    {
		    var sb = new StringBuilder(str.Length);
		    bool insideQuote = false;
		    foreach (char c in str)
		    {
			    if (!insideQuote && char.IsWhiteSpace(c))
				    continue;
			    if (c == '\'')
				    insideQuote = !insideQuote;
			    sb.Append(c);
		    }
		    return sb.ToString();
	    }
    }
}
