using System.Diagnostics;
using System.Text.Json;
using Raylib_cs;

class VideoHandler
{
	public static string Path;
 
	// Video information
	private static int width;
	private static int height;
	private static int frameCount;
	private static double frameRate;
 
	// Frame stuff
	private static byte[] rawData;
	private static int bytesPerFrame;
 
	public static void LoadVideo()
	{
		// Get video info
		GetVideoInformation();
 
		// Extract the entire video to a 
		// single byte array of YUV data
		GetRawFrameData();
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
		Console.WriteLine(processOutput);
 
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
		string frameRateString = videoStream.GetProperty("avg_frame_rate").GetString();
		frameRate = double.Parse(frameRateString.Split("/")[0]);
 
 
		// Print out all the extracted information
		// TODO: Don't do this in production
		// TODO: Put in method
		// TODO: Don't do
		Console.WriteLine("Extracted information from " + Path + ":");
		Console.WriteLine("Width:\t\t" + width);
		Console.WriteLine("Height:\t\t" + height);
		Console.WriteLine("Frames:\t\t" + VideoHandler.frameCount);
		Console.WriteLine("Frame Rate:\t" + VideoHandler.frameRate);
	}
 
	private static void GetRawFrameData()
	{
		// Make the FFMPEG process to get all the data
		// TODO: Maybe try and get the data in a lower quality so editing is faster
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = "ffmpeg.exe",
			// Arguments = $"-i {Path} -vf fps={frameRate} -f image2pipe -vcodec rawvideo -",
			Arguments = $"-i {Path} -vf fps={frameRate} -f image2pipe -pix_fmt rgb24 -vcodec rawvideo -",
 
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
 
		// Run the process and get the output as
		// a single byte array that can then
		// be split later when the frames are needed
		process.Start();
		using (MemoryStream stream = new MemoryStream())
		{
			process.StandardOutput.BaseStream.CopyTo(stream);
			rawData = stream.ToArray();
		}
 
		// Also get how many bytes per frame. This
		// will be used heaps later on so its better
		// to calculate it here and not all the time.
		int bytesPerPixel = 1 + 2; //? Y, then U and V
		bytesPerFrame = (width * height) * bytesPerPixel;

		//! debug
		// Say how many bytes we extracted
		// TODO: Calculate expected length and check it against this to see if its correct
		Console.WriteLine(rawData.Length);
	}
 
	public static Texture2D LoadFrame(int frameIndex)
	{
		// Get the raw bytes for the current frame
		int frameOffset = (frameIndex - 1) * bytesPerFrame;
		byte[] frameBytes = new byte[bytesPerFrame];
		Array.Copy(rawData, frameOffset, frameBytes, 0, bytesPerFrame);

		// Make the renderTexture to draw the frame on
		// TODO: Don't make a new render texture each time
		RenderTexture2D frame = Raylib.LoadRenderTexture(width, height);
		Raylib.BeginTextureMode(frame);
 
		// Loop through every pixel in the frame
		int byteIndex = 0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				// Get the color of the pixel here
				Color pixel = new Color(frameBytes[byteIndex], frameBytes[byteIndex + 1], frameBytes[byteIndex + 2], byte.MaxValue);
				byteIndex += 3;

				// Draw the pixel to the render texture
				Raylib.DrawPixel(x, y, pixel);
			}
		}
		Raylib.EndTextureMode();
 
		// When the render texture has finished
		// rendering the frame then save it
		Texture2D finalFrame;
		while (true)
		{
			// If its still loading then do nothing
			if (!Raylib.IsRenderTextureReady(frame)) continue;
			// Save the texture
			finalFrame = frame.Texture;
			break;
		}
 
		// Give back the final frame
		return finalFrame;
	}
}