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
	
	// TODO: Parallel.For or whatever it is
	// TODO: Make a second thread to load the video in the background, and also have a method to immediately load 10 or so frames so that if you jump around the video and have stuff loaded
	public static void LoadFrameBatch(int frameIndex)
	{
		// TODO: Make adjust for if it goes over length
		int batchSize = 10;

		// Store all of the frames as a byte array
		byte[][] allFrames = new byte[batchSize][];

		// Create the FFMPEG process to extract the
		// necessary frames from the video
		Process extractionProcess = new Process();
		extractionProcess.StartInfo = new ProcessStartInfo()
		{
			//? Reading as RGB byte array
			FileName = "ffmpeg.exe",
			// Arguments = $"-i {Path} -vf \"select='between(n\\,{frameIndex * batchSize}\\,{(frameIndex + 1) * batchSize - 1})'\" -pix_fmt rgb24 -f rawvideo -",
            Arguments = $"-i \"{Path}\" -vf select='gte(n\\,{frameIndex})' -vframes {batchSize} -f image2pipe -pix_fmt rgb24 -vcodec rawvideo -",

			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		extractionProcess.Start();

		// Loop through every frame that we need to load in
		Stream stream = extractionProcess.StandardOutput.BaseStream;
		for (int i = 0; i < batchSize; i++)
		{
			// Make the buffer for the current frame
			byte[] frameBuffer = new byte[bytesPerFrame];
			int totalBytes = 0;

			// Keep on piping in data until its full
			while (totalBytes < bytesPerFrame)
			{
				// Get how many bytes we need to pipe
				int chunk = 1024; //? 1 kilobyte 
				int neededBytes = Math.Max(totalBytes - bytesPerFrame, chunk);

				// Actually pipe the data
				int bytesRead = stream.Read(frameBuffer, 0, neededBytes);
				Console.WriteLine("read " + bytesRead + " bydes (needed bytes is) " + neededBytes);
			}
		}
		stream.Close();
	}
}