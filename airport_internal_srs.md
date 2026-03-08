**TÀI LIỆU ĐẶC TẢ YÊU CẦU PHẦN MỀM (SRS)**

Hệ thống Quản lý Nội bộ Nhân viên Lounge Sân bay

1\. Giới thiệu

1.1 Mục đích  
Tài liệu đặc tả yêu cầu phần mềm (SRS) này nhằm cung cấp cái nhìn tổng quan và chi tiết về các yêu cầu của dự án “Hệ thống quản lý nội bộ nhân viên lounge sân bay”.  Đây là nguồn tham khảo cho các bên liên quan như nhà phát triển phần mềm, kiểm thử viên, quản lý dự án và khách hàng.  Theo chuẩn tài liệu SRS, phần giới thiệu thường nêu mục đích, phạm vi và thuật ngữ dùng trong dự án ￼.

1.2 Phạm vi  
Hệ thống này được xây dựng để hỗ trợ quản lý hoạt động nội bộ của nhân viên trong phòng chờ (lounge) tại sân bay.  Nó bao gồm quản lý hồ sơ nhân viên, lên lịch ca làm, chấm công, quản lý khu vực lounge và phân công nhiệm vụ.  Phần mềm phục vụ cho các lounge của các hãng hàng không hoặc sân bay, giúp tối ưu hoá quy trình nội bộ và nâng cao trải nghiệm phục vụ khách VIP.

1.3 Đối tượng và vai trò  
Hệ thống có ba vai trò chính:  
	•	Admin: cấu hình hệ thống, quản lý người dùng, thiết lập phân quyền và giám sát dữ liệu.  
	•	Quản lý (Manager): quản lý nhân viên, tạo và phân công ca làm, theo dõi chấm công và phân công nhiệm vụ.  
	•	Nhân viên (Staff): xem lịch làm việc, chấm công, nhận thông báo và thực hiện nhiệm vụ được giao.

1.4 Thuật ngữ và viết tắt  
	•	Ca làm (Shift): Khoảng thời gian làm việc đã lên lịch cho nhân viên.  
	•	Chấm công (Attendance): Ghi nhận thời gian bắt đầu và kết thúc ca làm của nhân viên.  
	•	Task (Nhiệm vụ): Công việc cụ thể được giao cho nhân viên trong một ca làm.  
	•	Dashboard: Giao diện tổng hợp thông tin dành cho nhân viên hoặc quản lý.  
	•	VIP lounge: Khu vực phòng chờ cao cấp dành cho khách VIP.

2\. Tổng quan hệ thống

2.1 Mục tiêu hệ thống  
	•	Số hoá quy trình quản lý nội bộ của lounge, giảm phụ thuộc vào giấy tờ và liên lạc truyền thống.  
	•	Cung cấp công cụ cho quản lý để lập lịch ca làm, phân công công việc và theo dõi hiệu suất.  
	•	Tạo nền tảng cho nhân viên chấm công nhanh chóng (có thể qua ứng dụng di động hoặc web) và nhận nhiệm vụ theo thời gian thực.  
	•	Hỗ trợ hiển thị tình trạng khu vực lounge (sức chứa, khu vực bận rộn) để phân bổ nhân lực hợp lý.  
	•	Tiền đề tích hợp với các hệ thống khách hàng sau này (quản lý yêu cầu của khách, đặt dịch vụ, etc.).

2.2 Mô tả tổng quan hệ thống  
Hệ thống gồm một ứng dụng web dành cho quản lý và admin, cùng một ứng dụng di động dành cho nhân viên.  Tất cả giao diện người dùng sẽ truy cập vào một back‑end duy nhất thông qua API REST.  Ứng dụng back‑end được xây dựng theo kiến trúc monolithic với phân tầng (presentation, service, repository) để đơn giản hoá phát triển ban đầu và thuận tiện cho việc triển khai MVP.  Các chức năng chính bao gồm:  
	•	Quản lý người dùng: tạo, sửa, phân quyền và vô hiệu hóa tài khoản nhân viên.  
	•	Quản lý ca làm: định nghĩa khung giờ, lên lịch và gán nhân viên vào ca tương ứng.  
	•	Chấm công: nhân viên chấm giờ vào và ra; hệ thống ghi nhận thời gian thực và tính toán thời gian làm việc, trạng thái (đi muộn, về sớm, làm thêm).  
	•	Quản lý khu vực: định nghĩa các khu vực trong lounge, sức chứa và tình trạng hiện tại (đang phục vụ, cần dọn dẹp, …).  
	•	Quản lý nhiệm vụ: phân công công việc (task) như phục vụ đồ ăn, dọn dẹp, hỗ trợ khách; theo dõi tình trạng thực hiện.  
	•	Thông báo nội bộ: gửi thông báo về thay đổi ca làm, nhiệm vụ khẩn cấp hoặc thông tin quan trọng.

Ứng dụng back‑end là một khối thống nhất (monolith) nhưng được thiết kế theo hướng modular để chia nhỏ theo các phân hệ (quản lý nhân viên, ca làm, chấm công, nhiệm vụ) trong cùng một mã nguồn.  Việc sử dụng kiến trúc monolith giúp triển khai nhanh MVP, đơn giản hoá cấu hình và giảm chi phí vận hành.  Hệ thống vẫn có thể tích hợp một hàng đợi thông điệp nội bộ (ví dụ: sử dụng hang chờ trong database hoặc một thư viện queue đơn giản) để xử lý các tác vụ bất đồng bộ như gửi thông báo real‑time mà không cần tách thành dịch vụ riêng.

3\. Các yêu cầu chức năng (Functional Requirements)

Phần này trình bày danh sách tính năng (feature list), cùng với user story và tiêu chí chấp nhận (acceptance criteria) cho từng tính năng.  Cấu trúc này giúp đội phát triển hiểu rõ nhu cầu và xác nhận khi nào chức năng được coi là hoàn thành.

3.1 Đăng nhập, phân quyền và bảo mật  
Mô tả: Cung cấp chức năng xác thực người dùng và phân quyền sử dụng hệ thống.  Đây là chức năng cơ bản nêu trong các tài liệu SRS mẫu ￼.

User stories:  
	•	US01: Là nhân viên, tôi muốn đăng nhập bằng email/số điện thoại và mật khẩu để truy cập hệ thống và xem lịch làm việc.  
	•	US02: Là quản lý, tôi muốn được phân quyền quản lý để tạo ca làm, phân công nhiệm vụ và xem báo cáo.  
	•	US03: Là admin, tôi muốn có toàn quyền để cấu hình hệ thống, quản lý tài khoản và phân quyền cho người dùng.

Tiêu chí chấp nhận:  
	1\.	Hệ thống phải yêu cầu nhập thông tin xác thực và chỉ cho phép truy cập khi thông tin hợp lệ.  
	2\.	Sau khi đăng nhập thành công, người dùng được cấp token phiên làm việc; token hết hạn sẽ yêu cầu đăng nhập lại.  
	3\.	Người dùng chỉ truy cập được các chức năng tương ứng với vai trò (admin/manager/staff).  
	4\.	Thông tin nhạy cảm (mật khẩu) phải được lưu trữ dạng mã hóa.

3.2 Quản lý hồ sơ nhân viên  
Mô tả: Tạo, xem, cập nhật và vô hiệu hóa thông tin nhân viên, bao gồm dữ liệu cá nhân, vai trò và kỹ năng.

User stories:  
	•	US04: Là quản lý, tôi muốn thêm nhân viên mới với thông tin cá nhân và vai trò để họ có thể sử dụng hệ thống.  
	•	US05: Là quản lý, tôi muốn chỉnh sửa thông tin nhân viên (tên, liên hệ, bộ phận, kỹ năng) để dữ liệu luôn chính xác.  
	•	US06: Là admin, tôi muốn vô hiệu hóa tài khoản nhân viên khi họ nghỉ việc để đảm bảo an toàn.  
	•	US07: Là nhân viên, tôi muốn xem và cập nhật thông tin liên hệ cá nhân của mình.

Tiêu chí chấp nhận:  
	1\.	Chỉ quản lý và admin mới được tạo hoặc sửa hồ sơ của người khác; nhân viên chỉ sửa dữ liệu cá nhân (số điện thoại, địa chỉ,…).  
	2\.	Mọi thao tác thêm/sửa/xóa phải lưu lại log với thời gian và người thực hiện.  
	3\.	Trường thông tin bắt buộc: mã nhân viên, họ tên, email/số điện thoại, vai trò.  
	4\.	Khi vô hiệu hóa tài khoản, nhân viên không thể đăng nhập nhưng dữ liệu vẫn được lưu trữ cho báo cáo.

3.3 Quản lý ca làm (Shift Scheduling)  
Mô tả: Định nghĩa các ca làm việc (ví dụ: ca sáng, ca chiều), tạo lịch làm việc theo ngày/tháng, và gán nhân viên vào từng ca.

User stories:  
	•	US08: Là quản lý, tôi muốn định nghĩa các loại ca làm với khung giờ bắt đầu và kết thúc (06:00–14:00, 14:00–22:00, 22:00–06:00) để lập lịch dễ dàng.  
	•	US09: Là quản lý, tôi muốn lên lịch ca làm cho tuần/tháng và gán nhân viên vào các ca tương ứng để đảm bảo đủ nhân lực.  
	•	US10: Là nhân viên, tôi muốn xem lịch ca làm của mình theo ngày/tuần để chuẩn bị công việc.  
	•	US11: Là quản lý, tôi muốn thay đổi lịch và thông báo cho nhân viên khi có thay đổi khẩn cấp.

Tiêu chí chấp nhận:  
	1\.	Hệ thống cho phép tạo ca làm với tên, khung giờ và khu vực làm việc (nếu cần).  
	2\.	Khi lên lịch, quản lý chọn ngày, ca và nhân viên; lịch thể hiện dưới dạng bảng hoặc lịch tuần.  
	3\.	Nhân viên chỉ nhìn thấy lịch của mình và lịch tổng quát của bộ phận.  
	4\.	Khi lịch thay đổi, hệ thống gửi thông báo (push notification/email) cho nhân viên liên quan.  
	5\.	Không cho phép gán nhân viên vào hai ca trùng thời gian.

3.4 Chấm công và theo dõi thời gian làm việc  
Mô tả: Nhân viên chấm giờ vào (check‑in) và giờ ra (check‑out) cho mỗi ca; hệ thống tính toán thời gian làm việc và trạng thái (đúng giờ, muộn, về sớm, làm thêm).

User stories:  
	•	US12: Là nhân viên, tôi muốn chấm công thông qua ứng dụng di động để ghi nhận thời gian bắt đầu ca làm.  
	•	US13: Là nhân viên, tôi muốn chấm công kết thúc ca làm để tính tổng thời gian làm việc.  
	•	US14: Là quản lý, tôi muốn xem báo cáo chấm công theo ngày/tuần của từng nhân viên để đánh giá tuân thủ.  
	•	US15: Là admin, tôi muốn xuất báo cáo chấm công toàn bộ nhân viên cho bộ phận nhân sự.

Tiêu chí chấp nhận:  
	1\.	Nhân viên chỉ có thể chấm công khi đang trong lịch ca của mình (theo ngày và khung giờ).  Có thể cấu hình cho phép chấm trước/nhưng không vượt quá một khoảng thời gian nhất định (ví dụ 15 phút).  
	2\.	Hệ thống lưu trữ thời gian check‑in/check‑out và tự động tính thời lượng làm việc, trạng thái (đúng giờ, đi muộn, ra sớm, làm thêm).  
	3\.	Quản lý có thể xác nhận hoặc chỉnh sửa bản ghi chấm công trong trường hợp đặc biệt (quên chấm công).  
	4\.	Báo cáo chấm công cho phép lọc theo nhân viên, thời gian, ca làm và xuất ra file CSV/PDF.

3.5 Dashboard nhân viên và quản lý  
Mô tả: Cung cấp trang tổng quan hiển thị thông tin quan trọng theo vai trò.

User stories:  
	•	US16: Là nhân viên, tôi muốn xem nhanh lịch làm việc hôm nay, nhiệm vụ hiện tại và thông báo mới trên một màn hình tổng quan.  
	•	US17: Là quản lý, tôi muốn xem danh sách nhân viên đang làm việc, các ca còn thiếu nhân lực và các nhiệm vụ đang chờ xử lý.  
	•	US18: Là admin, tôi muốn xem số lượng người dùng, tình trạng hệ thống và thống kê tổng hợp.

Tiêu chí chấp nhận:  
	1\.	Dashboard tùy theo vai trò, chỉ hiển thị thông tin cần thiết.  
	2\.	Cập nhật theo thời gian thực (qua websockets/SignalR) khi có thay đổi ca làm, chấm công hoặc nhiệm vụ mới.  
	3\.	Cho phép truy cập nhanh tới các chức năng liên quan (xem chi tiết ca làm, nhận nhiệm vụ, v.v.).  
	4\.	Hiển thị cảnh báo nếu ca làm chưa đủ nhân viên hoặc nhân viên đi muộn.

3.6 Quản lý khu vực lounge  
Mô tả: Quản lý các khu vực trong lounge, sức chứa, tình trạng sử dụng và nhu cầu nhân sự.

User stories:  
	•	US19: Là quản lý, tôi muốn định nghĩa danh sách khu vực (khu vực chính, khu vực đồ ăn, phòng tắm, khu VIP…) và sức chứa của từng khu.  
	•	US20: Là nhân viên, tôi muốn cập nhật trạng thái khu vực (cần dọn dẹp, cần phục vụ, đầy khách) để quản lý nắm được tình hình.  
	•	US21: Là quản lý, tôi muốn xem tổng quan tình trạng các khu vực để phân bổ nhân viên hợp lý.

Tiêu chí chấp nhận:  
	1\.	Khu vực có tên, mô tả, sức chứa và trạng thái hiện tại.  
	2\.	Nhân viên có thể báo cáo tình trạng qua ứng dụng; hệ thống ghi nhận thời gian và người báo cáo.  
	3\.	Quản lý có thể lọc các khu vực theo trạng thái và xem lịch sử thay đổi.  
	4\.	Tích hợp cảnh báo khi khu vực gần đạt sức chứa tối đa hoặc cần hỗ trợ khẩn cấp.

3.7 Quản lý nhiệm vụ (Task Management)  
Mô tả: Tạo và phân công nhiệm vụ cho nhân viên, theo dõi quá trình thực hiện và đánh dấu hoàn thành.

User stories:  
	•	US22: Là quản lý, tôi muốn tạo nhiệm vụ như phục vụ đồ ăn, dọn dẹp khu vực, hỗ trợ khách để đảm bảo dịch vụ kịp thời.  
	•	US23: Là quản lý, tôi muốn phân công nhiệm vụ cho nhân viên rảnh rỗi hoặc có kỹ năng phù hợp.  
	•	US24: Là nhân viên, tôi muốn nhận thông báo về nhiệm vụ mới và cập nhật trạng thái (đang thực hiện, đã hoàn thành).  
	•	US25: Là quản lý, tôi muốn xem tiến độ và lịch sử nhiệm vụ để đánh giá hiệu quả công việc.

Tiêu chí chấp nhận:  
	1\.	Nhiệm vụ có tiêu đề, mô tả, mức độ ưu tiên, người được giao, thời gian tạo và trạng thái.  
	2\.	Khi tạo nhiệm vụ, hệ thống gửi thông báo cho nhân viên liên quan.  
	3\.	Nhân viên cập nhật trạng thái nhiệm vụ; hệ thống lưu lại thời gian cập nhật.  
	4\.	Quản lý có thể lọc nhiệm vụ theo trạng thái, nhân viên và thời gian để xem hoặc xuất báo cáo.

3.8 Thông báo nội bộ  
Mô tả: Gửi thông báo (push notification/email) đến nhân viên về lịch làm việc, thay đổi ca, nhiệm vụ gấp hoặc thông tin quan trọng.

User stories:  
	•	US26: Là quản lý, tôi muốn gửi thông báo cho tất cả nhân viên về thay đổi lịch hoặc quy định mới.  
	•	US27: Là hệ thống, tôi muốn tự động gửi thông báo khi ca làm thay đổi hoặc khi nhiệm vụ được giao.  
	•	US28: Là nhân viên, tôi muốn nhận thông báo kịp thời trên thiết bị di động để không bỏ lỡ công việc.

Tiêu chí chấp nhận:  
	1\.	Hệ thống hỗ trợ hai kênh thông báo: push notification (qua ứng dụng mobile) và email/SMS (tuỳ cấu hình).  
	2\.	Thông báo chứa nội dung rõ ràng (tiêu đề, nội dung, thời gian) và liên kết tới màn hình liên quan (lịch, nhiệm vụ,…).  
	3\.	Cho phép quản lý chọn nhóm người nhận (tất cả nhân viên, nhân viên theo ca, hoặc cá nhân).  
	4\.	Lưu lịch sử thông báo và trạng thái (đã đọc/chưa đọc) của từng nhân viên.

4\. Các yêu cầu phi chức năng (Non‑functional Requirements)  
	•	Hiệu năng: Hệ thống phải xử lý yêu cầu đăng nhập, chấm công và cập nhật nhiệm vụ trong vòng dưới 2 giây trên 95% trường hợp.  
	•	Khả năng mở rộng: Hệ thống sử dụng kiến trúc monolithic phân lớp; có thể mở rộng bằng cách tối ưu hoá và bổ sung các module mới.  Khi nhu cầu tăng về sau, kiến trúc có thể chuyển dần sang modular hoặc microservices.  
	•	Bảo mật: Mọi kết nối sử dụng giao thức HTTPS; dữ liệu nhạy cảm (mật khẩu, thông tin cá nhân) phải được mã hóa.  Phân quyền theo vai trò để ngăn truy cập trái phép ￼.  
	•	Khả năng sử dụng: Giao diện web và mobile phải thân thiện, hỗ trợ tiếng Việt, và tối ưu cho thao tác trên thiết bị di động.  Nhân viên phải chấm công và nhận nhiệm vụ nhanh chóng.  
	•	Khả năng bảo trì: Mã nguồn phải có cấu trúc rõ ràng, áp dụng mô hình clean architecture; tài liệu kỹ thuật đầy đủ để dễ bảo trì và mở rộng.

5\. Tech Stack / Công nghệ sử dụng  
	•	Back‑end:  
	•	Ngôn ngữ: C\# trên .NET 9 / ASP.NET Core (MVC hoặc Minimal APIs).  
	•	Kiến trúc: Monolithic với phân tầng rõ ràng (API layer, service layer, data access). Các phân hệ như quản lý nhân viên, ca làm, chấm công, nhiệm vụ hoạt động trong cùng một ứng dụng.  
	•	Giao tiếp: RESTful API phục vụ cho web và mobile; có thể sử dụng cơ chế hàng đợi nội bộ (in‑process queue hoặc thư viện background job) cho các tác vụ bất đồng bộ.  
	•	Realtime: SignalR (hoặc WebSocket) tích hợp trực tiếp trong ứng dụng để đẩy thông tin (dashboard, thông báo) theo thời gian thực.  
	•	Database & Cache:  
	•	PostgreSQL cho dữ liệu chính (nhân viên, ca làm, chấm công, nhiệm vụ).  
	•	Redis làm cache cho thông tin phiên làm việc và bảng dashboard.  
	•	Frontend:  
	•	Web: Next.js & React với TailwindCSS, dành cho admin và manager.  
	•	Mobile: Ứng dụng di động viết bằng React Native (hoặc Flutter) dành cho nhân viên chấm công và nhận nhiệm vụ.  
	•	DevOps:  
	•	Sử dụng Docker để đóng gói toàn bộ ứng dụng monolithic thành một container duy nhất.  Việc triển khai ban đầu có thể dùng Docker Compose để chạy các thành phần như ứng dụng, PostgreSQL và Redis trong cùng một máy chủ.  Sau này có thể mở rộng sang Kubernetes khi cần.  
	•	Sử dụng Nginx làm reverse proxy để phân phối lưu lượng giữa web và mobile tới API, đồng thời phục vụ nội dung tĩnh của frontend.  
	•	Công cụ giám sát (Prometheus, Grafana) và logging (Serilog) được cấu hình tập trung để theo dõi trạng thái ứng dụng và dễ dàng bảo trì.

6\. Kết luận

Tài liệu SRS này mô tả tổng quan mục đích, phạm vi, kiến trúc và yêu cầu chức năng cho hệ thống quản lý nội bộ nhân viên lounge sân bay.  Các chức năng tập trung vào quản lý nhân viên, ca làm, chấm công, phân công nhiệm vụ và thông báo nhằm nâng cao hiệu quả vận hành.  Tài liệu được xây dựng dựa trên các hướng dẫn chuẩn của SRS trong các tài liệu tham khảo ￼, phân chia rõ ràng giữa yêu cầu chức năng và phi chức năng.

Hệ thống được thiết kế theo kiến trúc monolithic với các phân hệ được tổ chức theo tầng.  Điều này giúp giảm độ phức tạp ban đầu, triển khai nhanh MVP và thuận tiện vận hành trong môi trường nội bộ.  Kiến trúc phân lớp tạo điều kiện để mở rộng và tách dần thành microservices khi hệ thống phát triển lớn hơn.  Trong giai đoạn đầu, hệ thống ưu tiên phát triển ứng dụng web cho quản lý và admin, đồng thời xây dựng ứng dụng di động cho nhân viên để thuận tiện chấm công và nhận nhiệm vụ.