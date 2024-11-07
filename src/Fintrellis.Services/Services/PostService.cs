using AutoMapper;
using Fintrellis.MongoDb.Interfaces;
using Fintrellis.Services.Interfaces;
using Fintrellis.Services.Models;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Fintrellis.Services.Services
{
    /// <summary>
    /// Service to manage posts
    /// </summary>
    public class PostService(ILogger<PostService> logger, IRepository<Post> repository, IMapper mapper) : IPostService
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

        public async Task<Post?> AddNewPostAsync(PostCreateRequest post)
        {
            var mappedPost = mapper.Map<Post>(post);
            mappedPost.PostId = Guid.NewGuid();
            await repository.InsertOneAsync(mappedPost);

            logger.LogInformation($"Adding new post with id {mappedPost.PostId}");
            return mappedPost;
        }

        public async Task<Post?> UpdatePostAsync(Guid postId, PostUpdateRequest post)
        {
            var entity = await repository.GetFirstOrDefaultAsync(x => x.PostId == postId);

            if (entity == null)
            {
                return null;
            }

            mapper.Map(post, entity);
            await repository.UpdateAsync(entity);

            return entity;
        }

        public async Task DeletePostAsync(Guid postId)
        {
            await repository.DeleteOneAsync(z => z.PostId == postId);
        }
    }
}
