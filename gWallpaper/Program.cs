﻿using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;

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

            var savePath = "";
            if (args.Length > 0)
            {
                savePath = args[0];
            }

            if (string.IsNullOrEmpty(savePath))
            {
                savePath = System.IO.Path.GetTempPath();
            }

            Console.WriteLine($"save path: {savePath}");

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

            url = $"https://cn.bing.com/HPImageArchive.aspx?format=xml&idx={id}&n=1&mkt=en-US";
            var bingURL = "https://cn.bing.com";
            
            
            var xmlString = new WebClient().DownloadString(url);

            var xml = new XmlDocument();
            xml.LoadXml(xmlString);

            var nodeImages = xml.SelectNodes("images")[0];
            var nodeImage = nodeImages.SelectNodes("image")[0];
            var xStartDate = nodeImage.SelectNodes("startdate")[0].InnerText;
            var xEndtDate = nodeImage.SelectNodes("enddate")[0].InnerText;
            var xUrl = nodeImage.SelectNodes("url")[0].InnerText;
            var xUrlBase = nodeImage.SelectNodes("urlBase")[0].InnerText;
            var xCopyright = $"{nodeImage.SelectNodes("copyright")[0].InnerText}";
            // var xCopyrightLink = nodeImage.SelectNodes("copyrightLink")[0].InnerText;

            var realUrl = $"{bingURL}{xUrlBase}_UHD.jpg";



            //var jsonString = new WebClient().DownloadString(url);

            
            //var json = System.Text.Json.JsonSerializer.Deserialize<BingStuff>(jsonString);

            var fileName = Path.Combine(savePath, GetTitle(xCopyright) + ".jpg");

            new WebClient().DownloadFile(realUrl, fileName);

            var wpFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "wallpaper.jpg");

            File.Copy(fileName, wpFile, true);

            Set(wpFile, Style.Stretched);

            Console.WriteLine($"Wallpaper #{id} set: {xCopyright}");
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

        static string GetTitle(string copyrights)
        {
            var last = copyrights.IndexOf('(');
            return ReplaceInvalidChars(copyrights.Substring(0, last - 1));
        }
        static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
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
