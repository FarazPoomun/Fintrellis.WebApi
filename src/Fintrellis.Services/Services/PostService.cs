using AutoMapper;
using Fintrellis.Services.Extensions;
using Fintrellis.Services.Interfaces;
using Fintrellis.Services.Models;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Fintrellis.Services.Services
{
    /// <summary>
    /// Service to manage posts
    /// </summary>
    public class PostService(ILogger<PostService> logger, ICachedRepository<Post> repository, IMapper mapper, IRetryHandler retryHandler) : IPostService
    {
        public async Task<IReadOnlyList<Post>?> GetPostsAsync(Guid? postId = null)
        {
            try
            {
                Expression<Func<Post, bool>>? predicate = postId == null ? null : (z) => z.PostId == postId;
                return await repository.GetAllAsync(postId.ToString(), predicate) ?? [];
            }
            catch (Exception ex)
            {
                logger.LogErrorMessage("Failed to get posts", ex);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<Post?> AddNewPostAsync(PostCreateRequest post)
        {
            try
            {
                var mappedPost = mapper.Map<Post>(post);
                var postId = Guid.NewGuid();
                mappedPost.PostId = postId;

                await InvokeWithPollyRetry(async () => await repository.InsertOneAsync(postId.ToString(), mappedPost));
                return mappedPost;
            }
            catch (Exception ex)
            {
                logger.LogErrorMessage("Failed to create new post", ex);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<Post?> UpdatePostAsync(Guid postId, PostUpdateRequest post)
        {
            try
            {
                var entity =   await repository.GetFirstOrDefaultAsync(postId.ToString(), x => x.PostId == postId);
                if (entity == null)
                {
                    return null;
                }

                mapper.Map(post, entity);

                await InvokeWithPollyRetry(async () => await repository.UpdateAsync(postId.ToString(), entity));
                return entity;
            }
            catch (Exception ex)
            {
                logger.LogErrorMessage($"Failed to update post with id {postId}", ex);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeletePostAsync(Guid postId)
        {
            try
            {
                await InvokeWithPollyRetry(async () => await repository.DeleteOneAsync(postId.ToString(), z => z.PostId == postId));
                return true;
            }
            catch (Exception ex)
            {
                logger.LogErrorMessage($"Failed to delete post with id {postId}", ex);
                return false;
            }
        }

        /// <inheritdoc/>
        private async Task InvokeWithPollyRetry(Func<Task> action)
        {
            await retryHandler.ExecuteWithRetryAsync(action);
        }
    }
}
