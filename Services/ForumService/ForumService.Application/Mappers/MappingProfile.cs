using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entities to DTOs
        CreateMap<Forum, ForumDto>();
        CreateMap<ForumThread, ForumThreadDto>()
            .ForMember(dest => dest.Vote, opt => opt.Ignore());
        CreateMap<Poll, PollItemDto>();
        CreateMap<Comment, CommentDto>();

    }
}