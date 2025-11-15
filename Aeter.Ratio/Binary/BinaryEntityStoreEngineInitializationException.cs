using System;

namespace Aeter.Ratio.Binary
{
    public class BinaryEntityStoreEngineInitializationException : Exception
    {
        public BinaryEntityStoreEngineInitializationException()
        {
        }

        public BinaryEntityStoreEngineInitializationException(string? message) : base(message)
        {
        }

        public BinaryEntityStoreEngineInitializationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        internal static BinaryEntityStoreEngineInitializationException HeaderFailed()
            => new($"Failed to initialize engine. Reason was 'Read header failed'");

        internal static BinaryEntityStoreEngineInitializationException TocFailed(string message)
            => new($"Failed to initialize table of content. Reason was '{message}'");

        internal static BinaryEntityStoreEngineInitializationException TocFailed(Exception exception)
            => new($"Failed to initialize table of content. Reason was '{exception.Message}'. See inner exception for further details", exception);
    }
}