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

            var request = new HttpRequestMessage(HttpMethod.Post, "api/Account/Login")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<HttpResponseMessage> RefreshToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/Account/refresh-token");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            return await _httpClient.SendAsync(request);
        }
        public async Task<HttpResponseMessage> Logout()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/Account/Logout");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            return response;
        }
        public async Task<HttpResponseMessage> Regester(RegisterModel user)
        {
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/Account/Register")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<HttpResponseMessage> GetUserInfo()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/account/userinfo");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            return response;
        }
    }
}
