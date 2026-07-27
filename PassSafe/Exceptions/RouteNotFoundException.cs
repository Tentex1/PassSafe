using System.Runtime.Serialization;

namespace PassSafe.Exceptions
{
    /// <summary>
    /// Thrown when the Navigation Service attempts to navigate to an unregistered route.
    /// </summary>
    [Serializable]
    public class RouteNotFoundException : Exception
    {
        public RouteNotFoundException() { }
        public RouteNotFoundException(string message) : base(message) { }
        public RouteNotFoundException(string message, Exception inner) : base(message, inner) { }
        [Obsolete]
        protected RouteNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}