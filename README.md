# ✂️ Clipper
Clipper is a simplified video editor that is designed for automatically, or manually, clipping existing clips to find part the parts that you really want to save. There is a massive focus on editing clips in bulk, and speed/efficiency is a top priority.

## Running
1. Make sure you have FFMPEG and FFPROBE downloaded and added to path *(instructions below)*
1. Download/build the project if there isn't already a release
1. Run the program with the first argument being the video path (eg: `clipper.exe ./video.mp4`)

### Installing FFMPEG/FFPROBE
Clipper uses FFMPEG and FFPROBE to perform all of the actual video editing. These are CLI tools. Follow these steps to download and add it to path:
1. Make a new directory where ever you want. I recommend `C:\Program Files` and call it `pathPrograms` or something
1. Open the start menu and search `env` and hit `Edit the system environment variables`. Open it.
1. Press the `Environment Variables` button in the bottom right.
1. In the top section, double click on the option that says `Path`.
1. On the right, press `new` and enter the path of the directory we just created *(something like `C:\Program Files\pathPrograms`)*.
1. Press `OK` on all three windows to close everything.
1. Download the correct version of FFMPEG for your computer [here](https://github.com/BtbN/FFmpeg-Builds/releases).
1. Extract it and go into the `bin` folder.
1. Copy all three files *(`ffplay.exe`, `ffprobe.exe`, and `ffmpeg.exe`)*
1. Paste them into the directory we added to path.
1. Close then open any terminals that you previously had open
1. Run `ffmpeg.exe`, `ffprobe.exe`, or `ffplay.exe` to check for if its working.
1. Done! If you wish to add any more CLI programs to your computer you can put them in this same directory and they will work.

---
*Made for NCEA level 3 scholarship project.*