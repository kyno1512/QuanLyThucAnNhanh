# DEBUG CHECKLIST - Tại sao không hiện sản phẩm?

## Các nguyên nhân có thể:

### 1. Database Connection Issue
- **Kiểm tra**: Mở `https://localhost:7104/api/Product/test-db`
- **Nếu lỗi**: Kiểm tra connection string trong `appsettings.json`
- **Kiểm tra**: SQL Server có đang chạy không?
- **Kiểm tra**: Database `QuanLyThucAnNhanh` có tồn tại không?

### 2. Database chưa có dữ liệu
- **Kiểm tra**: Bảng `Mon` và `NhomMon` có dữ liệu không?
- **Chạy SQL**: 
  ```sql
  SELECT COUNT(*) FROM Mon WHERE TrangThai = 1;
  SELECT COUNT(*) FROM NhomMon;
  ```

### 3. Lỗi trong code
- **Kiểm tra console log của backend** khi gọi API
- **Kiểm tra**: Có exception nào được log không?

### 4. CORS hoặc Routing Issue
- **Kiểm tra**: Endpoint `/api/Product/test` có hoạt động không?
- **Kiểm tra**: Swagger UI có hiển thị các endpoint không?

## Các bước debug:

1. **Test endpoint đơn giản**:
   - GET `https://localhost:7104/api/Product/test`
   - Nếu OK → API routing hoạt động
   - Nếu lỗi → Vấn đề routing hoặc server

2. **Test database**:
   - GET `https://localhost:7104/api/Product/test-db`
   - Xem response để biết database có kết nối được không

3. **Test endpoint MonController** (nếu có):
   - GET `https://localhost:7104/api/Mon`
   - GET `https://localhost:7104/api/Mon/nhommon`
   - Nếu hoạt động → Vấn đề ở ProductController/ProductService
   - Nếu không → Vấn đề ở database hoặc MonService

4. **Kiểm tra console log**:
   - Xem log trong terminal/console của backend
   - Tìm các dòng bắt đầu bằng "ERROR" hoặc "GetCategories called", "GetProducts called"

5. **Kiểm tra Network tab**:
   - Mở DevTools → Network tab
   - Click vào request bị lỗi
   - Xem Response tab để đọc chi tiết lỗi

## Giải pháp tạm thời:

Nếu database chưa có dữ liệu, có thể trả về mảng rỗng thay vì lỗi 500.

