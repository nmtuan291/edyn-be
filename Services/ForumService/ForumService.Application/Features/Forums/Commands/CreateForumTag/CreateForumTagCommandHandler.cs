using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.CreateForumTag;

public sealed class CreateForumTagCommandHandler : IRequestHandler<CreateForumTagCommand, ForumTagDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;

    public CreateForumTagCommandHandler(IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    public async Task<ForumTagDto> Handle(CreateForumTagCommand request, CancellationToken cancellationToken)
    {
        var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(request.ForumId, cancellationToken);
        if (forum == null)
            throw new ForumNotFoundException(request.ForumId);

        var canManage = await _permissionService.HasPermissionAsync(
            request.ForumId,
            request.ActingUserId,
            ForumPermissionType.ManageTags,
            cancellationToken);

        if (!canManage)
            throw new UnauthorizedAccessException("You do not have permission to manage forum tags.");

        var name = (request.Request.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Tag name is required.");
        if (name.Length > 64)
            throw new ArgumentException("Tag name must be at most 64 characters.");

        var color = string.IsNullOrWhiteSpace(request.Request.Color) ? "#808080" : request.Request.Color.Trim();
        if (color.Length > 32)
            throw new ArgumentException("Color must be at most 32 characters.");

        var added = await _unitOfWork.ForumRepo.AddForumTagCatalogIfNotExistsAsync(request.ForumId, name, color);
        if (!added)
            throw new InvalidOperationException("A tag with this name already exists for this forum.");

        await _unitOfWork.CommitAsync();

        var tags = await _unitOfWork.ForumRepo.GetForumTagCatalogAsync(request.ForumId, cancellationToken);
        return tags.Single(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
