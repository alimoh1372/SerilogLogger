#!/bin/bash
# اسکریپت دانلود Docker Images با مدیریت خطا پیشرفته

# رنگ‌ها
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# فایل‌های لاگ
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_FILE="${SCRIPT_DIR}/docker_pull_log.txt"
ERROR_FILE="${SCRIPT_DIR}/docker_pull_errors.txt"
SUCCESS_FILE="${SCRIPT_DIR}/docker_pull_success.txt"

# پاک کردن فایل‌های لاگ قبلی
> "$LOG_FILE"
> "$ERROR_FILE" 
> "$SUCCESS_FILE"

echo -e "${BLUE}=== شروع دانلود Docker Images ===${NC}"

# آرایه images کامل شده (شامل موارد جدید برای محیط آفلاین)
declare -a IMAGES=(
    # Zabbix Stack
    "zabbix/zabbix-server-mysql:7.4-ubuntu-latest"
    "zabbix/zabbix-web-nginx-mysql:7.4-ubuntu-latest" 
    "zabbix/zabbix-agent:7.4-ubuntu-latest"
    "zabbix/zabbix-proxy-mysql:7.4-ubuntu-latest"
    "zabbix/zabbix-server-mysql:latest"
    "zabbix/zabbix-web-nginx-mysql:latest"
    "zabbix/zabbix-agent:latest"
    
    # Database
    "mysql:8.0"
    "mysql:5.7"
    "postgres:15"
    "postgres:13"
    "mongo:latest"
    "mongo:6.0"
    
    # Monitoring & Observability
    "grafana/grafana:latest"
    "grafana/grafana:10.4.0"
    "prom/prometheus:latest"
    "prom/prometheus:v2.45.0"
    "prom/node-exporter:latest"
    "prom/node-exporter:v1.6.0"
    "prom/alertmanager:latest"
    "prom/alertmanager:v0.25.0"
    
    # ELK Stack
    "elasticsearch:8.11.0"
    "elasticsearch:7.17.0"
    "kibana:8.11.0"
    "kibana:7.17.0"
    "logstash:8.11.0"
    "logstash:7.17.0"
    "sebp/elk:8.11.0"
    
    # Message Brokers
    "redis:latest"
    "redis:7-alpine"
    "redis/redis-stack:latest"
    "rabbitmq:3.12-management"
    "rabbitmq:management-alpine"
    "apache/kafka:latest"
    
    # .NET Runtime Images
    "mcr.microsoft.com/dotnet/runtime:8.0"
    "mcr.microsoft.com/dotnet/runtime:9.0"
    "mcr.microsoft.com/dotnet/runtime:6.0"
    "mcr.microsoft.com/dotnet/aspnet:8.0"
    "mcr.microsoft.com/dotnet/aspnet:9.0"
    "mcr.microsoft.com/dotnet/aspnet:6.0"
    "mcr.microsoft.com/dotnet/sdk:8.0"
    "mcr.microsoft.com/dotnet/sdk:9.0"
    "mcr.microsoft.com/dotnet/sdk:6.0"
    
    # Web Servers & Reverse Proxies
    "nginx:alpine"
    "nginx:latest"
    "httpd:alpine"
    "traefik:latest"
    "haproxy:alpine"
    
    # Utility & DevOps Tools
    "portainer/portainer-ce:latest"
    "jenkins/jenkins:lts"
    "gitlab/gitlab-ce:latest"
    "sonarqube:community"
    "nexus3:latest"
    "registry:2"
    
    # Base Images
    "ubuntu:22.04"
    "ubuntu:20.04"
    "alpine:latest"
    "debian:bullseye-slim"
    "centos:7"
    
    # Networking & Security
    "owasp/zap2docker-stable"
    "wireshark/wireshark:latest"
    
    # Backup & Storage
    "minio/minio:latest"
    "restic/restic:latest"
	"mongo:latest
	"httpd:latest"
	"mysql:latest"
	"mcr.microsoft.com/dotnet/sdk:8.0"
	"mcr.microsoft.com/dotnet/aspnet:8.0"
	"mcr.microsoft.com/dotnet/runtime:8.0"
	"bitnami/kafka:latest"
	"docker-elk-setup:latest"
	"docker-elk-elasticsearch:latest"
	"docker-elk-kibana:latest"
	"node:22"
	"nginx:latest"
	"docker-elk-logstash:latest"
	"haproxy:lts-bookworm"
	"redis/redis-stack:latest"
	"redis:latest"
	"mcr.microsoft.com/mssql/server:latest"
	"postgres:latest"
	"rabbitmq:4.1.2-management"
	"cassandra:latest"
	"mcr.microsoft.com/dotnet/monitor:8.0"
	"mcr.microsoft.com/dotnet/sdk:6.0"
	"mcr.microsoft.com/dotnet/aspnet:6.0"
	"mcr.microsoft.com/dotnet/runtime:6.0"
	"node:22.12.0-alpine"
	"consul:1.15.4"
	"mcr.microsoft.com/dotnet/monitor:6.0"
)

# متغیرهای شمارش
total_images=${#IMAGES[@]}
success_count=0
error_count=0
current_image=0

# تابع دانلود با retry mechanism
pull_image_with_retry() {
    local image=$1
    local max_retries=3
    local retry_count=0
    
    while [ $retry_count -lt $max_retries ]; do
        if timeout 300 docker pull "$image" 2>&1; then
            return 0
        fi
        
        retry_count=$((retry_count + 1))
        if [ $retry_count -lt $max_retries ]; then
            echo -e "${YELLOW}⚠️  تلاش مجدد ($retry_count/$max_retries) برای: $image${NC}"
            sleep 5
        fi
    done
    
    return 1
}

# تابع اصلی دانلود
download_image() {
    local image=$1
    current_image=$((current_image + 1))
    
    echo -e "${BLUE}[${current_image}/${total_images}] دانلود: $image${NC}"
    echo "[$(date)] شروع دانلود: $image" >> "$LOG_FILE"
    
    # بررسی وجود image از قبل
    if docker images --format "{{.Repository}}:{{.Tag}}" | grep -q "^${image}$"; then
        echo -e "${GREEN}✅ از قبل موجود: $image${NC}"
        echo "$image - از قبل موجود" >> "$SUCCESS_FILE"
        success_count=$((success_count + 1))
        return 0
    fi
    
    # دانلود image
    if pull_image_with_retry "$image" >> "$LOG_FILE" 2>&1; then
        echo -e "${GREEN}✅ موفقیت‌آمیز: $image${NC}"
        
        # اطلاعات اضافی image
        image_size=$(docker images "$image" --format "{{.Size}}" | head -1)
        echo -e "${BLUE}   📦 حجم: $image_size${NC}"
        
        echo "$image - $image_size" >> "$SUCCESS_FILE"
        echo "[$(date)] موفق: $image ($image_size)" >> "$LOG_FILE"
        success_count=$((success_count + 1))
    else
        echo -e "${RED}❌ خطا: $image${NC}"
        echo "$image - خطا در دانلود" >> "$ERROR_FILE"
        echo "[$(date)] خطا: $image" >> "$LOG_FILE"
        error_count=$((error_count + 1))
    fi
    
    echo "----------------------------------------"
}

# بررسی وجود Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker نصب نیست!${NC}"
    exit 1
fi

if ! docker info &> /dev/null; then
    echo -e "${RED}❌ Docker در حال اجرا نیست!${NC}"
    exit 1
fi

# شروع دانلود
echo "تعداد images: $total_images"
echo "========================================"

start_time=$(date +%s)

for image in "${IMAGES[@]}"; do
    download_image "$image"
    
    # نمایش پیشرفت
    progress=$((current_image * 100 / total_images))
    echo -e "${CYAN}📊 پیشرفت: $progress% ($current_image/$total_images)${NC}"
    echo ""
done

end_time=$(date +%s)
duration=$((end_time - start_time))
hours=$((duration / 3600))
minutes=$(((duration % 3600) / 60))
seconds=$((duration % 60))

# گزارش نهایی
echo "========================================"
echo -e "${BLUE}📊 گزارش نهایی:${NC}"
echo -e "${GREEN}✅ موفق: $success_count${NC}"
echo -e "${RED}❌ خطا: $error_count${NC}"
echo -e "${BLUE}⏱️  زمان: ${hours}h ${minutes}m ${seconds}s${NC}"

# محاسبه حجم کل images
total_size=$(docker system df --format "{{.Size}}" | head -1)
echo -e "${BLUE}💾 حجم کل Docker: $total_size${NC}"

echo ""
echo -e "${BLUE}📄 فایل‌های لاگ:${NC}"
echo "  - لاگ کامل: $LOG_FILE"
echo "  - موفق: $SUCCESS_FILE ($success_count عدد)"
echo "  - خطا: $ERROR_FILE ($error_count عدد)"

if [[ $error_count -eq 0 ]]; then
    echo -e "${GREEN}🎉 همه images موفقیت‌آمیز دانلود شدند!${NC}"
else
    echo -e "${YELLOW}⚠️  $error_count image دانلود نشد. فایل خطاها را بررسی کنید.${NC}"
    echo ""
    echo -e "${RED}❌ Images با خطا:${NC}"
    if [[ -f "$ERROR_FILE" ]]; then
        cat "$ERROR_FILE" | head -10
    fi
fi

echo ""
echo -e "${GREEN}🐳 برای مشاهده images: docker images${NC}"