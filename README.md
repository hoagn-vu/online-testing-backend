# online-testing-backend
## Giới thiệu
Hệ thống thi trắc nghiệm trực tuyến là một nền tảng giúp tổ chức các kỳ thi trực tuyến một cách dễ dàng, hiệu quả và bảo mật. Hệ thống hỗ trợ nhiều vai trò người dùng như thí sinh, giám thị, quản trị viên, và cán bộ phụ trách ca thi. Các tính năng chính bao gồm:
- Tạo và quản lý ngân hàng câu hỏi.
- Xây dựng và tổ chức kỳ thi với nhiều cấp độ khó khác nhau.
- Hỗ trợ nộp bài tự động và lưu bài thi theo từng câu để tránh mất dữ liệu.
- Chống gian lận bằng các biện pháp giám sát.
- Báo cáo kết quả và phân tích dữ liệu điểm thi
## Liên kết khác
- Fontend: [https://github.com/hoagn-vu/fontend_online_testing](https://github.com/hoagn-vu/frontend_online_testing)
- Backend: [https://github.com/hoagn-vu/backend_online_testing](https://github.com/hoagn-vu/backend_online_testing)
- Jira: [https://hoangvu.atlassian.net/jira/software/projects/OT/boards/2](https://hoangvu.atlassian.net/jira/software/projects/OT/boards/2)
- SRS: 
- SDD: 
## Công nghệ sử dụng
- Backend:
  - Ngôn ngữ: C#
  - Framework: ASP.NET Core
  - Cơ sở dữ liệu: MongoDB
  - API giao tiếp: RESTful API với JSON
  - Authentication: JWT
- Frontend:
  - Ngôn ngữ: JavaScript, TypeScript
  - Thư viện/UI Framework: React.js
## Hướng dẫn cài đặt
### 1. Yêu cầu hệ thống
- Node.js: >= 16.x
- .NET Core SDK: >= 7.0
- MongoDB: >= 6.0
### 2. Cài đặt Backend
- Clone repo:
```sh
git clone https://github.com/your-repo/online-exam-system.git
cd online-exam-system/backend
```
- Cài đặt các thư viện cần thiết:
```sh
dotnet restore
```
- Chạy dự án:
```sh
dotnet run
```
- Kiểm thử và format code:
```sh
dotnet build
dotnet format backend_online_testing.sln
```

