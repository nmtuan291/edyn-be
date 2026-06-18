using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.EditThread;

public sealed class EditThreadCommandHandler : IRequestHandler<EditThreadCommand, ForumThreadDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EditThreadCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ForumThreadDto?> Handle(EditThreadCommand request, CancellationToken cancellationToken)
    {
        var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(request.ThreadId, cancellationToken: cancellationToken);
        if (thread == null)
            return null;

        if (thread.CreatorId != request.UserId)
            throw new UnauthorizedAccessException("You can only edit your own threads.");

        if (request.Request.Title != null)
            thread.Title = request.Request.Title;
        if (request.Request.Content != null)
            thread.Content = request.Request.Content;
        if (request.Request.Videos != null)
            thread.Videos = request.Request.Videos;

        thread.LastUpdatedAt = DateTime.UtcNow;

        _unitOfWork.ThreadRepo.UpdateThread(thread);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<ForumThreadDto>(thread);
    }
}
