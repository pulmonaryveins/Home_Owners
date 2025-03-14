using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using HomeOwners.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public class UserViewModel
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public List<string> Roles { get; set; } = new List<string>();
            public DateTime JoinDate { get; set; }
        }

        public PaginatedList<UserViewModel> Users { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentFilter { get; set; }
        public string RoleFilter { get; set; }
        public string NameSort { get; set; }
        public string EmailSort { get; set; }
        public string DateSort { get; set; }
        public string SortField { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, string roleFilter, int? pageIndex)
        {
            CurrentSort = sortOrder;
            RoleFilter = roleFilter;

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            NameSort = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            EmailSort = sortOrder == "email" ? "email_desc" : "email";
            DateSort = sortOrder == "date" ? "date_desc" : "date";

            // Parse sort order
            if (sortOrder == "name_desc")
            {
                SortField = "name";
                SortDirection = "desc";
            }
            else if (sortOrder == "email")
            {
                SortField = "email";
                SortDirection = "asc";
            }
            else if (sortOrder == "email_desc")
            {
                SortField = "email";
                SortDirection = "desc";
            }
            else if (sortOrder == "date")
            {
                SortField = "date";
                SortDirection = "asc";
            }
            else if (sortOrder == "date_desc")
            {
                SortField = "date";
                SortDirection = "desc";
            }
            else
            {
                SortField = "name";
                SortDirection = "asc";
            }

            // Get all users
            var users = _userManager.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString));
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(u => u.UserName);
                    break;
                case "email":
                    users = users.OrderBy(u => u.Email);
                    break;
                case "email_desc":
                    users = users.OrderByDescending(u => u.Email);
                    break;
                case "date":
                    users = users.OrderBy(u => u.CreatedDate);
                    break;
                case "date_desc":
                    users = users.OrderByDescending(u => u.CreatedDate);
                    break;
                default:
                    users = users.OrderBy(u => u.UserName);
                    break;
            }

            // Get all users with their roles
            var userList = await users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Apply role filter
                if (!string.IsNullOrEmpty(roleFilter) && !roles.Contains(roleFilter))
                {
                    continue;
                }

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    JoinDate = user.CreatedDate ?? DateTime.Now
                });
            }

            int pageSize = 10;
            Users = PaginatedList<UserViewModel>.Create(userViewModels.AsQueryable(), pageIndex ?? 1, pageSize);
        }
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
