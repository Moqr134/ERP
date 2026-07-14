using System.ComponentModel.DataAnnotations;
using Validation.Attribute;

namespace ERPDto.UserDto
{
    public class UserDetailDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreateDate { get; set; }
        public List<int> RoleIds { get; set; } = new();
        public List<string> RoleNames { get; set; } = new();
    }

    public class UsersListResponse
    {
        public List<UserDetailDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }

    public class UsersInfo
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int OnlineUsers { get; set; }
    }

    public class CreateUserModel
    {
        [StringValidate(3, 16, false)]
        public string Username { get; set; } = string.Empty;
        [StringValidate(5, 100, false)]
        public string Password { get; set; } = string.Empty;
        [EmailAddress]
        [StringValidate(5, 100, false)]
        public string Email { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار دور")]
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AssignUserRolesDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
        [MinLength(1, ErrorMessage = "يجب اختيار دور واحد على الأقل")]
        public List<int> RoleIds { get; set; } = new();
    }

    public class SetUserActiveDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChangePasswordDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
        [StringValidate(5, 100, false)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangeMyPasswordDto
    {
        [StringValidate(5, 100, false)]
        public string CurrentPassword { get; set; } = string.Empty;
        [StringValidate(5, 100, false)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UserPermissionViewDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public bool FromRole { get; set; }
        public bool? OverrideAllowed { get; set; }
        public bool IsEffective { get; set; }
    }
}
