#!/bin/bash
# اسکریپت نصب ابزارهای کاربردی برای محیط آفلاین

# رنگ‌ها
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# بررسی دسترسی sudo
if [ "$EUID" -ne 0 ]; then
    echo -e "${RED}لطفاً با sudo اجرا کنید${NC}"
    exit 1
fi

echo -e "${BLUE}=== نصب ابزارهای کاربردی برای محیط آفلاین ===${NC}"

# بروزرسانی سیستم
update_system() {
    echo -e "${YELLOW}🔄 بروزرسانی سیستم...${NC}"
    apt update && apt upgrade -y
    apt install -y software-properties-common apt-transport-https ca-certificates gnupg lsb-release
}

# نصب ابزارهای پایه
install_basic_tools() {
    echo -e "${YELLOW}🛠️  نصب ابزارهای پایه...${NC}"
    apt install -y \
        curl \
        wget \
        git \
        vim \
        nano \
        htop \
        tree \
        zip \
        unzip \
        jq \
        net-tools \
        tcpdump \
        nmap \
        telnet \
        ssh \
        rsync \
        screen \
        tmux \
        fail2ban \
        ufw \
        iptables-persistent
    
    echo -e "${GREEN}✅ ابزارهای پایه نصب شدند${NC}"
}

# نصب Docker
install_docker() {
    echo -e "${YELLOW}🐳 نصب Docker...${NC}"
    
    # حذف نسخه‌های قدیمی
    apt remove -y docker docker-engine docker.io containerd runc 2>/dev/null
    
    # اضافه کردن repository Docker
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
    echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
    
    apt update
    apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
    
    # راه‌اندازی Docker
    systemctl enable docker
    systemctl start docker
    
    # اضافه کردن user به گروه docker
    if [ ! -z "$SUDO_USER" ]; then
        usermod -aG docker $SUDO_USER
        echo -e "${GREEN}✅ کاربر $SUDO_USER به گروه docker اضافه شد${NC}"
    fi
    
    echo -e "${GREEN}✅ Docker نصب شد${NC}"
}

# نصب Docker Compose standalone
install_docker_compose() {
    echo -e "${YELLOW}🔧 نصب Docker Compose...${NC}"
    
    COMPOSE_VERSION=$(curl -s https://api.github.com/repos/docker/compose/releases/latest | grep tag_name | cut -d '"' -f 4)
    curl -L "https://github.com/docker/compose/releases/download/${COMPOSE_VERSION}/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
    ln -sf /usr/local/bin/docker-compose /usr/bin/docker-compose
    
    echo -e "${GREEN}✅ Docker Compose ${COMPOSE_VERSION} نصب شد${NC}"
}

# نصب Portainer
install_portainer() {
    echo -e "${YELLOW}🖥️  نصب Portainer...${NC}"
    
    docker volume create portainer_data
    docker run -d \
        --name portainer \
        --restart always \
        -p 9000:9000 \
        -v /var/run/docker.sock:/var/run/docker.sock \
        -v portainer_data:/data \
        portainer/portainer-ce:latest
    
    echo -e "${GREEN}✅ Portainer نصب شد - دسترسی: http://localhost:9000${NC}"
}

# نصب Nginx
install_nginx() {
    echo -e "${YELLOW}🌐 نصب Nginx...${NC}"
    
    apt install -y nginx
    systemctl enable nginx
    systemctl start nginx
    
    # تنظیم فایروال
    ufw allow 'Nginx Full' 2>/dev/null
    
    echo -e "${GREEN}✅ Nginx نصب شد${NC}"
}

# نصب Node.js و npm
install_nodejs() {
    echo -e "${YELLOW}📦 نصب Node.js...${NC}"
    
    curl -fsSL https://deb.nodesource.com/setup_lts.x | bash -
    apt install -y nodejs
    
    # نصب ابزارهای مفید npm
    npm install -g pm2 yarn
    
    echo -e "${GREEN}✅ Node.js $(node -v) و npm $(npm -v) نصب شدند${NC}"
}

# نصب Python tools
install_python_tools() {
    echo -e "${YELLOW}🐍 نصب Python tools...${NC}"
    
    apt install -y python3 python3-pip python3-venv python3-dev
    pip3 install --upgrade pip
    pip3 install virtualenv docker-compose ansible
    
    echo -e "${GREEN}✅ Python tools نصب شدند${NC}"
}

# نصب Monitoring tools
install_monitoring_tools() {
    echo -e "${YELLOW}📊 نصب ابزارهای مانیتورینگ...${NC}"
    
    apt install -y \
        iotop \
        iftop \
        nethogs \
        dstat \
        sysstat \
        lsof \
        strace \
        tcpdump \
        wireshark-common \
        bandwidthd
    
    echo -e "${GREEN}✅ ابزارهای مانیتورینگ نصب شدند${NC}"
}

# نصب Database clients
install_db_clients() {
    echo -e "${YELLOW}🗄️  نصب کلاینت‌های دیتابیس...${NC}"
    
    apt install -y \
        mysql-client \
        postgresql-client \
        redis-tools \
        mongodb-clients
    
    echo -e "${GREEN}✅ کلاینت‌های دیتابیس نصب شدند${NC}"
}

# تنظیمات امنیتی پایه
configure_security() {
    echo -e "${YELLOW}🔒 تنظیمات امنیتی...${NC}"
    
    # فعال‌سازی فایروال
    ufw --force enable
    ufw default deny incoming
    ufw default allow outgoing
    ufw allow ssh
    ufw allow 80/tcp
    ufw allow 443/tcp
    
    # تنظیمات fail2ban
    systemctl enable fail2ban
    systemctl start fail2ban
    
    # تنظیمات SSH (اختیاری)
    sed -i 's/#PermitRootLogin yes/PermitRootLogin no/' /etc/ssh/sshd_config 2>/dev/null
    
    echo -e "${GREEN}✅ تنظیمات امنیتی اعمال شدند${NC}"
}

# ایجاد اسکریپت‌های مفید
create_useful_scripts() {
    echo -e "${YELLOW}📜 ایجاد اسکریپت‌های مفید...${NC}"
    
    # اسکریپت نظافت سیستم
    cat > /usr/local/bin/system-cleanup << 'EOF'
#!/bin/bash
echo "🧹 شروع نظافت سیستم..."
apt autoremove -y
apt autoclean
docker system prune -f
docker image prune -f
journalctl --vacuum-time=7d
echo "✅ نظافت کامل شد!"
EOF
    chmod +x /usr/local/bin/system-cleanup
    
    # اسکریپت بیک‌آپ
    cat > /usr/local/bin/backup-docker << 'EOF'
#!/bin/bash
BACKUP_DIR="/backup/docker"
mkdir -p "$BACKUP_DIR"
echo "📦 شروع بیک‌آپ Docker..."
docker save $(docker images --format "{{.Repository}}:{{.Tag}}") > "$BACKUP_DIR/docker-images-$(date +%Y%m%d).tar"
echo "✅ بیک‌آپ کامل شد: $BACKUP_DIR"
EOF
    chmod +x /usr/local/bin/backup-docker
    
    echo -e "${GREEN}✅ اسکریپت‌های مفید ایجاد شدند${NC}"
}

# نصب ابزارهای شبکه پیشرفته
install_network_tools() {
    echo -e "${YELLOW}🌐 نصب ابزارهای شبکه...${NC}"
    
    apt install -y \
        mtr-tiny \
        traceroute \
        dig \
        whois \
        netcat \
        socat \
        ngrep \
        tshark
    
    echo -e "${GREEN}✅ ابزارهای شبکه نصب شدند${NC}"
}

# منوی انتخاب
show_menu() {
    echo ""
    echo -e "${BLUE}=== انتخاب کنید:${NC}"
    echo "1) نصب کامل (توصیه شده)"
    echo "2) بروزرسانی سیستم"
    echo "3) ابزارهای پایه"
    echo "4) Docker و Docker Compose"
    echo "5) Portainer"
    echo "6) Nginx"
    echo "7) Node.js"
    echo "8) Python tools"
    echo "9) ابزارهای مانیتورینگ"
    echo "10) کلاینت‌های دیتابیس"
    echo "11) تنظیمات امنیتی"
    echo "12) ابزارهای شبکه"
    echo "0) خروج"
    echo ""
}

# نصب کامل
install_all() {
    echo -e "${BLUE}🚀 شروع نصب کامل...${NC}"
    
    update_system
    install_basic_tools
    install_docker
    install_docker_compose
    install_nginx
    install_nodejs
    install_python_tools
    install_monitoring_tools
    install_db_clients
    install_network_tools
    configure_security
    create_useful_scripts
    install_portainer
    
    echo ""
    echo -e "${GREEN}🎉 نصب کامل به پایان رسید!${NC}"
    echo -e "${YELLOW}📋 خلاصه نصب شده:${NC}"
    echo "  - ابزارهای پایه سیستم"
    echo "  - Docker و Docker Compose"
    echo "  - Portainer (http://localhost:9000)"
    echo "  - Nginx"
    echo "  - Node.js و npm"
    echo "  - Python و pip"
    echo "  - ابزارهای مانیتورینگ"
    echo "  - کلاینت‌های دیتابیس"
    echo "  - ابزارهای شبکه"
    echo "  - تنظیمات امنیتی"
    echo ""
    echo -e "${BLUE}🔧 اسکریپت‌های مفید:${NC}"
    echo "  - system-cleanup (نظافت سیستم)"
    echo "  - backup-docker (بیک‌آپ Docker)"
    echo ""
    echo -e "${YELLOW}⚠️  لطفاً سیستم را ریبوت کنید.${NC}"
}

# حلقه اصلی
while true; do
    show_menu
    read -p "انتخاب شما: " choice
    
    case $choice in
        1) install_all; break ;;
        2) update_system ;;
        3) install_basic_tools ;;
        4) install_docker; install_docker_compose ;;
        5) install_portainer ;;
        6) install_nginx ;;
        7) install_nodejs ;;
        8) install_python_tools ;;
        9) install_monitoring_tools ;;
        10) install_db_clients ;;
        11) configure_security ;;
        12) install_network_tools ;;
        0) echo "خروج..."; exit 0 ;;
        *) echo -e "${RED}انتخاب نامعتبر!${NC}" ;;
    esac
    
    echo ""
    read -p "ادامه؟ (y/n): " continue_choice
    [[ $continue_choice != "y" && $continue_choice != "Y" ]] && break
done