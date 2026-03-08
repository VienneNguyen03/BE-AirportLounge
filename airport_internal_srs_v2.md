# **TÀI LIỆU ĐẶC TẢ YÊU CẦU PHẦN MỀM (SRS)**

## **Hệ thống Quản lý Nội bộ Nhân viên Lounge Sân bay –** 

## **Phiên bản 2**

### **1\. Giới thiệu**

\#\#\#\# 1.1 Mục đích

Tài liệu SRS phiên bản 2 mô tả chi tiết phạm vi và các yêu cầu cho hệ thống quản lý nội bộ nhân viên trong lounge sân bay.  So với phiên bản 1, tài liệu này **mở rộng các tính năng về quản lý hồ sơ cá nhân, theo dõi nghỉ phép, đánh giá hiệu suất, đào tạo, quá trình onboarding/offboarding và quản lý thẻ ID**.  Các yêu cầu được trình bày nhằm giúp đội phát triển, kiểm thử, quản lý dự án và các bên liên quan hiểu rõ mục tiêu và kiểm soát quá trình triển khai.

\#\#\#\# 1.2 Phạm vi

Hệ thống phục vụ cho các phòng chờ VIP tại sân bay và các hãng hàng không.  Ngoài các chức năng cốt lõi (quản lý nhân viên, ca làm, chấm công, nhiệm vụ và khu vực), phiên bản 2 còn bao gồm:

* Lưu trữ **thông tin cá nhân và hồ sơ đầy đủ** của nhân viên (địa chỉ, số căn cước công dân/hộ chiếu, ngày sinh, giới tính, trạng thái hôn nhân, liên hệ khẩn cấp…).  Các hệ thống EMS hiện đại cung cấp cơ sở dữ liệu trung tâm giúp dễ dàng thêm và cập nhật hồ sơ nhân viên .

* **Quản lý nghỉ phép**: cho phép nhân viên gửi yêu cầu nghỉ phép theo nhiều loại (nghỉ ốm, nghỉ phép năm, làm việc từ xa…) và quản lý phê duyệt; đồng thời theo dõi số ngày nghỉ đã dùng và còn lại .

* **Đánh giá hiệu suất và đào tạo**: thiết lập mục tiêu/KPI, thu thập phản hồi, thực hiện đánh giá 360°, quản lý các khóa đào tạo và ghi nhận tiến độ học tập .

* **Onboarding & offboarding**: hỗ trợ quá trình thu thập thông tin ứng viên mới, tạo nhiệm vụ giới thiệu và xử lý thủ tục nghỉ việc .

* **Quản lý thẻ ID nhân viên**: tự động tạo thẻ ID dựa trên hồ sơ nhân viên, cho phép in hàng loạt và thiết kế tùy chỉnh .

* **Cổng tự phục vụ và ứng dụng di động**: cho phép nhân viên tự quản lý thông tin cá nhân, yêu cầu nghỉ phép, xem ca làm, theo dõi nhiệm vụ qua web hoặc ứng dụng di động .

* **Phân tích nhân sự và báo cáo**: cung cấp số liệu về thời gian làm việc, tình trạng nghỉ phép, hiệu suất, đa dạng nhân sự để hỗ trợ quyết định .

\#\#\#\# 1.3 Đối tượng và vai trò

Các vai trò trong hệ thống vẫn được giữ nguyên (Admin, Manager, Staff) nhưng phạm vi trách nhiệm được mở rộng:

* **Admin**: cấu hình hệ thống, quản lý người dùng, phân quyền, khai báo loại nghỉ phép, quản lý danh mục đào tạo, cấu hình quy trình onboarding/offboarding và thiết lập mẫu thẻ ID.

* **Quản lý (Manager)**: quản lý hồ sơ nhân viên, tạo/duyệt yêu cầu nghỉ phép, đánh giá hiệu suất, phân công ca làm và nhiệm vụ, theo dõi đào tạo và phê duyệt hoàn thành khóa học.

* **Nhân viên (Staff)**: cập nhật thông tin cá nhân, xem và xác nhận lịch làm việc, chấm công, gửi yêu cầu nghỉ phép, tham gia đánh giá/đào tạo, nhận và hoàn thành nhiệm vụ.

\#\#\#\# 1.4 Thuật ngữ và viết tắt

Ngoài các thuật ngữ đã có ở phiên bản 1 (ca làm, chấm công, task, dashboard, VIP lounge), phiên bản 2 bổ sung:

* **CCCD**: Căn cước công dân – mã định danh cá nhân tại Việt Nam.

* **KPI**: Key Performance Indicator – chỉ số đo lường hiệu suất.

* **ID Card**: Thẻ nhận dạng nhân viên (có mã nhân viên, ảnh, thông tin cơ bản).

* **Onboarding**: Quy trình giới thiệu và hướng dẫn nhân viên mới.

* **Offboarding**: Quy trình hoàn tất thủ tục khi nhân viên thôi việc.

* **Leave Type**: Loại nghỉ phép (nghỉ phép năm, nghỉ ốm, nghỉ thai sản, v.v.).

\#\#\# 2\. Tổng quan hệ thống

\#\#\#\# 2.1 Mục tiêu hệ thống

Hệ thống hướng tới việc số hoá toàn bộ quy trình nội bộ của lounge sân bay, bao gồm quản trị nhân sự, điều phối ca làm, chấm công, quản lý nghỉ phép, đánh giá hiệu suất, đào tạo và phục vụ khách hàng VIP.  Mục tiêu cụ thể:

* Cung cấp **cơ sở dữ liệu nhân viên trung tâm**, lưu trữ an toàn các thông tin cá nhân, hồ sơ giấy tờ và lịch sử công tác .

* Tích hợp **quản lý nghỉ phép** với thời gian làm việc và ca làm nhằm tránh xung đột lịch và đảm bảo đủ nhân lực .

* Hỗ trợ **đánh giá hiệu suất toàn diện** thông qua mục tiêu, phản hồi và phân tích dữ liệu để nâng cao chất lượng dịch vụ .

* Xây dựng **cổng tự phục vụ** thân thiện, giúp nhân viên chủ động quản lý thông tin, lịch làm việc và các yêu cầu, đồng thời tối ưu hóa quy trình phê duyệt của quản lý .

* Tạo nền tảng **phân tích nhân sự** nhằm cung cấp các báo cáo về sự vắng mặt, hiệu suất, đào tạo, mức độ đa dạng và các chỉ số vận hành .

\#\#\#\# 2.2 Mô tả tổng quan hệ thống

Hệ thống vẫn áp dụng mô hình **monolithic** với phân tầng rõ ràng (presentation, service, repository) để giảm độ phức tạp và triển khai nhanh MVP.  Các phân hệ được mở rộng như sau:

* **Quản lý hồ sơ nhân viên**: ngoài các trường cơ bản, hệ thống lưu trữ ngày sinh, giới tính, địa chỉ thường trú, địa chỉ tạm trú, quốc tịch, số CCCD/hộ chiếu, mã số thuế, tình trạng hôn nhân, thông tin liên hệ khẩn cấp, tài khoản ngân hàng và tải lên tài liệu liên quan (ảnh chân dung, bản scan CCCD, chứng chỉ).  Nhân viên có thể cập nhật một số thông tin cá nhân, trong khi quản lý và admin có quyền chỉnh sửa toàn bộ hồ sơ .

* **Quản lý ca làm, chấm công và nhiệm vụ**: kế thừa từ phiên bản 1, với các cải tiến để liên kết dữ liệu nghỉ phép và thời gian làm việc, cảnh báo nếu nhân viên đã xin nghỉ trong ca đó.

* **Quản lý nghỉ phép**: bao gồm định nghĩa loại nghỉ phép, cấp phát số ngày nghỉ, quy trình gửi yêu cầu và phê duyệt, theo dõi số dư ngày nghỉ và tích hợp với lịch làm việc.  Tính năng này dựa trên hệ thống quản lý nghỉ phép phổ biến cho phép tùy chỉnh loại nghỉ (ốm, thai sản, làm việc từ xa…) và hạn mức .

* **Quản lý lương và phúc lợi**: cho phép ghi nhận mức lương cơ bản, phụ cấp, thưởng và các khoản khấu trừ; xuất báo cáo trả lương; hỗ trợ nhân viên xem bảng lương và lịch sử thanh toán .

* **Đánh giá hiệu suất & đào tạo**: mô-đun quản lý mục tiêu và KPI, lập kế hoạch đánh giá (hàng quý/năm), cho phép tự đánh giá, đánh giá bởi quản lý và đồng nghiệp, lưu trữ kết quả và đưa ra khuyến nghị đào tạo; quản lý các khóa học, cấp chứng nhận và theo dõi tiến độ .

* **Onboarding & offboarding**: quy trình tự động thu thập thông tin khi nhân viên gia nhập (thông tin cá nhân, kinh nghiệm, bằng cấp, tài khoản ngân hàng, thuế) và gán checklist nhiệm vụ; quy trình offboarding bao gồm yêu cầu nghỉ việc, khảo sát, hoàn trả tài sản, và khóa truy cập hệ thống .

* **ID Card Management**: module sinh thẻ nhân viên tự động dựa trên hồ sơ, hỗ trợ mẫu thẻ tùy chỉnh theo bộ phận/loại nhân viên, thêm ảnh, mã nhân viên, phòng ban và các thông tin như nhóm máu; cho phép in hàng loạt và quản lý lịch sử in thẻ .

* **Cổng tự phục vụ & ứng dụng di động**: nhân viên có thể đăng nhập trên web hoặc ứng dụng mobile để cập nhật hồ sơ, yêu cầu nghỉ phép, xem lương, học tập và đánh giá.  Khả năng truy cập di động giúp nhân viên thao tác mọi lúc mọi nơi .

* **Báo cáo & phân tích**: hệ thống cung cấp báo cáo về chấm công, nghỉ phép, hiệu suất, đào tạo và chi phí nhân sự; hỗ trợ xuất file CSV/PDF và biểu đồ trực quan; sử dụng dữ liệu để đưa ra quyết định nhân sự .

\#\#\# 3\. Các yêu cầu chức năng

Danh sách dưới đây gồm các chức năng chính.  Các user story và tiêu chí chấp nhận được trình bày để đảm bảo mỗi tính năng được hiểu rõ và xác nhận đúng.

\#\#\#\# 3.1 Đăng nhập, phân quyền và bảo mật

Giống phiên bản 1: hệ thống yêu cầu xác thực, phân quyền theo vai trò (admin/manager/staff) và bảo vệ dữ liệu nhạy cảm.  Ngoài ra, hệ thống cần áp dụng kiểm soát truy cập chi tiết (role‑based/attribute‑based access control) đối với thông tin nhạy cảm như hồ sơ nhân viên, lương, đánh giá hiệu suất .

\#\#\#\# 3.2 Quản lý hồ sơ nhân viên

**Mô tả:** Lưu trữ và cập nhật toàn bộ thông tin cá nhân và nghề nghiệp của nhân viên.

**User stories:**

* *US04*: Là **quản lý**, tôi muốn thêm nhân viên mới và nhập đầy đủ thông tin cá nhân (tên, ngày sinh, giới tính, địa chỉ thường trú, địa chỉ tạm trú, CCCD/hộ chiếu, quốc tịch, số điện thoại, email, tài khoản ngân hàng, mã số thuế, ngày vào làm) để thiết lập hồ sơ chuẩn . .

* *US05*: Là **quản lý**, tôi muốn cập nhật hoặc đính kèm hồ sơ giấy tờ như ảnh chân dung, scan CCCD, chứng nhận đào tạo để dữ liệu luôn đầy đủ và chính xác.

* *US06*: Là **admin**, tôi muốn tạo và quản lý mẫu thẻ ID, tự động sinh mã nhân viên và mã QR cho mỗi hồ sơ để tiện kiểm soát ra/vào .

* *US07*: Là **nhân viên**, tôi muốn xem và cập nhật thông tin liên hệ, địa chỉ và tài khoản ngân hàng của mình thông qua cổng tự phục vụ.

**Tiêu chí chấp nhận:**

1. Trường thông tin bắt buộc: mã nhân viên, họ tên, ngày sinh, giới tính, CCCD/hộ chiếu, quốc tịch, bộ phận, vị trí công việc, ngày vào làm, trạng thái làm việc.

2. Các trường mở rộng như địa chỉ, mã số thuế, tình trạng hôn nhân, người liên hệ khẩn cấp phải có định dạng và kiểm tra hợp lệ.

3. Hệ thống cho phép tải lên và quản lý file đính kèm (ảnh, tài liệu) liên kết với hồ sơ.

4. Mỗi nhân viên có thể sở hữu một hoặc nhiều thẻ ID; lịch sử phát hành thẻ được lưu và chỉ admin có quyền tái phát hành .

5. Mọi chỉnh sửa hồ sơ đều lưu log (người thực hiện, thời gian, nội dung thay đổi).

\#\#\#\# 3.3 Quản lý ca làm (Shift Scheduling)

Như phiên bản 1, với bổ sung:

* Khi gán ca làm, hệ thống kiểm tra xem nhân viên có yêu cầu nghỉ phép hay không; nếu có, thông báo xung đột và gợi ý nhân viên khác.

* Phân hệ ca làm liên kết với modul lương để tự động tính thêm phụ cấp ca đêm hoặc làm thêm.

\#\#\#\# 3.4 Chấm công và theo dõi thời gian làm việc

Giữ nguyên yêu cầu phiên bản 1, bổ sung:

* Cho phép ghi nhận nhiều lần ra/vào trong cùng ca (ví dụ nhân viên ra ngoài trong ca) thông qua bảng log sự kiện; hệ thống tự tính tổng thời gian làm việc .

* Kết nối dữ liệu chấm công với module lương để tính số giờ làm thêm và phụ cấp.

\#\#\#\# 3.5 Dashboard nhân viên và quản lý

Giữ nguyên yêu cầu cũ; bổ sung widgets hiển thị số ngày nghỉ còn lại, tiến độ hoàn thành khóa đào tạo và kết quả đánh giá gần nhất.

\#\#\#\# 3.6 Quản lý khu vực lounge

Giữ nguyên nội dung phiên bản 1\.

\#\#\#\# 3.7 Quản lý nhiệm vụ (Task Management)

Giữ nguyên nội dung phiên bản 1; bổ sung khả năng liên kết nhiệm vụ với yêu cầu đào tạo hoặc mục tiêu hiệu suất (ví dụ: nhiệm vụ thực hành sau khóa học).

\#\#\#\# 3.8 Thông báo nội bộ

Bổ sung thêm thông báo về yêu cầu nghỉ phép (được phê duyệt/từ chối), lịch đánh giá hiệu suất sắp tới, hoàn thành khóa học và phát hành thẻ ID mới.  Người dùng có thể chọn nhận thông báo qua email, ứng dụng mobile hoặc SMS.

\#\#\#\# 3.9 Quản lý nghỉ phép (Time Off Management)

**Mô tả:** Cung cấp quy trình đầy đủ để nhân viên yêu cầu nghỉ phép và quản lý phê duyệt.  Hệ thống hỗ trợ nhiều loại nghỉ và hạn mức linh hoạt.

**User stories:**

* *US29*: Là **nhân viên**, tôi muốn gửi yêu cầu nghỉ phép với thông tin loại nghỉ (phép năm, ốm, thai sản, làm việc từ xa…), ngày bắt đầu, ngày kết thúc và lý do để chờ phê duyệt .

* *US30*: Là **quản lý**, tôi muốn xem danh sách yêu cầu nghỉ phép của đội nhóm, phê duyệt hoặc từ chối và kèm nhận xét để đảm bảo công việc không bị gián đoạn .

* *US31*: Là **admin**, tôi muốn cấu hình danh sách loại nghỉ, số ngày nghỉ mặc định cho từng loại và cho từng nhân viên (theo thâm niên, hợp đồng).  Có thể đặt giới hạn tối đa cho mỗi loại nghỉ .

* *US32*: Là **nhân viên**, tôi muốn xem lịch sử nghỉ phép và số ngày nghỉ còn lại cũng như xem lịch nghỉ của đồng nghiệp để chủ động sắp xếp .

* *US33*: Là **hệ thống**, tôi muốn tự động tính toán và trừ số ngày nghỉ khi yêu cầu được phê duyệt; gửi email/thông báo tới nhân viên và quản lý .

**Tiêu chí chấp nhận:**

1. Phân hệ cho phép khai báo nhiều loại nghỉ với tham số: tên, mô tả, số ngày tối đa, yêu cầu giấy tờ chứng minh (nếu có).

2. Khi nhân viên gửi yêu cầu, hệ thống phải kiểm tra xung đột với ca làm và hiển thị cảnh báo; sau khi gửi, yêu cầu ở trạng thái “Pending”.

3. Quản lý được thông báo khi có yêu cầu mới; có thể phê duyệt (Approved), từ chối (Rejected) hoặc yêu cầu bổ sung thông tin; mọi quyết định được lưu log.

4. Khi phê duyệt, hệ thống cập nhật bảng nghỉ phép và lịch làm việc; khi từ chối, nhân viên nhận thông báo với lý do.

5. Phân hệ hỗ trợ xem lịch nghỉ dưới dạng lịch tuần/tháng, có thể lọc theo nhân viên hoặc bộ phận; cho phép xuất dữ liệu ra CSV/PDF.

6. Hệ thống hỗ trợ cấu hình **tăng ngày nghỉ theo thâm niên**, nghỉ theo phần trăm công việc, và **pro‑rated** khi nhân viên vào/ra giữa chừng .

\#\#\#\# 3.10 Quản lý lương và phúc lợi (Payroll & Compensation)

**Mô tả:** Lưu trữ thông tin lương cơ bản, phụ cấp, thưởng, khấu trừ và hỗ trợ tạo bảng lương hàng kỳ.  Đây là tính năng tùy chọn có thể được triển khai trong giai đoạn sau để cung cấp thông tin lương cho nhân viên và hỗ trợ phòng tài chính.

**User stories:**

* *US34*: Là **admin/quản lý**, tôi muốn khai báo mức lương cơ bản, phụ cấp (ăn trưa, ca đêm), thưởng và khấu trừ (bảo hiểm, thuế) của từng nhân viên để tính lương chính xác .

* *US35*: Là **nhân viên**, tôi muốn xem bảng lương và các khoản thanh toán (thời gian làm thêm, thưởng) thông qua cổng tự phục vụ để minh bạch .

* *US36*: Là **hệ thống**, tôi muốn tự động tính lương dựa trên dữ liệu chấm công và nghỉ phép; xuất bảng lương và lưu lịch sử.

**Tiêu chí chấp nhận:**

1. Cho phép khai báo cấu trúc lương và các khoản bổ sung/khấu trừ; hỗ trợ nhiều đồng tiền nếu cần.

2. Tự động tổng hợp số giờ làm việc, giờ làm thêm và số ngày nghỉ không hưởng lương để tính bảng lương.

3. Nhân viên chỉ xem được thông tin lương của mình thông qua cổng tự phục vụ; admin và bộ phận tài chính xem được tất cả.

4. Hệ thống xuất bảng lương ra PDF/Excel và hỗ trợ gửi email thông báo lương.

\#\#\#\# 3.11 Đánh giá hiệu suất và quản lý đào tạo

**Mô tả:** Hỗ trợ quản lý thiết lập mục tiêu công việc, theo dõi KPI, thu thập phản hồi và đánh giá định kỳ; cung cấp hệ thống quản lý khóa học và đào tạo cho nhân viên.

**User stories:**

* *US37*: Là **quản lý**, tôi muốn thiết lập mục tiêu và KPI cho mỗi nhân viên (ví dụ: số lượng nhiệm vụ hoàn thành, mức độ hài lòng của khách hàng) để đánh giá công bằng .

* *US38*: Là **nhân viên**, tôi muốn tự đánh giá và nhận phản hồi từ quản lý hoặc đồng nghiệp (360° review) để cải thiện hiệu suất .

* *US39*: Là **quản lý**, tôi muốn tạo các phiên đánh giá theo quý/năm, lưu kết quả và đưa ra kế hoạch cải thiện hoặc đề xuất thưởng/phạt.

* *US40*: Là **admin**, tôi muốn xây dựng thư viện khóa học (đào tạo nghiệp vụ, an toàn, kỹ năng mềm) với nhiều định dạng (video, tài liệu, câu hỏi) để nhân viên tự học .

* *US41*: Là **nhân viên**, tôi muốn truy cập khóa học, theo dõi tiến độ và làm bài kiểm tra để nhận chứng nhận sau khi hoàn thành .

* *US42*: Là **quản lý**, tôi muốn xem báo cáo tiến độ đào tạo của nhân viên để đánh giá năng lực và sắp xếp công việc phù hợp.

**Tiêu chí chấp nhận:**

1. Cho phép thiết lập mục tiêu/KPI và thời hạn cho từng nhân viên; hệ thống lưu trữ và hiển thị tiến độ thực hiện.

2. Quy trình đánh giá bao gồm tự đánh giá, đánh giá của quản lý và đồng nghiệp; kết quả được lưu và tra cứu.

3. Thư viện khóa học hỗ trợ phân loại theo chủ đề; mỗi khóa có mô tả, nội dung, câu hỏi kiểm tra và thang điểm; hệ thống ghi nhận thời gian hoàn thành và kết quả.

4. Nhân viên được nhắc lịch học và đánh giá qua thông báo; sau khi hoàn thành, hệ thống cập nhật hồ sơ năng lực và đề xuất nhiệm vụ phù hợp.

5. Báo cáo hiệu suất và đào tạo hiển thị tại dashboard của quản lý; cho phép xuất dữ liệu.

\#\#\#\# 3.12 Quy trình onboarding và offboarding

**Mô tả:** Tự động hóa việc tiếp nhận nhân viên mới và quá trình thôi việc.

**User stories:**

* *US43*: Là **quản lý**, tôi muốn có checklist onboarding gồm thu thập thông tin cá nhân, ký hợp đồng, cung cấp tài khoản, đào tạo ban đầu và phân công người hướng dẫn .

* *US44*: Là **nhân viên mới**, tôi muốn hoàn thành biểu mẫu trực tuyến để cung cấp thông tin (địa chỉ, kinh nghiệm, thuế) và nhận hướng dẫn rõ ràng về công việc .

* *US45*: Là **hệ thống**, tôi muốn ghi nhận trạng thái của từng bước onboarding (hoàn thành/chưa hoàn thành) và nhắc nhở các bên liên quan.

* *US46*: Là **admin**, tôi muốn quản lý quy trình offboarding: nhận yêu cầu nghỉ việc, thực hiện khảo sát, thu hồi thiết bị, khóa tài khoản và hoàn tất thanh toán .

**Tiêu chí chấp nhận:**

1. Quá trình onboarding có thể cấu hình checklist theo vị trí; mỗi bước có người chịu trách nhiệm.

2. Hệ thống cho phép nhân viên mới gửi thông tin và tài liệu trước ngày nhận việc; tự động tạo hồ sơ và cấp thẻ ID.

3. Trong offboarding, hệ thống ghi nhận ngày nghỉ việc, lý do, tình trạng bàn giao và khảo sát; khóa quyền truy cập sau khi hoàn tất.

4. Lưu lịch sử onboarding/offboarding để truy vết và cải thiện quy trình.

\#\#\#\# 3.13 Quản lý thẻ ID nhân viên

**Mô tả:** Quản lý việc tạo, phát hành và tái phát hành thẻ ID cho nhân viên.

**User stories:**

* *US47*: Là **admin**, tôi muốn cấu hình mẫu thẻ ID theo bộ phận hoặc loại nhân viên (dọc/ngang), bao gồm logo, họ tên, ảnh, mã nhân viên, phòng ban, nhóm máu và mã QR .

* *US48*: Là **hệ thống**, tôi muốn tự động lấy dữ liệu từ hồ sơ nhân viên để phát hành thẻ và lưu lịch sử phát hành.

* *US49*: Là **quản lý**, tôi muốn in thẻ ID hàng loạt cho nhân viên mới hoặc khi có thay đổi thông tin .

* *US50*: Là **admin**, tôi muốn thu hồi và tái phát hành thẻ khi nhân viên mất thẻ hoặc khi thông tin thay đổi.

**Tiêu chí chấp nhận:**

1. Hệ thống hỗ trợ quản lý nhiều mẫu thẻ; người dùng có thể xem trước trước khi in.

2. Thẻ bao gồm các trường tùy chỉnh (ảnh, mã nhân viên, tên, phòng ban, chức danh, nhóm máu, mã QR);

3. Chỉ admin có quyền phát hành hoặc thu hồi thẻ; mỗi lần phát hành lưu log với thời gian và lý do .

4. Nhân viên có thể xem thông tin trên thẻ thông qua cổng tự phục vụ.

\#\#\#\# 3.14 Quản lý tài liệu và hồ sơ (Documentation Management)

**Mô tả:** Lưu trữ an toàn các tài liệu nhân sự (hợp đồng, chứng chỉ, biên bản) và cho phép truy cập có kiểm soát.

**User stories:**

* *US51*: Là **quản lý**, tôi muốn tải lên hợp đồng lao động và giấy tờ liên quan vào hồ sơ nhân viên để lưu trữ tập trung.

* *US52*: Là **nhân viên**, tôi muốn xem và tải xuống các tài liệu của mình (hợp đồng, bảng lương, chứng chỉ) qua cổng tự phục vụ.

* *US53*: Là **admin**, tôi muốn quản lý phân quyền truy cập tài liệu (chỉ người được phép mới xem được) và theo dõi lịch sử truy cập.

**Tiêu chí chấp nhận:**

1. Tài liệu được lưu theo nhân viên và phân loại (hợp đồng, chứng chỉ, biên bản…); hỗ trợ tìm kiếm.

2. Dữ liệu được mã hóa khi lưu trữ; chỉ người có quyền mới xem/ tải về.

3. Hệ thống lưu log truy cập (ai, khi nào) để đảm bảo tuân thủ quy định bảo vệ dữ liệu.

\#\#\#\# 3.15 Cổng tự phục vụ và ứng dụng di động

**Mô tả:** Giao diện dành cho nhân viên sử dụng trên web hoặc mobile, cho phép tự thực hiện các tác vụ thường ngày.

**User stories:**

* *US54*: Là **nhân viên**, tôi muốn đăng nhập vào cổng tự phục vụ để xem ca làm, chấm công, yêu cầu nghỉ phép, xem bảng lương, hoàn thành khóa học và nhận thông báo trên điện thoại .

* *US55*: Là **quản lý**, tôi muốn phê duyệt yêu cầu nghỉ phép, xem báo cáo, tạo nhiệm vụ và đánh giá nhân viên từ ứng dụng di động để không bị gián đoạn công việc.

**Tiêu chí chấp nhận:**

1. Giao diện responsive hỗ trợ cả tiếng Việt và tiếng Anh; trải nghiệm nhất quán trên web và mobile.

2. Nhân viên có thể chấm công, gửi yêu cầu nghỉ phép, cập nhật hồ sơ, xem kết quả đánh giá và đào tạo, nhận thông báo push.

3. Ứng dụng hỗ trợ **offline mode** tạm thời khi mất kết nối (lưu dữ liệu cục bộ và đồng bộ lại khi có mạng).

4. Bảo mật: ứng dụng sử dụng mã hóa kết nối HTTPS và lưu trữ token an toàn; hỗ trợ đăng nhập bằng OTP hoặc sinh trắc học (nếu thiết bị hỗ trợ).

\#\#\#\# 3.16 Phân tích nhân sự và báo cáo

**Mô tả:** Cung cấp báo cáo và biểu đồ phân tích cho quản lý và admin.

**User stories:**

* *US56*: Là **quản lý**, tôi muốn xem báo cáo tổng hợp về tình trạng đi làm, nghỉ phép, giờ làm thêm để điều chỉnh nhân sự.

* *US57*: Là **quản lý**, tôi muốn xem bảng điểm đánh giá hiệu suất theo cá nhân và nhóm để đưa ra quyết định đào tạo hoặc khen thưởng .

* *US58*: Là **admin**, tôi muốn thống kê sự đa dạng nhân sự (giới tính, độ tuổi, kinh nghiệm) và theo dõi attrition để có chiến lược tuyển dụng phù hợp .

**Tiêu chí chấp nhận:**

1. Báo cáo chấm công: hiển thị tổng số giờ làm việc, số lần đi muộn/về sớm, làm thêm; xuất file CSV/PDF.

2. Báo cáo nghỉ phép: hiển thị số ngày phép đã dùng/ còn lại theo loại; thống kê nghỉ theo bộ phận.

3. Báo cáo hiệu suất: hiển thị KPI đạt được, xếp hạng, biểu đồ so sánh theo thời gian.

4. Báo cáo đào tạo: liệt kê khóa học đã tham gia, tiến độ, điểm số; phân tích khoảng cách kỹ năng để đưa ra chương trình đào tạo.

5. Hệ thống cung cấp dashboard trực quan và API xuất dữ liệu cho công cụ BI.

\#\#\# 4\. Các yêu cầu phi chức năng

Ngoài các yêu cầu đã nêu ở phiên bản 1, phiên bản 2 bổ sung:

* **Bảo mật dữ liệu cá nhân:** Hệ thống phải áp dụng mã hóa dữ liệu và tuân thủ chính sách bảo vệ dữ liệu, chỉ người dùng có thẩm quyền mới được truy cập thông tin nhạy cảm .

* **Tính sẵn sàng trên di động:** Ứng dụng mobile phải hoạt động mượt mà và hỗ trợ offline cache; đẩy thông báo khi có sự kiện quan trọng .

* **Khả năng mở rộng:** Kiến trúc monolithic được thiết kế module hóa để dễ dàng tách thành microservices khi số lượng nhân viên và yêu cầu hệ thống tăng lên .

* **Khả năng tích hợp:** Hệ thống phải có khả năng tích hợp với các dịch vụ bên ngoài như hệ thống email, SMS, cổng thanh toán và công cụ học trực tuyến.

* **Trải nghiệm người dùng:** Giao diện cần trực quan, dễ sử dụng; tự động hoá quy trình để giảm thao tác thủ công; hỗ trợ đa ngôn ngữ.

\#\#\# 5\. Tech Stack / Công nghệ sử dụng

Phiên bản 2 sử dụng các công nghệ tương tự phiên bản 1 nhưng bổ sung một số thành phần:

* **Back‑end:** Ứng dụng **ASP.NET Core** monolith với phân tầng; sử dụng Entity Framework Core để quản lý cơ sở dữ liệu; tích hợp gói background job (như Hangfire hoặc MediatR) để xử lý các tác vụ bất đồng bộ (gửi email, tạo báo cáo).  Sử dụng SignalR cho tính năng real‑time.

* **Database & Storage:** PostgreSQL cho dữ liệu quan hệ; Redis làm cache; hệ thống lưu trữ file (như AWS S3, Azure Blob hoặc lưu trữ cục bộ) để quản lý tài liệu và ảnh của nhân viên.  Cơ sở dữ liệu bao gồm các bảng mới như *leave\_types*, *leave\_requests*, *performance\_reviews*, *training\_courses*, *id\_cards*, *documents*…

* **Frontend:** Web app bằng Next.js/React phục vụ admin và manager; mobile app bằng React Native hoặc Flutter cho nhân viên.  Ứng dụng mobile hỗ trợ offline sync và push notification.

* **DevOps:** Docker hóa toàn bộ hệ thống thành một hoặc vài container; dùng Docker Compose cho môi trường phát triển.  Khi triển khai production có thể dùng Kubernetes; Nginx làm reverse proxy; sử dụng Prometheus/Grafana để giám sát và Serilog cho logging tập trung.

\#\#\# 6\. Kết luận

Phiên bản 2 của tài liệu SRS bổ sung nhiều chức năng để hệ thống quản lý nội bộ lounge sân bay trở thành một **nền tảng quản lý nhân sự toàn diện**.  Bên cạnh các tính năng cốt lõi (quản lý ca làm, chấm công, nhiệm vụ), hệ thống còn quản lý hồ sơ cá nhân, nghỉ phép, lương, hiệu suất, đào tạo, onboarding/offboarding và thẻ ID.  Các yêu cầu này được xây dựng dựa trên nghiên cứu các hệ thống EMS hiện đại: chúng bao gồm quản lý hồ sơ nhân viên, hệ thống nghỉ phép linh hoạt , chức năng đánh giá hiệu suất và đào tạo , quy trình onboarding/offboarding , và quản lý thẻ ID .  Kiến trúc monolithic modular tiếp tục được sử dụng cho giai đoạn MVP để triển khai nhanh và dễ bảo trì, đồng thời hệ thống được thiết kế để dễ dàng mở rộng và tích hợp trong tương lai.

