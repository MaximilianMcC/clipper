using System.Diagnostics;
using System.Text.Json;

class VideoHandler
{
	public static string Path;

	// Video information
	private static int Width;
	private static int Height;
	private static int FrameCount;
	private static double FrameRate;

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
		Width = videoStream.GetProperty("width").GetInt32();
		Height = videoStream.GetProperty("height").GetInt32();

		// Get the number of frames
		string frameCount = videoStream.GetProperty("nb_frames").GetString();
		FrameCount = int.Parse(frameCount);

		// Get the fps
		string frameRate = videoStream.GetProperty("avg_frame_rate").GetString();
		FrameRate = double.Parse(frameRate.Split("/")[0]);



		// Print out all the extracted information
		// TODO: Don't do this in production
		// TODO: Put in method
		// TODO: Don't do
		Console.WriteLine("Extracted information from " + Path + ":");
		Console.WriteLine("Width:\t\t" + Width);
		Console.WriteLine("Height:\t\t" + Height);
		Console.WriteLine("Frames:\t\t" + FrameCount);
		Console.WriteLine("Frame Rate:\t" + FrameRate);
	}

	private static void GetRawFrameData()
	{

	}

	public static void LoadFrame(int frameIndex)
	{

	}
}