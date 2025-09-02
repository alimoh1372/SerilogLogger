// ===================================
// Enrichers/ApplicationEnricher.cs - اضافه کردن metadata
// ===================================
using System;
using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace SerilogLogger.Utilities.Utilities;
public class ApplicationEnricher : ILogEventEnricher
{
	private readonly LogEventProperty _applicationId;
	private readonly LogEventProperty _applicationName;
	private readonly LogEventProperty _machineName;
	private readonly LogEventProperty _processId;

	public ApplicationEnricher(string applicationId, string applicationName)
	{
		_applicationId = new LogEventProperty("ApplicationId", new ScalarValue(applicationId));
		_applicationName = new LogEventProperty("ApplicationName", new ScalarValue(applicationName));
		_machineName = new LogEventProperty("MachineName", new ScalarValue(Environment.MachineName));
		_processId = new LogEventProperty("ProcessId", new ScalarValue(Process.GetCurrentProcess().Id));
		
	}

	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		logEvent.AddPropertyIfAbsent(_applicationId);
		logEvent.AddPropertyIfAbsent(_applicationName);
		logEvent.AddPropertyIfAbsent(_machineName);
		logEvent.AddPropertyIfAbsent(_processId);
	}
}