using System.Diagnostics;
using System.Text.Json;

class VideoManager
{
	public static string Path { get; set; }

	// Video properties
	public static double Fps { get; private set;}
	public static int FrameCount { get; private set;}
	public static int Width { get; private set;}
	public static int Height { get; private set;}

	// Raw frame data (YUV)
	private static byte[][] RawFrames;


	//? explain yuv: https://www.youtube.com/watch?v=32PPzwPjDZ8	


	// Load the video
	// TODO: Make non-blocking (multi-thread)
	public static void LoadVideo()
	{
		// Use FFPROBE to get all of the required
		// information about the video
		Console.WriteLine("Extracting information...");
		GetInformation();

		// Split the video into sections of
		// frames that contain their YUV data
		Console.WriteLine("Splitting frames...");
		SplitFrames();


		Console.WriteLine("Done!");
	}

	// Get video properties and whatnot
	private static void GetInformation()
	{
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = "ffprobe.exe",
			Arguments = $"-i {Path} -show_streams -print_format json -v error",

			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		process.Start();
		
		// Get the output as a string
		string extractedJson = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		// Parse the output to JSON so that the
		// values can be extracted and worked with
		// TODO: Make class and do with JsonSerializer object
		//? Using the first stream because thats normally video. Second stream is normally audio, and idk what the other steams are (irrelevant)
		JsonDocument extractedInformation = JsonDocument.Parse(extractedJson);
		JsonElement root = extractedInformation.RootElement.GetProperty("streams")[0];

		// Get the FPS
		string fpsString = root.GetProperty("avg_frame_rate").GetString();
		Fps = double.Parse(fpsString.Split("/")[0]);

		// Get the total amount of frames
		string frameCountString = root.GetProperty("nb_frames").GetString();
		FrameCount = int.Parse(frameCountString);

		// Get the width/height
		Width = root.GetProperty("width").GetInt32();
		Height = root.GetProperty("height").GetInt32();
	}

	// Split the video into frames
	//? https://chat.openai.com/share/8d5271d7-c12c-4c8d-acd9-5dfefa305dbc
	private static void SplitFrames()
	{
		// Figure out how many bytes one frame takes up
		//? 2 bytes per pixel because 1 for luminance(y) and another for either the u or v (chrominance)
		int bytesPerPixel = 2;
		int bytesPerFrame = (Width * Height) * bytesPerPixel;


		// Store all of the raw frame data
		int frameIndex = 0;
		RawFrames = new byte[FrameCount][];


		// Use FFMPEG to get the entire in
		// a massive raw byte array
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			//? idk why need to times by 2
			FileName = "ffmpeg.exe",
			Arguments = $"-1 {Path} -vf fps={Fps * 2} -f image2pipe -vcodec rawvideo |",

			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		process.Start();


		// Pipe the command output so that we can
		// use it
		using (Stream stream = process.StandardOutput.BaseStream)
		{
			// Store all of the incoming bytes
			//? 1024 is a standard size (1kb)
			byte[] buffer = new byte[1024];

			// Keep track of how many bytes
			// we have read for the current frame
			int totalBytesRead = 0;
			byte[] frameBuffer = new byte[bytesPerFrame];

			// Continuously pipe data from the video
			while (true)
			{
				// Get how many bytes we have read
				// and add the read bytes to the frame buffer.
				// And save the data that doesn't fit in the
				// current frame for next frame
				int bytesRead = stream.Read(frameBuffer, totalBytesRead, bytesPerFrame - totalBytesRead);
				totalBytesRead += bytesRead;

				// Check for if we have reached the
				// end of the stream (no more data)
				if (bytesRead == 0) break;

				// Check for if we have read enough
				// bytes to make up one entire frame
				// TODO: Use guard clause
				//? Not using guard clause here because its better for readability even thought we 3 indents in now
				if (totalBytesRead == bytesPerFrame)
				{
					// Add the data to the raw frame
					// data array so it can be processed
					// later when its ready to be drawn
					RawFrames[frameIndex] = frameBuffer;
					frameIndex++;

					// Reset the total bytes for the
					// next frame
					totalBytesRead = 0;
				}
			}

		}
	}
}