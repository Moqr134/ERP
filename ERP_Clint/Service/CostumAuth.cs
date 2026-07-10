using ERPDto.UserDto;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using PRMS_Clint.Services;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

namespace ERP_Clint.Service
{
    public class CostumAuth : AuthenticationStateProvider
    {
        private readonly IAccountService _accountService;
        private readonly AuthenticationState _anonymous;

        public CostumAuth(IAccountService accountService)
        {
            _accountService = accountService;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {                
                var response = await _accountService.GetUserInfo();
                if (response.StatusCode==HttpStatusCode.Unauthorized)
                {
                    var refreshResponse = await _accountService.RefreshToken();
                    if (!refreshResponse.IsSuccessStatusCode)
                    {
                        return _anonymous;
                    }
                    else
                    {
                        response = await _accountService.GetUserInfo();
                    }
                }
                if (!response.IsSuccessStatusCode)
                {
                    return _anonymous; 
                }

                var userInfo = await response.Content.ReadFromJsonAsync<UserInfoResponse>();

                if (userInfo == null || !userInfo.IsAuthenticated)
                {
                    return _anonymous;
                }
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userInfo.UserName),
                new Claim(ClaimTypes.Email, userInfo.Email)
            };

                foreach (var role in userInfo.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, "ApiAuthType");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return _anonymous; 
            }
        }

        public void NotifyUserAuthenticationChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}