using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Cust.Areas.Identity.Data;
using Cust.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cust.Pages.AppUsers
{
    public class DisplayItem
    {
        public CustUser UserItem { get; set; }
        public bool HasAdminClaim { get; set; }
        public DisplayItem(CustUser userItem, bool hasAdminClaim)
        {
            UserItem = userItem;
            HasAdminClaim = hasAdminClaim;
        }
    }
    [Authorize("CanManageUsers")]
    public class IndexModel : PageModel
    {
        public IList<CustUser> AppUser { get; set; } = default!; //
        public IList<DisplayItem> Users { get; set; } //

        readonly UserManager<CustUser> _userManager;
        private readonly Cust.Data.CustContext _context;

        public IndexModel(UserManager<CustUser> userManager,
            Cust.Data.CustContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            if (_context.CustUser != null)
            {
                //AppUser = await _context.CustUser.ToListAsync();
                AppUser = await _context.CustUser.ToListAsync();
                Users = new List<DisplayItem>();

                foreach (var u in AppUser)
                {
                    var claim = _userManager.GetClaimsAsync(u).Result.Where(c => c.Type == "IsAdmin").FirstOrDefault();
                    bool hasAdminClaim = claim != null;
                    //bool hasAdminClaim = claim != null ? true : false;
                    Users.Add(new DisplayItem(u, hasAdminClaim));
                }
            }
        }
    }
}

