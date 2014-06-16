using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalProgramming.Basics
{
    public class MatchException : Exception
    {
        public MatchException(Type sumType, Type attemptedToMatch)
            : base(string.Format("Match for {0} non exhaustive. Attempted to match against {1}!", sumType, attemptedToMatch))
        {

        }
    }
}
