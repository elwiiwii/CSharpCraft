using System.Text.Json;
using System.Reflection;

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

        public static OptionsFile Initialize()
        {

            if (File.Exists(optionsFileName))
            {
                string jsonString = File.ReadAllText(optionsFileName);
                //need to handle bad quality json
                return JsonSerializer.Deserialize<OptionsFile>(jsonString);
            }
            else
            {
                OptionsFile optionsFile = new();
                return JsonWrite(optionsFile);
            }
        }
    }
}
