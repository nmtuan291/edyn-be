using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Models;

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
        
        // Persistence models to domain entities
        CreateMap<CommentEf, Comment>();
        CreateMap<CommentVoteEf, CommentVote>();
        CreateMap<ForumEf, Forum>();
        CreateMap<ForumThreadEf, ForumThread>();
        CreateMap<PollEf, Poll>();
        CreateMap<TagEf, Tag>();
        CreateMap<ThreadVoteEf, ThreadVote>();
        CreateMap<PermissionEf, Permission>();
        
        // Entities to models
        CreateMap<Comment, CommentEf>();
        CreateMap<ForumThread, ForumThreadEf>();
        CreateMap<Poll, PollEf>();
        CreateMap<ThreadVote, ThreadVoteEf>();
        CreateMap<Permission, PermissionEf>();
        CreateMap<CommentVote, CommentVoteEf>();
    }
}