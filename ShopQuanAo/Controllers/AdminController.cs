using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using ShopQuanAo.Models;

namespace ShopQuanAo.Controllers
{
    public class AdminController : Controller
    {
        ShopQuanAoEntities db = new ShopQuanAoEntities();

        // Kiểm tra quyền admin
        private bool IsAdmin()
        {
            if (Session["MaUser"] == null || Session["VaiTro"] == null)
            {
                return false;
            }
            return Session["VaiTro"].ToString() == "Admin";
        }

        // GET: Admin
        public ActionResult Index()
        {
            // Kiểm tra quyền admin
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("DangNhap", "Authen");
            }

            var users = db.Users.ToList();
            return View(users);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("DangNhap", "Authen");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // POST: Admin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("DangNhap", "Authen");
            }

            try
            {
                var userInDb = db.Users.Find(user.MaUser);
                if (userInDb == null)
                {
                    TempData["ErrorMessage"] = "Tài khoản không tồn tại!";
                    return RedirectToAction("Index");
                }

                // Cập nhật thông tin (không cập nhật mật khẩu ở đây)
                userInDb.TenDangNhap = user.TenDangNhap;
                userInDb.HoTen = user.HoTen;
                userInDb.Email = user.Email;
                userInDb.DienThoai = user.DienThoai;
                userInDb.VaiTro = user.VaiTro;

                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
                return View(user);
            }
        }

        // POST: Admin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
            }

            try
            {
                var user = db.Users.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Tài khoản không tồn tại!" });
                }

                // Không cho phép xóa chính mình
                if (user.MaUser == (int)Session["MaUser"])
                {
                    return Json(new { success = false, message = "Bạn không thể xóa chính tài khoản của mình!" });
                }

                db.Users.Remove(user);
                db.SaveChanges();

                return Json(new { success = true, message = "Xóa tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Admin/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
            }

            try
            {
                var user = db.Users.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Tài khoản không tồn tại!" });
                }

                // Reset mật khẩu về mặc định: "123456"
                string defaultPassword = "123456";
                user.MatKhau = HashPassword(defaultPassword);
                db.SaveChanges();

                return Json(new { success = true, message = "Reset mật khẩu thành công! Mật khẩu mới: 123456" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Admin/ChangeRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeRole(int id, string vaiTro)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
            }

            try
            {
                var user = db.Users.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Tài khoản không tồn tại!" });
                }

                // Không cho phép thay đổi quyền của chính mình
                if (user.MaUser == (int)Session["MaUser"])
                {
                    return Json(new { success = false, message = "Bạn không thể thay đổi quyền của chính mình!" });
                }

                user.VaiTro = vaiTro;
                db.SaveChanges();

                return Json(new { success = true, message = "Cấp quyền thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult UserManagement()
        {
            if (Session["VaiTro"] == null || Session["VaiTro"].ToString() != "Admin")
                return RedirectToAction("Login", "Account");

            var users = db.Users.ToList();
            return View(users);
        }
    }
}
