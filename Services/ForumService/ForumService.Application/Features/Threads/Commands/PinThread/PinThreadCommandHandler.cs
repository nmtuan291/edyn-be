using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.PinThread;

public sealed class PinThreadCommandHandler : IRequestHandler<PinThreadCommand, ForumThreadDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly IMapper _mapper;

    public PinThreadCommandHandler(IUnitOfWork unitOfWork, IPermissionService permissionService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _mapper = mapper;
    }

    public async Task<ForumThreadDto?> Handle(PinThreadCommand request, CancellationToken cancellationToken)
    {
        var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(request.ThreadId, cancellationToken: cancellationToken);
        if (thread == null)
            return null;

        var canPin = await _permissionService.HasPermissionAsync(
            thread.ForumId,
            request.UserId,
            ForumPermissionType.PinThread,
            cancellationToken);

        if (!canPin)
            throw new UnauthorizedAccessException("You do not have permission to pin threads in this forum.");

        thread.IsPinned = request.IsPinned;
        _unitOfWork.ThreadRepo.UpdateThread(thread);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<ForumThreadDto>(thread);
    }
}
