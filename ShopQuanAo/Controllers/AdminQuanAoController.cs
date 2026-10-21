using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopQuanAo.Models;

namespace ShopQuanAo.Controllers
{
    public class AdminQuanAoController : Controller
    {
        ShopQuanAoEntities db = new ShopQuanAoEntities();
        // GET: AdminQuanAo
        public ActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("DangNhap", "Authen");

            var list = db.QuanAos.Include("TheLoai").ToList();
            return View(list);
        }
        private bool IsAdmin()
        {
            return Session["VaiTro"] != null && Session["VaiTro"].ToString() == "Admin";
        }
        public ActionResult Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("DangNhap", "Authen");

            var sp = db.QuanAos.Find(id);
            if (sp == null) return HttpNotFound();

            ViewBag.MaTheLoai = new SelectList(db.TheLoais, "MaTheLoai", "TenTheLoai", sp.MaTheLoai);
            return View(sp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(QuanAo model, HttpPostedFileBase HinhAnhFile)
        {
            if (!IsAdmin())
                return RedirectToAction("DangNhap", "Authen");

            var sp = db.QuanAos.Find(model.MaQuanAo);
            if (sp == null) return HttpNotFound();

            sp.TenQuanAo = model.TenQuanAo;
            sp.Gia = model.Gia;
            sp.MoTa = model.MoTa;
            sp.SoLuongTon = model.SoLuongTon;
            sp.MaTheLoai = model.MaTheLoai;
            sp.GioiTinh = model.GioiTinh;

            if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
            {
                // xóa ảnh cũ
                var oldPath = Server.MapPath("~/Content/img/" + sp.HinhAnh);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                var fileName = DateTime.Now.Ticks + Path.GetExtension(HinhAnhFile.FileName);
                var path = Server.MapPath("~/Content/img/QuanAo/" + fileName);
                HinhAnhFile.SaveAs(path);

                sp.HinhAnh = "QuanAo/" + fileName;
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("DangNhap", "Authen");

            var sp = db.QuanAos.Find(id);
            if (sp == null) return HttpNotFound();

            var imgPath = Server.MapPath("~/Content/img/" + sp.HinhAnh);
            if (System.IO.File.Exists(imgPath))
                System.IO.File.Delete(imgPath);

            db.QuanAos.Remove(sp);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}