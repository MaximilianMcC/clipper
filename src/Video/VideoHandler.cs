using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Raylib_cs;

class VideoHandler
{
	// I/O stuff
	public static string Path;

	// Video information
	private static int width;
	private static int height;
	private static int frameCount;
	private static double frameRate;

	// Frame stuff
	public static Texture2D[] Frames;
	private static int bytesPerPixel;
	private static int bytesPerFrame;
	private static RenderTexture2D renderTexture;

	public static void LoadVideo()
	{
		// Get video info
		GetVideoInformation();

		// Store all of the frames
		Frames = new Texture2D[frameCount];

		// Make a render texture for baking to
		renderTexture = Raylib.LoadRenderTexture(width, height);
	}

	private static void GetVideoInformation()
	{
		// Make a new process to run the FFMPEG command
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			// This command shows all of the stream information
			// in JSON format
			FileName = "ffprobe.exe",
			Arguments = $"-i {Path} -show_streams -print_format json -v error",

			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		// Get the output from the command
		process.Start();
		string processOutput = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		// Parse it to a JSON document so we
		// can parse it without a class like
		// how it can be done in languages
		// like JavaScript and Python
		JsonDocument json = JsonDocument.Parse(processOutput);
		JsonElement videoStream = json.RootElement.GetProperty("streams")[0];

		// Get the width and height
		width = videoStream.GetProperty("width").GetInt32();
		height = videoStream.GetProperty("height").GetInt32();

		// Get the number of frames
		string frameCountString = videoStream.GetProperty("nb_frames").GetString();
		frameCount = int.Parse(frameCountString);

		// Get the fps
		//? fps is both values divided by each other
		string frameRateString = videoStream.GetProperty("avg_frame_rate").GetString();
		string[] equation = frameRateString.Split("/");
		frameRate = double.Parse(equation[0]) / double.Parse(equation[1]);

		// Figure out how many bytes in a pixel,
		// and how many bytes in a frame
		bytesPerPixel = 3; //? R, G, B (24 bit)
		bytesPerFrame = (width * height) * bytesPerPixel;

		// Print out all the extracted information
		// TODO: Don't do this in production
		// TODO: Put in ToString()
		// TODO: Don't do
		Console.WriteLine("Extracted information from " + Path + ":");
		Console.WriteLine("Size:\t\t" + width + "x" + height);
		Console.WriteLine("Frames:\t\t" + frameCount + " @ " + frameRate.ToString("#.#") + "fps");
	}

	public static void LoadFrameBatch(int frameIndex)
	{
		// Make and run the FFMPEG command to extract the video data
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = "ffmpeg.exe",
			// Arguments = $"-i {Path} -f image2pipe -vcodec rawvideo -",
			Arguments = $"-i {Path} -f image2pipe -vcodec rawvideo -pix_fmt rgb24 -",

			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true
		};
		process.Start();

		// Open the stream and start reading the incoming data
		int framesRead = 0;
		using (Stream stream = process.StandardOutput.BaseStream)
		{
			// Store info for the current frames
			byte[] frameBuffer = new byte[bytesPerFrame];
			int totalBytesRead = 0;

			// Keep reading everything until theres nothing
			// left to read
			while (framesRead + 1 < 10)
			// while (framesRead + 1 < frameCount)
			{
				// Read the incoming bytes
				int bytesRemaining = bytesPerFrame - totalBytesRead;
				int bytesRead = stream.Read(frameBuffer, totalBytesRead, bytesRemaining);
				totalBytesRead += bytesRead;

				// Check for if we've collected enough
				// bytes to make a whole frame
				if (totalBytesRead == bytesPerFrame)
				{
					// Bake the frame and save it
					Frames[framesRead] = GenerateFrame(frameBuffer);
					Console.WriteLine($"Read frame {framesRead + 1}/{frameCount}");
					framesRead++;

					// Reset everything for the next frame
					// TODO: See if we don't need to make a new array and just change buffer position or something
					frameBuffer = new byte[bytesPerFrame];
					totalBytesRead = 0;
				}
				
			}
		}

		// TODO: Wait for exit
		//! process isn't ending for some reason
		// process.WaitForExit();

		Console.WriteLine("Done!");
	}

	// TODO: Draw some random stuff to see if issue with raylib
	private static Texture2D GenerateFrame(byte[] frameData)
	{

		// Loop through every pixel in the frame and draw it
		Raylib.BeginTextureMode(renderTexture);
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				// Get the color of the pixel
				//? 3 because 3 bytes (r, g, b)
				int pixelIndex = ((y * width) + x) * 3;
				Color pixel = new Color(frameData[pixelIndex], frameData[pixelIndex + 1], frameData[pixelIndex + 2], byte.MaxValue);

				// Draw the pixel
				Raylib.DrawPixel(x, y, pixel);
			}
		}
		Raylib.EndTextureMode();


		// Get the frame, then unload the render texture
		Texture2D frame = renderTexture.Texture;
		// Raylib.UnloadRenderTexture(renderTexture);

		// Give back the baked frame
		return frame;
	}
}