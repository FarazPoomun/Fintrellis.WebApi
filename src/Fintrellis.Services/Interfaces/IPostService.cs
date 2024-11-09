using Fintrellis.Services.Models;

namespace Fintrellis.Services.Interfaces
{
    /// <summary>
    /// Service to manage Posts
    /// </summary>
    public interface IPostService
    {
        /// <summary>
        /// Gets a list of Posts or optionally can be filtered by just one post Id
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<Post>?> GetPostsAsync(Guid? postId = null);

        /// <summary>
        /// Adds a new post
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        Task<Post?> AddNewPostAsync(PostCreateRequest post);

        /// <summary>
        /// updates an existing post
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        Task<Post?> UpdatePostAsync(Guid postId, PostUpdateRequest post);

        /// <summary>
        /// Deletes an post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        Task<bool> DeletePostAsync(Guid postId);
    }
}
