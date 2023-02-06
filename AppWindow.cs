using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;


namespace RayTracer_JP
{

	// Main window class and entrypoint for the raytracer application.
	public class AppWindow : GameWindow
	{

		public static RayTracerApp App;

		private static Vector2 mousePosition, lastMousePosition;
		private static bool terminated = false,
			firstMouseMove = true;


		// Raytracer application entrypoint.
		public static void Main(string[] args)
		{
			using (AppWindow win = new AppWindow(width: 1024, height: 512, "RayTracer_JP"))
				win.Run(30.0f, 0.0f);
		}


		// Initializes the window of the application.
		public AppWindow(int width, int height, string title) : base(width, height, OpenTK.Graphics.GraphicsMode.Default, title)
		{
			// Sets window size and initilazing the display and application.
			ClientSize = new Size(width, height);
			App = new RayTracerApp(new DisplayFrame(width/2, height, debugBgColor: Vec3Color.GrayScale(0.3f)));
			
			// Clips the cursor to the window-box and putting it in the centre of the window.
			CursorGrabbed = true;
			Mouse.SetPosition(X + (width / 2), Y + (height / 2));
		}


		// Sets GL states on window load.
		protected override void OnLoad( EventArgs e )
		{
			GL.ClearColor( 0, 0, 0, 0 );
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.DepthTest );
			GL.Hint( HintTarget.PerspectiveCorrectionHint, HintMode.Nicest );
		}

		// Unsets GL states on window unload.
		protected override void OnUnload( EventArgs e )
		{
			GL.DeleteTextures( 1, ref App.Tracer.Display.Id );
			Environment.Exit( 0 );
		}

		// Modifies window state on resize (Note: does not change the size of the pixel buffer).
		protected override void OnResize( EventArgs e )
		{
			// called upon window resize. 
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Viewport( 0, 0, Width, Height );
			GL.Ortho( -1.0, 1.0, -1.0, 1.0, 0.0, 4.0 );
		}


		// Ticks the app and draws the graphical views on rendering the window frame.
		protected override void OnRenderFrame( FrameEventArgs e )
		{
			// Tick the app, and exit on eventually caught termination.
			App.Tick();
			if( terminated )  
			{
				Exit();
				return;
			}

			// Draw the splitscreen debug- and camera views before swapping pixel buffers.
			App.DrawSplitViews();
			SwapBuffers();

			base.OnRenderFrame(e);
		}


		// Sets new camera-rotation values based on relative movement of mouse.
		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			// Save mouse-position, and last-mouseposition on first mouse move, as static class-bound variable.
			mousePosition = new Vector2(e.X, e.Y);
			if (firstMouseMove)
			{
				lastMousePosition = mousePosition;
				firstMouseMove = false;
				return;
			}

			// Rotate around x-axis (pitch change) relative to mouse y-position change.
			App.Tracer.Camera.RotateAroundXAxis(mousePosition.Y - lastMousePosition.Y);
			
			// Rotate round z-axis (yaw change) relative to mouse x-position change.
			App.Tracer.Camera.RotateAroundZAxis(mousePosition.X - lastMousePosition.X);

			// Remember current mouse position.
			lastMousePosition = mousePosition;
			base.OnMouseMove(e);
		}

		// Sets new camera field-of-view (through projection distance) on mousewheel-zoom.
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			App.Tracer.Camera.Zoom(-e.DeltaPrecise);
			base.OnMouseWheel(e);
		}

		// Processes keypresses on each frame-update.
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			this.ProcessKeyPress(Keyboard.GetState(), (float)e.Time);
			base.OnUpdateFrame(e);
		}

		// Process keypress relative to the pressed key and time passed since last frame-update.
		private void ProcessKeyPress(KeyboardState keyboard, float dt)
        {
			if (keyboard[Key.Escape])
			{
				terminated = true;
				return;
			}
			if (keyboard[Key.Right])
				App.Tracer.Camera.MoveRightBy(dt);
			else if (keyboard[Key.Left])
				App.Tracer.Camera.MoveLeftBy(dt);
			else if (keyboard[Key.Up])
				App.Tracer.Camera.MoveForwardBy(dt);
			else if (keyboard[Key.Down])
				App.Tracer.Camera.MoveBackwardBy(dt);
		}

	}

}