using Grpc.Core;
using UserService.Grpc;
using UserService.UserService.Application.Interfaces.Services;
using Google.Protobuf.WellKnownTypes;

namespace UserService.UserService.API.Grpc;


public class UserProfileGrpcService : UserProfileService.UserProfileServiceBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileGrpcService(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    public override async Task<ProfilesResponse> GetUserProfile(ProfileRequest request, ServerCallContext context)
    {
        var profiles =  await _userProfileService.GetUsers(request.Id);
        if (profiles == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"User {request.Id} not found"));
        
        var response = new ProfilesResponse();
        response.Profiles.AddRange(profiles.Select(u => new ProfileResponse
        {
            Id = u.AccountId,
            Username = u.UserName,
            Avatar = u.Avatar,
            Bio = u.Bio,
            BirthDay = Timestamp.FromDateTime(u.Birthday.ToUniversalTime()),
            Gender = u.Gender.HasValue ? (long)u.Gender.Value : -1
        }));
        
        return response;
    }

    public override async Task<CreateProfileResponse> CreateProfile(CreateProfileRequest request,
        ServerCallContext context)
    {
        try
        {
            await _userProfileService.CreateProfile(request);
            return new CreateProfileResponse()
            {
                Success = true,
                Message = "Profile created successfully."
            };
        }
        catch (Exception ex)
        {
            return new CreateProfileResponse()
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}