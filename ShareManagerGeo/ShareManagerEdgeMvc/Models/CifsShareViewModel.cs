using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using PagedList;
using System.Web.Mvc;

namespace ShareManagerEdgeMvc.Models
{
    public class CifsShareViewModel : DbContext
    {
        //public int? Page { get; set; }
        //public int? PageCount { get; set; }
        //public int? PageNumber { get; set; }
        public IPagedList<CifsShare> SearchResults { get; set; }
        public IEnumerable<CifsShare> CifsShares { get; set; }
        public IEnumerable<CifsPermissionRequest> CifsPermissionRequests { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; } 
        public string SearchButton { get; set; }
    }
}