using System.Net.Mail;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using AccountService;

namespace CSharpCraft.Competitive;

public class LoginScene(IScene prevScene) : IScene
{
    public string SceneName { get => "login"; }
    private Pico8Functions p8;
    private float cursorX;
    private float cursorY;
    private KeyboardState prevKeyboardState;
    private MouseState prevMouseState;

    private string email = string.Empty;
    private string code = string.Empty;
    private string username = string.Empty;
    private string password = string.Empty;
    private string currentInput = string.Empty;
    private Action<char> currentHandler;
    private string statusMessage = string.Empty;
    private bool isProcessing = false;
    private enum InputMode { Email, Code, Username, Password, TwoFactorCode }
    private bool isRegistering = true;
    private bool requiresTwoFactor = false;
    private TwoFactorType twoFactorType = TwoFactorType.None;

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        TextInputEXT.StartTextInput();
        EnableInputMode(InputMode.Email);
        prevKeyboardState = Keyboard.GetState();
        prevMouseState = Mouse.GetState();
        cursorX = prevMouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        cursorY = prevMouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);
    }

    private void EnableInputMode(InputMode mode)
    {
        if (currentHandler is not null)
        {
            TextInputEXT.TextInput -= currentHandler;
        }

        currentHandler = mode switch
        {
            InputMode.Email => OnEmailInput,
            InputMode.Code => OnCodeInput,
            InputMode.Username => OnUsernameInput,
            InputMode.Password => OnPasswordInput,
            InputMode.TwoFactorCode => OnTwoFactorInput,
            _ => throw new ArgumentOutOfRangeException()
        };

        TextInputEXT.TextInput += currentHandler;
        currentInput = string.Empty;
    }

    private async Task VerifyEmail()
    {
        try
        {
            isProcessing = true;
            statusMessage = "Verifying email...";
            
            var response = await AccountHandler.VerifyEmailBeforeRegistration(email);

            if (response.Success)
            {
                statusMessage = response.Message;
                EnableInputMode(InputMode.Code);
            }
            else
            {
                statusMessage = response.Message;
            }
        }
        catch (Exception ex)
        {
            statusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isProcessing = false;
        }
    }

    private async Task RegisterUser()
    {
        try
        {
            isProcessing = true;
            statusMessage = "Registering user...";

            var response = await AccountHandler.Register(email, username, password, code);

            if (response.Success)
            {
                statusMessage = response.Message;
                AccountHandler._isLoggedIn = true;
            }
            else
            {
                statusMessage = response.Message;
            }
        }
        catch (Exception ex)
        {
            statusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isProcessing = false;
        }
    }

    private async Task LoginUser()
    {
        try
        {
            isProcessing = true;
            statusMessage = "Logging in...";

            var response = await AccountHandler.Login(email, password);

            if (response.Success)
            {
                if (response.RequiresTwoFactor)
                {
                    requiresTwoFactor = true;
                    twoFactorType = response.TwoFactorType;
                    statusMessage = "Please enter your 2FA code";
                    EnableInputMode(InputMode.TwoFactorCode);
                }
                else
                {
                    statusMessage = response.Message;
                    AccountHandler._isLoggedIn = true;
                }
            }
            else
            {
                statusMessage = response.Message;
            }
        }
        catch (Exception ex)
        {
            statusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isProcessing = false;
        }
    }

    private async Task VerifyTwoFactor()
    {
        try
        {
            isProcessing = true;
            statusMessage = "Verifying 2FA code...";

            var response = await AccountHandler.Verify2FA(email, currentInput);

            if (response.Success)
            {
                statusMessage = response.Message;
                AccountHandler._isLoggedIn = true;
            }
            else
            {
                statusMessage = response.Message;
            }
        }
        catch (Exception ex)
        {
            statusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isProcessing = false;
        }
    }

    private void OnEmailInput(char c)
    {
        var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!#$%&'*+-/=?^_`{|}~.@";
        if (!allowed.Contains(c)) return;

        if (c == '@' && (currentInput.Contains('@') || currentInput.Length == 0)) return;
        if (c == '.' && !IsDotValid()) return;

        currentInput += c;
    }

    private bool IsDotValid()
    {
        if (currentInput.Length == 0)
            return false;

        if (currentInput.Last() == '.')
            return false;

        if (currentInput.Last() == '@')
            return false;

        if (currentInput.EndsWith("."))
            return false;

        if (!currentInput.Contains('@'))
        {
            if (currentInput.Length >= 2 && currentInput[^2] == '.')
                return false;
            return true;
        }

        var parts = currentInput.Split('@');
        var domain = parts[1];

        if (domain.Contains(".."))
            return false;

        if (domain.StartsWith("."))
            return false;

        if (domain.EndsWith("."))
            return false;

        return true;
    }

    private void OnCodeInput(char c)
    {
        var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        if (allowed.Contains(c))
        {
            currentInput += c;
        }
    }

    private void OnUsernameInput(char c)
    {
        var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
        if (allowed.Contains(c) && currentInput.Length < 16)
        {
            currentInput += c;
        }
    }

    private void OnPasswordInput(char c)
    {
        if (c >= 32 && c <= 126 && c != ' ')
        {
            currentInput += c;
        }
    }

    private void OnTwoFactorInput(char c)
    {
        var allowed = "0123456789";
        if (allowed.Contains(c) && currentInput.Length < 6)
        {
            currentInput += c;
        }
    }

    public void Update()
    {
        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        cursorX = mouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        cursorY = mouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        if (isProcessing) goto end_of_update;

        if (keyboardState.IsKeyDown(Keys.Back) && prevKeyboardState.IsKeyUp(Keys.Back) && currentInput.Length > 0)
        {
            currentInput = currentInput[..^1];
        }

        if (keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter) && currentInput.Length > 0)
        {
            switch (currentHandler.Method.Name)
            {
                case nameof(OnEmailInput):
                    email = currentInput;
                    currentInput = string.Empty;
                    if (isRegistering)
                    {
                        _ = VerifyEmail();
                    }
                    else
                    {
                        EnableInputMode(InputMode.Password);
                    }
                    break;
                case nameof(OnCodeInput):
                    code = currentInput;
                    currentInput = string.Empty;
                    EnableInputMode(InputMode.Username);
                    break;
                case nameof(OnUsernameInput):
                    username = currentInput;
                    currentInput = string.Empty;
                    EnableInputMode(InputMode.Password);
                    break;
                case nameof(OnPasswordInput):
                    password = currentInput;
                    currentInput = string.Empty;
                    if (isRegistering)
                    {
                        _ = RegisterUser();
                    }
                    else
                    {
                        _ = LoginUser();
                    }
                    break;
                case nameof(OnTwoFactorInput):
                    _ = VerifyTwoFactor();
                    break;
            }
        }

        if (keyboardState.IsKeyDown(Keys.Tab) && prevKeyboardState.IsKeyUp(Keys.Tab))
        {
            isRegistering = !isRegistering;
            statusMessage = isRegistering ? "Registration mode" : "Login mode";
            EnableInputMode(InputMode.Email);
        }

        end_of_update:

        prevKeyboardState = keyboardState;
        prevMouseState = mouseState;
    }

    public void Draw()
    {
        p8.Batch.GraphicsDevice.Clear(Color.Black);

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

        Shared.Printc(p8, isRegistering ? "Registration" : "Login", 64, 1, 7);
        Shared.Printc(p8, currentHandler.Method.Name, 64, 7, 7);
        Shared.Printc(p8, currentInput, 64, 13, 7);
        Shared.Printc(p8, statusMessage, 64, 19, 7);
        Shared.Printc(p8, "Press TAB to switch between Login/Register", 64, 25, 7);

        Shared.DrawCursor(p8, cursorX, cursorY);
    }

    public string SpriteImage => "";
    public string SpriteData => @"";
    public string FlagData => @"";
    public (int x, int y) MapDimensions => (0, 0);
    public string MapData => @"";
    public Dictionary<string, List<SongInst>> Music => new();
    public Dictionary<string, Dictionary<int, string>> Sfx => new();
    public void Dispose()
    {
        if (currentHandler is not null)
        {
            TextInputEXT.TextInput -= currentHandler;
        }
        TextInputEXT.StopTextInput();
    }
}
