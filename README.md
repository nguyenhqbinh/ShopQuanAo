# ShopQuanAo – Website bán quần áo

## Giới thiệu

ShopQuanAo là dự án website bán quần áo được xây dựng bằng ASP.NET MVC 5 và SQL Server, nhằm thực hành phát triển ứng dụng web theo mô hình MVC, quản lý cơ sở dữ liệu và xử lý các nghiệp vụ bán hàng.

## Công nghệ sử dụng

- **Ngôn ngữ:** C#
- **Framework:** ASP.NET MVC 5 (.NET Framework 4.7.2)
- **ORM:** Entity Framework 6
- **Cơ sở dữ liệu:** SQL Server
- **Frontend:** HTML, CSS, JavaScript, Bootstrap, jQuery
- **IDE:** Visual Studio 2022

## Chức năng chính

### Người dùng
- Xem danh sách và chi tiết sản phẩm.
- Lọc sản phẩm theo danh mục và giới tính.
- Xem các sản phẩm liên quan.
- Đăng ký, đăng nhập và đăng xuất.
- Thêm sản phẩm vào giỏ hàng, cập nhật số lượng.
- Đặt hàng và kiểm tra số lượng tồn kho.

### Quản trị viên (Admin)
- Quản lý sản phẩm: thêm, sửa, xóa.
- Quản lý tài khoản người dùng.
- Phân quyền Admin/User.
- Đặt lại mật khẩu tài khoản.

## Kiến thức áp dụng

- Mô hình MVC (Model – View – Controller).
- Entity Framework 6 để truy vấn và thao tác dữ liệu.
- LINQ để lọc và tìm kiếm sản phẩm.
- Xử lý Session trong đăng nhập và giỏ hàng.
- Xây dựng các chức năng CRUD.
- Kết nối và làm việc với SQL Server.

## Hướng dẫn chạy dự án

**Yêu cầu:**
- Visual Studio 2022.
- .NET Framework 4.7.2.
- SQL Server.

**Các bước:**
1. Clone repository về máy.
2. Mở file `ShopQuanAo.sln` bằng Visual Studio 2022.
3. Restore các NuGet package cần thiết.
4. Cấu hình connection string trong `Web.config` phù hợp với SQL Server.
5. Chuẩn bị cơ sở dữ liệu tương ứng.
6. Chạy dự án bằng IIS Express (F5).

## Mục đích dự án

Dự án được thực hiện trong quá trình học tập nhằm củng cố kiến thức C#, ASP.NET MVC, Entity Framework và SQL Server, đồng thời rèn luyện kỹ năng phát triển ứng dụng web.
