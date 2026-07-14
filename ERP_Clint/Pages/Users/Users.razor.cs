using ERP_Clint.Service;
using ERP_Clint.Service.UserAdmin;
using ERPDto.PaigingDto;
using ERPDto.RolesDto;
using ERPDto.UserDto;
using Microsoft.AspNetCore.Components;

namespace ERP_Clint.Pages.Users
{
    public partial class Users
    {
        [Inject] private IUserAdminService UserService { get; set; } = default!;
        [Inject] private IRoleAdminService RoleService { get; set; } = default!;

        private List<UserDetailDto> users = new();
        private List<RoleDto> roles = new();
        private UsersInfo? usersInfo;
        private UsersListResponse? listResponse;
        private PageDto page = new();
        private string searchTerm = string.Empty;
        private bool isLoading = true;
        private string? loadError;
        private string? actionError;
        private CancellationTokenSource? _searchCts;

        private bool isModalOpen;
        private UserDetailDto? userBeingEdited;

        private bool isPermissionsOpen;
        private UserDetailDto? permissionsUser;

        private bool isPasswordOpen;
        private UserDetailDto? passwordUser;

        private bool isDeleteModalOpen;
        private UserDetailDto? userBeingDeleted;

        private int CurrentTotalPages => listResponse?.PageCount ?? 0;

        protected override async Task OnInitializedAsync() => await LoadAll();

        private async Task LoadAll()
        {
            isLoading = true;
            loadError = null;
            page.SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();

            try
            {
                var usersTask = UserService.GetUsersAsync(page);
                var infoTask = UserService.GetUsersInfoAsync();
                var rolesTask = RoleService.GetAllRolesAsync();
                await Task.WhenAll(usersTask, infoTask, rolesTask);

                listResponse = await usersTask;
                users = listResponse.Items;
                usersInfo = await infoTask;
                roles = await rolesTask;
            }
            catch (Exception)
            {
                loadError = "تعذر تحميل المستخدمين، تأكد من اتصالك وحاول مرة أخرى";
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task OnSearchChanged(ChangeEventArgs e)
        {
            searchTerm = e.Value?.ToString() ?? string.Empty;
            page.PageIndex = 1;
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;
            try
            {
                await Task.Delay(350, token);
                if (!token.IsCancellationRequested)
                    await LoadAll();
            }
            catch (TaskCanceledException) { }
        }

        private async Task GoToPage(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > CurrentTotalPages || pageNumber == page.PageIndex) return;
            page.PageIndex = pageNumber;
            await LoadAll();
        }

        private static string GetInitial(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "U";
            return name.Trim()[0].ToString().ToUpper();
        }

        private void OpenAddModal()
        {
            userBeingEdited = null;
            isModalOpen = true;
        }

        private void OpenEditModal(UserDetailDto user)
        {
            userBeingEdited = user;
            isModalOpen = true;
        }

        private void CloseModal()
        {
            isModalOpen = false;
            userBeingEdited = null;
        }

        private async Task HandleSaved()
        {
            CloseModal();
            await LoadAll();
        }

        private void OpenPermissionsModal(UserDetailDto user)
        {
            permissionsUser = user;
            isPermissionsOpen = true;
        }

        private void ClosePermissionsModal()
        {
            isPermissionsOpen = false;
            permissionsUser = null;
        }

        private async Task HandlePermissionsSaved()
        {
            ClosePermissionsModal();
            await LoadAll();
        }

        private void OpenPasswordModal(UserDetailDto user)
        {
            passwordUser = user;
            isPasswordOpen = true;
        }

        private void ClosePasswordModal()
        {
            isPasswordOpen = false;
            passwordUser = null;
        }

        private Task HandlePasswordSaved()
        {
            ClosePasswordModal();
            return Task.CompletedTask;
        }

        private async Task ToggleActive(UserDetailDto user)
        {
            actionError = null;
            try
            {
                var response = await UserService.SetUserActiveAsync(new SetUserActiveDto
                {
                    UserId = user.Id,
                    IsActive = !user.IsActive
                });
                if (!response.IsSuccessStatusCode)
                {
                    actionError = await ApiErrorHelper.ReadMessageAsync(response, "تعذر تحديث حالة المستخدم");
                    return;
                }
                await LoadAll();
            }
            catch
            {
                actionError = "تعذر الاتصال بالخادم";
            }
        }

        private void OpenDeleteModal(UserDetailDto user)
        {
            userBeingDeleted = user;
            actionError = null;
            isDeleteModalOpen = true;
        }

        private void CloseDeleteModal()
        {
            isDeleteModalOpen = false;
            userBeingDeleted = null;
            actionError = null;
        }

        private async Task HandleDeleteConfirmed()
        {
            if (userBeingDeleted is null) return;
            actionError = null;
            try
            {
                var response = await UserService.DeleteUserAsync(userBeingDeleted.Id);
                if (!response.IsSuccessStatusCode)
                {
                    actionError = await ApiErrorHelper.ReadMessageAsync(response, "تعذر حذف المستخدم");
                    return;
                }
                CloseDeleteModal();
                await LoadAll();
            }
            catch
            {
                actionError = "تعذر الاتصال بالخادم";
            }
        }
    }
}
