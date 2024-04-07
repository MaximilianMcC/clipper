using System.Diagnostics;
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

	public static void LoadVideo()
	{
		// Get video info
		GetVideoInformation();

		// Store all of the frames
		Frames = new Texture2D[frameCount];
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
 
	public static Texture2D LoadFrameBatch(int frameIndex)
	{
		// Get how many frames we're going to load
		int batchSize = 10;
		int remainingFrames = frameCount - (frameIndex * batchSize);
		int batchCount = Math.Min(batchSize, remainingFrames);

		// Create the FFMPEG process to extract the
		// necessary frames from the video
		Process extractionProcess = new Process();
		extractionProcess.StartInfo = new ProcessStartInfo()
		{
			//? Reading as RGB byte array
			FileName = "ffmpeg.exe",
			Arguments = $"-i {Path} -vf \"select=between(n\\,{frameIndex}\\,{frameIndex + batchCount}),format=rgb24\" -f image2pipe -vcodec rawvideo -",

			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		// Start the extraction process, and open a 
		// stream to pipe the data from
		extractionProcess.Start();
		Stream stream = extractionProcess.StandardOutput.BaseStream;

		// Loop through every frame and pipe it
		byte[][] frames = new byte[batchCount][];
		for (int i = 0; i < batchCount; i++)
		{
			// Make a buffer to store all the bytes
			// for the current frame
			frames[i] = new byte[bytesPerFrame];

			// Pipe the frame
			int bytesRead = stream.Read(frames[i], 0, bytesPerFrame);
			Console.WriteLine($"Read {bytesRead} bytes from frame {frameIndex + i}");
		}

		// Close the stream since we've gotten
		// all the data we need
		stream.Close();

		// Convert all of the bytes to RGB, then draw
		// them to the texture so they can be displayed
		RenderTexture2D renderTexture = Raylib.LoadRenderTexture(width, height);

		// Loop through all frames and draw it to the render texture
		for (int i = 0; i < batchCount; i++)
		{
			//? no need to clear the screen because each frame is different
			Raylib.BeginTextureMode(renderTexture);

			// Loop through every pixel in the frame
			// TODO: Don't use nested loop
			int pixelIndex = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					// Get the color of the current pixel
					Color pixel = new Color(frames[i][pixelIndex], frames[i][pixelIndex + 1], frames[i][pixelIndex + 2], byte.MaxValue);
					pixelIndex += 3;

					// Draw the pixel
					Raylib.DrawPixel(x, y, pixel);
				}
			}

			Raylib.EndDrawing();

			// TODO: Use a busy loop and only do once finished drawing
			// Add the frame to the list of frames
			Frames[frameIndex + i] = renderTexture.Texture;
		}

		// Unload the render texture now that we've
		// drawn all of the frames
		Raylib.UnloadRenderTexture(renderTexture);
		


		//! debug
		return Frames[frameIndex];
	}
}