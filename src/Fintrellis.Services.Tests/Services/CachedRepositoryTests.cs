using Fintrellis.MongoDb.Interfaces;
using Fintrellis.MongoDb.Models;
using Fintrellis.Redis.Interfaces;
using Fintrellis.Services.Services;
using Moq;
using System.Linq.Expressions;

namespace Fintrellis.Services.Tests.Services
{
    public class CachedRepositoryTests
    {
        private readonly Mock<IRepository<TestEntity>> _repositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly CachedRepository<TestEntity> _cachedRepository;

        public CachedRepositoryTests()
        {
            _repositoryMock = new Mock<IRepository<TestEntity>>();
            _cacheServiceMock = new Mock<ICacheService>();
            _cachedRepository = new CachedRepository<TestEntity>(_repositoryMock.Object, _cacheServiceMock.Object);
        }

        [Fact]
        public async Task InsertOneAsync_ShouldCallInsertOnRepositoryAndCacheEntity()
        {
            // Arrange
            var entity = new TestEntity { Id = Guid.NewGuid().ToString() };
            var cacheId = "cacheKey";

            // Act
            await _cachedRepository.InsertOneAsync(cacheId, entity);

            // Assert
            _repositoryMock.Verify(r => r.InsertOneAsync(entity), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync(cacheId, entity, It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCachedEntity_WhenCacheExists()
        {
            // Arrange
            var entity = new TestEntity { Id = Guid.NewGuid().ToString() };
            var cacheId = "cacheKey";

            _cacheServiceMock.Setup(c => c.GetAsync<TestEntity>(cacheId)).ReturnsAsync(entity);

            // Act
            var result = await _cachedRepository.GetAllAsync(cacheId);

            // Assert
            Assert.Single(result);
            Assert.Equal(entity, result.First());
            _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFetchFromRepositoryAndCacheEntity_WhenCacheDoesNotExist()
        {
            // Arrange
            var cacheId = "cacheKey";
            var entities = new List<TestEntity> { new TestEntity { Id = Guid.NewGuid().ToString() } };

            _cacheServiceMock.Setup(c => c.GetAsync<TestEntity>(cacheId)).ReturnsAsync((TestEntity)null);
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<TestEntity, bool>>>())).ReturnsAsync(entities);

            // Act
            var result = await _cachedRepository.GetAllAsync(cacheId);

            // Assert
            Assert.Equal(entities, result);
            _cacheServiceMock.Verify(c => c.SetAsync(cacheId, entities.First(), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Fact]
        public async Task GetFirstOrDefaultAsync_ShouldReturnCachedEntity_WhenCacheExists()
        {
            // Arrange
            var entity = new TestEntity { Id = Guid.NewGuid().ToString() };
            var cacheId = "cacheKey";

            _cacheServiceMock.Setup(c => c.GetAsync<TestEntity>(cacheId)).ReturnsAsync(entity);

            // Act
            var result = await _cachedRepository.GetFirstOrDefaultAsync(cacheId);

            // Assert
            Assert.Equal(entity, result);
            _repositoryMock.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetFirstOrDefaultAsync_ShouldFetchFromRepositoryAndCacheEntity_WhenCacheDoesNotExist()
        {
            // Arrange
            var cacheId = "cacheKey";
            var entity = new TestEntity { Id = Guid.NewGuid().ToString() };

            _cacheServiceMock.Setup(c => c.GetAsync<TestEntity>(cacheId)).ReturnsAsync((TestEntity)null);
            _repositoryMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<TestEntity, bool>>>())).ReturnsAsync(entity);

            // Act
            var result = await _cachedRepository.GetFirstOrDefaultAsync(cacheId);

            // Assert
            Assert.Equal(entity, result);
            _cacheServiceMock.Verify(c => c.SetAsync(cacheId, entity, It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Fact]
        public async Task DeleteOneAsync_ShouldCallDeleteOnRepositoryAndRemoveFromCache()
        {
            // Arrange
            var cacheId = "cacheKey";
            Expression<Func<TestEntity, bool>> predicate = x => x.Id == Guid.NewGuid().ToString();

            // Act
            await _cachedRepository.DeleteOneAsync(cacheId, predicate);

            // Assert
            _repositoryMock.Verify(r => r.DeleteOneAsync(predicate), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveData(cacheId), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallUpdateOnRepository_RemoveFromCache_AndCacheEntity()
        {
            // Arrange
            var entity = new TestEntity { Id = Guid.NewGuid().ToString() };
            var cacheId = "cacheKey";

            // Act
            await _cachedRepository.UpdateAsync(cacheId, entity);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(entity), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveData(cacheId), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync(cacheId, entity, It.IsAny<DateTimeOffset>()), Times.Once);
        }

        public class TestEntity : Entity
        {
        }
    }
}
