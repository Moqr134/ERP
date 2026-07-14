using ERP_Clint.Service;
using ERPDto.PaigingDto;
using ERPDto.UserDto;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ERP_Clint.Service.UserAdmin
{
    public interface IUserAdminService
    {
        Task<UsersListResponse> GetUsersAsync(PageDto page);
        Task<UserDetailDto?> GetUserByIdAsync(int id);
        Task<UsersInfo?> GetUsersInfoAsync();
        Task<HttpResponseMessage> CreateUserAsync(CreateUserModel model);
        Task<HttpResponseMessage> UpdateUserAsync(UpdateUserModel model);
        Task<HttpResponseMessage> DeleteUserAsync(int id);
        Task<HttpResponseMessage> SetUserActiveAsync(SetUserActiveDto model);
        Task<HttpResponseMessage> AssignUserRolesAsync(AssignUserRolesDto model);
        Task<HttpResponseMessage> ChangePasswordAsync(ChangePasswordDto model);
        Task<List<UserPermissionViewDto>> GetUserPermissionsAsync(int userId);
        Task<HttpResponseMessage> UpdateUserPermissionsAsync(int userId, List<UserPermissionDto> permissions);
    }

    public class UserAdminService : IUserAdminService
    {
        private readonly HttpClient _httpClient;
        public UserAdminService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<UsersListResponse> GetUsersAsync(PageDto page)
        {
            var response = await _httpClient.PostAsJsonAsync("api/User/GetUsers", page);
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل المستخدمين", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<UsersListResponse>() ?? new UsersListResponse();
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/User/GetUserById/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل المستخدم", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<UserDetailDto>();
        }

        public async Task<UsersInfo?> GetUsersInfoAsync()
        {
            var response = await _httpClient.GetAsync("api/User/GetUsersInfo");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل إحصائيات المستخدمين", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<UsersInfo>();
        }

        public Task<HttpResponseMessage> CreateUserAsync(CreateUserModel model)
            => _httpClient.PostAsJsonAsync("api/User/CreateUser", model);

        public Task<HttpResponseMessage> UpdateUserAsync(UpdateUserModel model)
            => _httpClient.PutAsJsonAsync("api/User/UpdateUser", model);

        public Task<HttpResponseMessage> DeleteUserAsync(int id)
            => _httpClient.DeleteAsync($"api/User/DeleteUser/{id}");

        public Task<HttpResponseMessage> SetUserActiveAsync(SetUserActiveDto model)
            => _httpClient.PutAsJsonAsync("api/User/SetUserActive", model);

        public Task<HttpResponseMessage> AssignUserRolesAsync(AssignUserRolesDto model)
            => _httpClient.PutAsJsonAsync("api/User/AssignUserRoles", model);

        public Task<HttpResponseMessage> ChangePasswordAsync(ChangePasswordDto model)
            => _httpClient.PutAsJsonAsync("api/User/ChangePassword", model);

        public async Task<List<UserPermissionViewDto>> GetUserPermissionsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/User/GetUserPermissions/{userId}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل صلاحيات المستخدم", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<List<UserPermissionViewDto>>() ?? new();
        }

        public Task<HttpResponseMessage> UpdateUserPermissionsAsync(int userId, List<UserPermissionDto> permissions)
            => _httpClient.PutAsJsonAsync($"api/User/UpdateUserPermission/{userId}", permissions);
    }
}
