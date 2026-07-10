using ERP_Clint.Service;
using Microsoft.AspNetCore.Components;
using PRMS_Clint.Services;
using SherdProject.DTO;

namespace ERP_Clint.Pages.AccountPages
{
    public partial class Login
    {
        [Inject]
        private IAccountService AccountService { get; set; } = default!;
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        private CostumAuth costumAuth { get; set; }
        private LoginModel model = new();
        private bool isLoading = false;
        private bool showPassword = false;
        private string? errorMessage;

        private void TogglePassword() => showPassword = !showPassword;

        private async Task HandleLogin()
        {
            errorMessage = null;
            isLoading = true;

            try
            {
                var result = await AccountService.Login(model);
                if (!result.IsSuccessStatusCode)
                {
                    errorMessage = result.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    costumAuth.NotifyUserAuthenticationChanged();
                    NavigationManager.NavigateTo("/");
                }
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}
