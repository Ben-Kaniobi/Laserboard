Laserboard
==========
Smartboard simulation to use a laser pointer as marker/pen. No additional hardware is required, as it works with an ordinary webcam.
Emgu CV project written in C# and developped with Microsoft Visual Studio 2010 Express.

![Title art](/files/Images/Title.PNG "Title art")

Requirements
------------
### Executable only
* Microsoft .Net Framework 3.5
* Webcam
* Projector (of course it also works on a normal monitor)
* Laser pointer

### Project
* _Same as executable_
* Microsoft Visual C# Express
* [Emgu CV](http://sourceforge.net/projects/emgucv/)

"nvcuda.dll could not be found" error
-------------------------------------
You may get this error if you don't have an nVidia graphics card.
* Download nvcuda.zip from http://www.dll-files.com/dllindex/dll-files.shtml?nvcuda
* Copy nvcuda.dll from the archive to the same path where Laserboard.exe is or to %WINDIR%/System32/

Download
--------
* Project ([zip](../../archive/master.zip))
* Executable ([zip](../../releases))

Usage
-----
1. Connect your webcam and point it to the screen
2. Run *Laserboard.exe*
3. Move/resize the main window (labeled "Laserboard") so that the webcam sees the whole window, the checkboard will dissapear once the window was found by the webcam/software  
   *The prespective is now calibrated, you may press `P` or resize/move the window to redo this step*
4. Point your laser at the screen and press `L`, the main windows will display a webcam screenshot
5. User your mouse cursor to select the laser point, make sure to select as little of the black background as possible  
   *The laser pointer is now calibrated, you may redo step 4-5 at any time after calibrating the perspective*
6. Use the laser pointer to draw on the screen  
   *You may press `I` to display the available keybindings*
