using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class ManageForumModel : PageModel
    {
        private readonly ForumService _forumService;

        public ManageForumModel(ForumService forumService)
        {
            _forumService = forumService;
        }

        public List<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString,
            string categoryFilter, string flagFilter, int? pageIndex)
        {
            // Save current filters in ViewData
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CategoryFilter"] = categoryFilter;
            ViewData["FlagFilter"] = flagFilter;

            // Setup sort field and direction
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "date_desc"; // Default sort
            }

            ViewData["SortField"] = sortOrder.Contains("date") ? "date" : "title";
            ViewData["SortDirection"] = sortOrder.EndsWith("_desc") ? "desc" : "asc";

            // Handle search and pagination
            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            // Set page index
            PageIndex = pageIndex ?? 1;

            // Fetch data from database
            var forumPosts = await _forumService.GetForumPostsAsync(includeHidden: true);

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                forumPosts = forumPosts.Where(p =>
                    p.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.Content.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.UserName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                forumPosts = forumPosts.Where(p => p.Category == categoryFilter).ToList();
            }

            if (!string.IsNullOrEmpty(flagFilter))
            {
                forumPosts = flagFilter switch
                {
                    "Flagged" => forumPosts.Where(p => p.IsFlagged).ToList(),
                    "Hidden" => forumPosts.Where(p => !p.IsVisible).ToList(),
                    "Visible" => forumPosts.Where(p => p.IsVisible).ToList(),
                    _ => forumPosts
                };
            }

            // Apply sorting
            forumPosts = sortOrder switch
            {
                "date" => forumPosts.OrderBy(p => p.PostedDate).ToList(),
                "date_desc" => forumPosts.OrderByDescending(p => p.PostedDate).ToList(),
                "title" => forumPosts.OrderBy(p => p.Title).ToList(),
                "title_desc" => forumPosts.OrderByDescending(p => p.Title).ToList(),
                _ => forumPosts.OrderByDescending(p => p.PostedDate).ToList()
            };

            // Calculate total count for pagination
            TotalCount = forumPosts.Count;

            // Paginate
            ForumPosts = forumPosts
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostHidePostAsync(int id, string adminNotes)
        {
            var post = await _forumService.GetForumPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            await _forumService.TogglePostVisibilityAsync(id, adminNotes);

            TempData["StatusMessage"] = "Forum post has been hidden.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestorePostAsync(int id)
        {
            var post = await _forumService.GetForumPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            await _forumService.TogglePostVisibilityAsync(id);

            TempData["StatusMessage"] = "Forum post has been restored and is now visible.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFlagPostAsync(int id)
        {
            var post = await _forumService.GetForumPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            await _forumService.TogglePostFlagAsync(id);

            TempData["StatusMessage"] = "Forum post has been flagged for review.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnflagPostAsync(int id)
        {
            var post = await _forumService.GetForumPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            await _forumService.TogglePostFlagAsync(id);

            TempData["StatusMessage"] = "Flag has been removed from the forum post.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePostAsync(int id)
        {
            var post = await _forumService.GetForumPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            await _forumService.DeleteForumPostAsync(id);

            TempData["StatusMessage"] = "Forum post has been permanently deleted.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }
    }
}