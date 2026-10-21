using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopQuanAo.Models
{
    public class GioHangItem
    {
        public int MaQuanAo { get; set; }
        public string TenQuanAo { get; set; }
        public string HinhAnh { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }

        public decimal ThanhTien
        {
            get { return Gia * SoLuong; }
        }
    }
}