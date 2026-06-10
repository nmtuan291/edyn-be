using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.DeleteThread;

public sealed class DeleteThreadCommandHandler : IRequestHandler<DeleteThreadCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteThreadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteThreadCommand request, CancellationToken cancellationToken)
    {
        var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(request.ThreadId, cancellationToken: cancellationToken);
        if (thread == null)
            throw new KeyNotFoundException("Thread not found.");

        if (thread.CreatorId != request.UserId)
            throw new UnauthorizedAccessException("You can only delete your own threads.");

        await _unitOfWork.ThreadRepo.DeleteThreadByIdAsync(request.ThreadId);
        await _unitOfWork.CommitAsync();
    }
}
