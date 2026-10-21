using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using ShopQuanAo.Models;

namespace ShopQuanAo.Controllers
{
    public class HomeController : Controller
    {
        ShopQuanAoEntities db = new ShopQuanAoEntities();
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult Header()
        {
            return PartialView();
        }
        public PartialViewResult Banner()
        {
            return PartialView();
        }
        public PartialViewResult BannerSection()
        {
            return PartialView();
        }
        public PartialViewResult WomenBanner()
        {
            var womenProducts = db.QuanAos
        .Include(q => q.TheLoai)
        .Where(q => q.GioiTinh == "Nữ")
        .OrderBy(q => Guid.NewGuid())
        .Take(8)
        .ToList();

            return PartialView(womenProducts);
        }
        public PartialViewResult Deal()
        {
            return PartialView();
        }
        public PartialViewResult ManBanner()
        {
            var manProducts = db.QuanAos
        .Include(q => q.TheLoai)
        .Where(q => q.GioiTinh == "Nam")
        .OrderBy(q => Guid.NewGuid())
        .Take(8)
        .ToList();

            return PartialView(manProducts);
        }
        public PartialViewResult InsPhoto()
        {
            return PartialView();
        }
        public PartialViewResult Blog()
        {
            return PartialView();
        }
        public PartialViewResult Logo()
        {
            return PartialView();
        }
        public PartialViewResult Footer()
        {
            return PartialView();
        }
        public PartialViewResult Shop()
        { 
            return PartialView();
        }
        public PartialViewResult Chitietsanphamnu()
        {
            return PartialView();
        }
        public PartialViewResult Lienhe()
        {
            return PartialView();
        }
        public PartialViewResult Login()
        {
            return PartialView();
        }
        public PartialViewResult Shopaokhoacn()
        {
            return PartialView();
        }
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
    
}