using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Raylib_cs;

class VideoHandler
{
	// I/O stuff
	public static string Path;

	// Video information
	public static int Width;
	public static int Height;
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
		// renderTexture = Raylib.LoadRenderTexture(Width, Height);
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
		Width = videoStream.GetProperty("width").GetInt32();
		Height = videoStream.GetProperty("height").GetInt32();

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
		bytesPerFrame = (Width * Height) * bytesPerPixel;

		// Print out all the extracted information
		// TODO: Don't do this in production
		// TODO: Put in ToString()
		// TODO: Don't do
		Console.WriteLine("Extracted information from " + Path + ":");
		Console.WriteLine("Size:\t\t" + Width + "x" + Height);
		Console.WriteLine("Frames:\t\t" + frameCount + " @ " + frameRate.ToString("#.#") + "fps");
	}

	public static void LoadFrameBatch(int frameIndex, int framesToLoad)
	{
		// Make and run the FFMPEG command to extract the video data
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = "ffmpeg.exe",
			// Arguments = $"-i {Path} -vf \"select='gte(n,{frameIndex})&&lt(n,{frameIndex + framesToLoad})'\" -f image2pipe -vcodec rawvideo -pix_fmt rgb24 -",
			// Arguments = $"-i {Path} -f image2pipe -vcodec rawvideo -pix_fmt rgb24 -",
			// Arguments = $"-i {Path} -vf select='between(n\\,{frameIndex}\\,{frameIndex + framesToLoad}\\)' -f image2pipe -vcodec rawvideo -pix_fmt rgb24 -",
			// Arguments = $"-i {Path} -vf \"select='between(n\\,{frameIndex}\\,{frameIndex + framesToLoad}\\)\" -f image2pipe -vcodec rawvideo -pix_fmt rgb24 -",
			// Arguments = $"-i {Path} -vf \"select='gte(n\\,{frameIndex})&<(n\\,{frameIndex + framesToLoad})'\" -f image2pipe -vcodec rawvideo -pix_fmt rgb24 -",
			Arguments = $"-i {Path} -vf \"select='gte(n\\,{frameIndex})*lt(n\\,{frameIndex + framesToLoad})'\" -f image2pipe -vcodec rawvideo -pix_fmt rgb24 -",

			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true
		};
		process.Start();

		// Loop through all frames that we need to load
		for (int i = 0; i < framesToLoad; i++)
		{
			// Store all of the data for the current frame
			byte[] frameBuffer = new byte[bytesPerFrame];
			int totalBytesRead = 0;

			// Keep reading stuff until we have read a complete frame
			while (totalBytesRead != bytesPerFrame)
			{
				// Read the incoming bytes
				int bytesRemaining = bytesPerFrame - totalBytesRead;
				int bytesRead = process.StandardOutput.BaseStream.Read(frameBuffer, totalBytesRead, bytesRemaining);
				totalBytesRead += bytesRead;

				// Check for if we've collected enough
				// bytes to make a whole frame
				if (totalBytesRead == bytesPerFrame)
				{
					// Bake the frame and save it
					Frames[frameIndex + i] = GenerateFrame(frameBuffer);
					Console.WriteLine($"Read frame {frameIndex + i + 1}/{frameCount}");
					break;
				}
			}
		}

		// TODO: Wait for exit
		//! process isn't ending for some reason
		//! Process will still be running in background
		// process.WaitForExit();

		Console.WriteLine("Done!");
	}

	// Bake a texture to a texture
	//? Because OpenGL stink its drawn upside down so when rendering it needs to be flipped
	private static Texture2D GenerateFrame(byte[] frameData)
	{
		renderTexture = Raylib.LoadRenderTexture(Width, Height);

		// Loop through every pixel in the frame and draw it
		Raylib.BeginTextureMode(renderTexture);
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				// Get the color of the pixel
				//? 3 because 3 bytes (r, g, b)
				//? Apparently doing maths faster than using variable
				int pixelIndex = ((y * Width) + x) * 3;
				Color pixel = new Color(frameData[pixelIndex], frameData[pixelIndex + 1], frameData[pixelIndex + 2], byte.MaxValue);

				// Draw the pixel
				Raylib.DrawPixel(x, y, pixel);
			}
		}
		// Raylib.ClearBackground(Color.Green);
		Raylib.EndTextureMode();

		// Get the frame, then return it
		Texture2D frame = renderTexture.Texture;
		return frame;
	}
}