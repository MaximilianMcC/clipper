# ✂️ Clipper
Clipper is a very simple video editor that is designed to trim down clips to include only the part that was intended to be clipped. A focus is put on editing speed, and dealing with many clips in bulk.

---

For those that don't know what a clip it, its a short video that records what previously happened. This means people can record special moments without having to record all the time.

As an example, if someone was to record a clip that was 40 seconds, but they only wanted to save the last 5 seconds, they would need to manually open the clip in a video editor, find the part that they want, trim it on both ends, then export it. With clipper this can be done automatically, and adjustments can be made in seconds.

---

# Explanation and stuff
An document going through some of the processes that I used while developing this program can be found [here](./EXPLANATION.md).

---

# Launching
The program must be launched with these arguments:
```cmd
clipper.exe <PATH TO FFMPEG> <PATH TO VIDEO>
```

# Getting FFMPEG
This project uses a extremely popular library called `FFMPEG` to handle the video editing. To use it, download the latest version [here](https://github.com/BtbN/FFmpeg-Builds/releases), then extract the folder and copy everything inside `./bin` to a place of your choosing. Launch the program with the first argument being the location to all three files.

---

*This project is for my NCEA Level 3 DTS scholarship btw*