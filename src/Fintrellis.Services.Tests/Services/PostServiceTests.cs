using AutoMapper;
using Fintrellis.MongoDb.Interfaces;
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
        private readonly PostService _postService;

        public PostServiceTests()
        {
            _loggerMock = new Mock<ILogger<PostService>>();
            _repositoryMock = new Mock<IRepository<Post>>();
            _mapperMock = new Mock<IMapper>();
            _retryHandlerMock = new Mock<IRetryHandler>();

            _retryHandlerMock.Setup(r => r.ExecuteWithRetryAsync(It.IsAny<Func<Task>>()))
                 .Callback<Func<Task>>(async action => await action())
                 .Returns(Task.CompletedTask);

            _postService = new PostService(_loggerMock.Object, _repositoryMock.Object, _mapperMock.Object, _retryHandlerMock.Object);
        }

        #region GetPostsAsync

        [Fact]
        public async Task GetPostsAsync_WithPostId_ReturnsMatchingPost()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var expectedPosts = new List<Post> { new Post { PostId = postId } };
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Post, bool>>>()))
                           .ReturnsAsync(expectedPosts);

            // Act
            var result = await _postService.GetPostsAsync(postId);

            // Assert
            result.Should().ContainSingle(p => p.PostId == postId);
            _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<Post, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetPostsAsync_WithoutPostId_ReturnsAllPosts()
        {
            // Arrange
            var expectedPosts = new List<Post>
        {
            new Post { PostId = Guid.NewGuid() },
            new Post { PostId = Guid.NewGuid() }
        };
            _repositoryMock.Setup(r => r.GetAllAsync(null))
                           .ReturnsAsync(expectedPosts);

            // Act
            var result = await _postService.GetPostsAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedPosts);
            _repositoryMock.Verify(r => r.GetAllAsync(null), Times.Once);
        }

        #endregion

        #region AddNewPostAsync

        [Fact]
        public async Task AddNewPostAsync_WithValidPost_CreatesAndReturnsPost()
        {
            // Arrange
            var postRequest = new PostCreateRequest
            {
                Title = "New Post",
                Content = "Content",
                PublishedDate = DateTime.UtcNow,
                Author = "Author"
            };

            var mappedPost = new Post
            {
                Title = postRequest.Title,
                Content = postRequest.Content,
                PublishedDate = postRequest.PublishedDate,
                Author = postRequest.Author
            };

            _mapperMock.Setup(m => m.Map<Post>(postRequest)).Returns(mappedPost);

            _repositoryMock.Setup(r => r.InsertOneAsync(It.IsAny<Post>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _postService.AddNewPostAsync(postRequest);

            // Assert
            result.Should().NotBeNull();
            result!.PostId.Should().NotBeEmpty();
            result.Title.Should().Be(postRequest.Title);

            _retryHandlerMock.Verify(r => r.ExecuteWithRetryAsync(It.IsAny<Func<Task>>()), Times.Once); 
            _repositoryMock.Verify(r => r.InsertOneAsync(It.IsAny<Post>()), Times.Once); 
        }

        #endregion

        #region UpdatePostAsync

        [Fact]
        public async Task UpdatePostAsync_WithExistingPostId_UpdatesAndReturnsPost()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var existingPost = new Post { PostId = postId, Title = "Old Title" };
            var postUpdateRequest = new PostUpdateRequest { Title = "Updated Title" };

            _repositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Post, bool>>>()))
                           .ReturnsAsync(existingPost);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            // Act
            var result = await _postService.UpdatePostAsync(postId, postUpdateRequest);

            // Assert
            result.Should().NotBeNull();
            _retryHandlerMock.Verify(r => r.ExecuteWithRetryAsync(It.IsAny<Func<Task>>()), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Post>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePostAsync_WithNonExistingPostId_ReturnsNull()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var postUpdateRequest = new PostUpdateRequest { Title = "Updated Title" };

            _repositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Post, bool>>>()))
                           .ReturnsAsync((Post?)null);

            // Act
            var result = await _postService.UpdatePostAsync(postId, postUpdateRequest);

            // Assert
            result.Should().BeNull();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Post>()), Times.Never);
        }

        #endregion

        #region DeletePostAsync

        [Fact]
        public async Task DeletePostAsync_WithValidPostId_DeletesPost()
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
        }

        #endregion
    }
}
