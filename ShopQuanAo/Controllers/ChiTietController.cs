using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using ShopQuanAo.Models;
using System.Web.SessionState;

namespace ShopQuanAo.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class ChiTietController : Controller
    {
        ShopQuanAoEntities db = new ShopQuanAoEntities();
        // GET: ChiTiet
        public ActionResult Index(int? id)
        {
            if (id == null)
                return HttpNotFound();

            // SẢN PHẨM ĐANG XEM
            var sp = db.QuanAos
                .Include(q => q.TheLoai)
                .FirstOrDefault(q => q.MaQuanAo == id);

            if (sp == null)
                return HttpNotFound();

            // ✅ SẢN PHẨM LIÊN QUAN (CÙNG THỂ LOẠI)
            var sanPhamLienQuan = db.QuanAos
                .Include(q => q.TheLoai) 
                .Where(q =>
                    q.MaTheLoai == sp.MaTheLoai &&
                    q.GioiTinh == sp.GioiTinh &&
                    q.MaQuanAo != sp.MaQuanAo

                )
                .Take(4)
                .ToList();

            ViewBag.SanPhamLienQuan = sanPhamLienQuan;

            return View(sp);
        }

        // POST: ChiTiet/ThemVaoGioHang
        [HttpPost]
        public ActionResult ThemVaoGioHang(int? id, int? soLuong)
        {
            // Kiểm tra đăng nhập
            if (Session["MaUser"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!", requireLogin = true });
            }

            if (id == null)
            {
                return Json(new { success = false, message = "Mã sản phẩm không hợp lệ!" });
            }

            var sanPham = db.QuanAos.FirstOrDefault(q => q.MaQuanAo == id);
            if (sanPham == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
            }

            // Lấy số lượng (mặc định là 1 nếu không có)
            int quantity = soLuong ?? 1;
            if (quantity <= 0)
            {
                quantity = 1;
            }

            // Kiểm tra số lượng tồn kho
            if (sanPham.SoLuongTon < quantity)
            {
                return Json(new { success = false, message = $"Sản phẩm chỉ còn {sanPham.SoLuongTon} sản phẩm trong kho!" });
            }

            try
            {
                // Lấy giỏ hàng từ Session
                Dictionary<int, int> cart = GetCartFromSession();

                // Thêm hoặc cập nhật sản phẩm trong giỏ hàng
                if (cart.ContainsKey(id.Value))
                {
                    // Nếu sản phẩm đã có trong giỏ hàng, cộng thêm số lượng
                    int tongSoLuong = cart[id.Value] + quantity;
                    if (tongSoLuong > sanPham.SoLuongTon)
                    {
                        return Json(new { success = false, message = $"Số lượng vượt quá tồn kho! Tổng số lượng yêu cầu: {tongSoLuong}, còn lại: {sanPham.SoLuongTon}" });
                    }
                    cart[id.Value] = tongSoLuong;
                }
                else
                {
                    // Thêm sản phẩm mới vào giỏ hàng
                    cart[id.Value] = quantity;
                }

                // Lưu giỏ hàng vào Session
                Session["Cart"] = cart;

                // Tính tổng số lượng trong giỏ hàng
                int totalItems = cart.Sum(x => x.Value);

                return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng!", totalItems = totalItems });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // Helper method để lấy giỏ hàng từ Session
        private Dictionary<int, int> GetCartFromSession()
        {
            if (Session["Cart"] == null)
            {
                Session["Cart"] = new Dictionary<int, int>();
            }
            return Session["Cart"] as Dictionary<int, int>;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}