using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Exceptions
{
    public class InvalidCouponException : Exception
    {
        public InvalidCouponException(string message) : base(message) { }
    }
}
