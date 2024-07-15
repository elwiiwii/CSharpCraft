using System.Text.Json;

namespace CSharpCraft
{
    public class OptionsFile
    {
        public string Left { get; set; } = "Left";
        public string Right { get; set; } = "Right";
        public string Up { get; set; } = "Up";
        public string Down { get; set; } = "Down";
        public string Interact { get; set; } = "X";
        public string Menu { get; set; } = "Z";
        public string Pause { get; set; } = "Enter";
        
        private static readonly string optionsFileName = "settings.json";

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


}
