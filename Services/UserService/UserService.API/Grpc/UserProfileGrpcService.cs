using Grpc.Core;
using UserService.Grpc;
using UserService.UserService.Application.Interfaces.Services;
using Google.Protobuf.WellKnownTypes;

namespace UserService.UserService.API.Grpc;


public class UserProfileGrpcService : UserProfileService.UserProfileServiceBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserProfileGrpcService> _logger;

    public UserProfileGrpcService(IUserProfileService userProfileService, ILogger<UserProfileGrpcService> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
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
            BirthDay = u.Birthday != DateTime.MinValue 
                ? Timestamp.FromDateTime(DateTime.SpecifyKind(u.Birthday.GetValueOrDefault(), DateTimeKind.Utc))
                : Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)),
            Gender = u.Gender ?? -1
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
            _logger.LogError(ex, "Failed to create profile");
            return new CreateProfileResponse()
            {
                Success = false,
                Message = "An error occurred while creating the profile"
            };
        }
    }
}
