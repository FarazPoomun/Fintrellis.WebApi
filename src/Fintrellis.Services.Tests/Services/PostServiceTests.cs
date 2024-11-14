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
        private readonly Mock<ICachedRepository<Post>> _repositoryMock;
        private readonly Mock<IRetryHandler> _retryHandlerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PostService _postService;

        public PostServiceTests()
        {
            _loggerMock = new Mock<ILogger<PostService>>();
            _repositoryMock = new Mock<ICachedRepository<Post>>();
            _mapperMock = new Mock<IMapper>();
            _retryHandlerMock = new Mock<IRetryHandler>();

            _retryHandlerMock.Setup(r => r.ExecuteWithRetryAsync(It.IsAny<Func<Task>>()))
                 .Callback<Func<Task>>(async action => await action())
                 .Returns(Task.CompletedTask);

            _postService = new PostService(
                _loggerMock.Object,
                _repositoryMock.Object,
                _mapperMock.Object,
                _retryHandlerMock.Object);
        }

        [Fact]
        public async Task GetPostsAsync_ReturnsPosts_WhenSuccessful()
        {
            // Arrange
            var expectedPosts = new List<Post> { new Post { PostId = Guid.NewGuid() } };
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<string>(), It.IsAny<Expression<Func<Post, bool>>>()))
                           .ReturnsAsync(expectedPosts);

            // Act
            var result = await _postService.GetPostsAsync();

            // Assert
            Assert.Equal(expectedPosts, result);
        }

        [Fact]
        public async Task GetPostsAsync_ReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<string>(), It.IsAny<Expression<Func<Post, bool>>>()))
                           .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _postService.GetPostsAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddNewPostAsync_ReturnsPost_WhenSuccessful()
        {
            // Arrange
            var postCreateRequest = new PostCreateRequest();
            var mappedPost = new Post { PostId = Guid.NewGuid() };
            _mapperMock.Setup(m => m.Map<Post>(postCreateRequest)).Returns(mappedPost);

            // Act
            var result = await _postService.AddNewPostAsync(postCreateRequest);

            // Assert
            Assert.Equal(mappedPost, result);
            _repositoryMock.Verify(r => r.InsertOneAsync(It.IsAny<string>(), mappedPost), Times.Once);
        }

        [Fact]
        public async Task AddNewPostAsync_ReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            var postCreateRequest = new PostCreateRequest();
            _mapperMock.Setup(m => m.Map<Post>(postCreateRequest)).Throws(new Exception("Mapping failed"));

            // Act
            var result = await _postService.AddNewPostAsync(postCreateRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePostAsync_ReturnsUpdatedPost_WhenSuccessful()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var postUpdateRequest = new PostUpdateRequest();
            var existingPost = new Post { PostId = postId };
            _repositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<string>(), It.IsAny<Expression<Func<Post, bool>>>()))
                           .ReturnsAsync(existingPost);

            // Act
            var result = await _postService.UpdatePostAsync(postId, postUpdateRequest);

            // Assert
            Assert.Equal(existingPost, result);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<string>(), existingPost), Times.Once);
        }

        [Fact]
        public async Task UpdatePostAsync_ReturnsNull_WhenPostNotFound()
        {
            // Arrange
            var postId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<string>(), It.IsAny<Expression<Func<Post, bool>>>()))
                           .ReturnsAsync((Post)null);

            // Act
            var result = await _postService.UpdatePostAsync(postId, new PostUpdateRequest());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePostAsync_ReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            var postId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<string>(), It.IsAny<Expression<Func<Post, bool>>>()))
                           .Throws(new Exception("Database failure"));

            // Act
            var result = await _postService.UpdatePostAsync(postId, new PostUpdateRequest());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeletePostAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var postId = Guid.NewGuid();

            // Act
            var result = await _postService.DeletePostAsync(postId);

            // Assert
            Assert.True(result);
            _repositoryMock.Verify(r => r.DeleteOneAsync(It.IsAny<string>(), It.IsAny<Expression<Func<Post, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task DeletePostAsync_ReturnsFalse_WhenExceptionThrown()
        {
            // Arrange
            var postId = Guid.NewGuid();
            _retryHandlerMock.Setup(r => r.ExecuteWithRetryAsync(It.IsAny<Func<Task>>()))
                             .ThrowsAsync(new Exception("Retry handler failure"));

            // Act
            var result = await _postService.DeletePostAsync(postId);

            // Assert
            Assert.False(result);
        }

    }
}
