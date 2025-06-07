using ForumService.ForumService.Application;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using Moq;
using Xunit;

namespace ForumService.ForumService.Tests.UnitTests
{
    public class ForumThreadServiceTests
    {
        [Fact]
        public async Task GetThreadsByForumId_ReturnsMappedDtos()
        {
            // Arrange
            Guid forumId = Guid.NewGuid();
            var threads = new List<ForumThread>
            {
                new ForumThread
                {
                    Id = Guid.NewGuid(),
                    ForumId = forumId,
                    CreatorId = Guid.NewGuid(),
                    Title = "Test Title",
                    IsPinned = true,
                    Tags = new List<Tag>(),
                    Images = new List<string>(),
                    Content = "Test content",
                    Slug = "test-thread",
                    Upvote = 5,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now,
                }
            };

            var mockRepo = new Mock<IForumThreadRepository>();
            mockRepo.Setup(r => r.GetThreadsByForumIdAsync(forumId))
                .ReturnsAsync(threads);

            var mockUow = new Mock<IUnitOfWork>();
            mockUow.Setup(u => u.ForumThreads).Returns(mockRepo.Object);

            // Act
            var service = new ForumThreadService(mockUow.Object);
            var result = await service.GetThreadsByForumId(forumId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(forumId, result.First().Id);
            Assert.Equal("Test Title", result.First().Title);
        }
    }
}
