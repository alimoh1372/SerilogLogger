FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app/

COPY ./ .
WORKDIR /app/TestSerilog/
RUN dotnet restore *.csproj





RUN dotnet publish -c Release -o /app/out




FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
ENV ASPNETCORE_URLS="http://+:5000;"
EXPOSE 5000
COPY --from=build-env /app/out/ .


ENTRYPOINT [ "dotnet" , "TestSerilog.dll"]