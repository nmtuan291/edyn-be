using AuthService.AuthService.Application.Interfaces.Services;
using AuthService.Grpc;
using Grpc.Core;
using GrpcAccountServiceDef = AuthService.Grpc.AccountService;

namespace AuthService.AuthService.API.Grpc;

public class GrpcAccountService: GrpcAccountServiceDef.AccountServiceBase
{
    private readonly IAccountService _accountService;
    
    public GrpcAccountService(IAccountService accountService)
    {
        _accountService = accountService;
    }
    public override async Task<AccountResponse> GetAccountById(AccountRequest request, ServerCallContext context)
    {
        var account = await _accountService.GetAccount(request.Id);
        if (account == null)
        { 
            throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        }

        return new AccountResponse
        {
            Id = account.Id,
            Username = account.UserName,
            Email = account.Email,
            IsActive = account.IsActive.ToString()
        };
    }
}