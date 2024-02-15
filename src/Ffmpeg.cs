using System.Diagnostics;
using Raylib_cs;

class Ffmpeg
{
	public static void LoadVideo(string videoPath)
	{
		// TODO: Put this somewhere else
		string outputPath = "./assets/output.mp4";

		// Create the command to run, then run it
		// TODO: Run in another thread
		Process ffmpegProcess = new Process();
		ffmpegProcess.StartInfo = new ProcessStartInfo
		{
			FileName = VideoManager.FfmpegPath + "/ffmpeg.exe",
			Arguments = $"-i {videoPath} -f rawvideo -",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		ffmpegProcess.Start();

		// Get the data thats being piped in from FFMPEG
		// and process it to convert it into raylib textures
		// so that the video can be played
		using (Stream ffmpegOutput = ffmpegProcess.StandardOutput.BaseStream)
		{
			// Create a buffer to store all of the incoming data
			//? 1024 is a standard buffer size for something like this
			byte[] buffer = new byte[1024];
			int bytesRead;

			// Continuously cop the bytes that are coming in
			while ((bytesRead = ffmpegOutput.Read(buffer, 0, buffer.Length)) > 0)
			{
				// Turn the raw pixel data into a texture,
				// then add it to the video object thing
				Texture2D frameTexture = GenerateTextureFromPixels(buffer, bytesRead);
			}
		}

		// Wait for the ffmpeg stuff to finish
		ffmpegProcess.WaitForExit();
	}


	private static Texture2D GenerateTextureFromPixels(byte[] pixels, int length)
	{
		Texture2D frameTexture = new Texture2D();

		// Loop through all pixels in the array, 3 pixels
		// at a time (one byte for r, g, b)
		for (int i = 0; i < length - 3; i += 3)
		{
			// Get the rgb values of each pixel
			// and use it to make a raylib color
			byte red = pixels[i];
			byte green = pixels[i + 1];
			byte blue = pixels[i + 2];
			Color pixel = new Color(red, green, blue, byte.MaxValue);


			Debug.WriteLine(pixel);
		}

		Debug.WriteLine(pixels);
		Debug.WriteLine(length);

		return frameTexture;
	}
}