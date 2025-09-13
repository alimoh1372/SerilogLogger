// ===================================
// LogIgnoreAttribute.cs - Attribute برای مخفی کردن properties
// ===================================
using System;


namespace SerilogLogger.Utilities.Utilities;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class LogIgnoreAttribute : Attribute
{
}