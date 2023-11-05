This program generates a color palette for an input image as specified in the assignment.

The implementation is rather basic. 
1) The 3D space of colors is divided into cubes of equal size (64 pixels).
2) For each 10th pixel in the image, the pixel's color is added to the cube (which is represented by a list) that contains its color.
3) The n cubes with the most colors in them are selected. (n is the number of colors in the palette the user has chosen).
4) For each of the selected cube, an average color is calculated from all the pixels in that cube and gets printed out.

Example command: `dotnet run -i "input.png" --c 10 -o "output.png"`
