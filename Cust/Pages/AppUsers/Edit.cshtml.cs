using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cust.Data;
using Cust.Models;
using Cust.Areas.Identity.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Cust.Pages.AppUsers
{
    public class EditModel : PageModel
    {
        private readonly Cust.Data.CustContext _context;
        UserManager<CustUser> _userManager;
        Claim claimToEdit;
       // public bool CurrentAdminClaim;
        //bool InitialAdminClaim;

        public EditModel(UserManager<CustUser> userManager, Cust.Data.CustContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public CustUser AppUser { get; set; } = default!;
        [BindProperty]
        public bool CurrentAdminClaim { get; set; }
        [BindProperty]
        public bool InitialAdminClaim { get; set; }


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
            AppUser = user;

            claimToEdit = _userManager.GetClaimsAsync(user).Result.Where(c => c.Type == "IsAdmin").FirstOrDefault();
            InitialAdminClaim = claimToEdit != null ?  true : false;
            CurrentAdminClaim = InitialAdminClaim;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var userToUpdate = await _context.CustUser.FindAsync(AppUser.Id);


            if (userToUpdate == null)
            {
                return NotFound();
            }

            // Uses the posted form values from the PageContext property in the PageModel.
            // Updates only the properties listed(s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate).
            // Looks for form fields with a "AppUser" prefix.For example, Student.FirstMidName.It's not case sensitive
            if (await TryUpdateModelAsync<CustUser>(
                userToUpdate,
                "AppUser",
                s => s.Alias)) 

            {
                if (CurrentAdminClaim != InitialAdminClaim)

                {
                    if (CurrentAdminClaim == true)
                    {
                        Claim claim = new Claim("IsAdmin", "");
                        await _userManager.AddClaimAsync(userToUpdate, claim);
                    }
                    else
                    {
                        claimToEdit = _userManager.GetClaimsAsync(userToUpdate).Result.Where(c => c.Type == "IsAdmin").FirstOrDefault();
                        await _userManager.RemoveClaimAsync(userToUpdate, claimToEdit);
                    }
                }


                // _context.Attach(AppUser).State = EntityState.Modified;
                var i = await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        //public async Task<IActionResult> OnPostAsync()
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Page();
        //    }

        //    _context.Attach(AppUser).State = EntityState.Modified;
        //    //_context.Entry(AppUser).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!AppUserExists(AppUser.Id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return RedirectToPage("./Index");
        //}

        private bool AppUserExists(string id)
        {
            bool res = (_context.CustUser?.Any(e => e.Id == id)).GetValueOrDefault();
            return (_context.CustUser?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }
}
