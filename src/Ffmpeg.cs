using System.Diagnostics;

class Ffmpeg
{
	public enum Service
	{
		Ffmpeg,
		Ffprobe
	}

	// Run a FFMPEG command and get the output as a string
	//! do NOT use this method for larger commands that could take some time to execute
	// TODO: Make async/multithreaded to stop blocking
	public static string RunCommand(Service service, string command)
	{
		// Check for what FFMPEG binary thing they need
		// TODO: Path.join type thing
		string fileName = VideoManager.FfmpegPath;
		if (service == Service.Ffmpeg) fileName += "/ffmpeg.exe";
		else if (service == Service.Ffprobe) fileName += "/ffprobe.exe";

		// Create the process/command and run it
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = fileName,
			Arguments = command,

			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true
		};
		process.Start();

		// Get the command output as a string
		string commandOutput = process.StandardOutput.ReadToEnd();
		if (commandOutput == "") commandOutput = process.StandardError.ReadToEnd();

		// Wait for the process to finish and stuff
		process.WaitForExit();

		// Give back the command output
		return commandOutput;
	}
}