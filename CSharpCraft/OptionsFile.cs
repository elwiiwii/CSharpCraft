using System.Text.Json;
using System.Reflection;

namespace CSharpCraft
{
    public record Binding(string Bind1, string Bind2);


    public class OptionsFile
    {
        public Binding Left { get; set; } = new Binding("Left", "NumPad4");
        public Binding Right { get; set; } = new Binding("Right", "NumPad6");
        public Binding Up { get; set; } = new Binding("Up", "NumPad8");
        public Binding Down { get; set; } = new Binding("Down", "NumPad5");
        public Binding Use { get; set; } = new Binding("X", "V");
        public Binding Menu { get; set; } = new Binding("Z", "C");
        public Binding Pause { get; set; } = new Binding("Escape", "Enter");

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

        public static OptionsFile JsonWrite(OptionsFile file)
        {
            var jsonOptions = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
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
                var optionsFile = new OptionsFile();
                return JsonWrite(optionsFile);
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
