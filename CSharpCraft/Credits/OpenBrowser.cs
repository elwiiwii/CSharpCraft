using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.Credits.Credits
{
    public class OpenBrowser
    {
        public static void OpenUrl(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    try
                    {
                        Process.Start("xdg-open", url);
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    try
                    {
                        Process.Start("open", url);
                    }
                    catch
                    {
                        return;
                    }
                }
                else
                {
                    throw new PlatformNotSupportedException("OS not supported");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open URL: {ex.Message}");
                throw;
            }
        }
    }
}