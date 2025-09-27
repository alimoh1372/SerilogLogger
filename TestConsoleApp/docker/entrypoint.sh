#!/bin/bash

# Start Filebeat in foreground (blocking)
echo "$(date): Starting Filebeat in foreground..."
filebeat -e -c /etc/filebeat/filebeat.yml

# Start .NET app (Serilog logs تولید می‌کنه)
#exec dotnet /app/YourApp.dll