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
using HomeOwners.Models.Users;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace HomeOwners.Areas.Staff.Pages
{
    [Authorize(Policy = "RequireStaffRole")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public UsersModel(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender = null)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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

        public Dictionary<string, HomeOwnerUser> HomeOwnerDetails { get; set; } = new Dictionary<string, HomeOwnerUser>();
        public List<HomeOwnerUser> PendingHomeOwnerUsers { get; set; } = new List<HomeOwnerUser>();
        public int TotalHomeOwnerCount { get; set; }

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

            // Get all HomeOwner users directly from the role
            var homeOwners = await _userManager.GetUsersInRoleAsync("HomeOwner");

            // Store the count for display
            TotalHomeOwnerCount = homeOwners.Count;

            // Create HomeOwner user view models
            var userViewModels = new List<UserViewModel>();

            foreach (var user in homeOwners)
            {
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchString) &&
                    !user.UserName.Contains(searchString) &&
                    !user.Email.Contains(searchString))
                {
                    continue;
                }

                var roles = await _userManager.GetRolesAsync(user);
                DateTime joinDate = DateTime.Now; // Default value

                // Extract HomeOwner specific data
                if (user is HomeOwnerUser homeOwnerUser)
                {
                    joinDate = homeOwnerUser.CreatedDate;
                    HomeOwnerDetails[user.Id] = homeOwnerUser;

                    // Add pending HomeOwner users to the pending list
                    if (homeOwnerUser.AccountStatus == "pending")
                    {
                        PendingHomeOwnerUsers.Add(homeOwnerUser);
                    }
                }

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    JoinDate = joinDate
                });
            }

            // Apply sorting in memory
            switch (sortOrder)
            {
                case "name_desc":
                    userViewModels = userViewModels.OrderByDescending(u => u.UserName).ToList();
                    break;
                case "email":
                    userViewModels = userViewModels.OrderBy(u => u.Email).ToList();
                    break;
                case "email_desc":
                    userViewModels = userViewModels.OrderByDescending(u => u.Email).ToList();
                    break;
                case "date":
                    userViewModels = userViewModels.OrderBy(u => u.JoinDate).ToList();
                    break;
                case "date_desc":
                    userViewModels = userViewModels.OrderByDescending(u => u.JoinDate).ToList();
                    break;
                default:
                    userViewModels = userViewModels.OrderBy(u => u.UserName).ToList();
                    break;
            }

            int pageSize = 10;
            Users = PaginatedList<UserViewModel>.Create(userViewModels.AsQueryable(), pageIndex ?? 1, pageSize);
        }

        public async Task<IActionResult> OnPostApproveUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["StatusType"] = "Error";
                TempData["StatusMessage"] = "Invalid user ID.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["StatusType"] = "Error";
                TempData["StatusMessage"] = "User not found.";
                return RedirectToPage();
            }

            if (user is HomeOwnerUser homeOwnerUser)
            {
                homeOwnerUser.AccountStatus = "approved";
                var result = await _userManager.UpdateAsync(homeOwnerUser);

                if (result.Succeeded)
                {
                    // Send approval email notification
                    if (_emailSender != null)
                    {
                        await _emailSender.SendEmailAsync(
                            homeOwnerUser.Email,
                            "HomeOwners Account Approved",
                            $"Dear {homeOwnerUser.FullName},<br><br>" +
                            "Your account has been approved. You can now log in to the HomeOwners platform.<br><br>" +
                            "Regards,<br>The HomeOwners Association Team"
                        );
                    }

                    TempData["StatusType"] = "Success";
                    TempData["StatusMessage"] = $"Account for {homeOwnerUser.UserName} was approved successfully.";
                }
                else
                {
                    TempData["StatusType"] = "Error";
                    TempData["StatusMessage"] = "Error approving account: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                TempData["StatusType"] = "Error";
                TempData["StatusMessage"] = "User is not a HomeOwner.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectUserAsync(string userId, string rejectionReason)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["StatusType"] = "Error";
                TempData["StatusMessage"] = "Invalid user ID.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["StatusType"] = "Error";
                TempData["StatusMessage"] = "User not found.";
                return RedirectToPage();
            }

            if (user is HomeOwnerUser homeOwnerUser)
            {
                homeOwnerUser.AccountStatus = "rejected";
                homeOwnerUser.RejectionReason = rejectionReason ?? "";
                var result = await _userManager.UpdateAsync(homeOwnerUser);

                if (result.Succeeded)
                {
                    // Send rejection email notification
                    if (_emailSender != null)
                    {
                        string reasonText = string.IsNullOrEmpty(rejectionReason)
                            ? ""
                            : $"<br><br>Reason: {rejectionReason}";

                        await _emailSender.SendEmailAsync(
                            homeOwnerUser.Email,
                            "HomeOwners Account Application - Not Approved",
                            $"Dear {homeOwnerUser.FullName},<br><br>" +
                            "We regret to inform you that your account application has not been approved at this time." +
                            reasonText +
                            "<br><br>If you believe this is an error, please contact the administrator." +
                            "<br><br>Regards,<br>The HomeOwners Association Team"
                        );
                    }

                    TempData["StatusType"] = "Success";
                    TempData["StatusMessage"] = $"Account for {homeOwnerUser.UserName} was rejected.";
                }
                else
                {
                    TempData["StatusType"] = "Error";
                    TempData["StatusMessage"] = "Error rejecting account: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                TempData["StatusType"] = "Error";
                TempData["StatusMessage"] = "User is not a HomeOwner.";
            }

            return RedirectToPage();
        }
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
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