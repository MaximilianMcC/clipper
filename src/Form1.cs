using AForge.Video.FFMPEG;

namespace clipper;

public partial class Form1 : Form
{
	private PictureBox videoOutput;
	private Bitmap[] frames;
	private long frameCount;
	private double framesPerSecond;
	private int width;
	private int height;

	public Form1()
	{
		// Set form window stuff and whatnot idk
		Text = "Clipper r4";
		FormBorderStyle = FormBorderStyle.FixedSingle;
		MaximizeBox = false;
		Size = new Size(800, 600);

		// Event things
		KeyDown += new KeyEventHandler(HandleShortcuts);

		// Draw all the UI
		DrawUi();
	}

	private void DrawUi()
	{
		// Write some text idk
		Controls.Add(new Label()
		{
			Text = "Clipper (clipping rn (now))",
			Font = new Font("Segoe UI", 24, FontStyle.Bold),
			Location = new Point(10, 10),
			AutoSize = true
		});

		// Bit more text
		Controls.Add(new Label()
		{
			Text = "cheeky ctrl+o to open a clip",
			Font = new Font("Segoe UI", 16),
			Location = new Point(10, 80),
			AutoSize = true
		});

		// Video player
		videoOutput = new PictureBox()
		{
			BackColor = Color.Magenta,
			Location = new Point(0, 0),
			Size = new Size(400, 300)
		};
		Controls.Add(videoOutput);
	}

	private void HandleShortcuts(object sender, KeyEventArgs e)
	{
		// Check for if they wanna open a video (ctrl+o)
		if (e.Control && e.KeyCode == Keys.O) LoadVideoFromDialog();
	}

	private void LoadVideoFromDialog()
	{
		// Open a new file dialog to let the 
		// user select the video file that they
		// want to open so they can edit it
		OpenFileDialog fileDialog = new OpenFileDialog()
		{
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
			Filter = "Video files (*.mp4;*.avi;*.mov;*.wmv)|*.mp4;*.avi;*.mov;*.wmv|All files (*.*)|*.*",
			RestoreDirectory = true
		};

		// Open the dialog, then check for if they have
		// flicked us a file
		if (fileDialog.ShowDialog() == DialogResult.OK)
		{
			// Grab the location of the clip
			string filePath = fileDialog.FileName;

			// Load the video
			LoadVideo(filePath);
		}

		// Clean up
		fileDialog.Dispose();
	}

	// Load the video
	private void LoadVideo(string videoPath)
	{
		// Open the video
		VideoFileReader videoReader = new VideoFileReader();
		videoReader.Open(videoPath);

		// Extract all of the needed information
		width = videoReader.Width;
		height = videoReader.Height;
		frameCount = videoReader.FrameCount;
		framesPerSecond = videoReader.FrameRate;

		// Make a frames array to store all of the video
		// frames for playback later
		frames = new Bitmap[frameCount];

		// Loop over every frame in the video
		for (int i = 0; i < frameCount; i++)
		{			
			// Get the current frame, then add it to the
			// video frames array for later playback
			Bitmap currentFrame = videoReader.ReadVideoFrame();
			frames[i] = currentFrame;

			// Get rid of the frame
			currentFrame.Dispose();
			Console.Write($"\rLoaded frame {i}/{frameCount}");
		}
	}
}
