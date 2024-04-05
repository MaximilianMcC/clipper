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
 
	// Frame and FFMPEG stuff 
	private static int bytesPerPixel;
	private static int bytesPerFrame;
	private static Process extractionProcess;
 
	public static void LoadVideo()
	{
		// Get video info
		GetVideoInformation();
 
		// Extract the entire video to a 
		// single byte array of YUV data
		BeginExtractingFrames();
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
 
 
		// Print out all the extracted information
		// TODO: Don't do this in production
		// TODO: Put in ToString()
		// TODO: Don't do
		Console.WriteLine("Extracted information from " + Path + ":");
		Console.WriteLine("Size:\t\t" + width + "x" + height);
		Console.WriteLine("Frames:\t\t" + frameCount + " @ " + frameRate.ToString("#.#") + "fps");
	}
 
	private static void BeginExtractingFrames()
	{
		// TODO: Make the video be low quality, maybe 480p or 720p to make this process quicker

		// Figure out how many bytes in a pixel,
		// and how many bytes in a frame
		bytesPerPixel = 3; //? R, G, B (24 bit)
		bytesPerFrame = (width * height) * bytesPerPixel;

		// Create the FFMPEG process to extract all
		// of the information from the video.
		extractionProcess = new Process();
		extractionProcess.StartInfo = new ProcessStartInfo()
		{
			//? Reading as RGB byte array
			FileName = "ffmpeg.exe",
			Arguments = $"-i {Path} -vf fps={frameRate} -f image2pipe -pix_fmt rgb24 -vcodec rawvideo -",

			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		// Start the process. When bytes are
		// needed they will be extracted. This way
		// there is no rush for the process
		// to get all of the bytes out quickly.
		extractionProcess.Start();
	}
 
	public static Texture2D LoadFrame(int frameIndex)
	{
		// Store all the bytes for the current frame
		byte[] frameBytes = new byte[bytesPerFrame];

		// Begin to pipe the data from the process
		// TODO: Store the stream as private class variable thingy so we can seek and stuff. position change and keep it without doing maths
		using (Stream stream = extractionProcess.StandardOutput.BaseStream)
		{
			// Keep reading until we have enough
			// bytes to make up our frame
			int bytesRead = 0;
			while (bytesRead < bytesPerFrame)
			{
				// Read the byte
				byte newByte = (byte)stream.ReadByte();

				// Add the byte to the frame
				frameBytes[bytesRead] = newByte;
				bytesRead++;
			}
		}

		// Convert all of the bytes to RGB, then draw
		// them to the texture so they can be displayed
		// TODO: Private render texture. don't make new one 
		RenderTexture2D frame = Raylib.LoadRenderTexture(width, height);
		Raylib.BeginTextureMode(frame);
		int index = 0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				// Get the current pixel
				Color pixel = new Color(frameBytes[index], frameBytes[index + 1], frameBytes[index + 2], byte.MaxValue);
				index++;

				// Draw it
				Raylib.DrawPixel(x, y, pixel);
			}
		}
		Raylib.EndTextureMode();

		// Give back the frame
		// TODO: Unload the texture
		return frame.Texture;
	}
}