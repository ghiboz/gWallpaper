using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Net;
using System.Runtime.InteropServices;

namespace gWallpaper
{
    class Program
    {

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        static void Main(string[] args)
        {
            int size = 3840;

            Random r = new Random(DateTime.Now.Millisecond);
            Console.WriteLine("Enter image id: (0~7 from newest to latest, 9 random)");
            var value = Console.ReadLine();
            int id = 9;
            try
            {
                id = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
            }
            ciuzz:
            if (id == 9)
            {
                int rInt = r.Next(0, 8); //for ints
                id = rInt;
            }
            string url = $"https://bing.biturl.top/?resolution={size}&format=json&index={id}&mkt=en-US";
            var jsonString = new WebClient().DownloadString(url);
            var json = System.Text.Json.JsonSerializer.Deserialize<BingStuff>(jsonString);

            var fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".jpg";
            new WebClient().DownloadFile(json.url, fileName);

            Set(fileName, Style.Stretched);

            Console.WriteLine($"Wallpaper #{id} set: {json.copyright}");
            Console.WriteLine("Enter image id: (0~7 from newest to latest, 9 random)");
            value = Console.ReadLine();
            id = -1;
            try
            {
                id = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
            }
            if (id >= 0)
            {
                goto ciuzz;
            }
        }

        static void Set(string imgPath, Style style)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            switch (style)
            {
                case Style.Tiled:
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                    break;
                case Style.Centered:
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                case Style.Stretched:
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                default:
                    break;
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imgPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }

    enum Style : int
    {
        Tiled,
        Centered,
        Stretched
    }

    class BingStuff
    {
        public string url { get; set; }
        public string copyright { get; set; }
    }
}
