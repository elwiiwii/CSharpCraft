using System.Text.Json;
using System.Reflection;

namespace CSharpCraft
{
    public class OptionsFile
    {
        public string Left { get; set; } = "Left";
        public string Right { get; set; } = "Right";
        public string Up { get; set; } = "Up";
        public string Down { get; set; } = "Down";
        public string Use { get; set; } = "X";
        public string Menu { get; set; } = "Z";
        public string Pause { get; set; } = "Enter";
        
        private static readonly string optionsFileName = "settings.json";

        public List<FieldInfo> GetAllFields()
        {
            List<FieldInfo> fields = [];
            foreach (var fieldInfo in GetType().GetFields())
            {
                fields.Add(fieldInfo);
            }
            return fields;
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
                var optionsFile = new OptionsFile();
                var writeIndented = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(optionsFile, writeIndented);
                File.WriteAllText(optionsFileName, jsonString);
                return optionsFile;
            }
        }


    }

    public class KeyboardOptions
    {
        public (string, string) Left { get; set; } = ("Left", "NumPad4");
        public (string, string) Right { get; set; } = ("Right", "NumPad6");
        public (string, string) Up { get; set; } = ("Up", "NumPad8");
        public (string, string) Down { get; set; } = ("Down", "NumPad5");
        public (string, string) Use { get; set; } = ("X", "V");
        public (string, string) Menu { get; set; } = ("Z", "C");
        public (string, string) Pause { get; set; } = ("Escape", "Enter");
    }

    public class ControllerOptions
    {
        public string Left1 { get; set; } = "DPadLeft";
        public string Left2 { get; set; } = "LeftStickLeft";
        public string Right1 { get; set; } = "DPadRight";
        public string Right2 { get; set; } = "LeftStickRight";
        public string Up1 { get; set; } = "DPadUp";
        public string Up2 { get; set; } = "LeftStickUp";
        public string Down1 { get; set; } = "DPadDown";
        public string Down2 { get; set; } = "LeftStickDown";
        public string Interact1 { get; set; } = "A";
        public string Interact2 { get; set; } = "LeftShoulder";
        public string Menu1 { get; set; } = "B";
        public string Menu2 { get; set; } = "RightShoulder";
        public string Pause { get; set; } = "Start";
    }



}
