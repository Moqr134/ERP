using ERPDto.UserDto;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SherdProject.DTO;
using System.Text;
using System.Text.Json;

namespace PRMS_Clint.Services
{
    public interface IAccountService
    {
        public Task<HttpResponseMessage> Login(LoginModel user);
        public Task<HttpResponseMessage> Regester(RegisterModel user);
        public Task<HttpResponseMessage> RefreshToken();
        public Task<HttpResponseMessage> GetUserInfo();
        public Task<HttpResponseMessage> Logout();
    }
    public class AccountService : IAccountService
    {
        private readonly HttpClient _httpClient;
        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<HttpResponseMessage> Login(LoginModel user)
        {
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync("api/Account/Login", content);
        }

        public async Task<HttpResponseMessage> RefreshToken()
        {
            return await _httpClient.PostAsync("api/Account/refresh-token", null);
        }
        public async Task<HttpResponseMessage> Logout()
        {
            return await _httpClient.PostAsync("api/Account/Logout", null);
        }
        public async Task<HttpResponseMessage> Regester(RegisterModel user)
        {
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync("api/Account/Register", content);
        }

        public async Task<HttpResponseMessage> GetUserInfo()
        {
            return await _httpClient.GetAsync("api/account/userinfo");
        }
    }
}
