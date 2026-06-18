using AutoMapper;
using ForumService.ForumService.Application.Features.Threads.Queries.GetThreadById;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Domain.Entities;
using Moq;
using Xunit;

namespace ForumService.ForumService.Tests.UnitTests;

public class GetThreadByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMappedThreadWithCurrentUsersPollVote()
    {
        var forumId = Guid.NewGuid();
        var threadId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var thread = new ForumThread
        {
            Id = threadId,
            ForumId = forumId,
            CreatorId = Guid.NewGuid(),
            Title = "Test thread",
            IsPinned = false,
            Tags = [],
            Images = [],
            Content = "Thread content",
            Slug = "test-thread",
            Upvote = 3,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            PollVotes =
            [
                new PollVote
                {
                    UserId = userId,
                    ThreadId = threadId,
                    PollContent = "Option A"
                }
            ]
        };

        var mapper = new Mock<IMapper>();
        mapper
            .Setup(m => m.Map<ForumThreadDto>(It.IsAny<ForumThread>()))
            .Returns((ForumThread source) => new ForumThreadDto
            {
                Id = source.Id,
                ForumId = source.ForumId,
                CreatorId = source.CreatorId,
                Title = source.Title,
                IsPinned = source.IsPinned,
                Tags = source.Tags,
                Images = source.Images,
                Content = source.Content,
                Slug = source.Slug,
                Upvote = source.Upvote,
                CreatedAt = source.CreatedAt,
                LastUpdatedAt = source.LastUpdatedAt
            });

        var mockRepo = new Mock<IThreadQueryRepository>();
        mockRepo
            .Setup(r => r.GetThreadByIdAsync(threadId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(thread);

        var sut = new GetThreadByIdQueryHandler(mockRepo.Object, mapper.Object);

        var result = await sut.Handle(new GetThreadByIdQuery(threadId, userId.ToString()), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(threadId, result!.Id);
        Assert.Equal("Test thread", result.Title);
        Assert.Equal("Option A", result.UserPollVote);
    }

}
