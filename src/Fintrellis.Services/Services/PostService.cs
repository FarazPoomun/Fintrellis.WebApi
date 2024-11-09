using AutoMapper;
using Fintrellis.MongoDb.Interfaces;
using Fintrellis.Redis.Interfaces;
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
    public class PostService(ILogger<PostService> logger, IRepository<Post> repository, IMapper mapper, IRetryHandler retryHandler, ICacheService cacheService) : IPostService
    {
        public async Task<IReadOnlyList<Post>?> GetPostsAsync(Guid? postId = null)
        {
            try
            {
                Expression<Func<Post, bool>>? predicate = null;

                if (postId != null)
                {
                    predicate = z => z.PostId == postId;

                    // Only check cache if post id is provided 
                    // redis doesnt provide a native way to get all
                    var post = await cacheService.GetAsync<Post>(postId.ToString()!);

                    if (post != null)
                    {
                        return [post];
                    }
                }

                var posts = await repository.GetAllAsync(predicate);

                if (postId != null && posts.Count == 1)
                {
                    await CachePost(posts[0]);
                }

                return posts?? [];
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
                mappedPost.PostId = Guid.NewGuid();

                await InvokeWithPollyRetry(async () => await repository.InsertOneAsync(mappedPost));
                await CachePost(mappedPost);

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
                var entity = await cacheService.GetAsync<Post>(postId.ToString())??
                    await repository.GetFirstOrDefaultAsync(x => x.PostId == postId);

                if (entity == null)
                {
                    return null;
                }

                mapper.Map(post, entity);

                await InvokeWithPollyRetry(async () => await repository.UpdateAsync(entity));

                await cacheService.RemoveData(postId.ToString());
                await CachePost(entity);

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
                await cacheService.RemoveData(postId.ToString());
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
        
        private async Task CachePost(Post post)
        {
            await cacheService.SetAsync(post.PostId.ToString(), post, DateTimeOffset.UtcNow.AddMinutes(1));
        }
    }
}
