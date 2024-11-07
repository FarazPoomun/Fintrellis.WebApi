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
            var posts = await _postService.GetPostsAsync();
            return Ok(posts);
        }

        /// <summary>
        /// Get post by id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("{postId}")]
        public async Task<ActionResult> GetPost(Guid postId)
        {
            var posts = await _postService.GetPostsAsync(postId);
            return Ok(posts);
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
                return BadRequest();
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates an post by id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="post"></param>
        /// <returns></returns>

        [HttpPut("{postId}")]
        public async Task<ActionResult> PutPosts(Guid postId, PostUpdateRequest post)
        {
            var result = await _postService.UpdatePostAsync(postId, post);

            if (result == null)
            {
                return BadRequest();
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes an post by id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpDelete("{postId}")]
        public async Task<ActionResult> DeletePosts(Guid postId)
        {
            await _postService.DeletePostAsync(postId);
            return Ok();
        }
    }
}
