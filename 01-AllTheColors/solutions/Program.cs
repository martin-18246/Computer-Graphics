using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CommandLine;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Drawing;
using System.Diagnostics.Metrics;

public class Options
{
    [Option('n', "name", Required = true, HelpText = "Desired image name")]
    public string? ImageName { get; set; }

    [Option('w', "width", Required = true, HelpText = "Desired image width")]
    public int Width { get; set; }

    [Option('h', "height", Required = true, HelpText = "Desired image height")]
    public int Height { get; set; }

    [Option('m', "mode", Required = true, HelpText = "Mode: trivial, random, or ornament")]
    public string Mode { get; set; }

    [Option('s', "seed", Required = false, HelpText = "Random seed (if relevant for mode 'random')")]
    public int? Seed { get; set; }

    [Option("numCirclesVertical", Required = false, HelpText = "How many circles will be on the vertical axis.")]
    public int? numCirclesVertical { get; set; }

    [Option("radiusWidth", Required = false, HelpText = "Thickness of the circles.")]
    public int? radiusWidth { get; set; }
}

class Program
{
    public static byte r = 0;
    public static byte g = 0;
    public static byte b = 0;
    public static int width; 
    public static int height; 
    public static int maxNumColors = 16777216;
    public static string? filename;
    public static DateTime startTime;
    public static int intervalPrintInfo = 10000;
    public static string mode = "trivial";
    public static int? seed;
    public static int? numCirclesVertical;
    public static int? radiusWidth;

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed(RunOptions);
        startTime = DateTime.Now;

        if (mode == "trivial")
        {
            FillSimple();
        }

        else if (mode == "random") {
            FillRandom(seed.Value);

        }

        else if (mode == "ornament")
        {
            FillPatterns(5); 
        }

        DateTime endTime = DateTime.Now;

        Console.WriteLine($"Program finished in {Math.Round((endTime - startTime).TotalSeconds, 2)} seconds.");

    }

    static void RunOptions(Options opts)
    {
        filename = opts.ImageName;
        width = opts.Width;
        height = opts.Height;
        mode = opts.Mode.ToLower();
        seed = opts.Seed;

        if (opts.Width * opts.Height < 16777216)
        {
            Console.WriteLine("Image size must be at least 16 777 216 pixels.");
            Environment.Exit(0);
            return;
        }


        if (mode == "random" && !seed.HasValue)
        {
            Console.WriteLine("Seed is required for the 'random' mode.");
            Environment.Exit(0);
            return;
        }

        if (mode == "ornament" && (!opts.numCirclesVertical.HasValue || !opts.radiusWidth.HasValue))
        {
            Console.WriteLine("You must specify the properties of the circles.");
            Environment.Exit(0);
            return;
        }

        else
        {
            numCirclesVertical = opts.numCirclesVertical;
            radiusWidth = opts.radiusWidth;
        }

    }


    public static void ShiftColor()
    {
        if (r >= 255)
        {
            r = 0;
            if (g >= 255)
            {
                g = 0; 
                if (b >= 255)
                {
                    b = 0; 
                }
                
                else
                {
                    b++;
                }
            }

            else
            {
                g++; 
            }
        }

        else
        {
            r++;
        }
    }

    public static void FillSimple() // works, even with different resolution, basic takes 1.25 seconds
    {
        using (var image = new Image<Rgba32>(width, height))
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new Rgba32(r, g, b);
                    ShiftColor();
                }
            }


            SaveImage(image);
        }
    }

    public static void FillRandom(int seed)
    {
        Random random = new Random(seed);
        int[,] arrayAllColors = CreateArrayAllColors();
        int[] arrayInts = CreateArrayOfInts(maxNumColors, false, 0);
        arrayInts = ShuffleArray(arrayInts, random);



        using (var image = new Image<Rgba32>(width, height))
        {
            int count = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (count % intervalPrintInfo == 0)
                    {
                        PrintStatus(count, height * width, 2);
                    }

                    int[] randomColor = new int[3];

                    if (count >= arrayInts.Length)
                    {
                        randomColor = SelectRandomColor(random);
                    }

                    else
                    {
                        randomColor[0] = arrayAllColors[0, arrayInts[count]];
                        randomColor[1] = arrayAllColors[1, arrayInts[count]];
                        randomColor[2] = arrayAllColors[2, arrayInts[count]];
                    }

                    image[x, y] = new Rgba32((byte)randomColor[0], (byte)randomColor[1], (byte)randomColor[2]);
                    count++;
                }


            }


            SaveImage(image);
        }

    }

    public static void FillPatterns(int seed)
    {
        int[,] allColors = returnAllColorsArray();
        int numCirclesHeight = numCirclesVertical.Value;
        int outerRadius = height / (numCirclesHeight * 2);
        int circleWidth = radiusWidth.Value;
        int numCirclesWidth = width / (outerRadius * 2) + 1;
        int numCirclesTotal = numCirclesHeight * numCirclesWidth;
        int counterTotal = 0;

        int[,] centers = new int[2, numCirclesTotal];

        int counterCenters = 0; 
        for (int y = 0; y < numCirclesHeight ;y++)
        {
            for (int x = 0; x < numCirclesWidth; x++)
            {
                centers[0, counterCenters] = outerRadius + x * (outerRadius * 2); 
                centers[1, counterCenters] = outerRadius + y * (outerRadius * 2);
                counterCenters++;

            }
        }


        using (var image = new Image<Rgba32>(width, height))
        {
            int counter = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    if (isInCircle(outerRadius, circleWidth, centers, x, y, numCirclesHeight, numCirclesWidth))
                    {
                        if (counterTotal % intervalPrintInfo == 0)
                        {
                            PrintStatus(counterTotal, height * width, 1);
                        }
                        image[x, y] = new Rgba32((byte)allColors[0, counter], (byte)allColors[1, counter], (byte)allColors[2, counter]);
                        counter++;
                        counterTotal++;
                    }
                }
            }


            int numUsedColors = counter;

            Random random = new Random(seed);
            int[] arrayInts = CreateArrayOfInts(maxNumColors, true, numUsedColors);
            arrayInts = ShuffleArray(arrayInts, random);


            int blockSize = 256;
            counter = 0;

            for (int y1 = 0; y1 < height / blockSize; y1++)
            {
                for (int x1 = 0; x1 < width / blockSize; x1++)
                {
                    for (int y = 0; y < blockSize; y++)
                    {
                        for (int x = 0; x < blockSize; x++)
                        {

                            if (image[x1 * blockSize + x, y1 * blockSize + y].A == 0)
                            {
                                if (counterTotal % intervalPrintInfo == 0)
                                {
                                    PrintStatus(counterTotal, height * width, 2);
                                }

                                int[] randomColor = new int[3];

                                if (counter >= arrayInts.Length)
                                {
                                    randomColor = SelectRandomColor(random);
                                }

                                else
                                {
                                    randomColor[0] = allColors[0, arrayInts[counter]];
                                    randomColor[1] = allColors[1, arrayInts[counter]];
                                    randomColor[2] = allColors[2, arrayInts[counter]];
                                }

                                image[x1 * blockSize + x, y1 * blockSize + y] = new Rgba32((byte)randomColor[0], (byte)randomColor[1], (byte)randomColor[2]);
                                counter++;
                                counterTotal++;
                            }

                            ShiftColor();
                        }
                    }
                }
            }

            SaveImage(image);




        }




    }

    public static int[] CreateArrayOfInts(int length, bool cropped, int numUsedColors)
    {
        if (!cropped)
        {
            int[] arrayInts = new int[length];
            for (int i = 0; i < length; i++)
            {
                arrayInts[i] = i;
            }

            return arrayInts;
        }

        else
        {
            int[] arrayInts = new int[length - numUsedColors];
            for (int i = 0; i < length - numUsedColors; i++)
            {
                arrayInts[i] = i + numUsedColors;
            }

            return arrayInts;
        }

    }

    public static int[] SelectRandomColor(Random seed)
    {
        byte r = (byte)seed.Next(0, 255);
        byte g = (byte)seed.Next(0, 255);
        byte b = (byte)seed.Next(0, 255);

        return new int[] {r, g, b };
    }


    public static bool isInCircle(int outerRadius, int circleWidth, int[,] centers, float x, float y, int numCirclesHeight, int numCirclesWidth) // 10 sec
    {
        int column = (int)(x / (outerRadius * 2));
        int row = (int)(y / (outerRadius * 2));
        int indexBox = row * numCirclesWidth + column;

        if (column >= numCirclesWidth || row >= numCirclesHeight)
        {
            return false;
        }

        float distanceToCenter = calculateDistance(centers[0, indexBox], centers[1, indexBox], x, y);
        if (distanceToCenter >= outerRadius - circleWidth && distanceToCenter <= outerRadius || distanceToCenter <= outerRadius / 4 )
        {
            return true;
        }

        else
        {
            return false;
        }

    }

    public static int[] ShuffleArray(int[] arrayInts, Random random)
    {
        for (int i = arrayInts.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            int temp = arrayInts[i];
            arrayInts[i] = arrayInts[j];
            arrayInts[j] = temp;
        }

        return arrayInts;
    }

    public static int[,] CreateArrayAllColors()
    {
        int[,] arrayAllColors = new int[3, maxNumColors];
        byte[] color;
        r = 0;
        g = 0; 
        b = 0;

        for (int i = 0; i < maxNumColors; i++)
        {
            color = new byte[] { r, g, b };

            arrayAllColors[0, i] = color[0];
            arrayAllColors[1, i] = color[1];
            arrayAllColors[2, i] = color[2];
            ShiftColor();
        }

        return arrayAllColors;
    }

    public static int[,] returnAllColorsArray()
    {
        r = 0;
        g = 0;
        b = 0;
        int[,] allColors = new int[3, maxNumColors];
        for (int i = 0; i < maxNumColors; i++)
        {
            allColors[0, i] = r;
            allColors[1, i] = g;
            allColors[2, i] = b;
            ShiftColor();
        }


        return allColors; 
    }

    public static float convertDegreesToRadians(float degrees)
    {
        return degrees * ((float)(2f * Math.PI) / 360f); 
    }

    public static float convertRadiansToDegrees(float radians)
    {
        return radians * (360f / (float)(2f / Math.PI));
    }

    public static float calculateDistance(float x1, float x2, float y1, float y2)
    {
        float distanceXsquared = (float)Math.Pow(x1 - y1, 2);
        float distanceYsquared = (float)Math.Pow(x2 - y2, 2);
        return (float)Math.Pow(distanceXsquared + distanceYsquared, 0.5f); 
    }

    public static void SaveImage(Image<Rgba32> imageToSave)
    {
        
        Console.WriteLine("Saving...");
        imageToSave.Save(filename);
        PrintStatus(1, 1, 3);
        Console.WriteLine($"Image \"{filename}\" created successfully.");
    }


    public static void PrintStatus(int numItersNow, int totalIters, int phase)
    {
        Console.Clear();

        float percent = (float)Math.Round(((float)numItersNow / (float)totalIters) * 100f, 2);

        if (phase == 1)
        {
            Console.WriteLine($"Elapsed time: {Math.Round((DateTime.Now - startTime).TotalSeconds, 1).ToString("0.00")} seconds");

            Console.WriteLine($"Phase {phase}: loading circles...");
        }

        if (phase == 2)
        {
            Console.WriteLine($"Elapsed time: {Math.Round((DateTime.Now - startTime).TotalSeconds, 1)} seconds");

            Console.WriteLine($"Phase {phase}: filling up empty space with random colors...");

        }

        if (phase == 3)
        {


        }
        Console.WriteLine($"{percent.ToString("0.00")} % (increases much faster in Phase 2)");


    }


}
