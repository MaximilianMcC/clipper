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
			Arguments = $"-i {Path} -vf fps={frameRate} -f image2pipe -vcodec rawvideo -",
 
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
		int bytesPerPixel = 3; //? Y, U , V
		bytesPerFrame = (width * height) * bytesPerPixel;
	}
 
	public static Texture2D LoadFrame(int frameIndex)
	{
		// Maths rubbish
		int startIndex = frameIndex * bytesPerFrame;
		int chrominanceLength = (width * height) / 4;
 
		// Extract all of the channels
		byte[] luminance = rawData[startIndex..bytesPerFrame];
		byte[] blueChrominance = ExtractChannel(rawData[(startIndex - chrominanceLength - chrominanceLength)..chrominanceLength]);
		byte[] redChrominance = ExtractChannel(rawData[(startIndex - chrominanceLength)..chrominanceLength]);
 
		// Make the renderTexture to draw the frame on
		// TODO: Don't make a new render texture each time
		RenderTexture2D frame = Raylib.LoadRenderTexture(width, height);
 
		// Loop through every pixel in the frame
		Raylib.BeginTextureMode(frame);
		for (int i = 0; i < (width * height); i++)
		{	
			// Convert the pixel from YUV to RGB
			// Color pixelColor = YuvToRgb(luminance[i], blueChrominance[i], redChrominance[i]);
 
			// Get the position of the pixel using the index
			int y = i / width;
			int x = i % width;
 
			// Draw the pixel in the position
			// Raylib.DrawPixel(x, y, pixelColor);
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
 
	
	private static byte[] ExtractChannel(byte[] data)
	{
		// Store the new data
		byte[] expandedChannel = new byte[data.Length * 2];
		// Loop over the unexpanded data and
		// expand the values and save them
		// into the new data array
		// TODO: Don't use dataIndex. Use i
		int dataIndex = 0;
		for (int i = 0; i < data.Length; i++)
		{
			// Save the data on the x position
			expandedChannel[i] = data[dataIndex];
			expandedChannel[i + 1] = data[dataIndex];
 
			// Save the data on the y position
			expandedChannel[i + width] = data[dataIndex];
			expandedChannel[i + width + 1] = data[dataIndex];

			// Increase the index for next time
			dataIndex++;
		}
 
		// Give back the expanded data
		return expandedChannel;
	}
}