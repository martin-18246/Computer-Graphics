using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Drawing;
using CommandLine;


namespace ImagePalette2
{

    public class Options
    {
        [Option('i', Required = true, HelpText = "Input image name")]
        public string ImageName { get; set; }

        [Option('c', Required = true, HelpText = "Number of colors in a palette")]
        public int NumColors { get; set; }

        [Option('o', Required = false, HelpText = "Output file name")]
        public string? OutputName { get; set; }

    }

    public static class ProgramInfo
    {
        public static int skipPixelInterval = 10;
        public static int cubeSize = 64;
        public static int numSizeColorPalette = 8;
        public static string? inputFilename;
        public static string? outputFilename = "output.png";
    }


    class Program
    {

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(RunOptions);


            ColorSpace image1Space = new ColorSpace();
            image1Space.AddColorToList(8, 8, 8);

            using (Image<Rgba32> image = Image.Load<Rgba32>(ProgramInfo.inputFilename))
            {
                int imagePixels = image.Width * image.Height;
                int counter = 0;

                while (counter < imagePixels)
                {
                    int x = counter % image.Width;
                    int y = counter / image.Width;
                    counter += ProgramInfo.skipPixelInterval;
                    Rgba32 color = image[x, y];
                    byte red = color.R;
                    byte green = color.G;
                    byte blue = color.B;
                    image1Space.AddColorToList(red, green, blue);

                }

                image1Space.FillListIndices();
                image1Space.PutColorsIntoArray();


                int width1 = 20;
                int height1 = 50;

                using (var imageNew = new Image<Rgba32>(ProgramInfo.numSizeColorPalette * width1, height1))
                {

                    int counter3 = 1;


                    for (int col2 = 1; col2 < ProgramInfo.numSizeColorPalette + 1; col2++)
                    {
                        byte[] averageColor = new byte[3];
                        averageColor[0] = image1Space.colors[col2 - 1].R;
                        averageColor[1] = image1Space.colors[col2 - 1].G;
                        averageColor[2] = image1Space.colors[col2 - 1].B;
                        image1Space.PrintArray(averageColor);


                        for (int x = counter3 * width1 - width1; x < counter3 * width1; x++)
                        {
                            for (int y = 0; y < height1; y++)
                            {
                                imageNew[x, y] = new Rgba32(averageColor[0], averageColor[1], averageColor[2]);
                            }
                        }

                        counter3++;
                    }

                    if (ProgramInfo.outputFilename != null)
                    {
                        imageNew.Save(ProgramInfo.outputFilename);

                    }


                }




            }
        }

        static void RunOptions(Options opts)
        {
            ProgramInfo.inputFilename = opts.ImageName;
            ProgramInfo.numSizeColorPalette = opts.NumColors;




            ProgramInfo.outputFilename = opts.OutputName;





        }
    }

    public class ColorSpace
    {
        public List<List<byte[]>> listCubes = new List<List<byte[]>>();
        public List<int> listIndicesProlificCubes = new List<int>();
        public List<int> helloList = new List<int> { 1, 2, 3 };

        public int cubesPerAxis = 256 / ProgramInfo.cubeSize;
        public Rgba32[] colors = new Rgba32[ProgramInfo.numSizeColorPalette];




        public ColorSpace()
        {
            int numCubes = (int)Math.Pow((256 / ProgramInfo.cubeSize), 3);
            for (int i = 0; i < numCubes; i++)
            {
                listCubes.Add(new List<byte[]>());
            }
        }

        public void AddColorToList(byte r, byte g, byte b)
        {
            int rMultiplier = cubesPerAxis * cubesPerAxis;
            int gMultiplier = cubesPerAxis;
            int bMultiplier = 1;

            int littleCubeRAxis = r / ProgramInfo.cubeSize;
            int littleCubeGAxis = g / ProgramInfo.cubeSize;
            int littleCubeBAxis = b / ProgramInfo.cubeSize;

            int indexCube = littleCubeRAxis * rMultiplier + littleCubeGAxis * gMultiplier + littleCubeBAxis * bMultiplier;

            byte[] colorToAdd = { r, g, b };


            listCubes[indexCube].Add(colorToAdd);

        }


        public void FillListIndices()
        {
            int indexCounter = 0;
            foreach (var item in listCubes)
            {
                //if (listIndicesProlificCubes.Count < ProgramInfo.numSizeColorPalette)
                if (listIndicesProlificCubes.Count < ProgramInfo.numSizeColorPalette)
                {
                    listIndicesProlificCubes.Insert(0, indexCounter);
                }

                else if (item.Count > listCubes[listIndicesProlificCubes[0]].Count)
                {

                    listIndicesProlificCubes[0] = indexCounter;

                }

                else
                {
                    indexCounter++;
                    continue;
                }


                int indexNow = 0;

                while (indexNow < listIndicesProlificCubes.Count - 1)
                {
                    if (item.Count > listCubes[listIndicesProlificCubes[indexNow + 1]].Count)
                    {
                        SwitchTwoItemsInList(listIndicesProlificCubes, indexNow, indexNow + 1);
                    }

                    else
                    {
                        break;
                    }

                    indexNow++;
                }


                indexCounter++;
            }



        }


        public List<T> SwitchTwoItemsInList<T>(List<T> list1, int index1, int index2)
        {
            T temp = list1[index2];
            list1[index2] = list1[index1];
            list1[index1] = temp;

            return list1;
        }

        public byte[] CountAverageColorInCube(int indexCube)
        {
            int totalR = 0;
            int totalG = 0;
            int totalB = 0;
            int lenList = listCubes[indexCube].Count;

            foreach (var color1 in listCubes[indexCube])
            {
                totalR += color1[0];
                totalG += color1[1];
                totalB += color1[2];
            }

            int averageR;
            int averageG;
            int averageB;

            if (lenList == 0)
            {
                averageR = 0;
                averageG = 0;
                averageB = 0;
            }

            else
            {
                averageR = totalR / lenList;

                averageG = totalG / lenList;
                averageB = totalB / lenList;
            }


            byte[] averageColor = { (byte)averageR, (byte)averageG, (byte)averageB };

            return averageColor;
        }

        public void PrintArray(byte[] array1)
        {
            for (int i = 0; i < array1.Length; i++)
            {
                Console.Write(array1[i]);
                Console.Write(" ");
            }

            Console.WriteLine();
        }

        public void PutColorsIntoArray()
        {
            int counter4 = 0;
            foreach (var col in listIndicesProlificCubes)
            {
                byte[] averageColor = CountAverageColorInCube(col);

                colors[counter4] = new Rgba32(averageColor[0], averageColor[1], averageColor[2]);
                //Console.WriteLine(averageColor[0] + " " + averageColor[1] + " " + averageColor[2]); 
                counter4++;
            }
        }



    }
}