using AutoMapper;
using Fintrellis.MongoDb.Interfaces;
using Fintrellis.Redis.Interfaces;
using Fintrellis.Services.Interfaces;
using Fintrellis.Services.Models;
using Fintrellis.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace Fintrellis.Services.Tests.Services
{
    public class PostServiceTests
    {
        private readonly Mock<ILogger<PostService>> _loggerMock;
        private readonly Mock<IRepository<Post>> _repositoryMock;
        private readonly Mock<IRetryHandler> _retryHandlerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly PostService _postService;

        public PostServiceTests()
        {
            _loggerMock = new Mock<ILogger<PostService>>();
            _repositoryMock = new Mock<IRepository<Post>>();
            _mapperMock = new Mock<IMapper>();
            _retryHandlerMock = new Mock<IRetryHandler>();
            _cacheServiceMock = new Mock<ICacheService>();

            _retryHandlerMock.Setup(r => r.ExecuteWithRetryAsync(It.IsAny<Func<Task>>()))
                 .Callback<Func<Task>>(async action => await action())
                 .Returns(Task.CompletedTask);

            _postService = new PostService(
                _loggerMock.Object,
                _repositoryMock.Object,
                _mapperMock.Object,
                _retryHandlerMock.Object,
                _cacheServiceMock.Object);
        }

        #region GetPostsAsync

        [Fact]
        public async Task GetPostsAsync_WithCachedPost_ReturnsCachedPost()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var cachedPost = new Post { PostId = postId };
            _cacheServiceMock.Setup(c => c.GetAsync<Post>(postId.ToString())).ReturnsAsync(cachedPost);

            // Act
            var result = await _postService.GetPostsAsync(postId);

            // Assert
            result.Should().ContainSingle(p => p.PostId == postId);
            _cacheServiceMock.Verify(c => c.GetAsync<Post>(postId.ToString()), Times.Once);
            _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<Post, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetPostsAsync_WithoutCachedPost_FetchesFromRepository()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var expectedPosts = new List<Post> { new Post { PostId = postId } };
            _cacheServiceMock.Setup(c => c.GetAsync<Post>(postId.ToString())).ReturnsAsync((Post?)null);
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Post, bool>>>()))
                           .ReturnsAsync(expectedPosts);

            // Act
            var result = await _postService.GetPostsAsync(postId);

            // Assert
            result.Should().ContainSingle(p => p.PostId == postId);
            _cacheServiceMock.Verify(c => c.GetAsync<Post>(postId.ToString()), Times.Once);
            _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<Post, bool>>>()), Times.Once);
        }

        #endregion

        #region AddNewPostAsync

        [Fact]
        public async Task AddNewPostAsync_WithValidPost_CachesAndReturnsPost()
        {
            // Arrange
            var postRequest = new PostCreateRequest { Title = "New Post", Content = "Content" };
            var mappedPost = new Post { PostId = Guid.NewGuid(), Title = postRequest.Title, Content = postRequest.Content };

            _mapperMock.Setup(m => m.Map<Post>(postRequest)).Returns(mappedPost);
            _repositoryMock.Setup(r => r.InsertOneAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            // Act
            var result = await _postService.AddNewPostAsync(postRequest);

            // Assert
            result.Should().NotBeNull();
            _retryHandlerMock.Verify(r => r.ExecuteWithRetryAsync(It.IsAny<Func<Task>>()), Times.Once);
            _repositoryMock.Verify(r => r.InsertOneAsync(It.IsAny<Post>()), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync(mappedPost.PostId.ToString(), mappedPost, It.IsAny<DateTimeOffset>()), Times.Once);
        }

        #endregion

        #region UpdatePostAsync

        [Fact]
        public async Task UpdatePostAsync_WithExistingPost_CachesAndReturnsUpdatedPost()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var existingPost = new Post { PostId = postId, Title = "Old Title" };
            var postUpdateRequest = new PostUpdateRequest { Title = "Updated Title" };

            _cacheServiceMock.Setup(c => c.GetAsync<Post>(postId.ToString())).ReturnsAsync(existingPost);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            // Act
            var result = await _postService.UpdatePostAsync(postId, postUpdateRequest);

            // Assert
            result.Should().NotBeNull();
            _retryHandlerMock.Verify(r => r.ExecuteWithRetryAsync(It.IsAny<Func<Task>>()), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Post>()), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveData(postId.ToString()), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync(postId.ToString(), existingPost, It.IsAny<DateTimeOffset>()), Times.Once);
        }

        #endregion

        #region DeletePostAsync

        [Fact]
        public async Task DeletePostAsync_WithValidPostId_RemovesFromCacheAndDeletes()
        {
            // Arrange
            var postId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.DeleteOneAsync(It.IsAny<Expression<Func<Post, bool>>>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _postService.DeletePostAsync(postId);

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.DeleteOneAsync(It.IsAny<Expression<Func<Post, bool>>>()), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveData(postId.ToString()), Times.Once);
        }

        #endregion
    }

}
