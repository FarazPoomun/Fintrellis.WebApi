using AutoMapper;
using Fintrellis.MongoDb.Interfaces;
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
    public class PostService(ILogger<PostService> logger, IRepository<Post> repository, IMapper mapper, IRetryHandler retryHandler) : IPostService
    {
        public async Task<IEnumerable<Post>> GetPostsAsync(Guid? postId = null)
        {
            Expression<Func<Post, bool>>? predicate = null;

            if (postId != null)
            {
                predicate = z => z.PostId == postId;
            }

            var posts = await repository.GetAllAsync(predicate);
            return posts;
        }

        /// <inheritdoc/>
        public async Task<Post?> AddNewPostAsync(PostCreateRequest post)
        {
            try
            {
                var mappedPost = mapper.Map<Post>(post);
                mappedPost.PostId = Guid.NewGuid();

                await InvokeWithPollyRetry(async () => await repository.InsertOneAsync(mappedPost));

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
                var entity = await repository.GetFirstOrDefaultAsync(x => x.PostId == postId);

                if (entity == null)
                {
                    return null;
                }

                mapper.Map(post, entity);


                await InvokeWithPollyRetry(async () => await repository.UpdateAsync(entity));
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
                await InvokeWithPollyRetry(async () => await repository.DeleteOneAsync(z => z.PostId == postId));
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
