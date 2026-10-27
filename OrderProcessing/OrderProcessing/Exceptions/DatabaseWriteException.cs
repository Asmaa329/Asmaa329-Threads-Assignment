using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Exceptions
{
    public class DatabaseWriteException : Exception
    {
        public DatabaseWriteException(string message) : base(message) { }
    }
}
