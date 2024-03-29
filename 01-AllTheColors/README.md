# Task 01-AllTheColors
Your task is to write a **C# command-line** program which creates a PNG image
with **all RGB colors** in it. That means it must have at least
256 * 256 * 256 = 2^24 = 16M pixels.

* The pixel colors can be repeated, but no color from the 16M set can be missing!
* Your program should have several modes:
  * **trivial mode** using as simple implementation as possible
  * **random mode** (chaotic mode) where you cannot recognize any pattern
	in the result image
  * **ornament or pattern mode** where you try to implement any visually
	appealing ornaments or patterns (based on repetition, mandala-like,
	etc.)
* In any case, your image **must have all 2^24 RGB colors!**
* The program should use simple **command-line arguments** defining desired
  image size, image file name, mode and random seed if relevant for the mode.
  In case of image size smaller than 16M pixels program could write an error
  message and abort without generating an image.

## Notes
You should use `.NET 7` which is available for all platforms now.

### Image library
Using of a simple image processing library is recommended. My option would
be [SixLabors.ImageSharp](https://www.nuget.org/packages/SixLabors.ImageSharp/).
You can create an image just writting
```csharp
  var image = new Image<Rgba32>(width, height);
```
  Than you can set pixel colors as easy as
```csharp
  var color = new Rgba32((byte)0, (byte)255, (byte)100);
  image[x, y] = color;
```
  And finally you can save the image using
```csharp
  image.Save("output.png");
```
### Command-line parsing
For the parsing of command-line arguments I'd recommend
[CommandLineParser](https://www.nuget.org/packages/CommandLineParser/).
It is wise to have reasonable default settings, you can reduce debugging time.

Don't forget to include several reasonable examples in your documentation,
to show everything your program can do.

# Your Documentation
Use the [separate file DOC.md](DOC.md) to avoid merge conflicts.

# Credit points
**Basic solution: 9 points**
* all images must contain 16M colors
* image size in pixels via arguments
* output file name specified in an argument
* mode selection by command line argument
* at least three modes: `trivial`, `random`, `pattern` (pattern could be static)

**Bonus points: up to 6 more points**
* more patterns (parametrizable, visually more appealing)
  * "mandala" style (circular symmetry)
  * "ornament" could use a recursive pattern or another sort of repetition...
* more command-line arguments

## Use of AI assistant

Recommended but not rewarded. Only in exceptional cases, if working with
an assistant would lead to interesting progress or unexpected results,
extra points may be awarded.

# Example
This image is downsampled, so it doesn't contain enough colors to meet
the requirements of the task!

![Downsampled example image](example.jpg)
