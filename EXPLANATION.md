# Explanation
This md file will explain explain and document some of the tricker parts of this program. Mainly my ideas, and debugging steps.

## Why use raylib and C#
I've decided to use raylib for this project because I have been using it a lot very recently and I have fallen in love with it. It's incredibly well-built and has almost every feature that you could ask for. C# is, and has always been my favorite language for its syntax and powerful tools that help me speed up development time.

## Playing video with FFMPEG
Before starting this project I attempted to make a raylib video player, but I never quite got it working. My original code used a library that was made for extracting frames from a video to create thumbnail previews. I have used other programs and tools that use FFMPEG, so I did some research and decided that FFMPEG is the correct route for me.

### Using FFMPEG
The FFMPEG binaries must first be installed, then they can be used to interact with the clip. FFMPEG works by using different commands in a terminal, so C# processes must be made to run the commands, then the output can be piped out and used.