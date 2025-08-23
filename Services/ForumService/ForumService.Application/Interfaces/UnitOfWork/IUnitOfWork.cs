using ForumService.ForumService.Infrastructure.Interfaces;

namespace ForumService.ForumService.Application.Interfaces.UnitOfWork
{
    public interface IUnitOfWork: IDisposable
    {
        IForumRepository ForumRepo { get; }
        IThreadRepository ThreadRepo { get; }
        ICommentRepository CommentRepo { get; }
        IVoteRepository VoteRepo { get; }
        Task<int> CommitAsync();
    }
}
