#!/bin/bash
# ==============================================================================
# Script Tự Động Cài Đặt & Triển Khai ZHome Trên AWS EC2 (Ubuntu Linux)
# ==============================================================================
set -e

echo "🚀 BẮT ĐẦU CÀI ĐẶT & DEPLOY DỰ ÁN ZHOME TRÊN AWS EC2..."

# 1. Cập nhật hệ thống
echo "==> 1. Cập nhật hệ điều hành..."
sudo apt update && sudo apt upgrade -y

# 2. Cài đặt Docker & Docker Compose nếu chưa có
if ! command -v docker &> /dev/null
then
    echo "==> 2. Đang cài đặt Docker Engine & Docker Compose Plugin..."
    sudo apt install -y ca-certificates curl gnupg lsb-release
    sudo mkdir -p /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg --yes
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
    sudo apt update
    sudo apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin docker-compose
    sudo usermod -aG docker $USER
    echo "✓ Đã cài xong Docker!"
fi

# 3. Tạo Swapfile 2GB (phòng trường hợp máy EC2 t3.small / t2.micro bị thiếu RAM khi build Angular)
if [ ! -f /swapfile ]; then
    echo "==> 3. Đang tạo bộ nhớ ảo (Swapfile 2GB) tối ưu RAM..."
    sudo fallocate -l 2G /swapfile
    sudo chmod 600 /swapfile
    sudo mkswap /swapfile
    sudo swapon /swapfile
    echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
    echo "✓ Đã tạo Swapfile 2GB thành công!"
fi

# 4. Tạo file .env từ .env.example nếu chưa tồn tại
if [ ! -f .env ]; then
    echo "==> 4. Tạo file cấu hình môi trường .env..."
    cp .env.example .env
fi

# 5. Phân quyền thực thi script
chmod +x init-db.sh

# 6. Build và chạy toàn bộ container (SQL Server, Backend .NET, Frontend Nginx)
echo "==> 5. Đang tiến hành Build & Khởi động Docker Containers..."
sudo docker compose down || true
sudo docker compose up -d --build

# 7. Khởi tạo dữ liệu cơ sở dữ liệu
echo "==> 6. Đang nạp cơ sở dữ liệu ZHome..."
chmod +x ./init-db.sh
./init-db.sh

echo "=========================================================================="
echo "🎉 CHÚC MỪNG BẠN! HỆ THỐNG ZHOME ĐÃ ĐƯỢC DEPLOY THÀNH CÔNG TRÊN AWS EC2!"
echo "👉 Truy cập Web App tại: http://$(curl -s ifconfig.me)"
echo "👉 API Backend Swagger: http://$(curl -s ifconfig.me):5000/swagger (hoặc http://$(curl -s ifconfig.me)/swagger)"
echo "=========================================================================="
