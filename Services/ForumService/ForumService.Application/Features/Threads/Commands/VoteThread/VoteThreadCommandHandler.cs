using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Application.Requests;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.VoteThread;

public sealed class VoteThreadCommandHandler : IRequestHandler<VoteThreadCommand, VoteThreadResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly IMapper _mapper;

    public VoteThreadCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _mapper = mapper;
    }

    public async Task<VoteThreadResult> Handle(VoteThreadCommand request, CancellationToken cancellationToken)
    {
        var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(
            request.ThreadId,
            request.UserId,
            cancellationToken);

        if (thread == null)
            return new VoteThreadResult(null);

        var canVote = await _permissionService.HasPermissionAsync(
            thread.ForumId,
            request.UserId,
            ForumPermissionType.Vote,
            cancellationToken);

        if (!canVote)
            return new VoteThreadResult(null, Forbidden: true);

        var isVoteExists = thread.Vote(request.UserId, request.IsDownVote);
        _unitOfWork.ThreadRepo.UpdateThread(thread);

        if (isVoteExists)
        {
            await _unitOfWork.VoteRepo.UpdateThreadVoteRedisAsync(
                new ThreadVoteRedisUpdate(request.UserId, request.ThreadId, thread.ForumId, request.IsDownVote));

            var tagNames = thread.Tags?
                .Select(t => t.Name)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            if (tagNames is { Count: > 0 })
            {
                if (!request.IsDownVote)
                    await _unitOfWork.VoteRepo.IncrementTagAffinityAsync(request.UserId, tagNames);
                else
                    await _unitOfWork.VoteRepo.DecrementTagAffinityAsync(request.UserId, tagNames);
            }
        }
        else
        {
            await _unitOfWork.VoteRepo.RemoveThreadVoteRedisAsync(request.UserId, request.ThreadId, thread.ForumId);
        }

        await _unitOfWork.CommitAsync();
        return new VoteThreadResult(_mapper.Map<ForumThreadDto>(thread));
    }
}
