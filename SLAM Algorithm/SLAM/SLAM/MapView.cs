using Gtk;
using Cairo;
using System;
using System.Collections.Generic;

namespace SLAM
{
	public class MapView : Window
	{
		private const int MapWidth = 600; // 1 pixel = 1 cm.
		private const int MapHeight = 500;
		private const int TrueX = 0;
		private const int TrueY = 0;
		private const int CenterX = MapWidth / 2; // Our virtual center x coordinate.
		private const int CenterY = MapHeight / 2; // Our virtual center y coordinate.

		// Move the robot view elements to an external class later.
		private const int RobotWidth = 18;
		private const int RobotHeight = 23;
		private const int WheelWidth = 3;
		private const int WheelHeight = 6;

		private Map slamMap; // The back end model for the view.

		private TextView textView; // Textview to hold landmark information.

		/************************************************************
		 * Public Properties
		 ***********************************************************/

		public Map SlamMap
		{
			get
			{
				return this.slamMap;
			}
			set
			{
				this.slamMap = value;
			}
		}

		/************************************************************
		 * Public Constructors
		 ***********************************************************/

		public MapView (Map map) : base("Map")
		{
			this.slamMap = map; // Set the model map for this view.

			this.SetPosition (WindowPosition.Center);
			this.Resizable = false; 

			this.DeleteEvent += delegate
			{
				Application.Quit ();
			};

			DrawingArea drawingArea = new DrawingArea ();
			drawingArea.ExposeEvent += OnExpose;
			drawingArea.SetSizeRequest (MapWidth, MapHeight);

			TextBuffer textBuffer = new TextBuffer (new TextTagTable ());

			this.textView = new TextView ();
			this.textView.Editable = false;
			this.textView.Buffer = textBuffer;
			this.textView.CursorVisible = false;
			this.textView.Indent = 10;

			foreach (Landmark landmark in slamMap.SlamLandmarks)
			{
				this.textView.Buffer.Text += landmark.ToString ();
			}

			ScrolledWindow scrolledWindow = new ScrolledWindow ();
			scrolledWindow.Add (this.textView);

			VBox vbox = new VBox (false, 0);
			vbox.Add (drawingArea);
			vbox.Add (scrolledWindow);

			this.Add (vbox);
			this.ShowAll ();
		}

		/************************************************************
		 * Private Methods
		 ***********************************************************/	

		private void OnExpose (object sender, ExposeEventArgs args)
		{
			DrawingArea area = (DrawingArea)sender;
			Cairo.Context cairoContext = Gdk.CairoHelper.Create (area.GdkWindow);

			// Draw the Map.
			this.DrawBackground (cairoContext);
			this.DrawGrid (cairoContext);
			this.DrawRobot (cairoContext);

			// Don't bother attempting to draw landmarks if there
			// are none.
			if (this.slamMap.SlamLandmarks.Count > 0)
			{
				this.DrawLandmarks (cairoContext);
			}

			((IDisposable)cairoContext.GetTarget()).Dispose ();                                      
			((IDisposable)cairoContext).Dispose ();
		}

		private void DrawBackground (Cairo.Context cairoContext)
		{
			// Draw the background.
			cairoContext.Rectangle (TrueX, TrueY, MapWidth, MapHeight);
			cairoContext.SetSourceRGB(255, 255, 255);
			cairoContext.StrokePreserve ();
			cairoContext.Fill ();
		}

		private void DrawGrid (Cairo.Context cairoContext)
		{
			// Draw the grid Axis.
			cairoContext.LineWidth = 1.0;
			cairoContext.LineCap = LineCap.Butt;

			int width = MapWidth; 
			int height = MapHeight;

			// Y axis.
			cairoContext.SetSourceRGB(0, 0, 0);
			cairoContext.MoveTo (width / 2, 0.0);
			cairoContext.LineTo (width / 2, height);
			cairoContext.Stroke ();

			// TODO: These for loops aren't very intelligent, 
			// they break based on the scaling.
			for (int i = 50; i < height; i += 100)
			{
				if (i == height / 2)
				{
					continue;
				}

				cairoContext.MoveTo (width / 2, i);
				cairoContext.LineTo (width / 2 - 20.0, i);
				cairoContext.Stroke ();
			}

			for (int i = 100; i < height; i += 100)
			{
				if (i == height / 2)
				{
					continue;
				}

				cairoContext.MoveTo (width / 2, i);
				cairoContext.LineTo (width / 2 - 10.0, i);
				cairoContext.Stroke ();
			}

			// X axis.
			cairoContext.MoveTo (0.0, height / 2);
			cairoContext.LineTo (width, height / 2);
			cairoContext.Stroke ();

			// TODO: These for loops aren't very intelligent, 
			// they break based on the scaling.
			for (int i = 100; i < width; i += 100)
			{
				if (i == width / 2)
				{
					continue;
				}

				cairoContext.MoveTo (i, height / 2);
				cairoContext.LineTo (i, height / 2 + 20.0);
				cairoContext.Stroke ();
			}

			for (int i = 50; i < width; i += 100)
			{
				if (i == width / 2)
				{
					continue;
				}

				cairoContext.MoveTo (i, height / 2);
				cairoContext.LineTo (i, height / 2 + 10.0);
				cairoContext.Stroke ();
			}
		}

		private void DrawRobot (Cairo.Context cairoContext)
		{
			cairoContext.SetSourceRGB(255, 0, 0);
			cairoContext.Translate (CenterX + this.slamMap.RobotPosition [0], 
				CenterY - this.slamMap.RobotPosition [1]);
			cairoContext.Rotate (this.slamMap.RobotPosition [2]); // Rotate the robot based on its orientation in radians.

			// Draw the body.
			cairoContext.Rectangle (-(RobotWidth / 2), -(RobotHeight / 2), 
			                        RobotWidth, RobotHeight);
			cairoContext.StrokePreserve ();
			cairoContext.Fill ();

			cairoContext.SetSourceRGB(0, 0, 0);

			// Front indicator.
			cairoContext.Rectangle (-((RobotWidth / 2) - 8.0), -((RobotHeight / 2) - 5.0), 
			                        WheelWidth, WheelHeight);

			// Top left wheel.
			cairoContext.Rectangle (-((RobotWidth / 2) + 4.0), -(RobotHeight / 2), 
			                        WheelWidth, WheelHeight);

			// Top right wheel.
			cairoContext.Rectangle (((RobotWidth / 2) + 1.0), -(RobotHeight / 2), 
			                        WheelWidth, WheelHeight);

			// Bottom left wheel.
			cairoContext.Rectangle (-((RobotWidth / 2) + 4.0), -((RobotHeight / 2) - 17.0), 
			                        WheelWidth, WheelHeight);

			// Bottom right wheel.
			cairoContext.Rectangle (((RobotWidth / 2) + 1.0), -((RobotHeight / 2) - 17.0), 
			                        WheelWidth, WheelHeight);

			cairoContext.StrokePreserve ();
			cairoContext.Fill ();

			// Reset the drawing context.
			cairoContext.Rotate (-this.slamMap.RobotPosition [2]);
			cairoContext.Translate (-(CenterX + this.slamMap.RobotPosition [0]), 
				-(CenterY - this.slamMap.RobotPosition [1]));
		}

		private void DrawLandmarks (Cairo.Context cairoContext)
		{
			foreach (Landmark landmark in this.slamMap.SlamLandmarks)
			{
				// From our virtual center move to the landmark's position.
				// Also scale up landmark positions from metres to centimetres.
				cairoContext.Translate (CenterX + (landmark.pos [0] * 100), 
				                        CenterY - (landmark.pos [1] * 100));

				cairoContext.SetSourceRGB(0, 0, 255);
				cairoContext.Arc (0, 0, 15, 0, 2 * Math.PI);
				cairoContext.StrokePreserve ();
				cairoContext.Fill ();

				// Return to our virtual center after drawing the landmark.
				cairoContext.Translate (-(CenterX + (landmark.pos [0] * 100)), 
				                        -(CenterY - (landmark.pos [1] * 100)));
			}

			// Draw the landmark id in a seperate loop to avoid any weird
			// tearing behaviour.
			cairoContext.SetSourceRGB(255, 255, 255);

			cairoContext.SelectFontFace ("Sans Serif", FontSlant.Normal, FontWeight.Normal);
			cairoContext.SetFontSize (12);

			foreach (Landmark landmark in this.slamMap.SlamLandmarks)
			{
				// Center the text inside the landmark.
				TextExtents textExtents = cairoContext.TextExtents (landmark.id.ToString ());

				cairoContext.MoveTo (CenterX + (landmark.pos [0] * 100) - (textExtents.Width / 2), 
				                     CenterY - (landmark.pos [1] * 100) + (textExtents.Height / 2));

				cairoContext.ShowText (landmark.id.ToString ());
			}

			// Draw the equation of a line for each landmark.
			cairoContext.SetSourceRGB(0, 255, 0);

			foreach (Landmark landmark in this.slamMap.SlamLandmarks)
			{
				cairoContext.MoveTo (CenterX + ((landmark.b / -landmark.a) * 100), CenterY);
				cairoContext.LineTo (CenterX, CenterY - (landmark.b * 100));
				cairoContext.Stroke ();

				Console.WriteLine ("X = " +  ((landmark.b / -landmark.a) * 100));
				Console.WriteLine ("Y = " + (landmark.b * 100));
			}
		}
	}
}

