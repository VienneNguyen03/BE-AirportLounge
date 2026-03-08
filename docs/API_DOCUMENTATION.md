# Tài liệu API – Hệ thống Quản lý Nội bộ Nhân viên Lounge Sân bay

Tài liệu mô tả từng API: mục đích, quyền truy cập, và chi tiết từng trường trong request/response.

**Base URL:** `https://<host>/api`  
**Xác thực:** Hầu hết API yêu cầu header `Authorization: Bearer <access_token>` (trừ Auth).

---

## Mục lục

1. [Auth](#1-auth)
2. [Employees](#2-employees)
3. [Shifts](#3-shifts)
4. [Attendance](#4-attendance)
5. [Tasks](#5-tasks)
6. [Zones](#6-zones)
7. [Dashboard](#7-dashboard)
8. [Notifications](#8-notifications)
9. [Documents](#9-documents)
10. [IdCards](#10-idcards)
11. [Onboarding](#11-onboarding)
12. [Offboarding](#12-offboarding)
13. [Training](#13-training)
14. [Performance](#14-performance)
15. [Payroll](#15-payroll)
16. [Leaves](#16-leaves)
17. [Enums tham chiếu](#17-enums-tham-chiếu)

---

## 1. Auth

**Mục đích:** Đăng nhập và gia hạn phiên (refresh token). Không cần Bearer token.

### POST `/api/auth/login`

**Mục đích:** Xác thực người dùng bằng email/số điện thoại và mật khẩu, trả về access token và refresh token để gọi các API khác.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `emailOrPhone` | string | Có | Email hoặc số điện thoại đăng nhập |
| `password` | string | Có | Mật khẩu (plain text, truyền qua HTTPS) |

**Response (200):** `{ isSuccess, data: { accessToken, refreshToken, expiresAt, ... }, message }`

---

### POST `/api/auth/refresh`

**Mục đích:** Lấy access token mới khi token cũ sắp hết hạn, dùng refresh token đã nhận từ login.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `accessToken` | string | Có | Access token hiện tại (đang dùng) |
| `refreshToken` | string | Có | Refresh token nhận từ lần login |

**Response (200):** `{ isSuccess, data: { accessToken, refreshToken, expiresAt, ... }, message }`

---

## 2. Employees

**Mục đích:** Quản lý hồ sơ nhân viên (CRUD, vô hiệu hóa). Phục vụ SRS quản lý nhân sự và hồ sơ đầy đủ.

### POST `/api/employees`

**Quyền:** Admin, Manager  
**Mục đích:** Tạo nhân viên mới và tài khoản đăng nhập, nhập đầy đủ thông tin cá nhân và nghề nghiệp.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeCode` | string | Có | Mã nhân viên (duy nhất) |
| `fullName` | string | Có | Họ tên |
| `email` | string | Có | Email (dùng đăng nhập) |
| `phoneNumber` | string | Không | Số điện thoại |
| `password` | string | Có | Mật khẩu ban đầu |
| `role` | int (UserRole) | Có | 0=Staff, 1=Manager, 2=Admin |
| `department` | string | Không | Bộ phận |
| `position` | string | Không | Vị trí công việc |
| `skills` | string | Không | Kỹ năng (có thể chuỗi mô tả) |
| `dateOfBirth` | string (date) | Không | Ngày sinh (ISO 8601) |
| `nationalId` | string | Không | CCCD / hộ chiếu |
| `nationality` | string | Không | Quốc tịch |
| `gender` | int? (Gender) | Không | 0=Male, 1=Female, 2=Other |
| `maritalStatus` | int? (MaritalStatus) | Không | 0=Single, 1=Married, 2=Divorced, 3=Widowed |
| `permanentAddress` | string | Không | Địa chỉ thường trú |
| `temporaryAddress` | string | Không | Địa chỉ tạm trú |
| `taxCode` | string | Không | Mã số thuế |
| `bankAccountNumber` | string | Không | Số tài khoản ngân hàng |
| `bankName` | string | Không | Tên ngân hàng |
| `bloodType` | string | Không | Nhóm máu |
| `emergencyContactName` | string | Không | Tên người liên hệ khẩn cấp |
| `emergencyContactPhone` | string | Không | SĐT liên hệ khẩn cấp |
| `emergencyContactRelationship` | string | Không | Mối quan hệ (vd: Bố, Mẹ) |

**Response (201):** `{ isSuccess, data: <employeeId (Guid)>, message }`

---

### GET `/api/employees/{id}`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy chi tiết một nhân viên theo ID (xem hồ sơ).

**Path:** `id` (Guid) – ID nhân viên.

**Response (200):** `{ isSuccess, data: <EmployeeDto>, message }`

---

### GET `/api/employees`

**Quyền:** Admin, Manager  
**Mục đích:** Danh sách nhân viên có phân trang, tìm kiếm và lọc theo bộ phận.

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `search` | string | Tìm theo tên/mã/email |
| `department` | string | Lọc theo bộ phận |
| `pageNumber` | int | Trang (mặc định 1) |
| `pageSize` | int | Số bản ghi/trang (mặc định 10) |

**Response (200):** `{ isSuccess, data: { items, totalCount, pageNumber, pageSize }, message }`

---

### PUT `/api/employees/{id}`

**Quyền:** Admin, Manager  
**Mục đích:** Cập nhật thông tin nhân viên (tên, liên hệ, bộ phận, địa chỉ, v.v.). Body phải có `employeeId` trùng với `id` trong URL.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | Phải trùng với `id` trong URL |
| `fullName` | string | Có | Họ tên |
| `phoneNumber` | string | Không | Số điện thoại |
| `department` | string | Không | Bộ phận |
| `position` | string | Không | Vị trí |
| `skills` | string | Không | Kỹ năng |
| `address` | string | Không | Địa chỉ (chung) |
| `dateOfBirth` | string (date) | Không | Ngày sinh |
| `nationalId` | string | Không | CCCD/hộ chiếu |
| `nationality` | string | Không | Quốc tịch |
| `gender` | int? | Không | Gender enum |
| `maritalStatus` | int? | Không | MaritalStatus enum |
| `permanentAddress` | string | Không | Địa chỉ thường trú |
| `temporaryAddress` | string | Không | Địa chỉ tạm trú |
| `taxCode` | string | Không | Mã số thuế |
| `bankAccountNumber` | string | Không | Số TK ngân hàng |
| `bankName` | string | Không | Tên ngân hàng |
| `bloodType` | string | Không | Nhóm máu |
| `emergencyContactName` | string | Không | Liên hệ khẩn cấp – tên |
| `emergencyContactPhone` | string | Không | Liên hệ khẩn cấp – SĐT |
| `emergencyContactRelationship` | string | Không | Mối quan hệ |
| `profilePhotoUrl` | string | Không | URL ảnh đại diện |

**Response (200):** `{ isSuccess, data: true, message }`

---

### DELETE `/api/employees/{id}`

**Quyền:** Admin  
**Mục đích:** Vô hiệu hóa nhân viên (soft delete / deactivate). Nhân viên không đăng nhập được, dữ liệu vẫn lưu để báo cáo.

**Path:** `id` (Guid).

**Response (200):** `{ isSuccess, data, message }`

---

## 3. Shifts

**Mục đích:** Định nghĩa ca làm (khung giờ) và phân công nhân viên vào ca theo ngày. Phục vụ lên lịch ca và tránh trùng/trùng nghỉ phép.

### GET `/api/shifts`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy danh sách tất cả ca làm (định nghĩa: tên, giờ bắt đầu/kết thúc) để dùng khi tạo lịch hoặc gán ca.

**Response (200):** `{ isSuccess, data: [ { id, name, startTime, endTime, description }, ... ], message }`

---

### POST `/api/shifts`

**Quyền:** Admin, Manager  
**Mục đích:** Tạo ca làm mới (vd: ca sáng 06:00–14:00, ca chiều 14:00–22:00).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `name` | string | Có | Tên ca (vd: "Ca sáng") |
| `startTime` | string (TimeSpan) | Có | Giờ bắt đầu, format "HH:mm:ss" (vd: "06:00:00") |
| `endTime` | string (TimeSpan) | Có | Giờ kết thúc, format "HH:mm:ss" (vd: "14:00:00") |
| `description` | string | Không | Mô tả ca |

**Response (200):** `{ isSuccess, data: <shiftId (Guid)>, message }`

---

### POST `/api/shifts/assign`

**Quyền:** Admin, Manager  
**Mục đích:** Gán nhân viên vào một ca trong một ngày cụ thể. Hệ thống kiểm tra trùng ca và nghỉ phép đã duyệt; nếu có thì trả lỗi và gợi ý nhân viên khác.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `shiftId` | Guid | Có | ID ca làm |
| `employeeId` | Guid | Có | ID nhân viên |
| `date` | string (date) | Có | Ngày làm (ISO 8601 date) |
| `loungeZoneId` | Guid? | Không | ID khu vực lounge (nếu gán theo khu vực) |

**Response (200):** `{ isSuccess, data: <assignmentId (Guid)>, message }`

---

### GET `/api/shifts/schedule`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy lịch phân công ca trong khoảng thời gian; có thể lọc theo nhân viên (nhân viên xem lịch của mình, quản lý xem theo người).

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `startDate` | DateTime | Ngày bắt đầu |
| `endDate` | DateTime | Ngày kết thúc |
| `employeeId` | Guid? | Lọc theo nhân viên; null = tất cả (tùy quyền) |

**Response (200):** `{ isSuccess, data: [ schedule items ], message }`

---

## 4. Attendance

**Mục đích:** Chấm công vào/ra, báo cáo chấm công, chỉnh sửa/xác nhận bản ghi (quản lý). Dữ liệu dùng cho lương và tuân thủ.

### POST `/api/attendance/check-in`

**Quyền:** Đã đăng nhập  
**Mục đích:** Nhân viên chấm công vào ca. Chỉ thành công khi đang trong khung giờ ca được gán (cho phép vào sớm tối đa 15 phút). Trả về trạng thái OnTime hoặc Late.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên (thường là user đang đăng nhập) |

**Response (200):** `{ isSuccess, data: <attendanceId (Guid)>, message }`

---

### POST `/api/attendance/check-out`

**Quyền:** Đã đăng nhập  
**Mục đích:** Nhân viên chấm công kết thúc ca. Hệ thống tính giờ làm, cập nhật trạng thái EarlyLeave hoặc Overtime nếu ra sớm/muộn so với ca.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên |

**Response (200):** `{ isSuccess, data: true, message }` (message có thể chứa số giờ làm và trạng thái)

---

### GET `/api/attendance/report`

**Quyền:** Admin, Manager  
**Mục đích:** Báo cáo chấm công theo khoảng thời gian, có thể lọc theo nhân viên và trạng thái (đúng giờ, muộn, về sớm, làm thêm).

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `startDate` | DateTime | Từ ngày |
| `endDate` | DateTime | Đến ngày |
| `employeeId` | Guid? | Lọc theo nhân viên |
| `status` | int? (AttendanceStatus) | 0=OnTime, 1=Late, 2=EarlyLeave, 3=Overtime, 4=Absent |
| `pageNumber` | int | Mặc định 1 |
| `pageSize` | int | Mặc định 20 |

**Response (200):** `{ isSuccess, data: { items, totalCount, ... }, message }`

---

### PUT `/api/attendance/{id}`

**Quyền:** Admin, Manager  
**Mục đích:** Sửa bản ghi chấm công (vd: quên chấm, chỉnh giờ vào/ra, ghi chú). Body phải có `attendanceId` trùng với `id` trong URL.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `attendanceId` | Guid | Có | Phải trùng với `id` trong URL |
| `checkInTime` | string (date-time)? | Không | Giờ vào (chỉnh tay) |
| `checkOutTime` | string (date-time)? | Không | Giờ ra (chỉnh tay) |
| `status` | int? (AttendanceStatus) | Không | 0=OnTime, 1=Late, 2=EarlyLeave, 3=Overtime, 4=Absent |
| `notes` | string | Không | Ghi chú (vd: lý do chỉnh) |

**Response (200):** `{ isSuccess, data: true, message }`

---

### POST `/api/attendance/{id}/confirm`

**Quyền:** Admin, Manager  
**Mục đích:** Quản lý xác nhận bản ghi chấm công (đánh dấu đã kiểm tra/duyệt).

**Path:** `id` (Guid) – ID bản ghi chấm công.

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/attendance/export`

**Quyền:** Admin, Manager  
**Mục đích:** Xuất báo cáo chấm công ra file CSV hoặc PDF theo khoảng ngày và bộ lọc.

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `startDate` | DateTime | Từ ngày |
| `endDate` | DateTime | Đến ngày |
| `employeeId` | Guid? | Lọc theo nhân viên |
| `status` | int? (AttendanceStatus) | Lọc theo trạng thái |
| `format` | string | "csv" (mặc định) hoặc "pdf" |

**Response:** File download (CSV hoặc PDF).

---

## 5. Tasks

**Mục đích:** Quản lý nhiệm vụ (task): tạo, giao nhân viên, cập nhật trạng thái, danh sách và xuất báo cáo.

### POST `/api/tasks`

**Quyền:** Admin, Manager  
**Mục đích:** Tạo nhiệm vụ mới (vd: phục vụ đồ ăn, dọn dẹp khu vực). Có thể gán ngay cho nhân viên; hệ thống gửi thông báo cho người được giao.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `title` | string | Có | Tiêu đề nhiệm vụ |
| `description` | string | Không | Mô tả chi tiết |
| `priority` | int (TaskPriority) | Có | 0=Low, 1=Medium, 2=High, 3=Urgent |
| `assignedToId` | Guid? | Không | ID nhân viên được giao |
| `loungeZoneId` | Guid? | Không | ID khu vực lounge liên quan |
| `dueDate` | string (date-time)? | Không | Hạn hoàn thành |

**Response (200):** `{ isSuccess, data: <taskId (Guid)>, message }`

---

### PUT `/api/tasks/{id}/status`

**Quyền:** Đã đăng nhập (nhân viên cập nhật task của mình)  
**Mục đích:** Cập nhật trạng thái nhiệm vụ (Pending → InProgress → Completed). Khi chuyển Completed, hệ thống ghi nhận thời điểm hoàn thành.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `taskId` | Guid | Có | Phải trùng với `id` trong URL |
| `newStatus` | int (TaskItemStatus) | Có | 0=Pending, 1=InProgress, 2=Completed, 3=Cancelled |

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/tasks`

**Quyền:** Đã đăng nhập  
**Mục đích:** Danh sách nhiệm vụ có phân trang, lọc theo trạng thái, người được giao, độ ưu tiên.

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `status` | int? (TaskItemStatus) | 0=Pending, 1=InProgress, 2=Completed, 3=Cancelled |
| `assignedToId` | Guid? | Lọc theo nhân viên được giao |
| `priority` | int? (TaskPriority) | 0=Low, 1=Medium, 2=High, 3=Urgent |
| `pageNumber` | int | Mặc định 1 |
| `pageSize` | int | Mặc định 20 |

**Response (200):** `{ isSuccess, data: { items, totalCount, ... }, message }`

---

### GET `/api/tasks/export`

**Quyền:** Admin, Manager  
**Mục đích:** Xuất danh sách nhiệm vụ ra CSV hoặc PDF theo bộ lọc.

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `status` | int? (TaskItemStatus) | Lọc theo trạng thái |
| `assignedToId` | Guid? | Lọc theo người được giao |
| `priority` | int? (TaskPriority) | Lọc theo độ ưu tiên |
| `format` | string | "csv" (mặc định) hoặc "pdf" |

**Response:** File download.

---

## 6. Zones

**Mục đích:** Quản lý khu vực lounge (tên, sức chứa, trạng thái). Nhân viên/quản lý cập nhật trạng thái; hệ thống có thể cảnh báo khi Full hoặc NeedsSupport.

### POST `/api/zones`

**Quyền:** Admin, Manager  
**Mục đích:** Tạo khu vực lounge mới (vd: khu chính, khu đồ ăn, khu VIP).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `name` | string | Có | Tên khu vực |
| `description` | string | Không | Mô tả |
| `capacity` | int | Có | Sức chứa (số người) |

**Response (200):** `{ isSuccess, data: <zoneId (Guid)>, message }`

---

### PUT `/api/zones/{id}/status`

**Quyền:** Đã đăng nhập  
**Mục đích:** Cập nhật trạng thái khu vực (Available, InService, NeedsCleaning, NeedsSupport, Full, Closed). Có ghi log và gửi thông báo khi Full/NeedsSupport.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `zoneId` | Guid | Có | Phải trùng với `id` trong URL |
| `newStatus` | int (ZoneStatus) | Có | 0=Available, 1=InService, 2=NeedsCleaning, 3=NeedsSupport, 4=Full, 5=Closed |
| `notes` | string | Không | Ghi chú thay đổi |

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/zones`

**Quyền:** Đã đăng nhập  
**Mục đích:** Danh sách khu vực lounge, có thể lọc theo trạng thái.

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `status` | int? (ZoneStatus) | 0=Available, 1=InService, 2=NeedsCleaning, 3=NeedsSupport, 4=Full, 5=Closed |

**Response (200):** `{ isSuccess, data: [ zones ], message }`

---

### GET `/api/zones/alerts`

**Quyền:** Admin, Manager  
**Mục đích:** Lấy danh sách cảnh báo khu vực (vd: gần đầy, cần hỗ trợ) để phân bổ nhân lực.

**Response (200):** `{ isSuccess, data: [ alerts ], message }`

---

## 7. Dashboard

**Mục đích:** Trang tổng quan theo vai trò: nhân viên (lịch hôm nay, nhiệm vụ, thông báo), quản lý (nhân viên đang làm, ca thiếu người, task chờ), admin (thống kê hệ thống).

### GET `/api/dashboard/staff/{employeeId}`

**Quyền:** Staff, Manager, Admin  
**Mục đích:** Dashboard cho nhân viên: lịch làm hôm nay, nhiệm vụ hiện tại, thông báo mới (và có thể số ngày nghỉ còn lại, tiến độ đào tạo nếu có).

**Path:** `employeeId` (Guid).

**Response (200):** `{ isSuccess, data: { ... }, message }`

---

### GET `/api/dashboard/manager`

**Quyền:** Manager, Admin  
**Mục đích:** Dashboard quản lý: nhân viên đang làm, ca thiếu nhân lực, nhiệm vụ đang chờ, cảnh báo khu vực.

**Response (200):** `{ isSuccess, data: { ... }, message }`

---

### GET `/api/dashboard/admin`

**Quyền:** Admin  
**Mục đích:** Dashboard admin: số lượng người dùng, tình trạng hệ thống, thống kê tổng hợp.

**Response (200):** `{ isSuccess, data: { ... }, message }`

---

## 8. Notifications

**Mục đích:** Gửi thông báo nội bộ (push/real-time) và quản lý trạng thái đọc. Hỗ trợ gửi theo nhóm hoặc broadcast.

### POST `/api/notifications`

**Quyền:** Admin, Manager  
**Mục đích:** Gửi thông báo tới danh sách nhân viên hoặc tất cả (broadcast). Dùng cho thay đổi ca, quy định mới, thông báo khẩn.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `title` | string | Có | Tiêu đề |
| `content` | string | Có | Nội dung |
| `type` | int (NotificationType) | Có | 0=General, 1=ShiftChange, 2=TaskAssignment, 3=Urgent, 4=System, 5=LeaveRequest, 6=LeaveApproval, 7=PerformanceReview, 8=TrainingAssignment, 9=TrainingCompletion, 10=OnboardingTask, 11=IdCardIssued, 12=PayrollReady |
| `recipientIds` | Guid[]? | Không | Danh sách ID nhân viên nhận; null hoặc rỗng = gửi tất cả |
| `relatedEntityType` | string | Không | Loại entity liên quan (vd: "Shift", "Task") |
| `relatedEntityId` | Guid? | Không | ID entity (để deep link) |

**Response (200):** `{ isSuccess, data: <số người nhận (int)>, message }`

---

### PUT `/api/notifications/{id}/read`

**Quyền:** Đã đăng nhập  
**Mục đích:** Đánh dấu một thông báo là đã đọc.

**Path:** `id` (Guid) – ID thông báo.

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/notifications/my/{employeeId}`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy danh sách thông báo của một nhân viên, có thể chỉ lấy chưa đọc và phân trang.

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `employeeId` | Guid (path) | ID nhân viên |
| `unreadOnly` | bool? | true = chỉ thông báo chưa đọc |
| `pageNumber` | int | Mặc định 1 |
| `pageSize` | int | Mặc định 20 |

**Response (200):** `{ isSuccess, data: { items, totalCount, ... }, message }`

---

## 9. Documents

**Mục đích:** Lưu trữ tài liệu nhân sự (hợp đồng, chứng chỉ, ảnh) gắn với hồ sơ nhân viên; phân quyền xem/tải.

### POST `/api/documents`

**Quyền:** Admin, Manager  
**Mục đích:** Tải lên tài liệu vào hồ sơ nhân viên (file đã upload lên storage; API nhận metadata và đường dẫn).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên |
| `title` | string | Có | Tiêu đề tài liệu |
| `category` | int (DocumentCategory) | Có | 0=Contract, 1=Certificate, 2=IdDocument, 3=Photo, 4=PaySlip, 5=Other |
| `filePath` | string | Có | Đường dẫn file đã lưu (storage) |
| `fileSize` | long | Có | Kích thước file (bytes) |
| `contentType` | string | Không | MIME type (vd: application/pdf) |
| `isConfidential` | bool | Có | Có phải tài liệu mật không |

**Response (200):** `{ isSuccess, data: <documentId (Guid)>, message }`

---

### DELETE `/api/documents/{id}`

**Quyền:** Admin  
**Mục đích:** Xóa tài liệu khỏi hồ sơ (soft delete hoặc xóa bản ghi; file trên storage tùy triển khai).

**Path:** `id` (Guid).

**Response (200):** `{ isSuccess, data, message }`

---

### GET `/api/documents/{employeeId}`

**Quyền:** Đã đăng nhập (nhân viên chỉ xem được của mình)  
**Mục đích:** Lấy danh sách tài liệu của một nhân viên.

**Path:** `employeeId` (Guid).

**Response (200):** `{ isSuccess, data: [ documents ], message }`

---

## 10. IdCards

**Mục đích:** Quản lý thẻ ID nhân viên: cấp phát, thu hồi, xem theo nhân viên. Hệ thống tự sinh mã thẻ và QR.

### POST `/api/idcards`

**Quyền:** Admin  
**Mục đích:** Cấp phát thẻ ID cho nhân viên (tự sinh mã thẻ và dữ liệu QR từ hồ sơ).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên |
| `templateType` | string | Không | Loại mẫu thẻ (theo bộ phận/loại NV) |

**Response (200):** `{ isSuccess, data: <cardId (Guid)>, message }`

---

### POST `/api/idcards/{id}/revoke`

**Quyền:** Admin  
**Mục đích:** Thu hồi thẻ ID (mất thẻ, thôi việc). Body phải có `cardId` trùng với `id` trong URL.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `cardId` | Guid | Có | Phải trùng với `id` trong URL |
| `reason` | string | Có | Lý do thu hồi |

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/idcards/{employeeId}`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy danh sách thẻ ID (đang hiệu lực và đã thu hồi) của một nhân viên.

**Path:** `employeeId` (Guid).

**Response (200):** `{ isSuccess, data: [ idCards ], message }`

---

## 11. Onboarding

**Mục đích:** Quy trình tiếp nhận nhân viên mới: tạo quy trình onboarding và checklist nhiệm vụ (thu thập thông tin, đào tạo ban đầu, v.v.).

### POST `/api/onboarding`

**Quyền:** Admin, Manager  
**Mục đích:** Tạo quy trình onboarding cho nhân viên mới, kèm danh sách nhiệm vụ (task) và có thể gán mentor.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên mới |
| `assignedMentorId` | Guid? | Không | ID nhân viên mentor |
| `tasks` | array | Có | Danh sách nhiệm vụ onboarding |

**Mỗi phần tử trong `tasks`:**

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `title` | string | Có | Tiêu đề task |
| `description` | string | Không | Mô tả |
| `assignedToId` | Guid? | Không | Người chịu trách nhiệm thực hiện |
| `dueDate` | string (date-time)? | Không | Hạn hoàn thành |
| `sortOrder` | int | Có | Thứ tự hiển thị |

**Response (200):** `{ isSuccess, data: <processId (Guid)>, message }`

---

### POST `/api/onboarding/tasks/{id}/complete`

**Quyền:** Đã đăng nhập  
**Mục đích:** Đánh dấu hoàn thành một nhiệm vụ trong checklist onboarding.

**Path:** `id` (Guid) – ID nhiệm vụ onboarding.

**Response (200):** `{ isSuccess, data, message }`

---

### GET `/api/onboarding/{employeeId}`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy thông tin quy trình onboarding và danh sách task của một nhân viên.

**Path:** `employeeId` (Guid).

**Response (200):** `{ isSuccess, data: { process, tasks }, message }`

---

## 12. Offboarding

**Mục đích:** Quy trình thôi việc: khởi tạo offboarding, cập nhật khảo sát/bàn giao tài sản/khóa quyền, hoàn tất và vô hiệu hóa nhân viên.

### POST `/api/offboarding`

**Quyền:** Admin  
**Mục đích:** Khởi tạo quy trình offboarding cho nhân viên (ghi nhận ngày nghỉ việc, ngày làm việc cuối, lý do).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên |
| `resignationDate` | string (date) | Có | Ngày nộp đơn nghỉ việc |
| `lastWorkingDate` | string (date) | Có | Ngày làm việc cuối cùng |
| `reason` | string | Không | Lý do nghỉ việc |

**Response (200):** `{ isSuccess, data: <processId (Guid)>, message }`

---

### PUT `/api/offboarding/{id}`

**Quyền:** Admin  
**Mục đích:** Cập nhật tiến độ offboarding: khảo sát thoát, bàn giao tài sản, thu hồi quyền. Khi đủ điều kiện (survey + asset + access) hệ thống đánh dấu Completed và vô hiệu hóa nhân viên/tài khoản.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `processId` | Guid | Có | Phải trùng với `id` trong URL |
| `exitSurveyCompleted` | bool? | Không | Đã hoàn thành khảo sát thoát |
| `assetReturned` | bool? | Không | Đã bàn giao/trả thiết bị |
| `accessRevoked` | bool? | Không | Đã khóa quyền truy cập hệ thống |

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/offboarding/{employeeId}`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy thông tin quy trình offboarding của một nhân viên (nếu có).

**Path:** `employeeId` (Guid).

**Response (200):** `{ isSuccess, data: { ... }, message }`

---

## 13. Training

**Mục đích:** Quản lý khóa đào tạo, đăng ký nhân viên vào khóa, hoàn thành và chấm điểm; hỗ trợ chứng nhận.

### GET `/api/training/courses`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy danh sách khóa đào tạo (đang active) để đăng ký hoặc xem nội dung.

**Response (200):** `{ isSuccess, data: [ courses ], message }`

---

### POST `/api/training/courses`

**Quyền:** Admin  
**Mục đích:** Tạo khóa đào tạo mới (nghiệp vụ, an toàn, kỹ năng mềm…) với thời lượng, điểm đạt, link nội dung.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `title` | string | Có | Tên khóa học |
| `description` | string | Không | Mô tả |
| `category` | string | Không | Danh mục (vd: "An toàn", "Nghiệp vụ") |
| `durationInHours` | int | Có | Thời lượng (giờ) |
| `contentUrl` | string | Không | URL nội dung (video, tài liệu) |
| `passingScore` | decimal | Có | Điểm tối thiểu để đạt (vd: 70) |

**Response (200):** `{ isSuccess, data: <courseId (Guid)>, message }`

---

### POST `/api/training/enroll`

**Quyền:** Admin, Manager  
**Mục đích:** Đăng ký nhân viên vào một khóa học. Không cho đăng ký trùng (đã enrolled/in progress).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên |
| `courseId` | Guid | Có | ID khóa học |

**Response (200):** `{ isSuccess, data: <enrollmentId (Guid)>, message }`

---

### POST `/api/training/enrollments/{id}/complete`

**Quyền:** Admin, Manager  
**Mục đích:** Đánh dấu hoàn thành đăng ký đào tạo: nhập điểm và (tùy chọn) URL chứng chỉ. Hệ thống so với passingScore để set Completed hoặc Failed.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `enrollmentId` | Guid | Có | Phải trùng với `id` trong URL |
| `score` | decimal | Có | Điểm đạt được |
| `certificateUrl` | string | Không | URL chứng nhận (nếu có) |

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/training/enrollments/{employeeId}`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy danh sách đăng ký đào tạo (và tiến độ) của một nhân viên.

**Path:** `employeeId` (Guid).

**Response (200):** `{ isSuccess, data: [ enrollments ], message }`

---

## 14. Performance

**Mục đích:** Mục tiêu/KPI, đánh giá định kỳ (tự đánh giá + quản lý), hoàn tất đánh giá với điểm và kế hoạch cải thiện.

### POST `/api/performance/goals`

**Quyền:** Admin, Manager  
**Mục đích:** Tạo mục tiêu hiệu suất/KPI cho nhân viên (vd: số task hoàn thành, điểm hài lòng khách hàng).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên |
| `title` | string | Có | Tiêu đề mục tiêu |
| `description` | string | Không | Mô tả |
| `targetValue` | decimal | Có | Giá trị mục tiêu (số lượng, %, …) |
| `unit` | string | Không | Đơn vị (vd: "tasks", "%") |
| `dueDate` | string (date) | Có | Hạn đạt mục tiêu |

**Response (200):** `{ isSuccess, data: <goalId (Guid)>, message }`

---

### PUT `/api/performance/goals/{id}/progress`

**Quyền:** Đã đăng nhập  
**Mục đích:** Cập nhật tiến độ mục tiêu (giá trị hiện tại và/hoặc trạng thái).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `goalId` | Guid | Có | Phải trùng với `id` trong URL |
| `currentValue` | decimal | Có | Giá trị hiện tại đạt được |
| `status` | int? (GoalStatus) | Không | 0=NotStarted, 1=InProgress, 2=Achieved, 3=NotAchieved |

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/performance/goals/{employeeId}`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy danh sách mục tiêu hiệu suất của một nhân viên.

**Path:** `employeeId` (Guid).

**Response (200):** `{ isSuccess, data: [ goals ], message }`

---

### POST `/api/performance/reviews`

**Quyền:** Admin, Manager  
**Mục đích:** Tạo phiên đánh giá hiệu suất (quý/năm). Trạng thái ban đầu Draft; nhân viên có thể nộp tự đánh giá sau.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên được đánh giá |
| `period` | string | Có | Kỳ đánh giá (vd: "Q1 2025", "2025") |
| `reviewType` | int (ReviewType) | Có | 0=Quarterly, 1=Annual |
| `comments` | string | Không | Ghi chú ban đầu |

**Response (200):** `{ isSuccess, data: <reviewId (Guid)>, message }`

---

### POST `/api/performance/reviews/{id}/self-assessment`

**Quyền:** Đã đăng nhập (thường là nhân viên được đánh giá)  
**Mục đích:** Nộp tự đánh giá cho phiên đánh giá. Chuyển trạng thái sang ManagerReview.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `reviewId` | Guid | Có | Phải trùng với `id` trong URL |
| `selfAssessment` | string | Có | Nội dung tự đánh giá |

**Response (200):** `{ isSuccess, data: true, message }`

---

### POST `/api/performance/reviews/{id}/complete`

**Quyền:** Admin, Manager  
**Mục đích:** Hoàn tất đánh giá: nhập đánh giá của quản lý, điểm tổng và kế hoạch cải thiện. Chuyển trạng thái sang Completed.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `reviewId` | Guid | Có | Phải trùng với `id` trong URL |
| `managerAssessment` | string | Có | Đánh giá của quản lý |
| `overallScore` | decimal | Có | Điểm tổng (vd: thang 1–10 hoặc %) |
| `improvementPlan` | string | Không | Kế hoạch cải thiện / đào tạo |

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/performance/reviews/{employeeId}`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy danh sách phiên đánh giá hiệu suất của một nhân viên.

**Path:** `employeeId` (Guid).

**Response (200):** `{ isSuccess, data: [ reviews ], message }`

---

## 15. Payroll

**Mục đích:** Cấu hình lương (lương cơ bản, phụ cấp, khấu trừ), tính lương theo chấm công và nghỉ phép, duyệt và xem bảng lương.

### POST `/api/payroll/salary`

**Quyền:** Admin, Manager  
**Mục đích:** Thiết lập/cập nhật cấu trúc lương cho nhân viên (lương cơ bản, phụ cấp ăn/đi lại/ca đêm, khấu trừ BH/thuế, hiệu lực từ ngày).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên |
| `baseSalary` | decimal | Có | Lương cơ bản (tháng) |
| `mealAllowance` | decimal | Có | Phụ cấp ăn |
| `transportAllowance` | decimal | Có | Phụ cấp đi lại |
| `nightShiftAllowance` | decimal | Có | Phụ cấp ca đêm |
| `insuranceDeduction` | decimal | Có | Khấu trừ bảo hiểm |
| `taxDeduction` | decimal | Có | Khấu trừ thuế |
| `effectiveFrom` | string (date) | Có | Ngày hiệu lực |

**Response (200):** `{ isSuccess, data: <salaryStructureId (Guid)>, message }`

---

### GET `/api/payroll/salary/{employeeId}`

**Quyền:** Đã đăng nhập (nhân viên chỉ xem của mình)  
**Mục đích:** Xem cấu trúc lương hiện tại của một nhân viên.

**Path:** `employeeId` (Guid).

**Response (200):** `{ isSuccess, data: { ... }, message }`

---

### POST `/api/payroll/calculate`

**Quyền:** Admin, Manager  
**Mục đích:** Tính bảng lương cho một nhân viên trong tháng (dựa trên chấm công, giờ làm thêm, nghỉ không lương). Tạo bản ghi payroll; không tính trùng tháng.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên |
| `year` | int | Có | Năm (vd: 2025) |
| `month` | int | Có | Tháng (1–12) |

**Response (200):** `{ isSuccess, data: <payrollRecordId (Guid)>, message }`

---

### POST `/api/payroll/{id}/approve`

**Quyền:** Admin  
**Mục đích:** Duyệt bảng lương (đánh dấu đã duyệt, có thể dùng để khóa chỉnh sửa hoặc cho phép thanh toán).

**Path:** `id` (Guid) – ID bản ghi payroll.

**Response (200):** `{ isSuccess, data, message }`

---

### GET `/api/payroll/records/{employeeId}`

**Quyền:** Đã đăng nhập (nhân viên chỉ xem của mình)  
**Mục đích:** Lấy lịch sử bảng lương của nhân viên (có thể lọc theo năm).

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `employeeId` | Guid (path) | ID nhân viên |
| `year` | int? | Năm; mặc định năm hiện tại |

**Response (200):** `{ isSuccess, data: [ records ], message }`

---

## 16. Leaves

**Mục đích:** Loại nghỉ phép, cấu hình số ngày nghỉ theo năm, gửi yêu cầu nghỉ, duyệt/từ chối, xem số dư và lịch sử. Tích hợp với ca làm (cảnh báo xung đột).

### GET `/api/leaves/types`

**Quyền:** Đã đăng nhập  
**Mục đích:** Lấy danh sách loại nghỉ phép (phép năm, ốm, thai sản, không lương…) để dùng khi gửi yêu cầu hoặc cấu hình số ngày.

**Response (200):** `{ isSuccess, data: [ leaveTypes ], message }`

---

### POST `/api/leaves/types`

**Quyền:** Admin  
**Mục đích:** Tạo loại nghỉ phép mới (tên, số ngày mặc định/năm, có yêu cầu giấy tờ không).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `name` | string | Có | Tên loại (vd: "Annual Leave", "Sick Leave") |
| `description` | string | Không | Mô tả |
| `defaultDaysPerYear` | int | Có | Số ngày mặc định mỗi năm |
| `requiresDocumentation` | bool | Có | Có bắt buộc đính kèm giấy tờ không |

**Response (200):** `{ isSuccess, data: <leaveTypeId (Guid)>, message }`

---

### POST `/api/leaves/balance`

**Quyền:** Admin, Manager  
**Mục đích:** Cấu hình số ngày nghỉ (tổng) cho một nhân viên theo loại nghỉ và năm (có thể theo thâm niên/hợp đồng).

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `employeeId` | Guid | Có | ID nhân viên |
| `leaveTypeId` | Guid | Có | ID loại nghỉ |
| `year` | int | Có | Năm (vd: 2025) |
| `totalDays` | decimal | Có | Tổng số ngày được nghỉ trong năm |

**Response (200):** `{ isSuccess, data: <balanceId (Guid)>, message }`

---

### GET `/api/leaves/balance/{employeeId}`

**Quyền:** Đã đăng nhập (nhân viên chỉ xem của mình)  
**Mục đích:** Xem số dư nghỉ phép (theo loại) của nhân viên trong năm.

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `employeeId` | Guid (path) | ID nhân viên |
| `year` | int? | Năm; mặc định năm hiện tại |

**Response (200):** `{ isSuccess, data: [ balances ], message }`

---

### POST `/api/leaves/requests`

**Quyền:** Đã đăng nhập (nhân viên gửi cho chính mình)  
**Mục đích:** Gửi yêu cầu nghỉ phép. Hệ thống kiểm tra số dư, trùng ca làm và trùng ngày; thông báo cho quản lý.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `leaveTypeId` | Guid | Có | ID loại nghỉ |
| `startDate` | string (date) | Có | Ngày bắt đầu nghỉ |
| `endDate` | string (date) | Có | Ngày kết thúc nghỉ (>= startDate) |
| `reason` | string | Không | Lý do nghỉ |

**Response (200):** `{ isSuccess, data: <leaveRequestId (Guid)>, message }`

---

### POST `/api/leaves/requests/{id}/review`

**Quyền:** Admin, Manager  
**Mục đích:** Duyệt hoặc từ chối yêu cầu nghỉ phép. Khi duyệt, trừ số ngày vào balance và có thể cập nhật lịch; gửi thông báo cho nhân viên.

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `leaveRequestId` | Guid | Có | Phải trùng với `id` trong URL |
| `approve` | bool | Có | true = duyệt, false = từ chối |
| `comment` | string | Không | Nhận xét (đặc biệt khi từ chối) |

**Response (200):** `{ isSuccess, data: true, message }`

---

### GET `/api/leaves/requests`

**Quyền:** Đã đăng nhập  
**Mục đích:** Danh sách yêu cầu nghỉ phép có phân trang, lọc theo nhân viên, trạng thái, khoảng ngày.

| Query | Kiểu | Mô tả |
|-------|------|--------|
| `employeeId` | Guid? | Lọc theo nhân viên |
| `status` | int? (LeaveRequestStatus) | 0=Pending, 1=Approved, 2=Rejected, 3=Cancelled |
| `startDate` | DateTime? | Từ ngày |
| `endDate` | DateTime? | Đến ngày |
| `pageNumber` | int | Mặc định 1 |
| `pageSize` | int | Mặc định 10 |

**Response (200):** `{ isSuccess, data: { items, totalCount, ... }, message }`

---

### GET `/api/leaves/requests/pending`

**Quyền:** Admin, Manager  
**Mục đích:** Lấy danh sách yêu cầu nghỉ phép đang chờ duyệt (để quản lý xử lý nhanh).

**Response (200):** `{ isSuccess, data: [ requests ], message }`

---

## 17. Enums tham chiếu

Dùng trong request/response dưới dạng **số nguyên (int)**.

| Enum | Giá trị | Mô tả |
|------|---------|--------|
| **UserRole** | 0=Staff, 1=Manager, 2=Admin | Vai trò người dùng |
| **Gender** | 0=Male, 1=Female, 2=Other | Giới tính |
| **MaritalStatus** | 0=Single, 1=Married, 2=Divorced, 3=Widowed | Tình trạng hôn nhân |
| **TaskPriority** | 0=Low, 1=Medium, 2=High, 3=Urgent | Độ ưu tiên nhiệm vụ |
| **TaskItemStatus** | 0=Pending, 1=InProgress, 2=Completed, 3=Cancelled | Trạng thái nhiệm vụ |
| **ZoneStatus** | 0=Available, 1=InService, 2=NeedsCleaning, 3=NeedsSupport, 4=Full, 5=Closed | Trạng thái khu vực |
| **AttendanceStatus** | 0=OnTime, 1=Late, 2=EarlyLeave, 3=Overtime, 4=Absent | Trạng thái chấm công |
| **NotificationType** | 0=General, 1=ShiftChange, 2=TaskAssignment, 3=Urgent, 4=System, 5=LeaveRequest, 6=LeaveApproval, 7=PerformanceReview, 8=TrainingAssignment, 9=TrainingCompletion, 10=OnboardingTask, 11=IdCardIssued, 12=PayrollReady | Loại thông báo |
| **DocumentCategory** | 0=Contract, 1=Certificate, 2=IdDocument, 3=Photo, 4=PaySlip, 5=Other | Danh mục tài liệu |
| **LeaveRequestStatus** | 0=Pending, 1=Approved, 2=Rejected, 3=Cancelled | Trạng thái yêu cầu nghỉ phép |
| **ReviewType** | 0=Quarterly, 1=Annual | Loại đánh giá (quý/năm) |
| **ReviewStatus** | Draft, SelfAssessment, ManagerReview, Completed | Trạng thái phiên đánh giá (nội bộ) |
| **GoalStatus** | 0=NotStarted, 1=InProgress, 2=Achieved, 3=NotAchieved | Trạng thái mục tiêu KPI |

---

**Ghi chú chung:**

- **Guid:** định dạng `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` (lowercase hoặc uppercase đều được).
- **Date:** ISO 8601 date, ví dụ `"2025-03-08"`.
- **DateTime:** ISO 8601 date-time, ví dụ `"2025-03-08T14:30:00Z"`.
- **TimeSpan:** chuỗi giờ:phút:giây, ví dụ `"06:00:00"`, `"14:00:00"`.
- Response chuẩn: `{ "isSuccess": true|false, "data": ..., "message": "..." }`. Khi lỗi, `data` có thể null và `message` chứa mô tả lỗi.
