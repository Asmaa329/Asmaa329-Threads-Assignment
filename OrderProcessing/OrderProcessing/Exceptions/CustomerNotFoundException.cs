using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Exceptions
{

    public class CustomerNotFoundException : Exception
    {
            public CustomerNotFoundException(string message) : base(message) { }
    }
    
}
