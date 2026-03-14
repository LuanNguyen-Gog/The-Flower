# The Flower API Overview

## 📋 Các API Endpoints được tạo

Base URL: 
- **Development**: `http://localhost:5000`
- **Production**: `https://api.theflower.com`

Authentication: JWT Token (Header: `Authorization: Bearer {token}`)

---

## 🔐 AUTH - Xác thực tài khoản

### 1. **POST** `/api/auth/register`
- **Công dụng**: Đăng ký tài khoản mới
- **Request**: 
  ```json
  {
    "name": "John Doe",
    "email": "john@example.com",
    "password": "password123",
    "phoneNumber": "0901234567"
  }
  ```
- **Response**: `AuthResponse` (userId, email, accessToken, refreshToken)

### 2. **POST** `/api/auth/login`
- **Công dụng**: Đăng nhập lấy JWT token
- **Request**:
  ```json
  {
    "email": "john@example.com",
    "password": "password123"
  }
  ```
- **Response**: `AuthResponse` (accessToken, refreshToken, expiresIn)

### 3. **POST** `/api/auth/refresh-token`
- **Công dụng**: Làm mới access token khi hết hạn
- **Request**:
  ```json
  {
    "refreshToken": "..."
  }
  ```
- **Response**: `AuthResponse` (accessToken mới)

### 4. **POST** `/api/auth/logout` ⚡ Yêu cầu auth
- **Công dụng**: Đăng xuất, vô hiệu hóa token
- **Response**: Success message

---

## 🛍️ PRODUCTS - Hàng hóa

### 5. **GET** `/api/products`
- **Công dụng**: Lấy danh sách sản phẩm (có phân trang, filter)
- **Query Parameters**:
  - `pageNumber` (default: 1)
  - `pageSize` (default: 20)
  - `categoryId` (optional) - Lọc theo danh mục
  - `search` (optional) - Tìm kiếm theo tên
- **Response**: `PaginatedResponse<ProductDto>` (items, totalCount, totalPages)

### 6. **GET** `/api/products/{id}`
- **Công dụng**: Lấy chi tiết 1 sản phẩm
- **Path Parameter**: `id` - ID sản phẩm
- **Response**: `ProductDto` (tên, giá, mô tả, hình ảnh, rating, kho)

---

## 📂 CATEGORIES - Danh mục

### 7. **GET** `/api/categories`
- **Công dụng**: Lấy danh sách tất cả danh mục
- **Response**: `List<CategoryDto>` (categoryId, categoryName, description, imageUrl)

---

## 🛒 CART - Giỏ hàng (⚡ Yêu cầu auth)

### 8. **GET** `/api/cart`
- **Công dụng**: Lấy giỏ hàng của user hiện tại
- **Response**: `CartDto` (cartItems, totalPrice, totalItems)

### 9. **POST** `/api/cart/items`
- **Công dụng**: Thêm sản phẩm vào giỏ hàng
- **Request**:
  ```json
  {
    "productId": 1,
    "quantity": 2
  }
  ```
- **Response**: `CartDto` (giỏ hàng cập nhật)

### 10. **PUT** `/api/cart/items/{itemId}`
- **Công dụng**: Cập nhật số lượng sản phẩm trong giỏ
- **Path Parameter**: `itemId` - ID item trong giỏ
- **Request**:
  ```json
  {
    "quantity": 5
  }
  ```
- **Response**: `CartDto` (giỏ hàng cập nhật)

### 11. **DELETE** `/api/cart/items/{itemId}`
- **Công dụng**: Xóa 1 sản phẩm khỏi giỏ hàng
- **Response**: `CartDto` (giỏ hàng cập nhật)

### 12. **DELETE** `/api/cart/clear`
- **Công dụng**: Xóa tất cả sản phẩm trong giỏ hàng
- **Response**: Success message

---

## 📦 ORDERS - Đơn hàng (⚡ Yêu cầu auth)

### 13. **POST** `/api/orders`
- **Công dụng**: Tạo đơn hàng mới từ giỏ hàng
- **Request**:
  ```json
  {
    "shippingAddress": "123 Main St, City, Country",
    "paymentMethod": "COD",
    "notes": "Please deliver at 2 PM"
  }
  ```
- **Response**: `OrderDto` (orderId, orderCode, status, totalAmount, orderItems)

### 14. **GET** `/api/orders`
- **Công dụng**: Lấy danh sách đơn hàng của user (có phân trang)
- **Query Parameters**:
  - `pageNumber` (default: 1)
  - `pageSize` (default: 20)
- **Response**: `PaginatedResponse<OrderDto>` (danh sách đơn hàng)

### 15. **GET** `/api/orders/{id}`
- **Công dụng**: Lấy chi tiết 1 đơn hàng
- **Path Parameter**: `id` - ID đơn hàng
- **Response**: `OrderDto` (orderCode, status, items, totalAmount, createdAt)

---

## 🔔 NOTIFICATIONS - Thông báo (⚡ Yêu cầu auth)

### 16. **GET** `/api/notifications`
- **Công dụng**: Lấy danh sách thông báo (có phân trang)
- **Query Parameters**:
  - `pageNumber` (default: 1)
  - `pageSize` (default: 20)
- **Response**: `PaginatedResponse<NotificationDto>` (message, isRead, createdAt)

### 17. **GET** `/api/notifications/badge`
- **Công dụng**: Lấy số lượng thông báo chưa đọc + số lượng item trong giỏ
- **Response**:
  ```json
  {
    "unreadNotifications": 3,
    "cartItemCount": 5
  }
  ```

### 18. **PUT** `/api/notifications/{notificationId}/read`
- **Công dụng**: Đánh dấu 1 thông báo đã đọc
- **Path Parameter**: `notificationId` - ID thông báo
- **Response**: Success message

### 19. **PUT** `/api/notifications/read-all`
- **Công dụng**: Đánh dấu tất cả thông báo đã đọc
- **Response**: Success message

### 20. **DELETE** `/api/notifications/{notificationId}`
- **Công dụng**: Xóa 1 thông báo
- **Path Parameter**: `notificationId` - ID thông báo
- **Response**: Success message

---

## 👤 USER PROFILE - Hồ sơ người dùng (⚡ Yêu cầu auth)

### 21. **GET** `/api/users/profile`
- **Công dụng**: Lấy thông tin hồ sơ của user hiện tại
- **Response**: `UserDto` (email, name, phoneNumber, role, profileImageUrl, createdAt)

### 22. **PUT** `/api/users/profile`
- **Công dụng**: Cập nhật thông tin hồ sơ
- **Request**:
  ```json
  {
    "name": "New Name",
    "phoneNumber": "0987654321",
    "profileImageUrl": "https://..."
  }
  ```
- **Response**: `UserDto` (thông tin đã cập nhật)

### 23. **PUT** `/api/users/change-password`
- **Công dụng**: Thay đổi mật khẩu
- **Request**:
  ```json
  {
    "oldPassword": "oldpass123",
    "newPassword": "newpass123",
    "confirmPassword": "newpass123"
  }
  ```
- **Response**: Success message

---

## 📍 STORE LOCATIONS - Địa điểm cửa hàng

### 24. **GET** `/api/locations`
- **Công dụng**: Lấy danh sách tất cả cửa hàng (tọa độ GPS, giờ mở)
- **Response**: `List<StoreLocationDto>` (storeName, address, latitude, longitude, openingHours, closingHours)

### 25. **GET** `/api/locations/{id}`
- **Công dụng**: Lấy chi tiết 1 cửa hàng
- **Path Parameter**: `id` - ID cửa hàng
- **Response**: `StoreLocationDto` (tất cả thông tin cửa hàng)

---

## 🔗 SignalR Hubs - Real-time Communication

### Chat Hub: `wss://localhost:5001/hub/chat`
- **Công dụng**: Chat real-time giữa user và staff
- **Methods**: SendMessage, ReceiveMessage, UserJoined, UserLeft

### Notification Hub: `wss://localhost:5001/hub/notifications`
- **Công dụng**: Nhận thông báo real-time (đơn hàng, khuyến mãi, etc.)
- **Methods**: ReceiveNotification, MarkAsRead

---

## 📊 Response Format

**Success Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {...},
  "statusCode": 200
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "statusCode": 400
}
```

---

## 🔑 Common HTTP Status Codes

- `200` - OK, request thành công
- `201` - Created, tài nguyên được tạo
- `400` - Bad Request, request không hợp lệ
- `401` - Unauthorized, cần JWT token
- `403` - Forbidden, không có quyền access
- `404` - Not Found, tài nguyên không tồn tại
- `500` - Internal Server Error, lỗi server

---

## � API Workflows - Luồng hoạt động chính

### **1️⃣ Luồng Đăng ký & Đăng nhập**

```
Mobile App                          Backend API
    |                                   |
    |-- POST /api/auth/register ------>|
    |   (name, email, pwd, phone)      |
    |                                   | ✅ Validate & Save User
    |<---- AuthResponse --------|
    |   (userId, accessToken,   
    |    refreshToken)           |
    |                           |
    | [Token lưu vào SharedPreferences/DataStore]
    |
    |-- POST /api/auth/login ------->|
    |   (email, password)             |
    |                               | ✅ Verify & Generate JWT
    |<----- AuthResponse -----------|
    |   (accessToken, expiresIn)     |
```

**Bước:**
1. User nhập email, password, tên, số điện thoại → Register
2. Backend hash password, lưu vào DB, trả token
3. Mobile lưu token, dùng cho các request tiếp theo

---

### **2️⃣ Luồng Mua sắm (Browse → Cart → Checkout → Order)**

```
Mobile App                          Backend API
    |
    |-- GET /api/categories 
    |   (lấy danh sách danh mục)      ✅ Hiển thị menu
    |
    |-- GET /api/products?categoryId=1&search=...
    |   (lọc sản phẩm)                ✅ Lấy danh sách sản phẩm
    |
    |-- GET /api/products/{id}
    |   (xem chi tiết sản phẩm)        ✅ Hiển thị detail page
    |
    |-- POST /api/cart/items
    |   (thêm vào giỏ)                 ✅ Lưu vào DB
    |
    |-- GET /api/cart
    |   (xem giỏ hàng)                 ✅ Tính totalPrice
    |
    |-- PUT /api/cart/items/{id}
    |   (cập nhật số lượng)             ✅ Update quantity
    |
    |-- DELETE /api/cart/items/{id}
    |   (xóa item khỏi giỏ)            ✅ Delete item
    |
    |-- POST /api/orders
    |   (tạo đơn hàng)                 ✅ Move từ Cart → Orders
    |                                   ✅ Clear cart
    |                                   ✅ Generate orderCode
    |                                   ✅ Send notification
    |
    |-- GET /api/notifications/badge
    |   (badge count)                  ✅ Hiển thị thông báo
```

**Chi tiết Order Creation Flow:**
```
1. User xem giỏ hàng → Click "Thanh toán"
   ↓
2. Nhập địa chỉ giao hàng, chọn hình thức thanh toán (COD/Online)
   ↓
3. Gửi POST /api/orders
   ↓
4. Backend:
   - Create Notification (order pending)
   - Clear user's cart
   - Return OrderDto
   ↓
5. Mobile nhận OrderDto, hiển thị "Order ID: ..."
   ↓
6. Real-time notification qua SignalR: "Đơn hàng của bạn đã được tạo"
```

---

### **3️⃣ Luồng Thông báo (Notifications)**

```
Backend (Admin/System)              Mobile App
    |                                   |
    |-- Trigger Event:                 |
    |   - Đơn hàng được tạo              |
    |   - Đơn hàng được cập nhật         |
    |   - Khuyến mãi mới                |
    |   - Chat message mới              |
    |                                   |
    |-- CreateNotification() --------->|
    |   (save to DB)                    |
    |                                   |
    |-- SignalR SendNotificationAsync()|
    |   (broadcast real-time)           |
    |   (group: user-{userId})          |
    |                                   |
    |                                   | ✅ App nhận notification
    |                                   |    - Hiển thị Toast/Dialog
    |                                   |    - Update badge count
    |
    |-- GET /api/notifications -----<- |
    |   (sync notifications)            | ✅ Hiển thị Notification List
    |                                   |
    |-- GET /api/notifications/badge <-|
    |   (unreadNotifications count)     | ✅ Update badge icon
    |
    |-- PUT /api/notifications/{id}/read |
    |   (đánh dấu đã đọc)              |
```

---

### **4️⃣ Luồng Chat Real-time**

```
Mobile App (User A)                 Backend (SignalR)               Mobile App (User B)
    |                                       |                            |
    |-- Connect to /hub/chat
    |   (JWT token in query string)         |                            |
    |                                       |-- Broadcast to group       |
    |                                       |   (staff group)            |
    |                                       |                        ✅ Connected
    |
    |-- SendMessage("Hello") ------>|
    |   (message content)            | ✅ Save to ChatMessage DB
    |                                | ✅ Relay to all in group
    |                                |-----> ReceiveMessage("Hello") -->|
    |                                       |                           |
    |                                       |                      ✅ Display in chat
```

---

### **5️⃣ Luồng Cập nhật Hồ sơ**

```
Mobile App                          Backend API
    |
    |-- GET /api/users/profile
    |   (lấy thông tin hiện tại)      ✅ Hiển thị form
    |
    |-- PUT /api/users/profile
    |   (name, phone, profileImage)   ✅ Validate & Update
    |                                 ✅ Save to UserProfile
    |                                 ✅ Return updated UserDto
    |
    |-- PUT /api/users/change-password
    |   (oldPwd, newPwd)              ✅ Hash & Update password
    |                                 ✅ Invalidate old tokens (optional)
```

---

### **6️⃣ Luồng Theo dõi Đơn hàng**

```
Mobile App                          Backend API
    |
    |-- GET /api/orders
    |   (pageNumber=1, pageSize=10)  ✅ Get all orders (paginated)
    |                                 ✅ Return order list
    |
    |-- GET /api/orders/{orderId}
    |   (xem chi tiết 1 đơn)          ✅ Return order với orderItems,
    |                                    status, timeline
    |
    |-- Real-time Update qua SignalR
    |   (order status changed)        ✅ "Thay đổi sang: Đang giao"
```

---

### **7️⃣ Luồng Token Refresh**

```
Mobile App                          Backend API
    |
    | [Access Token hết hạn]
    |
    |-- POST /api/auth/refresh-token |
    |   (refreshToken)                | ✅ Verify refreshToken
    |                                 | ✅ Generate new accessToken
    |<----- New AccessToken ---------|
    |
    | [Update local token]
    |-- Retry original request ------>|
    |   (với token mới)                | ✅ Request thành công
```

---

### **8️⃣ Luồng Lấy Địa điểm Cửa hàng (Map Integration)**

```
Mobile App                          Backend API
    |
    |-- GET /api/locations
    |   (không cần auth)               ✅ Return all stores
    |                                   (storeName, address, lat, long,
    |                                    openingHours, closingHours)
    |
    | ✅ Display on Google Map
    | ✅ Show store info at bottom
    | ✅ Calculate distance
    | ✅ Open directions in Maps app
```

---

## �💡 Usage Summary

| Feature | Auth Required | Endpoint Count |
|---------|---|---|
| Auth | ❌ | 4 |
| Products | ❌ | 2 |
| Categories | ❌ | 1 |
| Cart | ✅ | 5 |
| Orders | ✅ | 3 |
| Notifications | ✅ | 5 |
| User Profile | ✅ | 3 |
| Store Locations | ❌ | 2 |
| **Total** | | **25** |

---

**Last Updated**: 14/03/2026  
**Version**: 1.0  
**API Status**: ✅ Production Ready
