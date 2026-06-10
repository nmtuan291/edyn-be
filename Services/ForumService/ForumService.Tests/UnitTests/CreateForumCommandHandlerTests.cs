using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Features.Forums.Commands.CreateForum;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Domain.Entities;
using Moq;
using Xunit;

namespace ForumService.ForumService.Tests.UnitTests;

public class CreateForumCommandHandlerTests
{
    [Fact]
    public async Task Handle_InsertsForumAndAddsCreatorAsAdmin()
    {
        var userId = Guid.NewGuid();
        var forumDto = new ForumDto
        {
            Name = "Test Forum",
            ShortName = "test-forum",
            Description = "A test forum description",
            ForumBanner = "https://example.com/banner.png",
            ForumImage = "https://example.com/image.png"
        };

        var mapper = new Mock<IMapper>();
        mapper
            .Setup(m => m.Map<ForumDto>(It.IsAny<Forum>()))
            .Returns((Forum source) => new ForumDto
            {
                Id = source.Id,
                Name = source.Name,
                ShortName = source.ShortName,
                Description = source.Description,
                ForumBanner = source.ForumBanner,
                ForumImage = source.ForumImage,
                CreatorId = source.CreatorId,
                CreatedAt = source.CreatedAt
            });

        var mockRepo = new Mock<IForumRepository>();
        mockRepo
            .Setup(r => r.InsertForumAsync(It.IsAny<Forum>()))
            .ReturnsAsync((Forum source) => source);
        mockRepo
            .Setup(r => r.InsertUserToForumAsync(It.IsAny<Guid>(), userId, It.IsAny<ForumRole>()))
            .Returns(Task.CompletedTask);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.ForumRepo).Returns(mockRepo.Object);
        mockUow.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        var sut = new CreateForumCommandHandler(mockUow.Object, mapper.Object);

        var result = await sut.Handle(new CreateForumCommand(forumDto, userId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(forumDto.Name, result!.Name);
        Assert.Equal(forumDto.ShortName, result.ShortName);
        mockRepo.Verify(r => r.InsertForumAsync(It.IsAny<Forum>()), Times.Once);
        mockRepo.Verify(r => r.InsertUserToForumAsync(It.IsAny<Guid>(), userId, ForumRole.Admin), Times.Once);
        mockUow.Verify(u => u.CommitAsync(), Times.Once);
    }
}
