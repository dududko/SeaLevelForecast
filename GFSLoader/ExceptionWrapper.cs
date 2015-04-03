using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodForecasting
{
    internal class ExceptionWrapper : ApplicationException
    {
        public ExceptionWrapper(){}

        public ExceptionWrapper(string message) : base(message){}

        public ExceptionWrapper(
            string message,
            Exception innerException) : base(message, innerException){}
    }
}
