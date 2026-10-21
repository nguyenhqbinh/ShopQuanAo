using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using ShopQuanAo.Models;

namespace ShopQuanAo.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class GioHangController : Controller
    {
        // GET: GioHang
        ShopQuanAoEntities db = new ShopQuanAoEntities();

        public ActionResult Index()
        {
            var cartDict = Session["Cart"] as Dictionary<int, int>;

            List<GioHangItem> gioHang = new List<GioHangItem>();

            if (cartDict != null)
            {
                foreach (var item in cartDict)
                {
                    var sp = db.QuanAos.FirstOrDefault(x => x.MaQuanAo == item.Key);
                    if (sp != null)
                    {
                        gioHang.Add(new GioHangItem
                        {
                            MaQuanAo = sp.MaQuanAo,
                            TenQuanAo = sp.TenQuanAo,
                            HinhAnh = sp.HinhAnh,
                            Gia = sp.Gia,
                            SoLuong = item.Value
                        });
                    }
                }
            }

            return View(gioHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var product = db.QuanAos.FirstOrDefault(x => x.MaQuanAo == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            if (quantity > product.SoLuongTon)
            {
                quantity = product.SoLuongTon;
            }

            var cartDict = Session["Cart"] as Dictionary<int, int>;
            if (cartDict == null)
            {
                cartDict = new Dictionary<int, int>();
            }

            cartDict[id] = quantity;
            Session["Cart"] = cartDict;

            // Tính lại line total và cart total
            decimal lineTotal = product.Gia * quantity;

            decimal cartTotal = 0;
            foreach (var item in cartDict)
            {
                var sp = db.QuanAos.FirstOrDefault(q => q.MaQuanAo == item.Key);
                if (sp != null)
                {
                    cartTotal += sp.Gia * item.Value;
                }
            }

            return Json(new
            {
                success = true,
                quantity = quantity,
                lineTotal = lineTotal,
                cartTotal = cartTotal,
                message = ""
            });
        }
    }
}