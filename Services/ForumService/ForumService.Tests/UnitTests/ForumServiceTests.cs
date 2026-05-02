using AutoMapper;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Grpc;
using Xunit;

namespace ForumService.ForumService.Tests.UnitTests;

public class ForumServiceTests
{
    [Fact]
    public async Task RemoveForumMember_ActingUserHasPermission_CallsRemoveForumUserAsync()
    {
        // Arrange
        var forumId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var actingUserId = Guid.NewGuid();
        
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService
            .Setup(s => s.HasPermissionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), ForumPermissionType.BanMember))
            .ReturnsAsync(true);

        var mockRepo = new Mock<IForumRepository>();
        mockRepo
            .Setup(r => r.RemoveForumUserAsync(forumId, targetUserId))
            .Returns(Task.CompletedTask);
        
        mockRepo
            .Setup(r => r.GetForumUserAsync(forumId, targetUserId))
            .ReturnsAsync(new ForumUser
            {
                ForumId = forumId,
                UserId = targetUserId,
                Role = ForumRole.Member
            });
        
        mockRepo
            .Setup(r => r.GetForumUserAsync(forumId, actingUserId))
            .ReturnsAsync(new ForumUser
            {
                ForumId = forumId,
                UserId = actingUserId,
                Role = ForumRole.Moderator
            });
        
        mockRepo
            .Setup(r => r.InvalidateCachedPermissionsAsync(forumId, targetUserId))
            .Returns(Task.CompletedTask);
        
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.ForumRepo).Returns(mockRepo.Object);
        mockUow.Setup(u => u.CommitAsync()).ReturnsAsync(1);
    
        var mockUserProfileService = new Mock<UserProfileService.UserProfileServiceClient>();
        var mockLogger = new Mock<ILogger<IForumService>>();
        var mockMapper = new Mock<IMapper>();
        
        var sut = new Application.ForumService(mockUow.Object, mockMapper.Object, mockLogger.Object, mockPermissionService.Object, mockUserProfileService.Object);
        
        // Act
        await sut.RemoveForumMember(forumId, targetUserId, actingUserId);
        
        // Assert
        mockRepo.Verify(r => r.RemoveForumUserAsync(forumId, targetUserId), Times.Once);
    }
}