using System.Diagnostics;
using Raylib_cs;

class Ffmpeg
{
	public enum Service
	{
		Ffmpeg,
		Ffprobe
	}

	// Run a FFMPEG command and get the output as a string
	//! do NOT use this method for larger commands that could take some time to execute
	// TODO: Make async/multithreaded to stop blocking
	public static string RunCommand(Service service, string command)
	{
		// Check for what FFMPEG binary thing they need
		// TODO: Path.join type thing
		string fileName = VideoManager.FfmpegPath;
		if (service == Service.Ffmpeg) fileName += "/ffmpeg.exe";
		else if (service == Service.Ffprobe) fileName += "/ffprobe.exe";

		// Create the process/command and run it
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = fileName,
			Arguments = command,

			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true
		};
		process.Start();

		// Get the command output as a string
		string commandOutput = process.StandardOutput.ReadToEnd();
		if (commandOutput == "") commandOutput = process.StandardError.ReadToEnd();

		// Wait for the process to finish and stuff
		process.WaitForExit();

		// Give back the command output
		return commandOutput;
	}

	// Get all of the pixels in each frame in a video
	public static Color[][] GetFrames()
	{
		// Calculate how many bytes one frame takes up
		// TODO: Get bytes per pixel from the json property thing
		int pixelCount = VideoManager.Width * VideoManager.Height;
		int bytesPerPixel = 3; //? Red, Green, Blue
		int bytesPerFrame = pixelCount * bytesPerPixel;

		// Store all of the frames as their raw bytes
		byte[][] rawFrames = new byte[VideoManager.FrameCount][];
		int frameIndex = 0;

		// Create the process/command and run it
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			//! idk what the * 2 has to do with it but its halved for some reason
			FileName = VideoManager.FfmpegPath + "/ffmpeg",
			Arguments = $"-i {VideoManager.VideoPath} -vf fps={VideoManager.Fps * 2} -f image2pipe -vcodec rawvideo -",

			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true
		};
		process.Start();

		// Pipe the output of the command so that we
		// can extract the bytes of the video
		Console.Write("Extracting frame data...\t");
		using (Stream stream = process.StandardOutput.BaseStream)
		{
			// Put all of the incoming data into a buffer
			byte[] buffer = new byte[1024];

			// Keep track of how many bytes we have read
			// in terms of the current frame
			int totalBytesRead = 0;
			byte[] frameBuffer = new byte[bytesPerFrame];

			while (true)
			{
				// Get how many bytes we have read, and add
				// them to the framebuffer array
				int bytesRead = stream.Read(frameBuffer, totalBytesRead, bytesPerFrame - totalBytesRead);
				totalBytesRead += bytesRead;

				// Check for if we've reached the
				// end of the stream (no more data)
				if (bytesRead == 0) break;

				// Check for if we've read enough
				// data to make the entre frame
				if (totalBytesRead == bytesPerFrame)
				{
					// Add the bytes for the current frame
					// into the frames array so they can be 
					// converted to textures later
					rawFrames[frameIndex] = frameBuffer;
					frameBuffer = new byte[bytesPerFrame];
					frameIndex++;

					// Reset the bytes read for the next frame
					totalBytesRead = 0;
				}
			}
		}
		Console.WriteLine("Done!");



		// Convert all of the raw bytes to colors
		// TODO: Don't use nested for loop
		Console.Write("Converting colors...\t");
		Color[][] frameColors = new Color[VideoManager.FrameCount][];

		for (int i = 0; i < rawFrames.Length; i++)
		{
			// Store the colors for the current frame
			Color[] colors = new Color[pixelCount];
			int colorIndex = 0;

			//? using 3 because 3 bytes for rgb
			for (int j = 0; j < rawFrames[i].Length - 3; j += 3)
			{
				// Get the color data
				byte red = rawFrames[i][j];
				byte green = rawFrames[i][j + 1];
				byte blue = rawFrames[i][j + 2];

				// Add it to the list of colors for the current frame
				colors[colorIndex] = new Color(red, green, blue, byte.MaxValue);
				colorIndex++;
			}
			frameColors[i] = colors;
		}
		Console.WriteLine("Done!");


		// Give back all of the frames as colors
		VideoManager.VideoLoaded = true;
		return frameColors;
	}

	// Convert pixels to a texture
	public static Texture2D GenerateFrame(Color[] pixels)
	{
		return new Texture2D();
	}
}