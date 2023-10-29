This program generates images that contain all colors. It takes arguments from the user such as image width, height, and mode, and creates the image accordingly. Example commands are shown below for each mode.

There are 3 modes:

### 1) trivial mode
This mode just generates in image of the specified size containing all the colors using the most trivial implementation.
example commands:

`--width 4096 --height 4096 -n trivial1.png -m trivial`

Note that the result image must have at least 16777216 pixels (otherwise it won't generate an image at all), but can have more.
	

### 2) random mode
This mode creates an image whose pixels have random value. The user can pick the seed for the randomness, and for one particular seed, the program will  always generate the same random image. The user MUST specify the seed, as shown in the example.
example commands:

`--width 4096 --height 4096 -n random1.png -m random --seed 10`
 
`--width 8192 --height 4096 -n random1.png -m random --seed 10`



### 3) ornament mode
This mode creates an image that contains a circle pattern. A grid of circles will be generated. Each of those circles has one addidiontal circle inside them. The user must specify how many circles will be on the y-axis (using the command numCirclesVertical), and how thick the circle will be (radiusWidth).
example commands:

`--width 4096 --height 4096 -n ornament1.png -m ornament --numCirclesVertical 1 --radiusWidth 500`
 
`--width 4096 --height 4096 -n ornament2.png -m ornament --numCirclesVertical 10 --radiusWidth 50`
 
`--width 4096 --height 4096 -n ornament3.png -m ornament --numCirclesVertical 20 --radiusWidth 10`
