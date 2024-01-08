Link to the video: https://vimeo.com/900913915?share=copy

This program is an implementation of the 4th homework, Mandala.

The user can modify these parameters:

- output file mask `-o anim/out{0:0000}.png`

- image width `-w 500`

- image height `-h 500`

- orders of symmetry `-n 4`

- gradient length for distance between lines `-l 25`

- gradient length for distance from the center `-d 50`

- number of frames `-f 60`

Here are some example prompts:

dotnet run -w 500 --height 500 -o anim/out{0:0000}.png -n 8 -l 25 -d 50 -f 60

dotnet run -w 500 --height 500 -o anim/out{0:0000}.png -n 2 -l 25 -d 50 -f 60

dotnet run -w 500 --height 500 -o anim/out{0:0000}.png -n 2 -l 15 -d 75 -f 60




