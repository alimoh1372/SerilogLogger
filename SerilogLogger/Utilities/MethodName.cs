using System.Runtime.CompilerServices;

namespace SerilogLogger.Utilities;

public static class MethodName
{
    public static string GetName([CallerMemberName] string callerName = null!)
        => callerName;
}

