using System.Diagnostics;

class Utils
{
	// Run an FFMPEG command
	// TODO: Maybe make a copy of the file before modifying
	// TODO: Make a new thread to run the commands in
	public static void RunFfmpegCommand(string args)
	{
		// TODO: Put this somewhere else
		string outputPath = "./assets/output.mp4";

		// Create the command to run
		ProcessStartInfo command = new ProcessStartInfo
		{
			FileName = VideoManager.FfmpegPath + "/ffmpeg.exe",
			Arguments = $"-i {VideoManager.VideoPath} {args} {outputPath}",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		// Make a new thread to run the command in
		Thread commandThread = new Thread(() => RunCommand(command));
		commandThread.Start();
		
		Console.WriteLine("Ran command");
	}

	private static void RunCommand(ProcessStartInfo command)
	{
		// Run the command
		Process? ffmpeg = Process.Start(command);

		// Get the output
		// TODO: Do something with these
		string output = ffmpeg.StandardOutput.ReadToEnd();
		string error = ffmpeg.StandardError.ReadToEnd();

		Console.WriteLine(output);
		Console.WriteLine(error);

		// Wait for the command to finish running
		// this could take a while because working with
		// video is kinda slow
		ffmpeg.WaitForExit();
	}
}