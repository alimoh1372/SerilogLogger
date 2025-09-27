# ===== Build .NET app =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .


# کپی پکیج‌های NuGet محلی
COPY /nuget-local/ /nuget-local/
# کپی فایل NuGet.Config
COPY NuGet.Config /src/NuGet.Config
# بازگرداندن پکیج‌ها

RUN ls -la /nuget-local/

RUN dotnet restore
RUN dotnet publish -c Release -o /out

# ===== Grab Vector binary from official image =====
FROM timberio/vector:0.49.0-debian AS vectorbin

# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# system deps
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates curl bash coreutils \
 && rm -rf /var/lib/apt/lists/*

# users & dirs
RUN useradd -r -s /bin/false appuser \
 && mkdir -p /etc/vector /var/lib/vector /var/log/app /var/log/vector
 
RUN chown -R appuser:appuser /var/log /etc/vector /var/lib/vector

# copy app
COPY --from=build /out /app
# copy vector
COPY --from=vectorbin /usr/bin/vector /usr/bin/vector

# configs & scripts
COPY docker/vector.yaml /etc/vector/vector.yaml
COPY LogConfiguration.json /app/LogConfiguration.json
COPY docker/entrypoint.sh /entrypoint.sh
COPY docker/vector-cleaner.sh /usr/local/bin/vector-cleaner.sh
RUN chmod +x /entrypoint.sh /usr/local/bin/vector-cleaner.sh \
 && chown -R appuser:appuser /var/lib/vector /var/log

ENV VECTOR_DATA_DIR=/var/lib/vector
ENV DOTNET_SystemGlobalization_Invariant=false
# Prometheus metrics
EXPOSE 9090   
# Vector API (اختیاری)
EXPOSE 8686    

USER appuser
WORKDIR /app
ENTRYPOINT ["/entrypoint.sh"]
