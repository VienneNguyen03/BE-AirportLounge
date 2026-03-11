# Tài liệu Phân tích Nghiệp vụ (Business Flow)
## Hệ thống Quản lý Nội bộ Nhân viên Lounge Sân bay

Tài liệu này trình bày chi tiết về **Luồng nghiệp vụ (Business Flow)** của 4 module quản trị nhân sự cốt lõi, được điều chỉnh đặc thù cho môi trường quản lý vận hành tại các **Phòng chờ Sân bay (Airport Lounge)**.

---

## 1. Module Leave Management (Quản lý Nghỉ phép)

**Vấn đề giải quyết:** Tại phòng chờ sân bay, nhân sự phục vụ (Lễ tân, Phục vụ ẩm thực, Tạp vụ) luôn phải được đảm bảo đủ số lượng dựa trên lịch bay hằng ngày. Nếu nhân viên nghỉ phép không có kế hoạch sẽ dẫn đến vỡ ca trực.

**Luồng nghiệp vụ (Business Flow):**
1. **Thiết lập & Cấp phát (Balance Allocation):** Đầu năm, HR/Admin cấp số quỹ ngày nghỉ phép (phép năm, phép ốm, v.v.) cho từng nhân viên dựa trên cấp bậc và thâm niên.
2. **Xin nghỉ (Requesting):** Nhân viên tiến hành tạo yêu cầu (`Draft` → `Submitted`). *Hệ thống tự động kiểm tra xem ngày nghỉ dự kiến có nằm trong lịch đã được phân công ca trực (Shift) hay không để bật cảnh báo.*
3. **Phê duyệt (Approval Workflow):** Quản lý Lounge nhận yêu cầu (`UnderReview`). Dựa trên tình hình nhân sự của ngày nghỉ đó, quản lý có thể yêu cầu bổ sung thông tin chứng từ (`NeedsInfo`), từ chối (`Rejected`), hoặc chấp thuận (`Approved`).
4. **Cập nhật & Đồng bộ:** Hệ thống tự động khấu trừ quỹ nghỉ phép (Leave Balance). Khi đến ngày nghỉ thực tế, trạng thái yêu cầu chuyển thành `Taken`. Nhân sự làm lịch ca cũng sẽ nhận được cảnh báo nếu xếp ca trùng vào ngày nhân viên đã được duyệt nghỉ.

---

## 2. Module ID Cards (Quản lý Thẻ An ninh / Thẻ định danh)

**Vấn đề giải quyết:** Sân bay là khu vực có chuẩn mực an ninh cực kỳ khắt khe. Việc một nhân viên thất lạc thẻ có thể dẫn đến rủi ro nghiêm trọng về an toàn. Do đó, việc quản lý chặt chẽ vòng đời thẻ là yêu cầu bắt buộc.

**Luồng nghiệp vụ (Business Flow):**
1. **Cấp phát (Issuance):** Khi nhân viên vượt qua giai đoạn thử việc hoặc cần thẻ để ra vào, hệ thống tạo bản ghi cấp thẻ và tự động sinh mã QR/Barcode (`Issued`).
2. **Kích hoạt (Activation):** Khi thẻ vật lý được in và giao tận tay nhân viên, quản lý kích hoạt trên hệ thống (`Active`). Từ đây, thẻ chính thức có hiệu lực dùng để check-in/chấm công.
3. **Sự cố & Báo mất (Incident Reporting):** Nếu nhân viên làm mất thẻ hoặc thẻ bị hỏng, họ phải lập tức báo cáo trên hệ thống. Trạng thái thẻ chuyển sang `Lost` hoặc `Damaged`.
4. **Xử lý sự cố (Resolution):** Giao diện quản lý hiển thị cảnh báo đỏ (Red flag). Admin thực hiện thu hồi quyền truy cập của thẻ cũ (`Revoked`) để ngăn chặn việc sử dụng thẻ trái phép.
5. **Cấp lại (Reissuing):** Nhân viên tạo yêu cầu cấp lại thẻ (`ReissueRequested`). Quản lý duyệt và tiến hành in thẻ mới (`Reissued`), liên kết thẻ mới với hồ sơ nhân viên.

---

## 3. Module Onboarding / Offboarding (Tiếp nhận & Nghỉ việc)

**Vấn đề giải quyết:** Dịch vụ Lounge đòi hỏi tiêu chuẩn chất lượng cao. Nhân viên mới cần quy trình đào tạo chuẩn y khoa, an toàn thực phẩm. Khi nhân viên nghỉ việc, toàn bộ tài sản và quyền truy cập (thẻ, tài khoản phần mềm) phải bị thu hồi ngay lập tức để bảo mật thông tin.

### Luồng nghiệp vụ Onboarding (Tiếp nhận):
1. **Khởi tạo:** Khi có nhân viên mới, tạo quy trình Onboarding (`Created`) và phân công một người hướng dẫn (`AssignedMentorId`).
2. **Thực hiện (`InProgress`):** Nhân viên mới cần hoàn thành một danh sách checklist bắt buộc (vd: Nộp hồ sơ y tế, Học quy trình an toàn bay, Nhận đồng phục).
3. **Giải quyết vướng mắc (`Blocked`):** Quá trình có thể bị `Blocked` nếu nhân viên thiếu giấy tờ quan trọng (vd: giấy khám sức khỏe). Quy trình chỉ được tiếp tục (`Unblocked`) khi bổ sung đủ.
4. **Hoàn tất (`Activated`):** Cả Mentor và Admin xác nhận checklist đã hoàn tất (`Completed`), nhân viên chính thức hoàn thành quá trình tiếp nhận và sẵn sàng được phân ca làm việc độc lập.

### Luồng nghiệp vụ Offboarding (Nghỉ việc):
1. **Đệ đơn:** Ghi nhận thông tin nhân viên xin nghỉ, xác định ngày làm việc cuối cùng.
2. **Quy trình thu hồi (Asset/Access Recovery):** Hệ thống yêu cầu tích đủ các bước nghiêm ngặt: 
   - Hoàn thành khảo sát nghỉ việc (Exit Survey).
   - Bàn giao công cụ dụng cụ, đồng phục.
   - Thu hồi Thẻ ID Card Sân bay (tích hợp gọi sang API của module ID Cards).
   - Khóa quyền truy cập phần mềm.
3. **Hoàn tất (`Completed`):** Chỉ khi toàn bộ các bước bàn giao đều hoàn thành (bao gồm cả quyết toán lương cuối cùng), hồ sơ nhân viên mới được đóng lại một cách an toàn.

---

## 4. Module Performance Review (Đánh giá Năng lực quá trình 9 Bước)

**Vấn đề giải quyết:** Đánh giá công bằng chất lượng phục vụ tại Lounge là việc rất khó nếu chỉ đánh giá 1 chiều từ quản lý. Cần có sự tham gia của đồng nghiệp làm cùng ca để đánh giá khách quan về khả năng teamwork và thái độ làm việc.

**Luồng nghiệp vụ (Business Flow):**
1. **Khởi động (`NotStarted`):** Quản lý hoặc HR tạo các đợt đánh giá định kỳ (Quý/Năm) cho nhân sự.
2. **Tự Đánh giá (`SelfSubmitted`):** Nhân viên tự đánh giá mức độ hoàn thành Mục tiêu KPIs và thái độ làm việc của bản thân trong kỳ.
3. **Đánh giá Chéo (`PeerReviewOpen`):** Các đồng nghiệp làm cùng ca (vd: phụ trách khu vực buffet, thu dọn) tiến hành đưa ra các phản hồi, đánh giá cho nhau. Chế độ ẩn danh (`Anonymous`) được hỗ trợ để đảm bảo tính khách quan. Sau hạn chót, chuyển sang `PeerReviewDone`.
4. **Quản lý Đánh giá (`ManagerSubmitted`):** Quản lý tham khảo điểm tự đánh giá, các phản hồi ẩn danh từ đồng nghiệp, kết hợp hiệu suất công việc thực tế để đưa ra đánh giá, nhận xét và điểm số tổng hợp.
5. **Hiệu chỉnh (`Calibration`):** Quản lý cấp cao của nhiều Lounge (quốc nội, quốc tế) họp bàn để chuẩn hóa điểm số, đảm bảo tính công bằng xuyên suốt, tránh việc có quản lý chấm quá thoáng hoặc quá gắt.
6. **Chốt điểm (`Finalized`):** Điểm đánh giá được chốt, lưu trữ vào hồ sơ. Kết quả được sử dụng cho việc xét duyệt tăng lương, thưởng, hoặc lập Kế hoạch cải thiện hiệu suất (`ImprovementPlan`).

---

## Mối liên hệ sinh thái (Ecosystem Integration)

Bốn module này không hoạt động độc lập mà bù trừ, liên kết chặt chẽ vòng đời của nhân viên (Employee Lifecycle):

Nhân sự vào làm, chạy quy trình **Onboarding** ➡️ Hoàn tất sẽ được cấp **ID Card** để ra vào sân bay ➡️ Trong quá trình làm việc, phát sinh các nhu cầu xin nghỉ **Leave Request** ➡️ Cuối năm/quý dùng dữ liệu quá trình để **Performance Review** ➡️ Khi hết gắn bó, tiến hành quy trình **Offboarding**, trong đó bắt buộc gọi API để **Revoke ID Card**.
