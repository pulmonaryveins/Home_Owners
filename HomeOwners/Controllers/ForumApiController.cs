using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeOwners.Controllers.Api
{
    [Route("api/forum")]
    [ApiController]
    [Authorize(Policy = "RequireAdminRole")]
    public class ForumApiController : ControllerBase
    {
        private readonly ForumService _forumService;

        public ForumApiController(ForumService forumService)
        {
            _forumService = forumService;
        }

        [HttpGet("posts/{postId}/comments")]
        public async Task<ActionResult<List<ForumComment>>> GetCommentsForPost(int postId)
        {
            var comments = await _forumService.GetCommentsForPostAsync(postId, includeHidden: true);
            return Ok(comments);
        }
    }
}