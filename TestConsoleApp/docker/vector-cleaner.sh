#!/bin/bash

# File Cleanup Manager بر اساس Vector Checkpoint
# فقط فایل‌هایی که در checkpoint قدیمی‌تر از فایل فعلی هستند جابجا می‌شوند

# Environment Variables
LOG_DIR="${LOG_DIR:-/var/log/app}"
PROCESSED_DIR="${PROCESSED_DIR:-/var/log/app/processed}"
VECTOR_DATA_DIR="${VECTOR_DATA_DIR:-/var/lib/vector}"
CHECKPOINT_FILE="${VECTOR_DATA_DIR}/checkpoints.json"
PROCESSED_RETENTION_DAYS="${PROCESSED_RETENTION_DAYS:-7}"

# ایجاد directory ها
mkdir -p "$PROCESSED_DIR"

# تابع برای بررسی وجود checkpoint file
check_checkpoint_exists() {
    if [ -f "$CHECKPOINT_FILE" ]; then
        return 0
    else
        echo "$(date): ERROR - Checkpoint file not found: $CHECKPOINT_FILE"
        echo "$(date): Vector might not be running or data directory is incorrect"
        return 1
    fi
}

# تابع برای یافتن فایل فعلی بر اساس checkpoint fingerprint
get_current_file_from_checkpoint() {
    if [ ! -f "$CHECKPOINT_FILE" ]; then
        echo "$(date): ERROR - Checkpoint file not found"
        return 1
    fi
    
    # تشخیص نوع fingerprint
    local fingerprint_type=$(jq -r '.checkpoints[0].fingerprint | keys[0]' "$CHECKPOINT_FILE" 2>/dev/null)
    
    case "$fingerprint_type" in
        "first_lines_checksum")
            # نوع checksum
            local fingerprint=$(jq -r '.checkpoints[0].fingerprint.first_lines_checksum' "$CHECKPOINT_FILE" 2>/dev/null)
            echo "$(date): Using checksum fingerprint: $fingerprint"
            
            for file in "$LOG_DIR"/log-*.json; do
                if [ -f "$file" ]; then
                    local file_checksum=$(head -n 10 "$file" 2>/dev/null | cksum | awk '{print $1}' 2>/dev/null)
                    
                    if [ "$file_checksum" == "$fingerprint" ]; then
                        echo "$(date): Found current file by checksum: $file"
                        echo "$file"
                        return 0
                    fi
                fi
            done
            ;;
            
        "dev_inode")
            # نوع device_and_inode
            local dev_inode=$(jq -r '.checkpoints[0].fingerprint.dev_inode | @json' "$CHECKPOINT_FILE" 2>/dev/null)
            local dev=$(echo "$dev_inode" | jq -r '.[0]' 2>/dev/null)
            local inode=$(echo "$dev_inode" | jq -r '.[1]' 2>/dev/null)
            
            echo "$(date): Using device_inode fingerprint: dev=$dev, inode=$inode"
            
            for file in "$LOG_DIR"/log-*.json; do
                if [ -f "$file" ]; then
                    local file_dev=$(stat -c%d "$file" 2>/dev/null)
                    local file_inode=$(stat -c%i "$file" 2>/dev/null)
                    
                    if [ "$file_dev" == "$dev" ] && [ "$file_inode" == "$inode" ]; then
                        echo "$(date): Found current file by device_inode: $file"
                        echo "$file"
                        return 0
                    fi
                fi
            done
            ;;
            
        *)
            echo "$(date): ERROR - Unknown fingerprint type: $fingerprint_type"
            return 1
            ;;
    esac
    
    echo "$(date): No matching file found for fingerprint type: $fingerprint_type"
    return 1
}

# تابع برای بررسی اینکه فایل قدیمی‌تر از فایل فعلی است
is_file_older_than_current() {
    local file_path="$1"
    local current_file="$2"
    
    # اگر فایل فعلی مشخص نیست
    if [ -z "$current_file" ] || [ "$current_file" == "null" ]; then
        echo "$(date): WARNING - No current file determined from checkpoint"
        return 1
    fi
    
    # اگر همان فایل فعلی است، هیچ کاری نکن (حتی اگر قدیمی باشد)
    if [ "$file_path" == "$current_file" ]; then
        echo "$(date): File is currently being processed by Vector - SKIP: $file_path"
        return 1
    fi
    
    # بررسی بر اساس نام فایل برای تشخیص ترتیب زمانی  
    local file_basename=$(basename "$file_path")
    local current_basename=$(basename "$current_file")
    
    # مقایسه string برای فایل‌های log-YYYY-MM-DD.json
    if [[ "$file_basename" < "$current_basename" ]]; then
        echo "$(date): File is older than current (by name): $file_basename < $current_basename"
        return 0
    fi
    
    # بررسی اضافی بر اساس تاریخ آخرین تغییر
    local file_mtime=$(stat -c%Y "$file_path" 2>/dev/null || echo "0")
    local current_mtime=$(stat -c%Y "$current_file" 2>/dev/null || echo "0")
    
    if [ "$file_mtime" -lt "$current_mtime" ]; then
        echo "$(date): File is older than current (by mtime): $file_path"
        return 0
    fi
    
    echo "$(date): File is not older than current - SKIP: $file_path"
    return 1
}

# حذف تابع verify_in_elasticsearch - دیگر نیاز نیست

# Main cleanup function
cleanup_processed_files() {
    echo "$(date): Starting checkpoint-based file cleanup..."
    
    # بررسی وجود checkpoint file
    if ! check_checkpoint_exists; then
        echo "$(date): Aborting cleanup - no checkpoint file"
        return 1
    fi
    
    # گرفتن فایل فعلی از checkpoint
    local current_file=$(get_current_file_from_checkpoint)
    if [ $? -ne 0 ] || [ -z "$current_file" ]; then
        echo "$(date): Could not determine current file from checkpoint, aborting"
        return 1
    fi
    
    echo "$(date): Current file from checkpoint: $current_file"
    
    local processed_count=0
    local skipped_count=0
    
    # پیدا کردن فایل‌های log و مرتب کردن آن‌ها
    find "$LOG_DIR" -name "log-*.json" -type f | sort | while read -r file; do
        if [ -f "$file" ]; then
            echo "$(date): Evaluating file: $file"
            
            # بررسی اینکه فایل قدیمی‌تر از فایل فعلی است
            if is_file_older_than_current "$file" "$current_file"; then
                
                # انتقال به processed directory - بدون نیاز به ES verification
                # چون acknowledgment=true در Vector تضمین می‌کند که داده‌ها ارسال شده‌اند
                local filename=$(basename "$file")
                local processed_file="$PROCESSED_DIR/$filename"
                
                if mv "$file" "$processed_file"; then
                    echo "$(date): SUCCESS - Moved to processed: $processed_file"
                    processed_count=$((processed_count + 1))
                    
                    # Compress برای صرفه‌جویی فضا
                    if gzip "$processed_file"; then
                        echo "$(date): Compressed: $processed_file.gz"
                    fi
                else
                    echo "$(date): ERROR - Failed to move file: $file"
                fi
            else
                echo "$(date): SKIP - File not eligible for processing: $file"
                skipped_count=$((skipped_count + 1))
            fi
        fi
    done
    
    echo "$(date): Cleanup completed. Processed: $processed_count, Skipped: $skipped_count"
}

# پاکسازی فایل‌های processed قدیمی
cleanup_old_processed() {
    echo "$(date): Cleaning up old processed files older than $PROCESSED_RETENTION_DAYS days..."
    
    local deleted_count=$(find "$PROCESSED_DIR" -name "*.gz" -mtime +$PROCESSED_RETENTION_DAYS -delete -print | wc -l)
    echo "$(date): Deleted $deleted_count old processed files"
}

# Health check ساده
health_check() {
    echo "$(date): === Health Check ==="
    local issues=0
    
    # Checkpoint file existence
    if check_checkpoint_exists; then
        echo "$(date): ✓ Vector checkpoint file exists"
        
        # نمایش اطلاعات checkpoint
        local current_file=$(get_current_file_from_checkpoint)
        if [ $? -eq 0 ] && [ -n "$current_file" ]; then
            echo "$(date): Current processing file: $current_file"
        else
            echo "$(date): ⚠ Could not determine current file from checkpoint"
        fi
    else
        echo "$(date): ✗ Vector checkpoint file missing"
        issues=$((issues + 1))
    fi
    
    # Directory permissions
    if [ -w "$LOG_DIR" ] && [ -w "$PROCESSED_DIR" ]; then
        echo "$(date): ✓ Directory permissions OK"
    else
        echo "$(date): ✗ Directory permission issues"
        issues=$((issues + 1))
    fi
    
    # Disk space
    local disk_usage=$(df "$LOG_DIR" | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$disk_usage" -lt 80 ]; then
        echo "$(date): ✓ Disk space OK ($disk_usage% used)"
    else
        echo "$(date): ⚠ Disk space warning ($disk_usage% used)"
        issues=$((issues + 1))
    fi
    
    # File counts
    local active_files=$(find "$LOG_DIR" -name "log-*.json" -type f | wc -l)
    local processed_files=$(find "$PROCESSED_DIR" -name "*.gz" -type f | wc -l)
    echo "$(date): Files - Active: $active_files, Processed: $processed_files"
    
    if [ $issues -eq 0 ]; then
        echo "$(date): ✓ All critical systems OK"
        return 0
    else
        echo "$(date): ✗ Found $issues critical issues"
        return 1
    fi
}

# Debug function برای checkpoint analysis
debug_checkpoint() {
    echo "$(date): === Checkpoint Debug Info ==="
    
    if [ -f "$CHECKPOINT_FILE" ]; then
        echo "$(date): Checkpoint file exists: $CHECKPOINT_FILE"
        echo "$(date): File size: $(stat -c%s "$CHECKPOINT_FILE") bytes"
        echo "$(date): Last modified: $(stat -c%y "$CHECKPOINT_FILE")"
        
        echo "$(date): Checkpoint contents:"
        jq '.' "$CHECKPOINT_FILE" 2>/dev/null || cat "$CHECKPOINT_FILE"
        echo ""
        
        # تحلیل محتویات checkpoint
        local checkpoint_count=$(jq '.checkpoints | length' "$CHECKPOINT_FILE" 2>/dev/null || echo "0")
        echo "$(date): Number of checkpoints: $checkpoint_count"
        
        for i in $(seq 0 $((checkpoint_count-1))); do
            echo "$(date): Analyzing checkpoint $i:"
            
            local fingerprint_type=$(jq -r ".checkpoints[$i].fingerprint | keys[0]" "$CHECKPOINT_FILE" 2>/dev/null)
            local fingerprint_value=$(jq -r ".checkpoints[$i].fingerprint.${fingerprint_type}" "$CHECKPOINT_FILE" 2>/dev/null)
            local position=$(jq -r ".checkpoints[$i].position" "$CHECKPOINT_FILE" 2>/dev/null)
            local modified=$(jq -r ".checkpoints[$i].modified" "$CHECKPOINT_FILE" 2>/dev/null)
            
            echo "$(date):   Fingerprint type: $fingerprint_type"
            echo "$(date):   Fingerprint value: $fingerprint_value"  
            echo "$(date):   Position: $position"
            echo "$(date):   Modified: $modified"
        done
        
        echo ""
        echo "$(date): Trying to match with existing files:"
        
        for file in "$LOG_DIR"/log-*.json; do
            if [ -f "$file" ]; then
                local file_size=$(stat -c%s "$file")
                local file_mtime=$(stat -c%Y "$file")
                local file_checksum=$(head -n 10 "$file" 2>/dev/null | cksum | awk '{print $1}' 2>/dev/null)
                
                echo "$(date):   File: $file"
                echo "$(date):     Size: $file_size bytes"
                echo "$(date):     Modified: $(date -d @$file_mtime)"
                echo "$(date):     First lines checksum: $file_checksum"
            fi
        done
        
        echo ""
        local current_file=$(get_current_file_from_checkpoint)
        echo "$(date): Determined current file: $current_file"
    else
        echo "$(date): Checkpoint file does not exist: $CHECKPOINT_FILE"
        
        echo "$(date): Checking Vector data directory:"
        if [ -d "$VECTOR_DATA_DIR" ]; then
            ls -la "$VECTOR_DATA_DIR"/
        else
            echo "$(date): Vector data directory does not exist: $VECTOR_DATA_DIR"
        fi
    fi
}

# Usage function
show_usage() {
    cat << EOF
Usage: $0 {cleanup|health|old-cleanup|debug|full}

Commands:
  cleanup     - Move files older than current checkpoint to processed
  health      - Check system health (checkpoint, permissions, disk)
  old-cleanup - Remove old processed files (older than $PROCESSED_RETENTION_DAYS days)
  debug       - Show checkpoint file contents and analysis
  full        - Run health check, cleanup, and old-cleanup

Environment Variables:
  LOG_DIR                 - Log directory (default: /var/log/app)
  PROCESSED_DIR          - Processed files directory (default: /var/log/app/processed)  
  VECTOR_DATA_DIR        - Vector data directory (default: /var/lib/vector)
  ES_ENDPOINT            - Elasticsearch endpoint (default: http://localhost:9200)
  PROCESSED_RETENTION_DAYS - Days to keep processed files (default: 7)

EOF
}

# Main execution
case "${1:-}" in
    "cleanup")
        cleanup_processed_files
        ;;
    "health")
        health_check
        ;;
    "old-cleanup")
        cleanup_old_processed
        ;;
    "debug")
        debug_checkpoint
        ;;
    "full")
        if health_check; then
            cleanup_processed_files
            cleanup_old_processed
        else
            echo "$(date): Health check failed, skipping cleanup operations"
            exit 1
        fi
        ;;
    *)
        show_usage
        exit 1
        ;;
esac

echo "$(date): File cleanup manager completed"
