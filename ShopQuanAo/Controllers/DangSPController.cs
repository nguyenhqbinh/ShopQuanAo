using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopQuanAo.Models;
using System.IO;

namespace ShopQuanAo.Controllers
{
    public class DangSPController : Controller
    {
        ShopQuanAoEntities db = new ShopQuanAoEntities();

        // GET: DangSP
        public ActionResult Index()
        {
            // Lấy danh sách thể loại để đổ vào dropdown
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            ViewBag.MaTheLoai = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai");
            return View();
        }

        // POST: DangSP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(QuanAo quanAo, HttpPostedFileBase HinhAnhFile)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");
            // Kiểm tra file upload
            if (HinhAnhFile == null || HinhAnhFile.ContentLength == 0)
            {
                ModelState.AddModelError("HinhAnhFile", "Vui lòng chọn hình ảnh sản phẩm");
            }
            else
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(HinhAnhFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("HinhAnhFile", "File ảnh không hợp lệ. Chỉ chấp nhận file: .jpg, .jpeg, .png, .gif");
                }
            }

            // Kiểm tra dữ liệu
            if (ModelState.IsValid && HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
            {
                try
                {
                    // Xử lý upload file ảnh
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(HinhAnhFile.FileName).ToLower();
                    
                    // Tạo tên file duy nhất
                    var fileName = Path.GetFileNameWithoutExtension(HinhAnhFile.FileName);
                    fileName = fileName + "_" + DateTime.Now.Ticks + fileExtension;
                    var path = Path.Combine(Server.MapPath("~/Content/img/QuanAo"), fileName);

                    // Đảm bảo thư mục tồn tại
                    var directory = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Lưu file
                    HinhAnhFile.SaveAs(path);
                    quanAo.HinhAnh = "QuanAo/" + fileName;

                    // Lưu vào database
                    db.QuanAos.Add(quanAo);
                    db.SaveChanges();
                    
                    ViewBag.SuccessMessage = "Đăng sản phẩm thành công!";
                    
                    // Reset form
                    ModelState.Clear();
                    ViewBag.MaTheLoai = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai");
                    return View(new QuanAo());
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi lưu sản phẩm: " + ex.Message;
                }
            }

            // Nếu có lỗi, giữ lại giá trị đã nhập
            ViewBag.MaTheLoai = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", quanAo.MaTheLoai);
            return View(quanAo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private bool IsAdmin()
        {
            return Session["VaiTro"] != null && Session["VaiTro"].ToString() == "Admin";
        }
    }
}