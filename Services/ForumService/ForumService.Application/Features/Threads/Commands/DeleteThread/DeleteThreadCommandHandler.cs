using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.DeleteThread;

public sealed class DeleteThreadCommandHandler : IRequestHandler<DeleteThreadCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;

    public DeleteThreadCommandHandler(IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    public async Task Handle(DeleteThreadCommand request, CancellationToken cancellationToken)
    {
        var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(request.ThreadId, cancellationToken: cancellationToken);
        if (thread == null)
            throw new KeyNotFoundException("Thread not found.");

        // The owner can always delete; otherwise a moderator with DeleteThread permission may.
        if (thread.CreatorId != request.UserId)
        {
            var canModerate = await _permissionService.HasPermissionAsync(
                thread.ForumId,
                request.UserId,
                ForumPermissionType.DeleteThread,
                cancellationToken);

            if (!canModerate)
                throw new UnauthorizedAccessException("You do not have permission to delete this thread.");
        }

        await _unitOfWork.ThreadRepo.DeleteThreadByIdAsync(request.ThreadId);
        await _unitOfWork.CommitAsync();
    }
}
