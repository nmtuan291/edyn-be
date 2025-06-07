using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.DependencyInjection;
using ForumService.ForumService.Application;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.UnitOfWork;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using Xunit;

namespace ForumService.ForumService.Tests.IntegrationTests
{
    public class ForumThreadServiceIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ForumDbContext _context;
        private readonly ForumThreadService _service;

        public ForumThreadServiceIntegrationTests()
        {
            // Setup services
            var services = new ServiceCollection();
            
            // Configure in-memory database
            services.AddDbContext<ForumDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            
            // Register dependencies
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ForumThreadService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<ForumDbContext>();
            _service = _serviceProvider.GetRequiredService<ForumThreadService>();
            
            // Ensure database is created
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetThreadsByForumId_WithRealData_ReturnsCorrectThreads()
        {
            // Arrange
            var forumId = Guid.NewGuid();
            var threadId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();

            // Seed test data
            var forum = new Forum
            {
                Id = forumId,
                Name = "Test Forum",
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow
            };

            var thread = new ForumThread
            {
                Id = threadId,
                ForumId = forumId,
                CreatorId = creatorId,
                Title = "Integration Test Thread",
                Content = "This is an integration test thread content",
                Slug = "integration-test-thread",
                IsPinned = false,
                Upvote = 10,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                Tags = new List<Tag>(),
                Images = new List<string>()
            };

            _context.Forums.Add(forum);
            _context.Threads.Add(thread);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetThreadsByForumId(forumId);

            // Assert
            Assert.NotNull(result);
            var threads = result.ToList();
            Assert.Single(threads);
            
            var retrievedThread = threads.First();
            Assert.Equal(forumId, retrievedThread.ForumId);
            Assert.Equal("Integration Test Thread", retrievedThread.Title);
            Assert.Equal("This is an integration test thread content", retrievedThread.Content);
            Assert.Equal("integration-test-thread", retrievedThread.Slug);
            Assert.Equal(10, retrievedThread.Upvote);
            Assert.False(retrievedThread.IsPinned);
        }

        [Fact]
        public async Task GetThreadsByForumId_WithMultipleThreads_ReturnsAllThreads()
        {
            // Arrange
            var forumId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();

            var forum = new Forum
            {
                Id = forumId,
                Name = "Multi Thread Forum",
                Description = "Forum with multiple threads",
                CreatedAt = DateTime.UtcNow
            };

            var threads = new List<ForumThread>
            {
                new ForumThread
                {
                    Id = Guid.NewGuid(),
                    ForumId = forumId,
                    CreatorId = creatorId,
                    Title = "First Thread",
                    Content = "First thread content",
                    Slug = "first-thread",
                    IsPinned = true,
                    Upvote = 5,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    LastUpdatedAt = DateTime.UtcNow.AddDays(-2),
                    Tags = new List<Tag>(),
                    Images = new List<string>()
                },
                new ForumThread
                {
                    Id = Guid.NewGuid(),
                    ForumId = forumId,
                    CreatorId = creatorId,
                    Title = "Second Thread",
                    Content = "Second thread content",
                    Slug = "second-thread",
                    IsPinned = false,
                    Upvote = 15,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    LastUpdatedAt = DateTime.UtcNow.AddDays(-1),
                    Tags = new List<Tag>(),
                    Images = new List<string>()
                }
            };

            _context.Forums.Add(forum);
            _context.Threads.AddRange(threads);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetThreadsByForumId(forumId);

            // Assert
            Assert.NotNull(result);
            var retrievedThreads = result.ToList();
            Assert.Equal(2, retrievedThreads.Count);
            
            Assert.Contains(retrievedThreads, t => t.Title == "First Thread" && t.IsPinned);
            Assert.Contains(retrievedThreads, t => t.Title == "Second Thread" && !t.IsPinned);
        }

        [Fact]
        public async Task GetThreadsByForumId_WithNonExistentForum_ReturnsEmptyCollection()
        {
            // Arrange
            var nonExistentForumId = Guid.NewGuid();

            // Act
            var result = await _service.GetThreadsByForumId(nonExistentForumId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCommentsByThreadId_WithRealData_ReturnsCorrectComments()
        {
            // Arrange
            var forumId = Guid.NewGuid();
            var threadId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var commenterId = Guid.NewGuid();

            // Seed test data
            var forum = new Forum
            {
                Id = forumId,
                Name = "Comment Test Forum",
                Description = "Forum for testing comments",
                CreatedAt = DateTime.UtcNow
            };

            var thread = new ForumThread
            {
                Id = threadId,
                ForumId = forumId,
                CreatorId = creatorId,
                Title = "Thread with Comments",
                Content = "Thread content",
                Slug = "thread-with-comments",
                IsPinned = false,
                Upvote = 0,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                Tags = new List<Tag>(),
                Images = new List<string>()
            };

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                ThreadId = threadId,
                OwnerId = commenterId,
                Content = "This is a test comment",
                Upvote = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Deleted = false,
                ParentId = Guid.Empty,
                ChildrenComments = new List<Comment>()
            };

            _context.Forums.Add(forum);
            _context.Threads.Add(thread);
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetCommentsByThreadId(threadId);

            // Assert
            Assert.NotNull(result);
            var comments = result.ToList();
            Assert.Single(comments);
            
            var retrievedComment = comments.First();
            Assert.Equal(threadId, retrievedComment.ThreadId);
            Assert.Equal(commenterId, retrievedComment.OwnerId);
            Assert.Equal("This is a test comment", retrievedComment.Content);
            Assert.Equal(3, retrievedComment.Upvote);
            Assert.False(retrievedComment.Deleted);
        }

        public void Dispose()
        {
            _context?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
} 