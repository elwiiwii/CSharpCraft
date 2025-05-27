using System.Net.Mail;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using AccountService;
using FixMath;
using SDL3;
using System.Runtime.InteropServices;

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
    private enum InputMode { Email, Password, TwoFactorCode, Code, Username, None }
    private bool isRegistering = true;
    private bool requiresTwoFactor = false;
    private TwoFactorType twoFactorType = TwoFactorType.None;

    private Selector loginRegisterSelector;
    private TextBox emailBox;
    private TextBox passwordBox;
    private TextBox twoFactorBox;
    private TextBox codeBox;
    private TextBox usernameBox;
    private Button loginButton;
    private Button verifyButton;
    private Button submitCodeButton;
    private Button registerButton;
    private bool isEmailVerified = false;
    private bool isVerifyingCode = false;
    
    private readonly Dictionary<InputMode, (TextBox box, Func<char, bool> validator)> inputHandlers;
    private readonly Dictionary<InputMode, Action> enterHandlers;
    private readonly Dictionary<TextBox, InputMode> textBoxToInputMode;
    private readonly List<TextBox> loginFlow;
    private readonly List<TextBox> registerFlow;

    public LoginScene(IScene _prevScene)
    {
        prevScene = _prevScene;
        
        inputHandlers = [];
        enterHandlers = [];
        textBoxToInputMode = [];
        loginFlow = [];
        registerFlow = [];
    }

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        var op1 = new SelectorOption { Name = "login" };
        var op2 = new SelectorOption { Name = "register" };
        loginRegisterSelector = new Selector(p8, (29, 29), [op1, op2]);
        
        emailBox = new TextBox(p8, (29, 41), (69, 69), "email:");
        passwordBox = new TextBox(p8, (29, 53), (69, 69), "password:");
        twoFactorBox = new TextBox(p8, (29, 65), (69, 69), "2fa code:");
        verifyButton = new Button((46, 53), "verify", false);
        submitCodeButton = new Button((79, 53), "->", false);
        registerButton = new Button((42, 77), "register", false);

        codeBox = new TextBox(p8, (29, 53), (43, 43), "");
        usernameBox = new TextBox(p8, (29, 53), (69, 69), "username:");
        loginButton = new Button((29, 65), "login", false);

        loginFlow.AddRange([emailBox, passwordBox, twoFactorBox]);
        registerFlow.AddRange([emailBox, codeBox, usernameBox, passwordBox]);

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
            var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return allowed.Contains(c) && codeBox.Text.Length <= 8;
        });

        inputHandlers[InputMode.Username] = (usernameBox, c => {
            var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
            return allowed.Contains(c) && usernameBox.Text.Length <= 16;
        });

        inputHandlers[InputMode.Password] = (passwordBox, c => c >= 32 && c <= 126 && c != ' ');

        inputHandlers[InputMode.TwoFactorCode] = (twoFactorBox, c => c >= '0' && c <= '9' && twoFactorBox.Text.Length < 6);

        enterHandlers[InputMode.Email] = () => {
            if (isRegistering)
            {
                if (IsValidEmail(emailBox.Text))
                {
                    _ = VerifyEmail();
                }
            }
            else
            {
                passwordBox.IsActive = true;
                emailBox.IsActive = false;
            }
        };

        enterHandlers[InputMode.Code] = () => {
            
        };

        enterHandlers[InputMode.Username] = () => {
            
        };

        enterHandlers[InputMode.Password] = () => {
            if (isRegistering)
            {
                _ = RegisterUser();
            }
            else
            {
                _ = LoginUser();
            }
        };

        enterHandlers[InputMode.TwoFactorCode] = () => {
            _ = VerifyTwoFactor();
        };

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

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async Task VerifyEmail()
    {
        try
        {
            isProcessing = true;
            statusMessage = "Checking email...";
            
            // First check if email is already registered
            var checkResponse = await AccountHandler.CheckEmailExists(emailBox.Text);
            if (checkResponse.Exists)
            {
                statusMessage = "This email is already registered. Please login instead.";
                return;
            }

            statusMessage = "Sending verification email...";
            var response = await AccountHandler.VerifyEmailBeforeRegistration(emailBox.Text);

            if (response.Success)
            {
                statusMessage = "Verification email sent! Please check your inbox.";
                isVerifyingCode = true;
                codeBox.IsActive = true;
                emailBox.IsActive = false;
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

    private async Task SubmitVerificationCode()
    {
        try
        {
            isProcessing = true;
            statusMessage = "Verifying code...";
            
            var response = await AccountHandler.VerifyEmailCodeForRegistration(emailBox.Text, codeBox.Text);

            if (response.Success)
            {
                statusMessage = "Email verified successfully! Please set your username and password.";
                isEmailVerified = true;
                isVerifyingCode = false;
                codeBox.IsActive = false;
                usernameBox.IsActive = true;
            }
            else
            {
                statusMessage = response.Message;
                isVerifyingCode = false;
            }
        }
        catch (Exception ex)
        {
            statusMessage = $"Error: {ex.Message}";
            isVerifyingCode = false;
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
            statusMessage = "Creating account...";

            var response = await AccountHandler.Register(emailBox.Text, usernameBox.Text, passwordBox.Text, codeBox.Text);

            if (response.Success)
            {
                _ = LoginUser();
                statusMessage = response.Message;
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
                    p8.LoadCart(prevScene);
                    statusMessage = response.Message;
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

    private void ResetVerificationState()
    {
        isEmailVerified = false;
        isVerifyingCode = false;
        codeBox.SetText("");
        usernameBox.SetText("");
        passwordBox.SetText("");
        statusMessage = "Email changed. Please verify the new email address.";
    }

    private void OnEmailInput(char c)
    {
        if (inputHandlers[InputMode.Email].validator(c))
        {
            // Store the old email to check if it changed
            var oldEmail = emailBox.Text;
            inputHandlers[InputMode.Email].box.HandleInput(c);
            
            // If email changed and we were in a verified state, reset the verification
            if (oldEmail != emailBox.Text && (isEmailVerified || isVerifyingCode))
            {
                ResetVerificationState();
            }
        }
    }

    private void OnEmailBackspace()
    {
        // Store the old email to check if it changed
        var oldEmail = emailBox.Text;
        emailBox.HandleBackspace();
        
        // If email changed and we were in a verified state, reset the verification
        if (oldEmail != emailBox.Text && (isEmailVerified || isVerifyingCode))
        {
            ResetVerificationState();
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
        if (inputHandlers[InputMode.Code].validator(c))
        {
            inputHandlers[InputMode.Code].box.HandleInput(c);
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
            inputHandlers[InputMode.TwoFactorCode].box.HandleInput(c);
        }
    }

    private List<string> SplitStatusMessage(string message)
    {
        const int SCREEN_WIDTH = 128;
        const int MARGIN = 5;
        const int CHAR_WIDTH = 4;
        const int MAX_CHARS_PER_LINE = (SCREEN_WIDTH - (MARGIN * 2)) / CHAR_WIDTH;

        var lines = new List<string>();
        var words = message.Split(' ');
        var currentLine = new System.Text.StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + (currentLine.Length > 0 ? 1 : 0) > MAX_CHARS_PER_LINE)
            {
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                }

                if (word.Length > MAX_CHARS_PER_LINE)
                {
                    var remainingWord = word;
                    while (remainingWord.Length > 0)
                    {
                        var chunk = remainingWord.Substring(0, Math.Min(MAX_CHARS_PER_LINE, remainingWord.Length));
                        lines.Add(chunk);
                        remainingWord = remainingWord.Substring(chunk.Length);
                    }
                    continue;
                }
            }

            if (currentLine.Length > 0)
            {
                currentLine.Append(' ');
            }
            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine.ToString());
        }

        return lines;
    }

    public void Update()
    {
        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        cursorX = mouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        cursorY = mouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        if (isProcessing) goto end_of_update;

        if (loginRegisterSelector.Sel == 0 && isRegistering)
        {
            isRegistering = false;
            isEmailVerified = false;
            isVerifyingCode = false;
            passwordBox.StartPos = (29, 53);
        }
        else if (loginRegisterSelector.Sel == 1 && !isRegistering)
        {
            isRegistering = true;
            isEmailVerified = false;
            isVerifyingCode = false;
            passwordBox.StartPos = (29, 65);
        }

        if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.V) && prevKeyboardState.IsKeyUp(Keys.V))
        {
            var activeBox = textBoxToInputMode.Keys.FirstOrDefault(box => box.IsActive);
            if (activeBox is not null)
            {
                try
                {
                    var clipboardText = SDL.SDL_GetClipboardText();
                    if (!string.IsNullOrEmpty(clipboardText))
                    {
                        activeBox.HandlePaste(clipboardText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error pasting from clipboard: {ex.Message}");
                }
            }
        }

        if (keyboardState.IsKeyDown(Keys.Back) && prevKeyboardState.IsKeyUp(Keys.Back))
        {
            if (emailBox.IsActive)
            {
                OnEmailBackspace();
            }
            else
            {
                usernameBox.HandleBackspace();
                passwordBox.HandleBackspace();
                codeBox.HandleBackspace();
                twoFactorBox.HandleBackspace();
            }
        }

        if (keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter))
        {
            var activeBox = textBoxToInputMode.Keys.FirstOrDefault(box => box.IsActive);
            if (activeBox is not null && textBoxToInputMode.TryGetValue(activeBox, out var mode) && enterHandlers.ContainsKey(mode))
            {
                enterHandlers[mode]();
            }
        }

        if (isRegistering)
        {
            if (isEmailVerified)
            {
                registerButton.IsActive = !string.IsNullOrEmpty(usernameBox.Text) && !string.IsNullOrEmpty(passwordBox.Text);
                registerButton.Update(p8, cursorX, cursorY);
            }
            else if (isVerifyingCode)
            {
                submitCodeButton.IsActive = !string.IsNullOrEmpty(codeBox.Text);
                submitCodeButton.Update(p8, cursorX, cursorY);
            }
            else if (IsValidEmail(emailBox.Text))
            {
                verifyButton.IsActive = true;
                verifyButton.Update(p8, cursorX, cursorY);
            }
            else
            {
                verifyButton.IsActive = false;
            }
        }
        else
        {
            loginButton.Update(p8, cursorX, cursorY);
        }

        if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
        {
            loginRegisterSelector.Update(p8, cursorX, cursorY);
            
            if (isRegistering)
            {
                emailBox.ActiveUpdate(p8, cursorX, cursorY);
                
                if (!isEmailVerified)
                {
                    if (isVerifyingCode)
                    {
                        codeBox.ActiveUpdate(p8, cursorX, cursorY);
                        if (!string.IsNullOrEmpty(codeBox.Text) && submitCodeButton.IsHovered)
                        {
                            _ = SubmitVerificationCode();
                        }
                    }
                    else if (IsValidEmail(emailBox.Text) && verifyButton.IsHovered)
                    {
                        _ = VerifyEmail();
                    }
                }
                else
                {
                    codeBox.ActiveUpdate(p8, cursorX, cursorY);
                    usernameBox.ActiveUpdate(p8, cursorX, cursorY);
                    passwordBox.ActiveUpdate(p8, cursorX, cursorY);
                    if (!string.IsNullOrEmpty(usernameBox.Text) && !string.IsNullOrEmpty(passwordBox.Text) && registerButton.IsHovered)
                    {
                        _ = RegisterUser();
                    }
                }
            }
            else
            {
                emailBox.ActiveUpdate(p8, cursorX, cursorY);
                passwordBox.ActiveUpdate(p8, cursorX, cursorY);
                twoFactorBox.ActiveUpdate(p8, cursorX, cursorY);
                if (loginButton.IsHovered)
                {
                    _ = LoginUser();
                }
            }
        }

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

        loginRegisterSelector.Draw(p8);
        
        if (isRegistering)
        {
            emailBox.Draw(p8);
            
            if (isEmailVerified)
            {
                usernameBox.Draw(p8);
                passwordBox.Draw(p8);
                registerButton.Draw(p8);
            }
            else if (isVerifyingCode)
            {
                codeBox.Draw(p8);
                submitCodeButton.Draw(p8);
            }
            else if (IsValidEmail(emailBox.Text))
            {
                verifyButton.Draw(p8);
            }
        }
        else
        {
            emailBox.Draw(p8);
            passwordBox.Draw(p8);
            twoFactorBox.Draw(p8);
            loginButton.Draw(p8);
        }

        if (!string.IsNullOrEmpty(statusMessage))
        {
            var lines = SplitStatusMessage(statusMessage);
            for (int i = 0; i < lines.Count; i++)
            {
                Shared.Printc(p8, lines[i], 64, 120 - ((lines.Count - 1 - i) * 7), 15);
            }
        }

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
