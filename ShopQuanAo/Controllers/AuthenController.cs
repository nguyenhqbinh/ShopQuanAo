using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using ShopQuanAo.Models;
using System.Web.Helpers;

namespace ShopQuanAo.Controllers
{
    public class AuthenController : Controller
    {
        private ShopQuanAoEntities db = new ShopQuanAoEntities();

        // GET: Authen/DangNhap
        public ActionResult DangNhap()
        {
            // Hiển thị thông báo lỗi từ TempData nếu có
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            return View();
        }

        // POST: Authen/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(string TenDangNhap, string MatKhau)
        {
            if (string.IsNullOrEmpty(TenDangNhap) || string.IsNullOrEmpty(MatKhau))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            // Mã hóa mật khẩu để so sánh
            string hashedPassword = HashPassword(MatKhau);

            // Tìm user trong database
            var user = db.Users.FirstOrDefault(u => u.TenDangNhap == TenDangNhap && u.MatKhau == hashedPassword);

            if (user != null)
            {
                // Lưu thông tin user vào Session
                Session["MaUser"] = user.MaUser;
                Session["TenDangNhap"] = user.TenDangNhap;
                Session["HoTen"] = user.HoTen;
                Session["Email"] = user.Email;
                Session["VaiTro"] = user.VaiTro;

                // Chuyển hướng về trang chủ
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }
        }

        // GET: Authen/DangKy
        public ActionResult DangKy()
        {
            // Tạo Captcha (phép toán đơn giản)
            GenerateCaptcha();
            return View();
        }

        // POST: Authen/DangKy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(User user, string XacNhanMatKhau, string CaptchaAnswer)
        {
            if (string.IsNullOrEmpty(user.TenDangNhap) || string.IsNullOrEmpty(user.MatKhau))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin!";
                GenerateCaptcha();
                return View(user);
            }

            // Kiểm tra Captcha
            if (Session["CaptchaResult"] == null || 
                string.IsNullOrEmpty(CaptchaAnswer) || 
                !int.TryParse(CaptchaAnswer, out int userAnswer) ||
                userAnswer != (int)Session["CaptchaResult"])
            {
                ViewBag.ErrorMessage = "Mã xác nhận không đúng! Vui lòng thử lại.";
                GenerateCaptcha();
                return View(user);
            }

            // Kiểm tra mật khẩu xác nhận
            if (user.MatKhau != XacNhanMatKhau)
            {
                ViewBag.ErrorMessage = "Mật khẩu xác nhận không khớp!";
                GenerateCaptcha();
                return View(user);
            }

            // Kiểm tra tên đăng nhập đã tồn tại chưa
            if (db.Users.Any(u => u.TenDangNhap == user.TenDangNhap))
            {
                ViewBag.ErrorMessage = "Tên đăng nhập đã tồn tại!";
                GenerateCaptcha();
                return View(user);
            }

            // Kiểm tra email đã tồn tại chưa (nếu có)
            if (!string.IsNullOrEmpty(user.Email) && db.Users.Any(u => u.Email == user.Email))
            {
                ViewBag.ErrorMessage = "Email đã được sử dụng!";
                GenerateCaptcha();
                return View(user);
            }

            try
            {
                // Mã hóa mật khẩu
                user.MatKhau = HashPassword(user.MatKhau);

                // Mặc định vai trò là "KhachHang" nếu không có
                if (string.IsNullOrEmpty(user.VaiTro))
                {
                    user.VaiTro = "KhachHang";
                }

                // Thêm user vào database
                db.Users.Add(user);
                db.SaveChanges();

                // Lưu thông tin user vào Session
                Session["MaUser"] = user.MaUser;
                Session["TenDangNhap"] = user.TenDangNhap;
                Session["HoTen"] = user.HoTen;
                Session["Email"] = user.Email;
                Session["VaiTro"] = user.VaiTro;

                // Chuyển hướng về trang chủ
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi đăng ký: " + ex.Message;
                GenerateCaptcha();
                return View(user);
            }
        }

        // Đăng xuất
        public ActionResult DangXuat()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        // Hàm mã hóa mật khẩu bằng MD5
        private string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // Helper method để tạo Captcha mới
        private void GenerateCaptcha()
        {
            Random random = new Random();
            int num1 = random.Next(1, 10);
            int num2 = random.Next(1, 10);
            int result = num1 + num2;
            
            Session["CaptchaResult"] = result;
            ViewBag.CaptchaQuestion = num1 + " + " + num2 + " = ?";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult QuenMatKhau(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập email!";
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Email không tồn tại!";
                return View();
            }

            Session["ResetUserId"] = user.MaUser;
            return RedirectToAction("DatLaiMatKhau");
        }
        public ActionResult DatLaiMatKhau()
        {
            if (Session["ResetUserId"] == null)
                return RedirectToAction("DangNhap");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatLaiMatKhau(string MatKhauMoi, string XacNhanMatKhau)
        {
            if (Session["ResetUserId"] == null)
                return RedirectToAction("DangNhap");

            if (MatKhauMoi != XacNhanMatKhau)
            {
                ViewBag.ErrorMessage = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            int userId = (int)Session["ResetUserId"];
            var user = db.Users.Find(userId);

            user.MatKhau = HashPassword(MatKhauMoi);
            db.SaveChanges();

            Session.Remove("ResetUserId");

            TempData["ErrorMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction("DangNhap");
        }
    }
}