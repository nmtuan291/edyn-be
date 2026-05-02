using ForumService.ForumService.Application;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Application.Permissions;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Repositories;
using ForumService.ForumService.Infrastructure.UnitOfWork;
using Moq;
using Xunit;

namespace ForumService.ForumService.Tests.UnitTests;

public class PermissionServiceTests
{
    private sealed class TestUnitOfWork : IUnitOfWork
    {
        public IForumRepository ForumRepo => throw new InvalidOperationException();
        public IThreadRepository ThreadRepo => throw new InvalidOperationException();
        public ICommentRepository CommentRepo => throw new InvalidOperationException();
        public IVoteRepository VoteRepo => throw new InvalidOperationException();
        
        public Task<int> CommitAsync() { return Task.FromResult(0); }

        public void Dispose()
        {
            
        }
    }

    private static IEnumerable<IRolePermissionStrategy> _strategies =
    [
        new AdminRolePermissionStrategy(),
        new MemberRolePermissionStrategy(),
        new SuperModeratorRolePermissionStrategy(),
        new ModeratorRolePermissionStrategy()
    ];

    [Fact]
    public void GetDefaultPermissions_Member_ReturnsDefaultPermissions()
    {
        // Arrange
        var sut = new PermissionService(new TestUnitOfWork(), _strategies);
        var expected = new MemberRolePermissionStrategy().GetDefaultPermissions();
        
        // Act
        var actual = sut.GetDefaultPermissions(ForumRole.Member);
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void GetDefaultPermissions_UnknownRole_ReturnsNone()
    {
        // Arrange
        var mockPermissionService = new PermissionService(new TestUnitOfWork(), _strategies);
        
        // Act
        var mockRolePermission = mockPermissionService.GetDefaultPermissions((ForumRole)999);
        
        // Assert
        Assert.Equal(ForumPermissionType.None, mockRolePermission);
    }

    [Fact]
    public async Task HasPermission_ModHasBanMemberPermission_ReturnsTrue()
    {
        // Arrange
        var forumId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockRepo = new Mock<IForumRepository>();
        mockRepo
            .Setup(r => r.GetCachedPermissionsAsync(forumId, userId))
            .ReturnsAsync((int?)null);

        mockRepo
            .Setup(r => r.GetForumUserAsync(forumId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ForumUser()
            {
                ForumId = forumId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                Role = ForumRole.Moderator,
                Active = true
            });

        mockRepo
            .Setup(r => r.SetCachedPermissionsAsync(forumId, userId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.ForumRepo).Returns(mockRepo.Object);
        
        var sut = new PermissionService(mockUow.Object, _strategies);
        
        // Act
        var result = await sut.HasPermissionAsync(forumId, userId, ForumPermissionType.BanMember);
        
        // Assert
        Assert.True(result);
    }
}