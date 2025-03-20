using CSharpCraft.Pico8;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;

namespace CSharpCraft.Pcraft
{
    public class CustomMapCraft : PcraftBase
    {
        public override string SceneName => "custom map";

        private byte[] surfaceArray = [];
        private byte[] caveArray = [];

        public byte[] ImageToByteArray(string imagePath)
        {
            using var fileStream = File.OpenRead(imagePath);
            fileStream.Position = 0;
            IImageFormat format = Image.DetectFormat(fileStream);

            using var image = Image.Load(fileStream);
            using var memoryStream = new MemoryStream();

            image.Save(memoryStream, GetEncoder(format));
            return memoryStream.ToArray();
        }

        private static IImageEncoder GetEncoder(IImageFormat format)
        {
            return format switch
            {
                BmpFormat => new BmpEncoder(),
                JpegFormat => new JpegEncoder(),
                PngFormat => new PngEncoder(),
                _ => new PngEncoder()
            };
        }

        protected override void CreateMap()
        {
            for (int i = 0; i < levelsx; i++)
            {
                for (int j = 0; j < levelsy; j++)
                {
                    p8.Mset(i + levelx, j + levely, level[i][j].Double);
                }
            }
        }

        public override void Init(Pico8Functions pico8)
        {
            base.Init(pico8);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}
