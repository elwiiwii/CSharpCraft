﻿using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu;

public class GeneralOptions(int startIndex = 0) : IScene, IDisposable
{

    public string SceneName { get => "options"; }
    private Pico8Functions p8;

    private int menuSelected;

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        menuSelected = startIndex;
    }

    public void Update()
    {
        if (menuSelected < 0) { menuSelected = 0; }

        PropertyInfo[] properties = typeof(OptionsFile).GetProperties();
        List<PropertyInfo> propertyList = [];
        foreach (PropertyInfo prop in properties)
        {
            if (prop.Name.StartsWith("Gen_")) { propertyList.Add(prop); }
        }
        PropertyInfo curProperty = propertyList[menuSelected];

        if (p8.Btnp(0) || p8.Btnp(1))
        {
            
            if (curProperty.Name == "Gen_Sound_On")
            {
                curProperty.SetValue(p8.OptionsFile, !(bool)curProperty.GetValue(p8.OptionsFile));
                OptionsFile.JsonWrite(p8.OptionsFile);
                p8.Mute();
            }
            else if (curProperty.Name == "Gen_Fullscreen")
            {
                curProperty.SetValue(p8.OptionsFile, !(bool)curProperty.GetValue(p8.OptionsFile));
                OptionsFile.JsonWrite(p8.OptionsFile);
                p8.Graphics.ToggleFullScreen();
            }

            if (p8.Btnp(0))
            {
                if (curProperty.Name.EndsWith("_Vol"))
                {
                    curProperty.SetValue(p8.OptionsFile, Math.Max(0, (int)curProperty.GetValue(p8.OptionsFile) - 10));
                    OptionsFile.JsonWrite(p8.OptionsFile);
                }
                else if (curProperty.Name.StartsWith("Gen_Window_"))
                {
                    curProperty.SetValue(p8.OptionsFile, Math.Max(128, (int)curProperty.GetValue(p8.OptionsFile) - 128));
                    OptionsFile.JsonWrite(p8.OptionsFile);
                    p8.Graphics.PreferredBackBufferWidth = p8.OptionsFile.Gen_Window_Width;
                    p8.Graphics.PreferredBackBufferHeight = p8.OptionsFile.Gen_Window_Height;
                    p8.Graphics.ApplyChanges();
                }
            }
            if (p8.Btnp(1))
            {
                if (curProperty.Name.EndsWith("_Vol"))
                {
                    curProperty.SetValue(p8.OptionsFile, Math.Min(100, (int)curProperty.GetValue(p8.OptionsFile) + 10));
                    OptionsFile.JsonWrite(p8.OptionsFile);
                }
                else if (curProperty.Name.StartsWith("Gen_Window_"))
                {
                    curProperty.SetValue(p8.OptionsFile, Math.Min(16384, (int)curProperty.GetValue(p8.OptionsFile) + 128));
                    OptionsFile.JsonWrite(p8.OptionsFile);
                    p8.Graphics.PreferredBackBufferWidth = p8.OptionsFile.Gen_Window_Width;
                    p8.Graphics.PreferredBackBufferHeight = p8.OptionsFile.Gen_Window_Height;
                    p8.Graphics.ApplyChanges();
                }
            }
        }

        if (p8.Btnp(2)) { menuSelected -= 1; }
        if (p8.Btnp(3)) { menuSelected += 1; }

        if (menuSelected < 0) { p8.LoadCart(new GeneralOptionsTitle()); return; }
        menuSelected = GeneralFunctions.Loop(menuSelected, propertyList.Count);
    }

    public void Draw()
    {
        p8.Cls();

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        
        p8.Batch.Draw(p8.TextureDictionary["OptionsBackground5"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        PropertyInfo[] properties = typeof(OptionsFile).GetProperties();
        int x = 15;
        int y = 43;
        int step = 8;

        if (menuSelected > -1)
        {
            Vector2 position5 = new((x - 4) * p8.Cell.Width, (menuSelected * step + y) * p8.Cell.Height);
            p8.Batch.Draw(p8.TextureDictionary["Arrow"], position5, null, p8.Colors[6], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
        }

        p8.Batch.Draw(p8.TextureDictionary["Checker"], new Vector2(x * p8.Cell.Width, (y - 5) * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        foreach (PropertyInfo property in properties)
        {
            if (property.Name.StartsWith("Gen_"))
            {
                p8.Print($"{property.Name.Substring(4).ToLower()} : {property.GetValue(p8.OptionsFile).ToString().ToLower()}", x + 2, y, 6);
                y += step;
            }
        }

        p8.Batch.Draw(p8.TextureDictionary["Checker"], new Vector2(x * p8.Cell.Width, (y + 1) * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipVertically, 0);
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

    }

}
