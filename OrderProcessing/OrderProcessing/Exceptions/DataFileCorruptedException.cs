using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Exceptions
{
    public class DataFileCorruptedException : Exception
    {
        public DataFileCorruptedException(string fileName, string message)
        : base($"The file '{fileName}' is corrupted. Details: {message}")
        {
        }
    }
}
