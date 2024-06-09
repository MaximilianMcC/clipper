namespace clipper;

public partial class Form1 : Form
{
	public Form1()
	{
		// Set form window stuff and whatnot idk
		Text = "Clipper r4";
		FormBorderStyle = FormBorderStyle.FixedSingle;
		MaximizeBox = false;
		Size = new Size(800, 600);

		// Write some text idk
		Label label = new Label()
		{
			Text = "Clipper (clipping rn (now))",
			Font = new Font("Segoe UI", 24, FontStyle.Bold),
			Location = new Point(10, 10),
			AutoSize = true
		};
		Controls.Add(label);
	}
}
