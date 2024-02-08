using System.Diagnostics;

class Utils
{
	// Run an FFMPEG command
	// TODO: Maybe make a copy of the file before modifying
	public static void RunFfmpegCommand(string args)
	{
		// TODO: Put this somewhere else
		string outputPath = "./assets/test.mp4";

		// Create the command to run
		ProcessStartInfo command = new ProcessStartInfo
		{
			FileName = VideoManager.FfmpegPath,
			Arguments = $"-i {VideoManager.VideoPath} {args} {outputPath}",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		// Run the command
		Process? ffmpeg = Process.Start(command);

		// Get the output
		// TODO: Do something with these
		string output = ffmpeg.StandardOutput.ReadToEnd();
		string error = ffmpeg.StandardError.ReadToEnd();

		// Wait for the command to finish running
		// this could take a while because working with
		// video is kinda slow
		ffmpeg.WaitForExit();
	}
}