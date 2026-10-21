using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ShopQuanAo.Models
{
    [MetadataType(typeof(QuanAoMetadata))]
    public partial class QuanAo
    {
    }

    public class QuanAoMetadata
    {
        [AllowHtml]
        public string MoTa { get; set; }
    }
}