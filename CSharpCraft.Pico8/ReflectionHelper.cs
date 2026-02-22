using System.Reflection;

namespace CSharpCraft.Pico8;

/// <summary>
/// Centralizes reflection operations for accessing game-specific types and members.
/// Replaces scattered reflection hacks with proper error handling and logging.
/// </summary>
public static class ReflectionHelper
{
    private const string GameAssemblyName = "CSharpCraft.Game";
    private const string OptionsTypeName = "CSharpCraft.OptionsMenu.OptionsFile";
    private const string TitleScreenTypeName = "CSharpCraft.Game.TitleScreen";
    private const string SoundPropertyName = "Gen_Sound_On";
    private const string JsonWriteMethodName = "JsonWrite";

    /// <summary>
    /// Load a type from the CSharpCraft.Game assembly by name.
    /// </summary>
    public static Type? GetGameType(string fullTypeName)
    {
        try
        {
            var assembly = Assembly.Load(GameAssemblyName);
            if (assembly == null)
            {
                LogError($"Failed to load assembly: {GameAssemblyName}");
                return null;
            }

            var type = assembly.GetType(fullTypeName);
            if (type == null)
            {
                LogWarning($"Type not found: {fullTypeName} in {GameAssemblyName}");
                return null;
            }

            return type;
        }
        catch (Exception ex)
        {
            LogError($"Exception loading type {fullTypeName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get a static property value from a game type.
    /// </summary>
    public static T? GetStaticProperty<T>(Type type, string propertyName) where T : class
    {
        try
        {
            var property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
            if (property == null)
            {
                LogWarning($"Static property not found: {type.Name}.{propertyName}");
                return null;
            }

            var value = property.GetValue(null);
            return value as T;
        }
        catch (Exception ex)
        {
            LogError($"Exception getting static property {propertyName} from {type.Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get a static field value from a game type.
    /// </summary>
    public static T? GetStaticField<T>(Type type, string fieldName) where T : class
    {
        try
        {
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
            if (field == null)
            {
                LogWarning($"Static field not found: {type.Name}.{fieldName}");
                return null;
            }

            var value = field.GetValue(null);
            return value as T;
        }
        catch (Exception ex)
        {
            LogError($"Exception getting static field {fieldName} from {type.Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get a property value from an instance.
    /// </summary>
    public static T? GetProperty<T>(object? instance, string propertyName) where T : class
    {
        try
        {
            if (instance == null)
            {
                LogWarning($"Cannot get property {propertyName} from null instance");
                return null;
            }

            var property = instance.GetType().GetProperty(propertyName);
            if (property == null)
            {
                LogWarning($"Property not found: {instance.GetType().Name}.{propertyName}");
                return null;
            }

            var value = property.GetValue(instance);
            return value as T;
        }
        catch (Exception ex)
        {
            LogError($"Exception getting property {propertyName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Set a property value on an instance.
    /// </summary>
    public static bool SetProperty(object instance, string propertyName, object? value)
    {
        try
        {
            if (instance == null)
            {
                LogWarning($"Cannot set property {propertyName} on null instance");
                return false;
            }

            var property = instance.GetType().GetProperty(propertyName);
            if (property == null)
            {
                LogWarning($"Property not found: {instance.GetType().Name}.{propertyName}");
                return false;
            }

            property.SetValue(instance, value);
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Exception setting property {propertyName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Invoke a static method on a type.
    /// </summary>
    public static object? InvokeStaticMethod(Type type, string methodName, params object?[] args)
    {
        try
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                LogWarning($"Static method not found: {type.Name}.{methodName}");
                return null;
            }

            return method.Invoke(null, args);
        }
        catch (Exception ex)
        {
            LogError($"Exception invoking static method {methodName} on {type.Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create an instance of a type from the game assembly.
    /// </summary>
    public static object? CreateGameInstance(string fullTypeName)
    {
        try
        {
            var type = GetGameType(fullTypeName);
            if (type == null)
                return null;

            return Activator.CreateInstance(type, false);
        }
        catch (Exception ex)
        {
            LogError($"Exception creating instance of {fullTypeName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Game-specific: Get the OptionsFile instance and toggle sound setting.
    /// </summary>
    public static bool ToggleSoundSetting()
    {
        try
        {
            var optionsType = GetGameType(OptionsTypeName);
            if (optionsType == null)
                return false;

            var optionsInstance = CreateGameInstance(OptionsTypeName);
            if (optionsInstance == null)
                return false;

            // Get current value
            var property = optionsType.GetProperty(SoundPropertyName);
            if (property == null)
            {
                LogWarning($"Property {SoundPropertyName} not found on {OptionsTypeName}");
                return false;
            }

            var currentValue = property.GetValue(optionsInstance) as bool? ?? true;
            var newValue = !currentValue;

            // Set new value
            if (!SetProperty(optionsInstance, SoundPropertyName, newValue))
                return false;

            // Persist to file
            InvokeStaticMethod(optionsType, JsonWriteMethodName, optionsInstance);
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Exception in ToggleSoundSetting: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Game-specific: Try to get the TitleScreen instance.
    /// </summary>
    public static IScene? GetTitleScreen()
    {
        try
        {
            var titleScreenType = GetGameType(TitleScreenTypeName);
            if (titleScreenType == null)
                return null;

            var instance = Activator.CreateInstance(titleScreenType, false);
            if (instance is IScene scene)
                return scene;

            LogWarning($"{TitleScreenTypeName} does not implement IScene");
            return null;
        }
        catch (Exception ex)
        {
            LogError($"Exception getting TitleScreen: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Simple logging - replace with proper logger if needed.
    /// </summary>
    private static void LogError(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[ReflectionHelper ERROR] {message}");
    }

    /// <summary>
    /// Simple logging - replace with proper logger if needed.
    /// </summary>
    private static void LogWarning(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[ReflectionHelper WARN] {message}");
    }
}
