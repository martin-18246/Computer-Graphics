using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Drawing;
using SixLabors.ImageSharp.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace MandalaAnimation
{
    public class Options
    {
        [Option('w', "width", Required = false, HelpText = "Image width in pixels.")]
        public int Width { get; set; } = 500;

        [Option('h', "height", Required = false, HelpText = "Image height in pixels.")]
        public int Height { get; set; } = 500;

        [Option('p', "fps", Required = false, Default = 30.0f, HelpText = "Frames per second.")]
        public float FPS { get; set; } = 30.0f;

        [Option('f', "frames", Required = false, Default = 2, HelpText = "Number of output frames.")]
        public int Frames { get; set; } = 2;   

        [Option('o', "output", Required = false, Default = "anim/out{0:0000}.png", HelpText = "Output file-name mask.")]
        public string FileMask { get; set; } = "anim/out{0:0000}.png";

        [Option('s', "seed", Required = false, Default = 55, HelpText = "Seed for randomness.")]
        public int Seed { get; set; } = 55;

        [Option('n', "numLines", Required = false, Default = 3, HelpText = "Number of orders of symmetry.")]
        public int Order { get; set; } = 55;

        [Option('d', "distanceCenter", Required = false, Default = 3, HelpText = "Distance of the period of circles.")]
        public int Distance { get; set; } = 50;

        [Option('l', "distanceLine", Required = false, Default = 3, HelpText = "Length of the gradient between lines.")]
        public int DistanceLine { get; set; } = 25;

        [Option('m', "mode", Required = false, Default = "normal", HelpText = "Length of the gradient between lines.")]
        public string Mode { get; set; } = "normal";
    }

    public static class ProgramInfo
    {
        public static int implementation = 2;
        public static int numImages = 1;
        
        public static float angleCenterDot = 0;


        public static int lineAngle = 0;
        public static int rateSpin = 0;
        
        public static int centerDiskRadius = 50;

        public static int modifier1 = 0;
        public static int modifier2 = 0;
        public static int modifier3 = 0;
    }

    public class Point
    {
        public int x { get; set; }
        public int y { get; set; }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public float distance(Point pointA, Point pointB)
        {
            int xDifference = pointB.x - pointA.x;
            int yDifference = pointB.y - pointA.y;
            int xDifferenceSquared = (int)Math.Pow(xDifference, 2);
            int yDifferenceSquared = (int)Math.Pow(yDifference, 2);

            return (float)Math.Sqrt(xDifferenceSquared + yDifferenceSquared);
        }


    }

    public class Line
    {
        public Point pointA { get; set; }
        public Point pointB { get; set; }

        public float slope;
        public float yIntercept;

        public Line(Point pointA, Point pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;

            calculateSlopeInterceptForm();
        }

        public void calculateSlopeInterceptForm()
        {
            int xDifference = pointB.x - pointA.x;
            int yDifference = pointB.y - pointA.y;
            float slope = (float)yDifference / (float)xDifference;
            if (xDifference == 0)
            {
                slope = 999;
            }
            float yIntercept = pointA.y - slope * pointA.x;

            this.slope = slope;
            this.yIntercept = yIntercept;
        }

        public float DistanceToLine(Point point)
        {
            float A = -slope;
            float B = 1;
            float C = -yIntercept;

            float numerator = Math.Abs(A * point.x + B * point.y + C);
            float denominator = (float)Math.Sqrt(A * A + B * B);

            float distance = numerator / denominator;
            return distance;
        }

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            int interval1Start = 0;
            int interval1End = 60;
            int interval2Start = interval1End;
            int interval2End = 2 * interval1End;
            int interval3Start = interval2End;
            int interval3End = interval2End + interval1End;



            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                ProgramInfo.implementation = o.Mode == "cursed" ?  1 : 2;

                Random random = new Random(629);

                ProgramInfo.modifier1 = random.Next(0, 255);
                ProgramInfo.modifier2 = random.Next(0, 255);
                ProgramInfo.modifier3 = random.Next(0, 255);


                List<int> listNums = new List<int> { 0, 1, 2 };
                List<int> inverseColors = new List<int>();
                inverseColors.Add(random.Next(0, 2));
                inverseColors.Add(random.Next(0, 2));
                inverseColors.Add(random.Next(0, 2));

                listNums = ShuffleList(listNums);

                byte randomComponentColor = (byte)random.Next(0, 255);

                Point centerPoint = new Point(o.Width / 2, o.Height / 2);
                float percentageDone = 0;

                int maximumLineRotationPerFrameInDegrees = 15;
                float spinningOutput = 0;

                for (int frame = 0; frame < o.Frames; frame++)
                {

                    percentageDone = (float)(frame - interval1Start) / (float)(interval1End - interval1Start);

                    ProgramInfo.modifier1 += 5;
                    ProgramInfo.modifier2 += 20;
                    ProgramInfo.modifier3 += 40;
                    spinningOutput = 0.1f;
                    ProgramInfo.centerDiskRadius += (int)(xFourPower(percentageDone) * 0.05f);


                    ProgramInfo.rateSpin += (int)(spinningOutput * (float)maximumLineRotationPerFrameInDegrees);

                    using (var image = new Image<Rgba32>(o.Width, o.Height))
                    {
                        List<Line> lines = GenerateLines(frame, o.Width, o.Height, o.Order);
                        for (int y = 0; y < o.Height; y++)
                        {
                            for (int x = 0; x < o.Width; x++)
                            {
                                Point currentPoint = new Point(x, y);
                                float distanceToLine = DistanceToClosestLine(lines, currentPoint);
                                float distanceToCenter = (float)Math.Sqrt(Math.Pow(currentPoint.x - centerPoint.x, 2) + Math.Pow(currentPoint.y - centerPoint.y, 2));

                                distanceToCenter += ProgramInfo.centerDiskRadius;
                                distanceToCenter %= o.Distance;
                                //Console.WriteLine(distanceToCenter);

                                float percentageDistance = distanceToLine / o.DistanceLine;
                                float percentageToCenter = distanceToCenter / o.Distance;

                                ColorPixel(percentageDistance, percentageToCenter, image, x, y, random, randomComponentColor, listNums, inverseColors);

                            }
                        }
                        var circle1 = new EllipsePolygon(centerPoint.x, centerPoint.y, 10);
                        string imageName = string.Format(o.FileMask, frame);
                        image.Save(imageName);
                        Console.WriteLine($"Image {imageName} created successfully.");
                    }
                }
            });
        }

        public static void AnimateDots(Image<Rgba32> image, int numCenters, int numMoons, float speedOrbitingCenters, float speedOrbitingDots, float distanceFromCentersOfOrbiting, float distanceFromCenter)
        {
            ProgramInfo.angleCenterDot += 5;
            float positionCenterX = (float)Math.Cos(degreesToRadians(ProgramInfo.angleCenterDot)) * distanceFromCenter;
            float positionCenterY = (float)Math.Sin(degreesToRadians(ProgramInfo.angleCenterDot)) * distanceFromCenter;

            Point abstractCenter = new Point((int)positionCenterX, (int)positionCenterY);


        }

        public static void ColorPixel(float percentageToLine, float percentageToCenter, Image<Rgba32> image, int x, int y, Random random, byte randomComponentColor1, List<int> listNums1, List<int> inverseList)
        {
            switch (ProgramInfo.implementation)
            {
                case 0:
                    byte red = (byte)(255 - percentageToLine * 255);
                    byte blue = (byte)(percentageToCenter * 255);
                    image[x, y] = new Rgba32(red, red, blue);
                    break;
                case 1:
                    List<int> listNums = new List<int> { 0, 1, 2 };
                    listNums = ShuffleList(listNums);

                    byte randomComponentColor = (byte)random.Next(0, 255);


                    byte[] arrayColor = new byte[3];

                    arrayColor[listNums[0]] = randomComponentColor;
                    arrayColor[listNums[1]] = (byte)(percentageToLine * 255);
                    arrayColor[listNums[2]] = (byte)(percentageToCenter * 255);


                    SixLabors.ImageSharp.Color colorPixel = new Rgba32(arrayColor[0], arrayColor[1], arrayColor[2]);
                    image[x, y] = colorPixel;
                    break;
                case 2:
                    byte[] arrayColor1 = new byte[3];

                    arrayColor1[listNums1[0]] = randomComponentColor1;
                    arrayColor1[listNums1[1]] = (byte)(percentageToLine * 255);
                    arrayColor1[listNums1[2]] = (byte)(percentageToCenter * 255);


                    arrayColor1[listNums1[0]] = (byte)(arrayColor1[listNums1[0]] + ProgramInfo.modifier1);
                    arrayColor1[listNums1[1]] = (byte)(arrayColor1[listNums1[1]] + ProgramInfo.modifier2);
                    arrayColor1[listNums1[2]] = (byte)(arrayColor1[listNums1[2]] + ProgramInfo.modifier3);

                    arrayColor1[1] = (byte)(255 - arrayColor1[1]);

                    SixLabors.ImageSharp.Color colorPixel1 = new Rgba32(arrayColor1[0], arrayColor1[1], arrayColor1[2]);
                    image[x, y] = colorPixel1;
                    break;

            }
        }

        static List<T> ShuffleList<T>(List<T> list)
        {
            Random random = new Random(1234);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }


        public static List<Line> GenerateLines(int imageNum, int width, int height, int order)
        {
            Point centerPoint = new Point(width / 2, height / 2);
            float radius = 40f;

           
            float angleBetweenTwoLines = 360f / (float)(2 * order);

            List<Line> lines = new List<Line>();
            ProgramInfo.rateSpin += ProgramInfo.lineAngle;

            for (int i = 0; i < order; i++)
            {
                float degreeNow = (i * angleBetweenTwoLines) + ProgramInfo.rateSpin;
                float xRel = ((float)Math.Sin(degreesToRadians(degreeNow))) * radius;
                float yRel = ((float)Math.Cos(degreesToRadians(degreeNow))) * radius;



                Point pointA = new Point(centerPoint.x - (int)xRel, centerPoint.y - (int)yRel);
                Point pointB = new Point(centerPoint.x + (int)xRel, centerPoint.y + (int)yRel);

                Line newLine = new Line(pointA, pointB);

                lines.Add(newLine);


            }

            return lines;

        }




        public static float DistanceToClosestLine(List<Line> lines, Point point)
        {
            float minDistance = lines[0].DistanceToLine(point);


            foreach (Line line in lines)
            {
                float distanceNow = line.DistanceToLine(point);

                if (distanceNow < minDistance)
                {
                    minDistance = distanceNow;
                }
            }

            return minDistance;
        }


        public static float degreesToRadians(float degrees)
        {
            return ((float)(2 * Math.PI) / 360) * degrees;
        }


        public static double xFourPower(double x)
        {
            return x * x;
        }        



    }
}