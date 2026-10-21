using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using System.Web.Mvc;
using ShopQuanAo.Models;
using System.Web.UI;

namespace ShopQuanAo.Controllers
{
    public class NhomController : Controller
    {
        // GET: Nhom
        ShopQuanAoEntities db = new ShopQuanAoEntities();
        public ActionResult Index(string gioiTinh, int? page)
        {
            var ds = db.QuanAos.AsQueryable();   // lấy tất cả

            // Nếu có giới tính thì mới lọc
            if (!string.IsNullOrEmpty(gioiTinh))
            {
                ds = ds.Where(x => x.GioiTinh == gioiTinh);
            }
            int pageSize = 12; // mỗi trang 12 sản phẩm ~ 4 hàng x 3 cột
            int pageNumber = (page ?? 1);

            return View(ds.OrderBy(x => x.MaQuanAo).ToPagedList(pageNumber, pageSize));
        }
    }
}