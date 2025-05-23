using System.Net.Mail;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using AccountService;
using FixMath;

namespace CSharpCraft.Competitive;

public class LoginScene : IScene
{
    public string SceneName { get => "login"; }
    public double Fps { get => 60.0; }
    private Pico8Functions p8;
    private IScene prevScene;
    private float cursorX;
    private float cursorY;
    private KeyboardState prevKeyboardState;
    private MouseState prevMouseState;

    private string currentInput = string.Empty;
    private Action<char>? currentHandler;
    private string statusMessage = string.Empty;
    private bool isProcessing = false;
    private enum InputMode { Email, Code, Username, Password, TwoFactorCode, None }
    private bool isRegistering = true;
    private bool requiresTwoFactor = false;
    private TwoFactorType twoFactorType = TwoFactorType.None;

    private Selector loginRegisterSelector;
    private TextBox emailBox;
    private TextBox codeBox;
    private TextBox usernameBox;
    private TextBox passwordBox;
    private TextBox twoFactorBox;

    private readonly Dictionary<InputMode, (TextBox box, Func<char, bool> validator)> inputHandlers;
    private readonly Dictionary<InputMode, Action> enterHandlers;
    private readonly Dictionary<TextBox, InputMode> textBoxToInputMode;

    public LoginScene(IScene _prevScene)
    {
        prevScene = _prevScene;
        
        inputHandlers = [];
        enterHandlers = [];
        textBoxToInputMode = [];
    }

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        var op1 = new SelectorOption { Name = "login" };
        var op2 = new SelectorOption { Name = "register" };
        loginRegisterSelector = new Selector(p8, (29, 41), [op1, op2]);
        
        emailBox = new TextBox(p8, (29, 53), (69, 69), "email:", string.Empty);
        codeBox = new TextBox(p8, (29, 65), (69, 69), "code:", string.Empty);
        usernameBox = new TextBox(p8, (29, 77), (69, 69), "username:", string.Empty);
        passwordBox = new TextBox(p8, (29, 89), (69, 69), "password:", string.Empty);
        twoFactorBox = new TextBox(p8, (29, 101), (69, 69), "2fa code:", string.Empty);

        // Map text boxes to their input modes
        textBoxToInputMode[emailBox] = InputMode.Email;
        textBoxToInputMode[codeBox] = InputMode.Code;
        textBoxToInputMode[usernameBox] = InputMode.Username;
        textBoxToInputMode[passwordBox] = InputMode.Password;
        textBoxToInputMode[twoFactorBox] = InputMode.TwoFactorCode;

        inputHandlers[InputMode.Email] = (emailBox, c => {
            var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!#$%&'*+-/=?^_`{|}~.@";
            if (!allowed.Contains(c)) return false;
            if (c == '@' && (emailBox.Text.Contains('@') || emailBox.Text.Length == 0)) return false;
            if (c == '.' && !IsDotValid(emailBox.Text)) return false;
            return true;
        });

        inputHandlers[InputMode.Code] = (codeBox, c => {
            var allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return allowed.Contains(c) && codeBox.Text.Length <= 8;
        });

        inputHandlers[InputMode.Username] = (usernameBox, c => {
            var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
            return allowed.Contains(c) && usernameBox.Text.Length <= 16;
        });

        inputHandlers[InputMode.Password] = (passwordBox, c => c >= 32 && c <= 126 && c != ' ');

        inputHandlers[InputMode.TwoFactorCode] = (twoFactorBox, c => c >= '0' && c <= '9' && twoFactorBox.Text.Length < 6);

        //enterHandlers[InputMode.Email] = () => {
        //    if (isRegistering)
        //    {
        //        _ = VerifyEmail();
        //    }
        //    else
        //    {
        //        usernameBox.IsActive = true;
        //        emailBox.IsActive = false;
        //    }
        //};
        //
        //enterHandlers[InputMode.Username] = () => {
        //    passwordBox.IsActive = true;
        //    usernameBox.IsActive = false;
        //};
        //
        //enterHandlers[InputMode.Password] = () => {
        //    if (isRegistering)
        //    {
        //        _ = RegisterUser();
        //    }
        //    else
        //    {
        //        _ = LoginUser();
        //    }
        //};

        TextInputEXT.StartTextInput();
        EnableInputMode(InputMode.None);
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
            InputMode.None => null,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (currentHandler is not null)
        {
            TextInputEXT.TextInput += currentHandler;
        }
        currentInput = string.Empty;
    }

    private async Task VerifyEmail()
    {
        try
        {
            isProcessing = true;
            statusMessage = "Verifying email...";
            
            var response = await AccountHandler.VerifyEmailBeforeRegistration(emailBox.Text);

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

            var response = await AccountHandler.Register(emailBox.Text, usernameBox.Text, passwordBox.Text, codeBox.Text);

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

            var response = await AccountHandler.Login(emailBox.Text, passwordBox.Text);

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

            var response = await AccountHandler.Verify2FA(emailBox.Text, currentInput);

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
        if (inputHandlers[InputMode.Email].validator(c))
        {
            inputHandlers[InputMode.Email].box.HandleInput(c);
        }
    }

    private bool IsDotValid(string input)
    {
        if (input.Length == 0)
            return false;

        if (input.Last() == '.')
            return false;

        if (input.Last() == '@')
            return false;

        if (input.EndsWith("."))
            return false;

        if (!input.Contains('@'))
        {
            if (input.Length >= 2 && input[^2] == '.')
                return false;
            return true;
        }

        var parts = input.Split('@');
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
        if (inputHandlers[InputMode.Username].validator(c))
        {
            inputHandlers[InputMode.Username].box.HandleInput(c);
        }
    }

    private void OnPasswordInput(char c)
    {
        if (inputHandlers[InputMode.Password].validator(c))
        {
            inputHandlers[InputMode.Password].box.HandleInput(c);
        }
    }

    private void OnTwoFactorInput(char c)
    {
        if (inputHandlers[InputMode.TwoFactorCode].validator(c))
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

        if (keyboardState.IsKeyDown(Keys.Back) && prevKeyboardState.IsKeyUp(Keys.Back))
        {
            emailBox.HandleBackspace();
            usernameBox.HandleBackspace();
            passwordBox.HandleBackspace();
            codeBox.HandleBackspace();
            twoFactorBox.HandleBackspace();
        }

        if (keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter))
        {
            var activeBox = textBoxToInputMode.Keys.FirstOrDefault(box => box.IsActive);
            if (activeBox is not null && textBoxToInputMode.TryGetValue(activeBox, out var mode) && enterHandlers.ContainsKey(mode))
            {
                enterHandlers[mode]();
            }
        }

        if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
        {
            loginRegisterSelector.Update(p8, cursorX, cursorY);
            emailBox.ActiveUpdate(p8, cursorX, cursorY);
            codeBox.ActiveUpdate(p8, cursorX, cursorY);
            usernameBox.ActiveUpdate(p8, cursorX, cursorY);
            passwordBox.ActiveUpdate(p8, cursorX, cursorY);
            twoFactorBox.ActiveUpdate(p8, cursorX, cursorY);
        }

        // Set input mode based on active text box
        var activeTextBox = textBoxToInputMode.Keys.FirstOrDefault(box => box.IsActive);
        EnableInputMode(activeTextBox is not null ? textBoxToInputMode[activeTextBox] : InputMode.None);

        end_of_update:

        prevKeyboardState = keyboardState;
        prevMouseState = mouseState;
    }

    public void Draw()
    {
        p8.Batch.GraphicsDevice.Clear(Color.Black);

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

        p8.Rectfill(0, 0, 127, 127, 17);

        //Shared.Printc(p8, isRegistering ? "Registration" : "Login", 64, 1, 7);
        //Shared.Printc(p8, currentHandler?.Method.Name ?? "Email", 64, 7, 7);
        //Shared.Printc(p8, currentInput, 64, 13, 7);
        //Shared.Printc(p8, statusMessage, 64, 19, 7);
        //Shared.Printc(p8, "Press TAB to switch between Login/Register", 64, 25, 7);

        //p8.Pset(F32.FromInt(29 + 4), F32.FromInt(41 - 1), 7);
        loginRegisterSelector.Draw(p8);
        emailBox.Draw(p8);
        usernameBox.Draw(p8);
        passwordBox.Draw(p8);

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
