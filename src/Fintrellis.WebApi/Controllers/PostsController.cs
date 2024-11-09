using Fintrellis.Services.Interfaces;
using Fintrellis.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fintrellis.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController(IPostService postService) : ControllerBase
    {
        private readonly IPostService _postService = postService;

        /// <summary>
        /// Gets a full list of all posts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetPosts()
        {
            var result = await _postService.GetPostsAsync();

            if (result == null)
            {
                return BadRequest("Failed to get posts.");
            }

            return Ok(result);
        }

        /// <summary>
        /// Get post by id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("{postId:guid}")]
        public async Task<ActionResult> GetPost(Guid postId)
        {
            var result = await _postService.GetPostsAsync(postId);

            if (result == null)
            {
                return BadRequest("Failed to get post.");
            }

            return Ok(result);
        }

        /// <summary>
        /// Creates an post
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> PostPosts(PostCreateRequest post)
        {
            var result = await _postService.AddNewPostAsync(post);

            if (result == null)
            {
                return BadRequest("Failed to create post.");
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates an post by id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="post"></param>
        /// <returns></returns>

        [HttpPut("{postId:guid}")]
        public async Task<ActionResult> PutPosts(Guid postId, PostUpdateRequest post)
        {
            var result = await _postService.UpdatePostAsync(postId, post);

            if (result == null)
            {
                return BadRequest("Failed to update post.");
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes an post by id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpDelete("{postId:guid}")]
        public async Task<ActionResult> DeletePosts(Guid postId)
        {
            var result = await _postService.DeletePostAsync(postId);

            if (!result)
            {
                return BadRequest("Failed to delete post.");
            }

            return Ok();
        }
    }
}
