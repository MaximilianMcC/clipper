using System.Diagnostics;
using System.Runtime.InteropServices;
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

	public static Texture2D[] GetFrames()
	{
		// Store all of the frames
		Texture2D[] frames = new Texture2D[VideoManager.FrameCount];
		int frameIndex = 0;

		// Calculate how many bytes one frame takes up
		// TODO: Get bytes per pixel from the json property thing
		int pixelCount = VideoManager.Width * VideoManager.Height;
		int bytesPerPixel = 3; //? Red, Green, Blue
		int bytesPerFrame = pixelCount * bytesPerPixel;

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
				// Get how many bytes we have read
				int bytesRead = stream.Read(frameBuffer, totalBytesRead, bytesPerFrame - totalBytesRead);
				totalBytesRead += bytesRead;

				// Check for if we've reached the
				// end of the stream (no more data)
				if (bytesRead == 0) break;

				// Check for if we've read enough
				// data to make the entre frame
				if (totalBytesRead == bytesPerFrame)
				{
					// Turn the bytes into a texture
					Texture2D frame = BytesToTexture(frameBuffer);
					frames[frameIndex] = frame;
					frameIndex++;

					// Reset the bytes read for the next frame
					totalBytesRead = 0;
				}
			}

		}

		// Say that we have finished loading all of the frames
		VideoManager.VideoLoaded = true;

		// Give back all of the frames
		return frames;
	}

	// TODO: Do this another way
	private static Texture2D BytesToTexture(byte[] frameBytes)
	{
		// Make a render texture to draw the image on
		RenderTexture2D renderTexture = Raylib.LoadRenderTexture(VideoManager.Width, VideoManager.Height);

		// Loop through every pixel in the frame and
		// draw it on the render texture
		//? 3 because 3 bytes is one pixel (RGB)
		// TODO: Get the colors before drawing
		Raylib.BeginTextureMode(renderTexture);
		for (int i = 0; i < frameBytes.Length - 3; i += 3)
		{
			// Get the current coordinates
			// from the index
			int x = i % VideoManager.Width;
			int y = i / VideoManager.Width;

			// Convert the bytes into a color
			byte red = frameBytes[i];
			byte green = frameBytes[i + 1];
			byte blue = frameBytes[i + 2];
			Color color = new Color(red, green, blue, byte.MaxValue);

			// Draw the color on the render texture
			Raylib.DrawPixel(x, y, color);
		}
		Raylib.EndTextureMode();

		// Convert the image to a texture
		// TODO: Don't unload render texture. Save it for every frame and only unload at end
		Texture2D frame = renderTexture.Texture;
		Raylib.UnloadRenderTexture(renderTexture);

		// Give back the video frame
		Console.WriteLine("Loaded frame");
		return frame;
	}
}