using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Exceptions
{
    public class SmtpUnavailableException : Exception
    {
        public SmtpUnavailableException(string message) : base(message) { }
    }
}
