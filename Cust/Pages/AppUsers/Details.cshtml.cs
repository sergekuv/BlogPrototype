using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cust.Data;
using Cust.Models;
using Cust.Areas.Identity.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Cust.Pages.AppUsers
{
    public class DetailsModel : PageModel
    {
        public IList<Claim> Claims;
        SignInManager<CustUser> _signInManager;
        UserManager<CustUser> _userManager;

        private readonly Cust.Data.CustContext _context;
        public DetailsModel(UserManager<CustUser> userManager,
            SignInManager<CustUser> signInManager, Cust.Data.CustContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public CustUser AppUser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null || _context.CustUser == null)
            {
                return NotFound();
            }

            var user = await _context.CustUser.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                AppUser = user;
                //var u = _userManager.Users.Include(u => u.IsAdmin);
                //var claim = new Claim("Test", DateTime.Now.ToString());
                //await _userManager.AddClaimAsync(AppUser, claim);

                Claims = _userManager.GetClaimsAsync(user).Result;

            }
            return Page();

        }
    }
}
