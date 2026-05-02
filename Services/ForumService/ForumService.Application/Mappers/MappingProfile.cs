using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Models;
using Google.Protobuf.WellKnownTypes;
using UserService.Grpc;

namespace ForumService.ForumService.Application.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entities to DTOs
        CreateMap<Forum, ForumDto>();
        CreateMap<ForumTagCatalogEf, ForumTagDto>();
        CreateMap<ForumThread, ForumThreadDto>()
            .ForMember(dest => dest.Vote, opt => opt.Ignore());
        CreateMap<Poll, PollItemDto>();
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.Vote, opt => opt.Ignore());
        
        // Persistence models to domain entities
        CreateMap<CommentEf, Comment>();
        CreateMap<CommentVoteEf, CommentVote>();
        CreateMap<ForumEf, Forum>().ReverseMap();
        CreateMap<ForumThreadEf, ForumThread>();
        CreateMap<PollEf, Poll>();
        CreateMap<TagEf, Tag>();
        CreateMap<ThreadVoteEf, ThreadVote>();
        CreateMap<ForumUserEf, ForumUser>();
        CreateMap<ForumUser, ForumUserEf>();
        
        // Entities to models
        CreateMap<Comment, CommentEf>();
        CreateMap<ForumThread, ForumThreadEf>();
        CreateMap<Tag, TagEf>()
            .ForMember(d => d.Id, o => o.Ignore());
        CreateMap<Poll, PollEf>();
        CreateMap<ThreadVote, ThreadVoteEf>();
        CreateMap<CommentVote, CommentVoteEf>();
        
        CreateMap<ProfileResponse, UserDto>().ConvertUsing(s => MapProfileResponseToUserDto(s));
    }

    private static UserDto MapProfileResponseToUserDto(ProfileResponse s)
    {
        _ = Guid.TryParse(s.Id, out var id);
        var birthDay = s.BirthDay != null ? s.BirthDay.ToDateTime() : default;
        return new UserDto
        {
            Id = id,
            BirthDay = birthDay,
            Username = s.Username ?? string.Empty,
            Avatar = s.Avatar ?? string.Empty,
            Bio = s.Bio ?? string.Empty,
            Gender = s.Gender < 0 ? null : s.Gender,
        };
    }
}