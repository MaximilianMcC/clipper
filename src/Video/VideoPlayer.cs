using System.Numerics;
using Raylib_cs;

class VideoPlayer
{
	// Media controls
	public static bool Playing { get; set; }
	public static float Volume { get; set; }
	public static bool Looped { get; set; }
	public static int FrameIndex { get; set; }

	// Timing stuff
	private static double timeLastFrameShown;

	// Loading stuff
	private static readonly int FullBatchSize = 1;


	public static void Update()
	{
		// Check for if the video is paused or not
		if (Playing == false) return;

		// Check for if we need to show the next frame
		// TODO: Figure out if it should be > or >=
		// TODO: Use guard clause
		double currentTime = Raylib.GetTime();
		double elapsedTime = currentTime - timeLastFrameShown;
		bool needNextFrame = elapsedTime > (1f / VideoHandler.FrameRate);

		if (needNextFrame)
		{
			// Update the frame counter
			timeLastFrameShown = currentTime;

			// Increase the frame index
			FrameIndex++;

			// Check for if the frame ahead of the one we're showing rn has been loaded
			if (VideoHandler.Frames[FrameIndex].Id == 0)
			{
				//TODO: Load in a couple more than one frame
				VideoHandler.GenerateFrame(VideoHandler.RawFrames[FrameIndex], FrameIndex);
			}
		}

		// Play the videos audio
		Raylib.UpdateMusicStream(VideoHandler.Audio);
	}

	public static void Render()
	{
		// Draw the video filling up the entire screen
		// TODO: Respect the aspect ratio and don't make look like its from 2010
		Rectangle source = new Rectangle(0, 0, VideoHandler.Width, -VideoHandler.Height);
		Rectangle destination = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
		Raylib.DrawTexturePro(VideoHandler.Frames[FrameIndex], source, destination, Vector2.Zero, 0f, Color.White);

		// TODO: Draw play button and whatnot. Maybe actually chuck in a 'Editor' class or something
	}
}