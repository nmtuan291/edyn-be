using UserService.Grpc;
using UserService.UserService.Application.Interfaces.Repositories;
using UserService.UserService.Application.Interfaces.Services;

namespace UserService.UserService.API.Grpc;

public class GrpcProfileService: ProfileService.ProfileServiceBase
{
    private readonly IUserProfileService _userProfileService;

    public GrpcProfileService(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }
    
    public async Task<CreateProfileResponse> CreateProfile(CreateProfileRequest request)
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