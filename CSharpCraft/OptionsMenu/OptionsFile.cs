using System.Text.Json;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;
using System.Security.Cryptography;
using CSharpCraft.Pcraft;

namespace CSharpCraft.OptionsMenu
{
    public record Binding(string Bind1, string Bind2);
    
    public class OptionsFile
    {
        public Binding Kbm_Left { get; set; } = new Binding("Left", "NumPad4");
        public Binding Kbm_Right { get; set; } = new Binding("Right", "NumPad6");
        public Binding Kbm_Up { get; set; } = new Binding("Up", "NumPad8");
        public Binding Kbm_Down { get; set; } = new Binding("Down", "NumPad5");
        public Binding Kbm_Use { get; set; } = new Binding("X", "V");
        public Binding Kbm_Menu { get; set; } = new Binding("Z", "C");
        public Binding Kbm_Pause { get; set; } = new Binding("Escape", "Enter");

        public Binding Con_Left { get; set; } = new Binding("DPadLeft", "LeftThumbstickLeft");
        public Binding Con_Right { get; set; } = new Binding("DPadRight", "LeftThumbstickRight");
        public Binding Con_Up { get; set; } = new Binding("DPadUp", "LeftThumbstickUp");
        public Binding Con_Down { get; set; } = new Binding("DPadDown", "LeftThumbstickDown");
        public Binding Con_Use { get; set; } = new Binding("A", "LeftShoulder");
        public Binding Con_Menu { get; set; } = new Binding("B", "RightShoulder");
        public Binding Con_Pause { get; set; } = new Binding("Start", "Back");

        public bool Gen_Sound_On { get; set; } = true;
        public int Gen_Music_Vol { get; set; } = 100;
        public int Gen_Sfx_Vol { get; set; } = 100;
        public int Pcraft_Soundtrack { get; set; } = 0;
        public int Pcraft_Sfx_Pack { get; set; } = 0;

        public bool Gen_Fullscreen { get; set; } = false;
        public int Gen_Window_Width { get; set; } = 512;
        public int Gen_Window_Height { get; set; } = 512;

        private static readonly string optionsFileName = "settings.json";
        private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true, WriteIndented = true };

        public List<FieldInfo> GetAllFields()
        {
            List<FieldInfo> fields = [];
            foreach (FieldInfo fieldInfo in GetType().GetFields())
            {
                fields.Add(fieldInfo);
            }
            return fields;
        }

        public static OptionsFile JsonWrite(OptionsFile file)
        {
            string jsonString = JsonSerializer.Serialize(file, jsonOptions);
            File.WriteAllText(optionsFileName, jsonString);
            return file;
        }

        private static OptionsFile CreateNewOptionsFile()
        {
            OptionsFile optionsFile = new();
            JsonWrite(optionsFile); // Save new defaults to disk
            return optionsFile;
        }

        private static void FixFile(OptionsFile file)
        {
            PropertyInfo[] properties = typeof(OptionsFile).GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                PropertyInfo? propertyName = typeof(OptionsFile).GetProperty(prop.Name);
                if (prop.Name.StartsWith("Kbm_"))
                {
                    Binding binding = (Binding)propertyName.GetValue(file);
                    Binding @default = (Binding)propertyName.GetValue(new OptionsFile());
                    if (binding.Bind1 is null || !KeyNames.keyNames.ContainsKey(binding.Bind1))
                    {
                        Binding newBinding = new Binding(@default.Bind1, binding.Bind2);
                        propertyName.SetValue(file, newBinding);
                        JsonWrite(file);
                    }
                    if (binding.Bind2 is null || !KeyNames.keyNames.ContainsKey(binding.Bind2))
                    {
                        Binding newBinding = new Binding(binding.Bind1, @default.Bind2);
                        propertyName.SetValue(file, newBinding);
                        JsonWrite(file);
                    }
                }
                else if (prop.Name.StartsWith("Con_"))
                {
                    Binding binding = (Binding)propertyName.GetValue(file);
                    Binding @default = (Binding)propertyName.GetValue(new OptionsFile());
                    if (binding.Bind1 is null || !ButtonNames.buttonNames.ContainsKey(binding.Bind1))
                    {
                        Binding newBinding = new Binding(@default.Bind1, binding.Bind2);
                        propertyName.SetValue(file, newBinding);
                        JsonWrite(file);
                    }
                    if (binding.Bind2 is null || !ButtonNames.buttonNames.ContainsKey(binding.Bind2))
                    {
                        Binding newBinding = new Binding(binding.Bind1, @default.Bind2);
                        propertyName.SetValue(file, newBinding);
                        JsonWrite(file);
                    }
                }
                else if (prop.Name == "Gen_Sound_On")
                {
                    //bool val = (bool)propertyName.GetValue(file);
                    //bool @default = (bool)propertyName.GetValue(new OptionsFile());
                    //if (!(val == true | val == false))
                    //{
                    //    propertyName.SetValue(file, @default);
                    //    JsonWrite(file);
                    //}
                }
                else if (prop.Name == "Gen_Fullscreen")
                {
                    //bool val = (bool)propertyName.GetValue(file);
                    //bool @default = (bool)propertyName.GetValue(new OptionsFile());
                    //if (!(val == true | val == false))
                    //{
                    //    propertyName.SetValue(file, @default);
                    //    JsonWrite(file);
                    //}
                }
                else if (prop.Name.EndsWith("_Vol"))
                {
                    int val = (int)propertyName.GetValue(file);
                    int @default = (int)propertyName.GetValue(new OptionsFile());
                    if (val < 0 || val > 100)
                    {
                        propertyName.SetValue(file, @default);
                        JsonWrite(file);
                    }
                }
                else if (prop.Name == "Pcraft_Soundtrack")
                {
                    int val = (int)propertyName.GetValue(file);
                    int @default = (int)propertyName.GetValue(new OptionsFile());
                    if (val < 0 || val > new PcraftSingleplayer().Music.Count - 1)
                    {
                        propertyName.SetValue(file, @default);
                        JsonWrite(file);
                    }
                }
                else if (prop.Name == "Pcraft_Sfx_Pack")
                {
                    int val = (int)propertyName.GetValue(file);
                    int @default = (int)propertyName.GetValue(new OptionsFile());
                    if (val < 0 || val > new PcraftSingleplayer().Sfx.Count - 1)
                    {
                        propertyName.SetValue(file, @default);
                        JsonWrite(file);
                    }
                }
                else if (prop.Name.StartsWith("Gen_Window_"))
                {
                    int val = (int)propertyName.GetValue(file);
                    int @default = (int)propertyName.GetValue(new OptionsFile());
                    if (val < 1 || val > 16384)
                    {
                        propertyName.SetValue(file, @default);
                        JsonWrite(file);
                    }
                }
            }
        }

        public static (OptionsFile file, bool error) Initialize()
        {
            const string optionsFileName = "settings.json";

            if (File.Exists(optionsFileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(optionsFileName);
                    OptionsFile? result = JsonSerializer.Deserialize<OptionsFile>(jsonString);

                    FixFile(result);

                    return (result, false);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Corrupted JSON: {ex.Message}. Recreating file.");
                    File.Delete(optionsFileName);
                    return (CreateNewOptionsFile(), true);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    Console.WriteLine($"File error: {ex.Message}. Using defaults.");
                    return (new OptionsFile(), true);
                }
            }
            else
            {
                return (CreateNewOptionsFile(), true);
            }
        }
    }
}
