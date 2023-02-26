using System;
using System.Collections.Generic;
using System.Text;

namespace FluffRest.Exception
{
    public class FluffDuplicateParameterException : System.Exception
    {
        public FluffDuplicateParameterException(string message) : base(message) { }
    }
}
