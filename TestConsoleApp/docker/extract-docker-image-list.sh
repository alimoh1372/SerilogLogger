#!/bin/bash
# اسکریپت استخراج لیست کامل Docker Images موجود در سیستم

# رنگ‌ها برای خروجی بهتر
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

OUTPUT_FILE="docker_images_list.txt"
FORMATTED_OUTPUT="docker_images_formatted.txt"

echo -e "${BLUE}=== استخراج لیست Docker Images ===${NC}"

# بررسی وجود Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker نصب نیست!"
    exit 1
fi

if ! docker info &> /dev/null; then
    echo "❌ Docker در حال اجرا نیست!"
    exit 1
fi

echo "📋 در حال استخراج لیست images..."

# استخراج لیست ساده
docker images --format "{{.Repository}}:{{.Tag}}" | grep -v "<none>" > "$OUTPUT_FILE"

# استخراج لیست فرمت شده برای اسکریپت
echo '# لیست Docker Images موجود در سیستم' > "$FORMATTED_OUTPUT"
echo '# تاریخ تولید: '$(date) >> "$FORMATTED_OUTPUT"
echo 'declare -a IMAGES=(' >> "$FORMATTED_OUTPUT"

docker images --format "{{.Repository}}:{{.Tag}}" | grep -v "<none>" | while read line; do
    echo "    \"$line\"" >> "$FORMATTED_OUTPUT"
done

echo ')' >> "$FORMATTED_OUTPUT"

# نمایش آمار
total_images=$(docker images --format "{{.Repository}}:{{.Tag}}" | grep -v "<none>" | wc -l)

echo -e "${GREEN}✅ استخراج کامل شد!${NC}"
echo "📊 تعداد images: $total_images"
echo "📄 فایل‌های ایجاد شده:"
echo "   - لیست ساده: $OUTPUT_FILE"
echo "   - فرمت اسکریپت: $FORMATTED_OUTPUT"
echo ""
echo "🔍 5 اولین image:"
head -5 "$OUTPUT_FILE"