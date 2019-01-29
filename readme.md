# BrailleBooks

A tool to convert any image into Minecraft written book. It uses braille Unicode characters to achieve better resolution.

## Examples

Give commands and original images can be found in Examples folder

![Cube](https://raw.githubusercontent.com/BYSTACK/BrailleBooks/master/Cube.png)
![Mario](https://raw.githubusercontent.com/BYSTACK/BrailleBooks/master/Mario.png)

## Usage

.NET Framework 4.6.1 is required to run the application.

To convert an image into a book, drag an image file onto the program. Supported formats are PNG, JPEG, and GIF. Each frame of a gif file will be placed on a separate page. 

Another way to make a multi-page book is to drag a folder onto the program. The folder should contain PNG or JPEG files named as "01.png", "02.png" etc. (with trailing zeros).

To achieve best results, use 76x56 images (or smaller). Larger images will be downscaled.

The resulting command will be saved in command.mcfunction file near the original image/folder. You can either open it with a text editor and copypaste the command into a command block or add the file to a datapack. Be aware that there is a limit of 32200 characters when using a commands block, any commands longer than this can only be run through a datapack.

## Usage again if above isn't clear enough

![Usage](https://raw.githubusercontent.com/BYSTACK/BrailleBooks/master/Usage.png)