using ERP_Clint.Service;
using ERPDto.RolesDto;
using ERPDto.UserDto;
using System.Net.Http.Json;

namespace ERP_Clint.Service.UserAdmin
{
    public interface IRoleAdminService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(int id);
        Task<HttpResponseMessage> CreateRoleAsync(RoleDto role);
        Task<HttpResponseMessage> UpdateRoleAsync(RoleDto role);
        Task<HttpResponseMessage> DeleteRoleAsync(int id);
        Task<List<PermissionDto>> GetAllPermissionsAsync();
        Task<RolePermissionViewDto?> GetRolePermissionsAsync(int roleId);
        Task<HttpResponseMessage> SetRolePermissionsAsync(RolePermissionDto model);
    }

    public class RoleAdminService : IRoleAdminService
    {
        private readonly HttpClient _httpClient;
        public RoleAdminService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var response = await _httpClient.GetAsync("api/Role/GetAllRoles");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل الأدوار", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<List<RoleDto>>() ?? new();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Role/GetRoleById/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل الدور", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<RoleDto>();
        }

        public Task<HttpResponseMessage> CreateRoleAsync(RoleDto role)
            => _httpClient.PostAsJsonAsync("api/Role/CreateRole", role);

        public Task<HttpResponseMessage> UpdateRoleAsync(RoleDto role)
            => _httpClient.PutAsJsonAsync("api/Role/UpdateRole", role);

        public Task<HttpResponseMessage> DeleteRoleAsync(int id)
            => _httpClient.DeleteAsync($"api/Role/DeleteRole/{id}");

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            var response = await _httpClient.GetAsync("api/Role/GetAllPermissions");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل الصلاحيات", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<List<PermissionDto>>() ?? new();
        }

        public async Task<RolePermissionViewDto?> GetRolePermissionsAsync(int roleId)
        {
            var response = await _httpClient.GetAsync($"api/Role/GetRolePermissions/{roleId}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل صلاحيات الدور", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<RolePermissionViewDto>();
        }

        public Task<HttpResponseMessage> SetRolePermissionsAsync(RolePermissionDto model)
            => _httpClient.PostAsJsonAsync("api/Role/CreateRolePermission", model);
    }
}
