using ERP_Clint.Service;
using Microsoft.AspNetCore.Components;
using PRMS_Clint.Services;
using SherdProject.DTO;
using System.Text.Json;

namespace ERP_Clint.Pages.AccountPages
{
    public partial class Login
    {
        [Inject]
        private IAccountService AccountService { get; set; } = default!;
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        private CostumAuth costumAuth { get; set; } = default!;
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
                    errorMessage = await ExtractErrorMessage(result);
                }
                else
                {
                    costumAuth.NotifyUserAuthenticationChanged();
                    NavigationManager.NavigateTo("/");
                }
            }
            catch
            {
                errorMessage = "تعذر الاتصال بالخادم";
            }
            finally
            {
                isLoading = false;
            }
        }

        private static async Task<string> ExtractErrorMessage(HttpResponseMessage result)
        {
            var raw = await result.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
                return "فشل تسجيل الدخول، تحقق من البيانات";

            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("Message", out var message))
                    return message.GetString() ?? "فشل تسجيل الدخول";
                if (doc.RootElement.TryGetProperty("message", out var message2))
                    return message2.GetString() ?? "فشل تسجيل الدخول";
            }
            catch
            {
                // not JSON
            }

            return raw.Length > 200 ? "فشل تسجيل الدخول، تحقق من البيانات" : raw.Trim('"');
        }
    }
}
