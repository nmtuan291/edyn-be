using ForumService.ForumService.Application.Interfaces.Repositories;

namespace ForumService.ForumService.Application.Interfaces.UnitOfWork
{
    public interface IUnitOfWork: IDisposable
    {
        IForumRepository ForumRepo { get; }
        IThreadRepository ThreadRepo { get; }
        ICommentRepository CommentRepo { get; }
        IVoteRepository VoteRepo { get; }
        IOutboxRepository OutboxRepo { get; }
        Task<int> CommitAsync();
    }
}
