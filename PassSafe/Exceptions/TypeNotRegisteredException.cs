using System.Runtime.Serialization;

namespace PassSafe.Exceptions
{
    /// <summary>
    /// Thrown when a requested Type is not registered in the Dependency Injection container.
    /// </summary>
    [Serializable]
    public class TypeNotRegisteredException : Exception
    {
        public TypeNotRegisteredException() { }
        public TypeNotRegisteredException(string message) : base(message) { }
        public TypeNotRegisteredException(string message, Exception inner) : base(message, inner) { }
        [Obsolete]
        protected TypeNotRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}