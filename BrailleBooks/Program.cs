using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrailleBooks
{
    class Program
    {
        public static Dictionary<string, Color> avalColors = new Dictionary<string, Color>()
        {
            { "black", Color.FromArgb(0,0,0) },
            { "dark_blue",Color.FromArgb(0,0, 0xAA) },
            { "dark_green",Color.FromArgb(0,0xAA,0) },
            { "dark_aqua",Color.FromArgb(0,0xAA,0xAA) },
            { "dark_red",Color.FromArgb(0xAA,0,0) },
            { "dark_purple",Color.FromArgb(0xAA,0,0xAA) },
            { "gold",Color.FromArgb(0xFF,0xAA,0x00) },
            { "gray",Color.FromArgb(0xAA,0xAA,0xAA) },
            { "dark_gray",Color.FromArgb(0x55,0x55,0x55) },
            { "blue",Color.FromArgb(0x55,0x55,0xFF) },
            { "green",Color.FromArgb(0x55,0xFF,0x55) },
            { "aqua",Color.FromArgb(0x55,0xFF,0xFF) },
            { "red",Color.FromArgb(0x55,0x55,0x55) },
            { "light_purple",Color.FromArgb(0xFF,0x55,0xFF) },
            { "yellow",Color.FromArgb(0xFF,0xFF,0x55) },
            { "white",Color.FromArgb(0xFF,0xFF,0xFF) },
        };

        public static string GetClosestColorID(Color c)
        {
            string closest = "black";
            double minDist = double.PositiveInfinity;
            foreach (KeyValuePair<string, Color> arrC in avalColors)
            {
                Color color = arrC.Value;
                int dRSq = (color.R - c.R) * (color.R - c.R);
                int dGSq = (color.G - c.G) * (color.G - c.G);
                int dBSq = (color.B - c.B) * (color.B - c.B);

                double dist = (dRSq + dGSq + dBSq);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = arrC.Key;
                }
            }

            return closest;
        }

        public static Color GetPixel(Bitmap image, int x, int y)
        {
            Color pixel;
            try
            {
                pixel = image.GetPixel(x, y);
            }
            catch (Exception)
            {
                pixel = Color.White;
            }
            return pixel;
        }


        public static List<Bitmap> LoadFrameList(string path)
        {
            if (path.Last() == '/' || path.Last() == '\\')
                path.Remove(path.Length - 1, 1);

            List<Bitmap> frameList = new List<Bitmap>();

            if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg")) //single frame
            {
                frameList.Add(new Bitmap(path));
            }
            else if (path.EndsWith(".gif")) //animation
            {
                Bitmap bitmap = new Bitmap(path);
                var dimensions = new FrameDimension(bitmap.FrameDimensionsList[0]);
                var frameCount = bitmap.GetFrameCount(dimensions);
                for (int i = 0; i < frameCount; i++)
                {
                    bitmap.SelectActiveFrame(dimensions, i);
                    Bitmap frame = new Bitmap(bitmap);
                    frameList.Add(frame);
                }
            }
            else if (!path.Contains('.')) //frames of an animation in a folder
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                IEnumerable<FileInfo> files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
                files = files.Where((info) => info.Extension == ".png" || info.Extension == ".jpg" || info.Extension == ".jpeg");
                files.OrderBy((file) => file.Name);

                foreach (FileInfo file in files)
                {
                    Bitmap bitmap = new Bitmap(file.FullName);
                    frameList.Add(bitmap);
                }
            }
            else
            {
                return null;
            }

            return frameList;
        }

        public static void ResizeFrameList(List<Bitmap> frameList)
        {
            for (int i = 0; i < frameList.Count; i++)
            {
                double wperc = frameList[i].Width / 76.0;
                double hperc = frameList[i].Height / 56.0;

                if (wperc <= 1 && hperc <= 1) //pad image up to 76 x 56
                {
                    Bitmap bitmap = new Bitmap(76, 56);
                    Graphics canvas = Graphics.FromImage(bitmap);
                    canvas.Clear(Color.White);
                    canvas.DrawImage(frameList[i], 0, 0);
                    frameList[i] = bitmap;
                }
                else //resize keeping side ratio
                {
                    double factor = 1;
                    if (wperc > hperc)
                    {
                        factor = 76.0 / frameList[i].Width;
                    }
                    else
                    {
                        factor = 56.0 / frameList[i].Height;
                    }

                    int newW = (int)(frameList[i].Width * factor);
                    int newH = (int)(frameList[i].Height * factor);

                    Bitmap bitmap = new Bitmap(76, 56);
                    Graphics canvas = Graphics.FromImage(bitmap);
                    canvas.Clear(Color.White);
                    canvas.DrawImage(frameList[i], 0, 0, newW, newH);

                    frameList[i] = bitmap;
                }
            }
        }

        public static List<List<string>> CreateColorMapList(List<Bitmap> frameList)
        {
            List<List<string>> colorMapList = new List<List<string>>();

            foreach (Bitmap image in frameList)
            {
                List<string> colorMap = new List<string>();

                for (int j = 0; j < image.Height / 4; j++)
                {
                    for (int i = 0; i < image.Width / 2; i++)
                    {
                        int R = 0, G = 0, B = 0;
                        int dotCount = 0;
                        for (int k = 0; k < 2; k++)
                        {
                            for (int l = 0; l < 4; l++)
                            {
                                Color pixel = GetPixel(image, i * 2 + k, j * 4 + l);
                                if (GetClosestColorID(pixel) == "white")
                                    continue;
                                R += pixel.R;
                                G += pixel.G;
                                B += pixel.B;
                                dotCount++;
                            }
                        }
                        //image.Save("wtf.png");
                        if (dotCount == 0)
                            dotCount = 1;
                        string closestColor = GetClosestColorID(Color.FromArgb(R / dotCount, G / dotCount, B / dotCount));

                        colorMap.Add(closestColor);
                    }
                }

                colorMapList.Add(colorMap);
            }

            return colorMapList;
        }

        public static List<Bitmap> CreateDotMapList(List<Bitmap> frameList)
        {
            List<Bitmap> dotMapList = new List<Bitmap>();

            foreach (Bitmap image in frameList)
            {
                Bitmap dotMap = new Bitmap(image.Width, image.Height);

                for (int j = 0; j < image.Height; j++)
                {
                    for (int i = 0; i < image.Width; i++)
                    {
                        Color pixel = GetPixel(image, i, j);
                        if (GetClosestColorID(pixel) != "white")
                        {
                            dotMap.SetPixel(i, j, Color.Black);
                        }
                        else
                        {
                            dotMap.SetPixel(i, j, Color.White);
                        }

                    }
                }

                dotMapList.Add(dotMap);
            }

            return dotMapList;
        }

        public static string CreateBraillePage(Bitmap dotMap)
        {
            StringBuilder strBuilder = new StringBuilder((dotMap.Width / 2 + 1) * dotMap.Height / 4);

            for (int j = 0; j < dotMap.Height; j += 4)
            {
                for (int i = 0; i < dotMap.Width; i += 2)
                {
                    //7 4
                    //6 3
                    //5 2
                    //1 0

                    Color[] pixels = new Color[8];
                    pixels[0] = GetPixel(dotMap, i + 1, j + 3);
                    pixels[1] = GetPixel(dotMap, i, j + 3);
                    pixels[2] = GetPixel(dotMap, i + 1, j + 2);
                    pixels[3] = GetPixel(dotMap, i + 1, j + 1);
                    pixels[4] = GetPixel(dotMap, i + 1, j);
                    pixels[5] = GetPixel(dotMap, i, j + 2);
                    pixels[6] = GetPixel(dotMap, i, j + 1);
                    pixels[7] = GetPixel(dotMap, i, j);

                    int brailleCharacter = 0;

                    foreach (Color pixel in pixels)
                    {
                        brailleCharacter <<= 1;
                        if (GetClosestColorID(pixel) != "white")
                            brailleCharacter |= 1;
                    }

                    brailleCharacter |= 0x2800;
                    strBuilder.Append((char)brailleCharacter);
                }
            }

            return strBuilder.ToString();
        }

        public static List<string> CreateBraillePageList(List<Bitmap> dotMapList)
        {
            List<string> pageList = new List<string>();

            foreach (Bitmap dotMap in dotMapList)
            {
                string frame = CreateBraillePage(dotMap);
                pageList.Add(frame);
            }

            return pageList;
        }

        public static List<string> CreateFormattedPageList(List<string> braillePageList, List<List<string>> colorMapList)
        {
            List<string> FormattedPageList = new List<string>();

            for (int i = 0; i < braillePageList.Count; i++)
            {
                StringBuilder formattedPage = new StringBuilder();

                string page = braillePageList[i];
                List<string> colorMap = colorMapList[i];

                string curColor = colorMap.First();

                formattedPage.Append("[{\\\"text\\\":\\\"");
                for (int j = 0; j < colorMap.Count; j++)
                {
                    string color = colorMap[j];
                    char character = page[j];

                    if (color != curColor)
                    {
                        formattedPage.Append("\\\",\\\"color\\\":\\\"");
                        formattedPage.Append(curColor);
                        formattedPage.Append("\\\"},{\\\"text\\\":\\\"");
                        curColor = color;
                    }

                    formattedPage.Append(character);

                    if (j % 38 == 37)
                        formattedPage.Append(@"\\n");
                }
                formattedPage.Append("\\\",\\\"color\\\":\\\"");
                formattedPage.Append(curColor);
                formattedPage.Append("\\\"}]");

                FormattedPageList.Add(formattedPage.ToString());
            }

            return FormattedPageList;
        }

        public static string CreateCommand(List<string> FormattedPageList, string name, string author)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("give @p written_book{pages:[");

            foreach (string page in FormattedPageList)
            {
                stringBuilder.Append("\"");
                stringBuilder.Append(page);
                stringBuilder.Append("\",");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append("],title:\"" + name + "\",author:\"" + author + "\"}");

            return stringBuilder.ToString();
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please drag .png, .jpg, .gif or a folder with images onto a program instead of just launching it (read readme)");
                Console.ReadKey();
                return;
            }
            string path = args[0];

            Console.WriteLine("Loading...");
            List<Bitmap> frames = LoadFrameList(path);
            Console.WriteLine("Resizing...");
            ResizeFrameList(frames);
            Console.WriteLine("Converting...");
            List<List<string>> colorMapList = CreateColorMapList(frames);
            List<Bitmap> dotMapList = CreateDotMapList(frames);
            List<string> braillePageList = CreateBraillePageList(dotMapList);

            string title, author;
            Console.WriteLine("Enter book title and press Enter:");
            title = Console.ReadLine();
            title = string.IsNullOrEmpty(title) ? "Sample Title" : title;

            Console.WriteLine("Enter book author's name and press Enter:");
            author = Console.ReadLine();
            author = string.IsNullOrEmpty(author) ? "Sample Author" : author;

            Console.WriteLine("Generating command...");
            List<string> formattedPageList = CreateFormattedPageList(braillePageList, colorMapList);
            string command = CreateCommand(formattedPageList, title, author);

            Console.WriteLine("Saving...");
            using (var writer = new StreamWriter("command.mcfunction", false, new UTF8Encoding(false)))
            {
                writer.Write(command);
            }
            Console.WriteLine("Done! Saved as command.mcfunction. Press any key to exit");
            Console.ReadKey();
        }
    }
}
