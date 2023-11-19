This program is an implementation of the 4th homework, Mandala.

The program has two modes: normal and cursed.

The user can modify these parameters:
	- output file name `-o output.png`
 
	- image width `-w 500`
 
	- image height `-h 500`
 
	- orders of symmetry `-n 4`
 
	- seed for randomness `-s 1000`

	- mode `-m normal` or `-m cursed`
 
	- gradient length for distance between lines `-l 25`
 
	- gradient length for distance from the center `-d 50`
 

Here are some example prompts:
Normal mode:

`dotnet run -w 500 --height 500 -o output.png -n 8 -l 25 -d 50 -m normal -s 666`

`dotnet run -w 500 --height 500 -o output.png -n 2 -l 25 -d 50 -m normal -s 666`

`dotnet run -w 500 --height 500 -o output.png -n 2 -l 15 -d 75 -m normal -s 667`

Cursed mode:

`dotnet run -w 500 --height 500 -o output.png -n 8 -l 25 -d 50 -m cursed -s 666`

`dotnet run -w 500 --height 500 -o output.png -n 1 -l 75 -d 150 -m cursed -s 666`


	
How the program works:

- the program generates lines:		
  on a circle around the center of the image, n points are generated. Each lined is defined by the cetner point and one of the points on the cirsle.
- for each pixel in the image, its distance to the closest line is calculated
  from the distance of each pixel to its closes line, its color is calculated.
- similarly, for each pixel in the image, its distance from the center is calculated, and it modifies the pixel's color
- the way colors are calculated is not too complex, but difficult to explain. on feature is that according to the randomness seed, some components of the color are reversed: instead of r = x, the pixel will have color r = 255 - x
- for each component of the color (r, g, or b), the randomness seed determines if it will be affected by the distance to closest line, distance to center, or random number 0-255

