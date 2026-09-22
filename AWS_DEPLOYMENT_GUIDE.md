# HƯỚNG DẪN TRIỂN KHAI DỰ ÁN ZHOME LÊN AMAZON WEB SERVICES (AWS EC2)

Tài liệu này hướng dẫn chi tiết từng bước để đưa toàn bộ hệ thống **ZHome** (Frontend Angular + Backend .NET 9 Web API + Cơ sở dữ liệu SQL Server) lên máy ảo **AWS EC2 (Ubuntu Linux)** với chi phí tối ưu nhất và chỉ bằng 1 câu lệnh tự động.

---

## BƯỚC 1: TẠO MÁY ẢO EC2 TRÊN AWS CONSOLE

1. Đăng nhập vào [AWS Management Console](https://console.aws.amazon.com/).
2. Tìm kiếm dịch vụ **EC2** $\rightarrow$ Chọn **Launch Instance** (Khởi chạy máy ảo).
3. **Cấu hình máy ảo**:
   - **Name**: `ZHome-Production-Server`
   - **Application and OS Images (AMI)**: Chọn **Ubuntu** (Ubuntu Server 24.04 LTS hoặc 22.04 LTS).
   - **Instance Type**:
     - Khuyên dùng: `t3.medium` (2 vCPU, 4GB RAM) hoặc `t3.small` (2 vCPU, 2GB RAM - *đã có script tự động tạo Swap 2GB*).
   - **Key pair (login)**: Chọn tạo Key Pair mới (dạng `.pem`), tải về máy tính và cất giữ cẩn thận (ví dụ: `zhome-key.pem`).
4. **Network Settings (Security Group / Mở cổng Firewall)**:
   - Nhấn **Edit** và tích chọn mở các cổng:
     - **SSH (Port 22)**: Để remote từ máy tính của bạn (`My IP` hoặc `0.0.0.0/0`).
     - **HTTP (Port 80)**: Cho phép người dùng truy cập web (`0.0.0.0/0`).
     - **HTTPS (Port 443)**: Cho phép truy cập bảo mật SSL (`0.0.0.0/0`).
     - **Custom TCP (Port 5000)**: Cho phép truy cập trực tiếp Backend API / Swagger (`0.0.0.0/0`).
5. **Storage**: Đặt dung lượng ổ cứng tối thiểu **30 GiB gp3** (gói Free Tier miễn phí 30GB).
6. Nhấn **Launch Instance**.

---

## BƯỚC 2: KẾT NỐI VÀO MÁY ẢO EC2

Mở terminal trên máy tính (PowerShell, Command Prompt hoặc Git Bash) và chạy lệnh:

```bash
# Cấp quyền cho key pem (nếu dùng Linux/Mac)
chmod 400 zhome-key.pem

# Kết nối SSH (thay YOUR_EC2_PUBLIC_IP bằng địa chỉ IP Public của máy EC2)
ssh -i "zhome-key.pem" ubuntu@YOUR_EC2_PUBLIC_IP
```

---

## BƯỚC 3: SAO CHÉP MÃ NGUỒN LÊN EC2

Bạn có 2 cách đưa mã nguồn lên EC2:

### Cách 1: Sử dụng Git (Khuyên dùng)
```bash
# Clone repository của bạn về EC2
git clone https://github.com/SONDHHE180524/ZHome.git
cd ZHome
```

### Cách 2: Upload trực tiếp từ máy tính lên EC2 qua SCP
Từ máy tính cục bộ của bạn, mở terminal tại thư mục chứa source code và chạy:
```bash
scp -i "zhome-key.pem" -r ./* ubuntu@YOUR_EC2_PUBLIC_IP:~/zhome/
```
Sau đó trên EC2:
```bash
cd ~/zhome
```

---

## BƯỚC 4: CHẠY SCRIPT TỰ ĐỘNG TRIỂN KHAI (CHỈ 1 BƯỚC)

Trên màn hình terminal EC2, bạn chỉ cần gõ:

```bash
chmod +x deploy-aws.sh
./deploy-aws.sh
```

Script sẽ tự động:
1. Cài đặt Docker & Docker Compose.
2. Cấu hình Swapfile 2GB chống tràn RAM.
3. Build Docker container cho .NET 9 API và Angular SPA.
4. Tải và khởi động Microsoft SQL Server 2022.
5. Tự động nạp dữ liệu bảng và mẫu từ file `ZHome_fixed.sql`.

---

## BƯỚC 5: TRUY CẬP VÀ KIỂM TRA HỆ THỐNG

Sau khi script hoàn tất, bạn có thể mở trình duyệt:
* 🌐 **Website ZHome**: `http://YOUR_EC2_PUBLIC_IP`
* 🔌 **API Swagger UI**: `http://YOUR_EC2_PUBLIC_IP/swagger` (hoặc `http://YOUR_EC2_PUBLIC_IP:5000/swagger`)

---

## BƯỚC 6 (TÙY CHỌN): GẮN TÊN MIỀN & CÀI ĐẶT SSL (HTTPS MIỄN PHÍ)

Nếu bạn có tên miền riêng (ví dụ `zhome.vn` hoặc `trozhome.com`):
1. Trỏ bản ghi **A Record** của tên miền về `YOUR_EC2_PUBLIC_IP`.
2. Chạy lệnh cài Certbot để cấp chứng chỉ HTTPS miễn phí từ Let's Encrypt:
```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d yourdomain.com
```

---

## 🛠️ CÁC LỆNH QUẢN LÝ THƯỜNG DÙNG TRÊN EC2

* **Xem trạng thái các container**:
  ```bash
  docker compose ps
  ```
* **Xem logs hoạt động**:
  ```bash
  docker compose logs -f
  # Hoặc xem riêng API:
  docker compose logs -f zhome-api
  ```
* **Khởi động lại / Dừng hệ thống**:
  ```bash
  docker compose restart
  docker compose down
  ```
* **Cập nhật code mới từ Git**:
  ```bash
  git pull
  docker compose up -d --build
  ```
