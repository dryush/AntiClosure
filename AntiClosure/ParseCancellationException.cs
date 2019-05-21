using System;
using System.Runtime.Serialization;

namespace AntiClosure
{
    internal class ParseCancellationException : Exception
    {
        public ParseCancellationException()
        {
        }

        public ParseCancellationException(string message) : base("parser error: " + message)
        {
            
        }
        
    }
}