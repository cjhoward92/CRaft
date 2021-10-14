using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRaft.Utils
{
    public class ValidationUtils
    {
        public static void IsRequired(string variableName, string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException($"{variableName} must have a value");
        }

        public static void IsAboveZero(string variableName, ulong value)
        {
            if (value == 0)
                throw new ArgumentException($"{variableName} must be greater than zero");
        }
    }
}
