// Services/ForumService.cs
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeOwners.Services
{
    public class ForumService
    {
        private readonly HomeDbContext _context;

        public ForumService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<ForumPost> TogglePostFlagAsync(int postId, string adminNotes = null)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
            {
                return null;
            }

            post.IsFlagged = !post.IsFlagged;

            if (!string.IsNullOrEmpty(adminNotes))
            {
                post.AdminNotes = adminNotes;
            }

            await _context.SaveChangesAsync();
            return post;
        }

        // Get all forum posts (visible only by default)
        public async Task<List<ForumPost>> GetForumPostsAsync(bool includeHidden = false, string category = null, string search = null, string sort = null)
        {
            IQueryable<ForumPost> query = _context.ForumPosts;

            // Apply visibility filter
            if (!includeHidden)
            {
                query = query.Where(p => p.IsVisible);
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Content.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.UserName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "Oldest First":
                        query = query.OrderByDescending(p => p.IsPinned).ThenBy(p => p.PostedDate);
                        break;
                    case "Most Comments":
                        // Join with comments to count and order by number of comments
                        return await query
                            .GroupJoin(
                                _context.ForumComments.Where(c => c.IsVisible),
                                post => post.Id,
                                comment => comment.ForumPostId,
                                (post, comments) => new { Post = post, CommentCount = comments.Count() })
                            .OrderByDescending(x => x.Post.IsPinned)  // First by pinned status
                            .ThenByDescending(x => x.CommentCount)    // Then by comment count
                            .Select(x => x.Post)
                            .ToListAsync();
                    case "Newest First":
                    default:
                        query = query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.PostedDate);
                        break;
                }
            }
            else
            {
                // Default sorting is newest first, with pinned posts first
                query = query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.PostedDate);
            }

            return await query.ToListAsync();
        }

        // Get forum posts by category
        public async Task<List<ForumPost>> GetForumPostsByCategoryAsync(string category, bool includeHidden = false)
        {
            if (includeHidden)
            {
                return await _context.ForumPosts
                    .Where(p => p.Category == category)
                    .OrderByDescending(p => p.PostedDate)
                    .ToListAsync();
            }
            else
            {
                return await _context.ForumPosts
                    .Where(p => p.Category == category && p.IsVisible)
                    .OrderByDescending(p => p.PostedDate)
                    .ToListAsync();
            }
        }

        // Get a single forum post by ID
        public async Task<ForumPost> GetForumPostByIdAsync(int id)
        {
            return await _context.ForumPosts
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Get forum posts by user ID
        public async Task<List<ForumPost>> GetForumPostsByUserIdAsync(string userId)
        {
            return await _context.ForumPosts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PostedDate)
                .ToListAsync();
        }

        // Toggle post pin status
        public async Task<ForumPost> TogglePostPinAsync(int postId)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
            {
                return null;
            }

            // Toggle the IsPinned property
            post.IsPinned = !post.IsPinned;
            await _context.SaveChangesAsync();
            return post;
        }

        // Get related posts based on category and tags
        public async Task<List<ForumPost>> GetRelatedPostsAsync(int postId, int count = 5)
        {
            var currentPost = await GetForumPostByIdAsync(postId);
            if (currentPost == null)
            {
                return new List<ForumPost>();
            }

            // Get posts from the same category, but not the current post
            var relatedPosts = await _context.ForumPosts
                .Where(p => p.Id != postId && p.Category == currentPost.Category && p.IsVisible)
                .OrderByDescending(p => p.PostedDate)
                .Take(count)
                .ToListAsync();

            // If we don't have enough related posts, get some recent posts
            if (relatedPosts.Count < count)
            {
                var additionalPostsNeeded = count - relatedPosts.Count;
                var existingIds = relatedPosts.Select(p => p.Id).ToList();
                existingIds.Add(postId); // Also exclude the current post

                var additionalPosts = await _context.ForumPosts
                    .Where(p => !existingIds.Contains(p.Id) && p.IsVisible)
                    .OrderByDescending(p => p.PostedDate)
                    .Take(additionalPostsNeeded)
                    .ToListAsync();

                relatedPosts.AddRange(additionalPosts);
            }

            return relatedPosts;
        }

        // Create a new forum post
        public async Task<ForumPost> CreateForumPostAsync(ForumPost post)
        {
            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        // Update an existing forum post
        public async Task<ForumPost> UpdateForumPostAsync(ForumPost post)
        {
            post.EditedDate = DateTime.Now;
            _context.Entry(post).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return post;
        }

        // Delete a forum post
        public async Task<bool> DeleteForumPostAsync(int postId)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
            {
                return false;
            }

            // Delete all comments related to this post
            var comments = await _context.ForumComments.Where(c => c.ForumPostId == postId).ToListAsync();
            _context.ForumComments.RemoveRange(comments);

            // Delete the post
            _context.ForumPosts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        // Toggle visibility of a forum post
        public async Task<ForumPost> TogglePostVisibilityAsync(int postId, string adminNotes = null)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
            {
                return null;
            }

            post.IsVisible = !post.IsVisible;

            if (!string.IsNullOrEmpty(adminNotes))
            {
                post.AdminNotes = adminNotes;
            }

            await _context.SaveChangesAsync();
            return post;
        }

        // Flag a post for moderation
        public async Task<ForumPost> FlagPostAsync(int postId, string adminNotes = null)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
            {
                return null;
            }

            post.IsFlagged = true;

            if (!string.IsNullOrEmpty(adminNotes))
            {
                post.AdminNotes = adminNotes;
            }

            await _context.SaveChangesAsync();
            return post;
        }

        // Get comments for a post
        public async Task<List<ForumComment>> GetCommentsForPostAsync(int postId, bool includeHidden = false)
        {
            IQueryable<ForumComment> query = _context.ForumComments.Where(c => c.ForumPostId == postId);

            if (!includeHidden)
            {
                query = query.Where(c => c.IsVisible);
            }

            return await query.OrderBy(c => c.PostedDate).ToListAsync();
        }

        // Add a comment to a post
        public async Task<ForumComment> AddCommentAsync(ForumComment comment)
        {
            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        // Update a comment
        public async Task<ForumComment> UpdateCommentAsync(ForumComment comment)
        {
            comment.EditedDate = DateTime.Now;
            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return comment;
        }

        // Delete a comment
        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.ForumComments.FindAsync(commentId);
            if (comment == null)
            {
                return false;
            }

            _context.ForumComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get comment by ID
        public async Task<ForumComment> GetCommentByIdAsync(int commentId)
        {
            return await _context.ForumComments.FindAsync(commentId);
        }

        // Get count of comments for each post
        public async Task<Dictionary<int, int>> GetCommentCountsAsync(List<int> postIds)
        {
            var commentCounts = await _context.ForumComments
                .Where(c => postIds.Contains(c.ForumPostId) && c.IsVisible)
                .GroupBy(c => c.ForumPostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToListAsync();

            return commentCounts.ToDictionary(x => x.PostId, x => x.Count);
        }

        // Get category counts for sidebar display
        public async Task<Dictionary<string, int>> GetCategoryCountsAsync()
        {
            var categoryCounts = await _context.ForumPosts
                .Where(p => p.IsVisible)
                .GroupBy(p => p.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            return categoryCounts.ToDictionary(x => x.Category, x => x.Count);
        }

        // Get popular posts for sidebar display
        public async Task<List<ForumPost>> GetPopularPostsAsync(int count = 5)
        {
            // Join with comments to count and order by number of comments
            var popularPosts = await _context.ForumPosts
                .Where(p => p.IsVisible)
                .GroupJoin(
                    _context.ForumComments.Where(c => c.IsVisible),
                    post => post.Id,
                    comment => comment.ForumPostId,
                    (post, comments) => new { Post = post, CommentCount = comments.Count() })
                .OrderByDescending(x => x.CommentCount)
                .Take(count)
                .Select(x => x.Post)
                .ToListAsync();

            return popularPosts;
        }

        // Search posts by keyword
        public async Task<List<ForumPost>> SearchPostsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<ForumPost>();
            }

            return await _context.ForumPosts
                .Where(p => p.IsVisible &&
                    (p.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.UserName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(p => p.PostedDate)
                .ToListAsync();
        }

        // Toggle comment visibility (for moderation)
        public async Task<ForumComment> ToggleCommentVisibilityAsync(int commentId)
        {
            var comment = await _context.ForumComments.FindAsync(commentId);
            if (comment == null)
            {
                return null;
            }

            comment.IsVisible = !comment.IsVisible;
            await _context.SaveChangesAsync();
            return comment;
        }
    }
}