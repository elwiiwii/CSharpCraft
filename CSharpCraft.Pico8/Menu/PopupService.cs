namespace CSharpCraft.Pico8.Menu;

/// <summary>
/// Popup notification service with severity-based color coding.
/// Manages the popup animation lifecycle: show → grow in → pause → shrink out → deactivate.
/// 
/// Animation model:
/// - Frame starts at FrameStep (1.5) on Show()
/// - Each Update() advances frame by FrameStep until HalfDuration (30)
/// - At HalfDuration, frame negates (begins shrinking)
/// - When |frame| drops below FrameStep, popup deactivates
/// 
/// Color coding:
/// - Info: PICO-8 color 8 (red) background, color 15 (peach) text
/// - Error: PICO-8 color 2 (dark purple) background, color 7 (white) text
/// </summary>
public class PopupService : IPopupService
{
    private readonly IGraphicsAPI _graphics;

    // Popup animation state
    private string _popupText = "";
    private double _popupFrame;
    private PopupSeverity _popupSeverity;

    private const int HalfDuration = 30;
    private const double FrameStep = 1.5;

    // Severity color mappings (PICO-8 palette indices)
    private static readonly Dictionary<PopupSeverity, (int background, int text)> SeverityColors = new()
    {
        { PopupSeverity.Info, (8, 15) },   // red background, peach text
        { PopupSeverity.Error, (2, 7) }    // dark purple background, white text
    };

    public PopupService(IGraphicsAPI graphics)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
    }

    public bool HasActivePopup => Math.Abs(_popupFrame) > 0;

    /// <inheritdoc />
    public void Show(string text, PopupSeverity severity = PopupSeverity.Info)
    {
        _popupText = text;
        _popupFrame = FrameStep;
        _popupSeverity = severity;
    }

    /// <inheritdoc />
    public void Update()
    {
        if (Math.Abs(_popupFrame) < FrameStep)
        {
            _popupFrame = 0;
        }
        else if (_popupFrame != 0 && _popupFrame < HalfDuration)
        {
            _popupFrame += FrameStep;
        }
        else if (_popupFrame != 0 && _popupFrame >= HalfDuration)
        {
            _popupFrame *= -1;
        }
    }

    /// <inheritdoc />
    public void Draw((int w, int h) resolution)
    {
        if (!HasActivePopup) return;

        int clampFrame = Math.Abs((int)Math.Floor(_popupFrame)) > 7
            ? 7
            : Math.Abs((int)Math.Floor(_popupFrame));

        if (clampFrame > 0)
        {
            var (bgColor, textColor) = SeverityColors.GetValueOrDefault(_popupSeverity, (8, 15));

            int x1 = 0;
            int y1 = resolution.h - clampFrame;
            int x2 = resolution.w - 1;
            int y2 = resolution.h - clampFrame + 7;

            _graphics.Rectfill(x1, y1, x2, y2, bgColor);
            _graphics.Print(_popupText, 1, y1 + 1, textColor);
        }
    }
}
