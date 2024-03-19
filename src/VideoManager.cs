using System.Diagnostics;
using System.Text.Json;
using Raylib_cs;

class VideoManager
{
	public static string Path { get; set; }

	// Diagnostics stuff
	public static bool Loading { get; private set; }

	// Video properties
	public static double Fps { get; private set; }
	public static int FrameCount { get; private set; }
	public static int Width { get; private set; }
	public static int Height { get; private set; }

	// Frame stuff
	private static byte[][] RawFrames;
	private static Texture2D?[] Frames;

	// Playback stuff
	public static bool Paused { get; set; }



	//? https://chat.openai.com/share/8d5271d7-c12c-4c8d-acd9-5dfefa305dbc
	//? https://www.youtube.com/watch?v=32PPzwPjDZ8	




	// Load the video
	// TODO: Make non-blocking (multi-thread)
	public static void LoadVideo()
	{
		Loading = true;

		// Use FFPROBE to get all of the required
		// information about the video
		Console.WriteLine("Extracting information...");
		Thread getInformationThread = new Thread(() => GetInformation());
		getInformationThread.Start();
		getInformationThread.Join();

		// Split the video into sections of
		// frames that contain their YUV data
		Console.WriteLine("Splitting frames...");
		Thread splitFramesThread = new Thread(() => SplitFrames());
		splitFramesThread.Start();
		splitFramesThread.Join();

		// Make a place to store all of the frames
		//? null means the current frame hasn't been loaded yet
		Frames = new Texture2D?[FrameCount];

		Console.WriteLine("Done!");
		Loading = false;
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
	// TODO: Only split a third or something then if we get close to reaching the end of split frames load in the other third, then again untill the whole thing loaded.
	//! idk if these optimizations are really necessary because the clips are gonna be short they gonna load super fast
	// TODO: First read the entire byte stream, then once done split it. Don't split as its being read
	private static void SplitFrames()
	{
		// Figure out how many bytes one frame takes up
		// TODO: Actually do that calculation if doing the read entire then split method
		//? I got 3 from the formula (totalBytes / totalFrames) / (width * height)
		//? and I did it for multiple videos and got 3 as a result for all of them
		int bytesPerPixel = 3;
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
			Arguments = $"-i {Path} -vf fps={Fps * 2} -f image2pipe -vcodec rawvideo -",

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



	public static void UpdateVideo()
	{
		// Check for if it's paused
		if (Paused) return;

		// TODO: Check for if we're due for the next frame
	}



	public static void RenderVideo()
	{


		
	}


	// Convert all of the YUV values to
	// RGB values, then bake them into a 
	// render texture so the frame can be
	// drawn.
	public static Texture2D LoadFrame(int frameIndex)
	{
		int pixels = Width * Height;

		// Get all of the luminance values
		byte[] luminance = RawFrames[frameIndex].Take(pixels).ToArray();

		// Get all of the chrominance values
		// TODO: Don't do this rinky Skip and Take linq thing
		byte[] blueChrominance = ExpandFrame(RawFrames[frameIndex].Skip(pixels).Take(pixels / 4).ToArray());
		byte[] redChrominance = ExpandFrame(RawFrames[frameIndex].Skip(pixels + (pixels / 4)).Take(pixels / 4).ToArray());

		// Draw everything to the render texture
		// TODO: Don't make, then unload new render texture for each frame. reuse
		RenderTexture2D frameRenderTexture = Raylib.LoadRenderTexture(Width, Height);
		Raylib.BeginTextureMode(frameRenderTexture);
		for (int i = 0; i < (pixels); i++)
		{
			// Get the current pixel
			Color pixel = YuvToRgb(luminance[i], blueChrominance[i], redChrominance[i]);

			// Get the coordinates, then draw the pixel
			// to the screen
			// TODO: See if nested for loop faster
			int x = i % Width;
			int y = i / Width;
			Raylib.DrawPixel(x, y, pixel);
		}
		Raylib.EndTextureMode();

		// Save the frame texture and get rid of
		// the render texture
		Texture2D frameTexture = frameRenderTexture.Texture;
		Raylib.UnloadRenderTexture(frameRenderTexture);

		// Give back the frame
		return frameTexture;
	}

	// 'Expand' all of the pixels in a frame (420)
	// TODO: Make it based off the YUV format string
	private static byte[] ExpandFrame(byte[] data)
	{
		// Store the new expanded frame
		byte[] expandedFrame = new byte[Width * Height];

		// Loop through every pixel that we have
		// TODO: Use i+2 instead of index
		int index = 0;
		for (int i = 0; i < data.Length; i++)
		{
			// Get its value
			byte value = data[i];

			// Set the the pixels on the top
			expandedFrame[index] = value;
			expandedFrame[index + 1] = value;

			// Set the pixels on the bottom
			expandedFrame[index + Width] = value;
			expandedFrame[index + Width + 1] = value;
		
			// Increase the index for next time
			index += 2;
		}

		return expandedFrame;
	}

	// Convert a YUV pixel to an RGB pixel for drawing
	private static Color YuvToRgb(byte y, byte u, byte v)
	{
		// Do the actual conversion
		int r = (int)(y + 1.403 * (v - 128));
		int g = (int)(y - 0.344 * (u - 128) - 0.714 * (v - 128));
		int b = (int)(y + 1.770 * (u - 128));

		// Clamp the values to bytes
		r = Math.Max(0, Math.Min(255, r));
		g = Math.Max(0, Math.Min(255, g));
		b = Math.Max(0, Math.Min(255, b));

		// Give back the color
		return new Color(r, g, b, byte.MaxValue);
	}

}