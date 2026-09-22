#!/bin/bash
echo "==> Đang kiểm tra và khởi tạo Database ZHome..."

# Wait until SQL Server is ready
until docker exec -i zhome-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ZHomeStrongPassword2026!" -C -Q "SELECT 1" &> /dev/null || docker exec -i zhome-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "ZHomeStrongPassword2026!" -Q "SELECT 1" &> /dev/null
do
  echo "Đang đợi SQL Server khởi động..."
  sleep 3
done

echo "==> SQL Server đã sẵn sàng! Đang nạp dữ liệu từ ZHome_fixed.sql..."
docker exec -i zhome-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "ZHomeStrongPassword2026!" -C -i /docker-entrypoint-initdb.d/ZHome_fixed.sql 2>/dev/null || docker exec -i zhome-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "ZHomeStrongPassword2026!" -i /docker-entrypoint-initdb.d/ZHome_fixed.sql

echo "==> Hoàn tất khởi tạo Database ZHome!"
