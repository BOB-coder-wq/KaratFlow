using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KaratFlowAvalonia.Models;
using KaratFlowAvalonia.Services;

namespace KaratFlowAvalonia.ViewModels
{
    public partial class LoginWindowViewModel : ViewModelBase
    {
        private readonly LocalAuthService _authService;
        private readonly GoogleOAuthService _googleOAuthService;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _email = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;
        private bool _isLoginMode = true;
        private bool _isCreateAccountMode = false;

        public User? CurrentUser { get; private set; }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsLoginMode
        {
            get => _isLoginMode;
            set => SetProperty(ref _isLoginMode, value);
        }

        public bool IsCreateAccountMode
        {
            get => _isCreateAccountMode;
            set => SetProperty(ref _isCreateAccountMode, value);
        }

        public IRelayCommand LoginCommand { get; }
        public IRelayCommand CreateAccountCommand { get; }
        public IRelayCommand GoogleSignInCommand { get; }
        public IRelayCommand ShowLoginModeCommand { get; }
        public IRelayCommand ShowCreateAccountModeCommand { get; }

        public event EventHandler? LoginSuccess;

        public LoginWindowViewModel(LocalAuthService authService, GoogleOAuthService googleOAuthService)
        {
            _authService = authService;
            _googleOAuthService = googleOAuthService;

            LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
            CreateAccountCommand = new RelayCommand(async () => await CreateAccountAsync(), CanCreateAccount);
            GoogleSignInCommand = new RelayCommand(async () => await GoogleSignInAsync());
            ShowLoginModeCommand = new RelayCommand(ShowLoginMode);
            ShowCreateAccountModeCommand = new RelayCommand(ShowCreateAccountMode);
        }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Username and password are required";
                    return;
                }

                // Demo mode: Allow any login with password "demo123"
                if (Password == "demo123")
                {
                    var demoUser = Models.User.CreateGuest();
                    demoUser.Username = Username;
                    demoUser.Email = $"{Username.ToLower()}@karatflow.demo";
                    CurrentUser = demoUser;
                    OnLoginSuccess();
                    return;
                }
                
                var response = await _authService.LoginAsync(Username, Password);
                
                if (response.Success && response.User != null)
                {
                    CurrentUser = response.User;
                    OnLoginSuccess();
                }
                else
                {
                    ErrorMessage = response.Message + " (Try password 'demo123' for demo access)";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateAccountAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || 
                    string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    ErrorMessage = "All fields are required";
                    return;
                }

                // Demo mode: Allow any account creation with password "demo123"
                if (Password == "demo123")
                {
                    var demoUser = Models.User.CreateGuest();
                    demoUser.Username = Username;
                    demoUser.Email = Email;
                    CurrentUser = demoUser;
                    OnLoginSuccess();
                    return;
                }
                
                var response = await _authService.CreateAccountAsync(Username, Email, Password, ConfirmPassword);
                
                if (response.Success && response.User != null)
                {
                    CurrentUser = response.User;
                    OnLoginSuccess();
                }
                else
                {
                    ErrorMessage = response.Message + " (Try password 'demo123' for demo access)";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Account creation failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GoogleSignInAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                // Get Google OAuth URL
                var authUrl = _googleOAuthService.GetAuthorizationUrl();
                Console.WriteLine($"OAuth URL: {authUrl}");

                // Open browser for OAuth
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);

                // Wait for OAuth callback (simplified - in production, use a proper callback handler)
                await Task.Delay(10000); // Wait for user to complete OAuth

                // For demo purposes, show a message
                ErrorMessage = "Google OAuth requires browser completion. Please use the OAuth callback handler.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Google Sign-In failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowLoginMode()
        {
            IsLoginMode = true;
            IsCreateAccountMode = false;
            ClearError();
        }

        private void ShowCreateAccountMode()
        {
            IsLoginMode = false;
            IsCreateAccountMode = true;
            ClearError();
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
        }

        private bool CanLogin()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private bool CanCreateAccount()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Username) && 
                   !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && 
                   !string.IsNullOrWhiteSpace(ConfirmPassword);
        }

        private void OnLoginSuccess()
        {
            Console.WriteLine($"LoginSuccess event triggered! Current user: {CurrentUser?.Username} ({CurrentUser?.Email})");
            LoginSuccess?.Invoke(this, EventArgs.Empty);
        }

        public void SetGoogleUser(GoogleUser googleUser)
        {
            try
            {
                IsLoading = true;
                ClearError();

                var response = _authService.LoginWithGoogleAsync(googleUser.Id, googleUser.Email, googleUser.Name, googleUser.Picture).Result;
                
                if (response.Success && response.User != null)
                {
                    CurrentUser = response.User;
                    OnLoginSuccess();
                }
                else
                {
                    ErrorMessage = response.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Google login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
