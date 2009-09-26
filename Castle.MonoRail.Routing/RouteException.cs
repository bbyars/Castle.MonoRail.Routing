using System;
using System.Runtime.Serialization;

namespace Castle.MonoRail.Routing
{
    public class RouteException : ApplicationException
    {
        public RouteException()
        {
        }

        public RouteException(string message) : base(message)
        {
        }

        public RouteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RouteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}