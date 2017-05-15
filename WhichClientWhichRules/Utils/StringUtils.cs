using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhichClientWhichRules.Utils
{
    public static class StringUtils
    {
	    public static string GetFirstEnclosed(this string str, char betweenA, char andB)
	    {
		    var inBetween = str.Split(betweenA, andB);
		    if (inBetween.Length < 3)
			    return null;
		    return inBetween[1];
	    }

	    public static string WithoutWhitespaces(this string str)
	    {
		    return new string(str.Where(c => !char.IsWhiteSpace(c)).ToArray());
	    }
	}
}
