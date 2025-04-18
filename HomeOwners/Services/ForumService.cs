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

        // Get all forum posts (visible only by default)
        public async Task<List<ForumPost>> GetForumPostsAsync(bool includeHidden = false)
        {
            if (includeHidden)
            {
                return await _context.ForumPosts
                    .OrderByDescending(p => p.PostedDate)
                    .ToListAsync();
            }
            else
            {
                return await _context.ForumPosts
                    .Where(p => p.IsVisible)
                    .OrderByDescending(p => p.PostedDate)
                    .ToListAsync();
            }
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

        // Toggle visibility of a forum post
        public async Task<ForumPost> TogglePostVisibilityAsync(int postId, string adminNotes = null)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post != null)
            {
                post.IsVisible = !post.IsVisible;

                if (!string.IsNullOrEmpty(adminNotes))
                {
                    post.AdminNotes = adminNotes;
                }

                await _context.SaveChangesAsync();
            }
            return post;
        }

        // Flag or unflag a post
        public async Task<ForumPost> TogglePostFlagAsync(int postId)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post != null)
            {
                post.IsFlagged = !post.IsFlagged;
                await _context.SaveChangesAsync();
            }
            return post;
        }

        // Delete a forum post
        public async Task DeleteForumPostAsync(int postId)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post != null)
            {
                _context.ForumPosts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }

        // Get comments for a post
        public async Task<List<ForumComment>> GetCommentsForPostAsync(int postId, bool includeHidden = false)
        {
            if (includeHidden)
            {
                return await _context.ForumComments
                    .Where(c => c.ForumPostId == postId)
                    .OrderBy(c => c.PostedDate)
                    .ToListAsync();
            }
            else
            {
                return await _context.ForumComments
                    .Where(c => c.ForumPostId == postId && c.IsVisible)
                    .OrderBy(c => c.PostedDate)
                    .ToListAsync();
            }
        }

        // Add a comment
        public async Task<ForumComment> AddCommentAsync(ForumComment comment)
        {
            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        // Toggle comment visibility
        public async Task<ForumComment> ToggleCommentVisibilityAsync(int commentId)
        {
            var comment = await _context.ForumComments.FindAsync(commentId);
            if (comment != null)
            {
                comment.IsVisible = !comment.IsVisible;
                await _context.SaveChangesAsync();
            }
            return comment;
        }

        // Delete a comment
        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _context.ForumComments.FindAsync(commentId);
            if (comment != null)
            {
                _context.ForumComments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }
    }
}