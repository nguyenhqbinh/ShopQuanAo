using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ShopQuanAo.Models;
using System.Web.SessionState;
namespace ShopQuanAo.Controllers

{
    [SessionState(SessionStateBehavior.Required)]
    public class CheckOutController : Controller
    {
        ShopQuanAoEntities db = new ShopQuanAoEntities();

        // GET: CheckOut
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (Session["MaUser"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thanh toán!";
                return RedirectToAction("DangNhap", "Authen");
            }

            // Lấy giỏ hàng từ Session
            var cart = GetCartFromSession();
            
            if (cart == null || cart.Count == 0)
            {
                ViewBag.ErrorMessage = "Giỏ hàng của bạn đang trống!";
            return View();
            }

            // Lấy thông tin sản phẩm trong giỏ hàng
            var cartItems = new List<CartItem>();
            decimal tongTien = 0;

            foreach (var item in cart)
            {
                var sanPham = db.QuanAos.Include(q => q.TheLoai).FirstOrDefault(q => q.MaQuanAo == item.Key);
                if (sanPham != null)
                {
                    var cartItem = new CartItem
                    {
                        MaQuanAo = sanPham.MaQuanAo,
                        TenQuanAo = sanPham.TenQuanAo,
                        HinhAnh = sanPham.HinhAnh,
                        Gia = sanPham.Gia,
                        SoLuong = item.Value,
                        ThanhTien = sanPham.Gia * item.Value
                    };
                    cartItems.Add(cartItem);
                    tongTien += cartItem.ThanhTien;
                }
            }

            ViewBag.CartItems = cartItems;
            ViewBag.TongTien = tongTien;

            // Nếu user đã đăng nhập, điền sẵn thông tin
            var khachHang = new KhachHang();
            if (Session["MaUser"] != null)
            {
                int maUser = (int)Session["MaUser"];   // 🔥 FIX Ở ĐÂY
                var user = db.Users.FirstOrDefault(u => u.MaUser == maUser);
                if (user != null)
                {
                    khachHang.HoTenKhachHang = user.HoTen;
                    khachHang.Email = user.Email;
                    khachHang.DienThoai = user.DienThoai;
                }
            }

            return View(khachHang);
        }

        // POST: CheckOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(KhachHang khachHang)
        {
            // Kiểm tra đăng nhập
            if (Session["MaUser"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thanh toán!";
                return RedirectToAction("DangNhap", "Authen");
            }

            try
            {
                // Lấy giỏ hàng từ Session
                var cart = GetCartFromSession();
                
                if (cart == null || cart.Count == 0)
                {
                    ViewBag.ErrorMessage = "Giỏ hàng của bạn đang trống!";
                    return View(khachHang);
                }

                // Validate thông tin khách hàng
                if (string.IsNullOrEmpty(khachHang.HoTenKhachHang))
                {
                    ModelState.AddModelError("HoTenKhachHang", "Vui lòng nhập họ tên khách hàng");
                }
                if (string.IsNullOrEmpty(khachHang.DienThoai))
                {
                    ModelState.AddModelError("DienThoai", "Vui lòng nhập số điện thoại");
                }
                if (string.IsNullOrEmpty(khachHang.DiaChi))
                {
                    ModelState.AddModelError("DiaChi", "Vui lòng nhập địa chỉ");
                }

                if (!ModelState.IsValid)
                {
                    // Hiển thị lại giỏ hàng
                    var cartItems = new List<CartItem>();
                    decimal tongTien = 0;

                    foreach (var item in cart)
                    {
                        var sanPham = db.QuanAos.Include(q => q.TheLoai).FirstOrDefault(q => q.MaQuanAo == item.Key);
                        if (sanPham != null)
                        {
                            var cartItem = new CartItem
                            {
                                MaQuanAo = sanPham.MaQuanAo,
                                TenQuanAo = sanPham.TenQuanAo,
                                HinhAnh = sanPham.HinhAnh,
                                Gia = sanPham.Gia,
                                SoLuong = item.Value,
                                ThanhTien = sanPham.Gia * item.Value
                            };
                            cartItems.Add(cartItem);
                            tongTien += cartItem.ThanhTien;
                        }
                    }

                    ViewBag.CartItems = cartItems;
                    ViewBag.TongTien = tongTien;
                    return View(khachHang);
                }

                // Tìm hoặc tạo khách hàng
                KhachHang khachHangDB = null;
                if (!string.IsNullOrEmpty(khachHang.Email))
                {
                    khachHangDB = db.KhachHangs.FirstOrDefault(k => k.Email == khachHang.Email);
                }

                if (khachHangDB == null)
                {
                    khachHangDB = new KhachHang
                    {
                        HoTenKhachHang = khachHang.HoTenKhachHang,
                        Email = khachHang.Email,
                        DienThoai = khachHang.DienThoai,
                        DiaChi = khachHang.DiaChi
                    };
                    db.KhachHangs.Add(khachHangDB);
                    db.SaveChanges();
                }
                else
                {
                    // Cập nhật thông tin khách hàng
                    khachHangDB.HoTenKhachHang = khachHang.HoTenKhachHang;
                    khachHangDB.DienThoai = khachHang.DienThoai;
                    khachHangDB.DiaChi = khachHang.DiaChi;
                    db.SaveChanges();
                }

                // Tính tổng tiền
                decimal tongTienDonHang = 0;
                foreach (var item in cart)
                {
                    var sanPham = db.QuanAos.FirstOrDefault(q => q.MaQuanAo == item.Key);
                    if (sanPham != null)
                    {
                        tongTienDonHang += sanPham.Gia * item.Value;
                    }
                }

                // Tạo đơn hàng
                var donHang = new DonHang
                {
                    NgayDatHang = DateTime.Now,
                    TongTien = tongTienDonHang,
                    TrangThaiDonHang = "Đang xử lý",
                    MaKhachHang = khachHangDB.MaKhachHang,
                    MaUserTaoDon = Session["MaUser"] != null ? (int?)Session["MaUser"] : null
                };

                db.DonHangs.Add(donHang);
                db.SaveChanges();

                // Tạo chi tiết đơn hàng
                foreach (var item in cart)
                {
                    var sanPham = db.QuanAos.FirstOrDefault(q => q.MaQuanAo == item.Key);
                    if (sanPham != null)
                    {
                        // Kiểm tra số lượng tồn kho
                        if (sanPham.SoLuongTon < item.Value)
                        {
                            ViewBag.ErrorMessage = $"Sản phẩm {sanPham.TenQuanAo} chỉ còn {sanPham.SoLuongTon} sản phẩm trong kho!";
                            db.DonHangs.Remove(donHang);
                            db.SaveChanges();
                            
                            var cartItems = new List<CartItem>();
                            decimal tongTien = 0;

                            foreach (var cartItem in cart)
                            {
                                var sp = db.QuanAos.Include(q => q.TheLoai).FirstOrDefault(q => q.MaQuanAo == cartItem.Key);
                                if (sp != null)
                                {
                                    var cartItemObj = new CartItem
                                    {
                                        MaQuanAo = sp.MaQuanAo,
                                        TenQuanAo = sp.TenQuanAo,
                                        HinhAnh = sp.HinhAnh,
                                        Gia = sp.Gia,
                                        SoLuong = cartItem.Value,
                                        ThanhTien = sp.Gia * cartItem.Value
                                    };
                                    cartItems.Add(cartItemObj);
                                    tongTien += cartItemObj.ThanhTien;
                                }
                            }

                            ViewBag.CartItems = cartItems;
                            ViewBag.TongTien = tongTien;
                            return View(khachHang);
                        }

                        var chiTietDonHang = new ChiTietDonHang
                        {
                            MaDonHang = donHang.MaDonHang,
                            MaQuanAo = item.Key,
                            SoLuong = item.Value,
                            GiaBan = sanPham.Gia
                        };

                        db.ChiTietDonHangs.Add(chiTietDonHang);

                        // Cập nhật số lượng tồn kho
                        sanPham.SoLuongTon -= item.Value;
                    }
                }

                db.SaveChanges();

                // Xóa giỏ hàng sau khi thanh toán thành công
                Session["Cart"] = null;

                // Chuyển đến trang xác nhận
                return RedirectToAction("ThanhCong", new { id = donHang.MaDonHang });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi xử lý đơn hàng: " + ex.Message;
                
                // Hiển thị lại giỏ hàng
                var cart = GetCartFromSession();
                if (cart != null && cart.Count > 0)
                {
                    var cartItems = new List<CartItem>();
                    decimal tongTien = 0;

                    foreach (var item in cart)
                    {
                        var sanPham = db.QuanAos.Include(q => q.TheLoai).FirstOrDefault(q => q.MaQuanAo == item.Key);
                        if (sanPham != null)
                        {
                            var cartItem = new CartItem
                            {
                                MaQuanAo = sanPham.MaQuanAo,
                                TenQuanAo = sanPham.TenQuanAo,
                                HinhAnh = sanPham.HinhAnh,
                                Gia = sanPham.Gia,
                                SoLuong = item.Value,
                                ThanhTien = sanPham.Gia * item.Value
                            };
                            cartItems.Add(cartItem);
                            tongTien += cartItem.ThanhTien;
                        }
                    }

                    ViewBag.CartItems = cartItems;
                    ViewBag.TongTien = tongTien;
                }

                return View(khachHang);
            }
        }

        // GET: CheckOut/ThanhCong
        public ActionResult ThanhCong(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var donHang = db.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.ChiTietDonHangs)
                .Include(d => d.ChiTietDonHangs.Select(c => c.QuanAo))
                .FirstOrDefault(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(donHang);
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

    // Class helper để hiển thị giỏ hàng
    public class CartItem
    {
        public int MaQuanAo { get; set; }
        public string TenQuanAo { get; set; }
        public string HinhAnh { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }
}