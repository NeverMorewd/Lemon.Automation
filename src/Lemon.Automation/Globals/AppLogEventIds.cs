using Microsoft.Extensions.Logging;

namespace Lemon.Automation.Globals
{
    public static class AppLogEventIds
    {
        public static EventId GrpcService => new(1000, nameof(GrpcService));
        public static EventId GrpcClient => new(1001, nameof(GrpcClient));
        public static EventId Fatal => 9999;
        public static EventId CanIgnore => 0000;

        public static EventId Global => 90001;
    }
}
