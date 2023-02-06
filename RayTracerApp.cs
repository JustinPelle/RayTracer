using OpenTK;

namespace RayTracer_JP
{

	// Wrapper class of the ray-tracer.
	// Handles setting up of the scene-objects and tracer itself
	// and delegates calls from the window to the raytracer.
	public class RayTracerApp
	{

		public RayTracer Tracer;

		public RayTracerApp(DisplayFrame display)
		{
			// Generating the primitives for the scene.
			Primitive[] prims = new Primitive[10] {
				new Plane(pos: new Vector3(25f, 25f, -1f), ori:new Vector3(0f, 0f, 1f), c:Vec3Color.White, isMirror: false, hasCheckersTexture: true),
				new Sphere(pos:new Vector3(-3f, 5f, 0f), ori:Vector3.Zero, r:0.8f, c:Vec3Color.Red, isMirror: false ),
				new Sphere(pos:new Vector3(0f, 5f, 0f), ori:Vector3.Zero, r:0.8f, c:Vec3Color.Green, isMirror: false ),
				new Sphere(pos:new Vector3(3f, 5f, 0f), ori:Vector3.Zero, r:0.8f, c:Vec3Color.Blue, isMirror: false ),
				new Sphere(pos:new Vector3(-2f, 7f, 2f), ori:Vector3.Zero, r:0.8f, c:Vec3Color.Yellow, isMirror: false ),
				new Sphere(pos:new Vector3(0f, 9f, 4f), ori:Vector3.Zero, r:0.8f, c:Vec3Color.Purple, isMirror: false ),
				new Sphere(pos:new Vector3(2f, 7f, 2f), ori:Vector3.Zero, r:0.8f, c:Vec3Color.Cyan, isMirror: false ),
				new Sphere(pos:new Vector3(0f, 7f, 1.5f), ori:Vector3.Zero, r:0.8f, c:Vec3Color.White, isMirror: true ),
				new Sphere(pos:new Vector3(-4f, 7f, 2f), ori:Vector3.Zero, r:0.8f, c:Vec3Color.Orange, isMirror: true ),
				new Sphere(pos:new Vector3(4f, 7f, 2f), ori:Vector3.Zero, r:0.8f, c:Vec3Color.Orange, isMirror: true )
			};

			// Generating the lights for the scene.
			Light[] lights = new Light[2] {
				new Light(new Vector3(-1.5f, 6f, 4f), c:Vec3Color.White),
				new Light(new Vector3(1.5f, 6f, 4f), c:Vec3Color.White)
			};

			// Initialising the ray-tracer with the given display, scene and camera-specifics.
			this.Tracer = new RayTracer(
				display,
				scene: new Scene(prims, lights, bgColor: Vec3Color.Black),
				camPos: new Vector3(0f, 1f, 0f),
				camOri: new Vector3(0f, 1f, 0f),
				camOri2: new Vector3(0f, 0f, 1f),
				projectionDistance: 0.8f
			);
		}

		// App tick; regenerate rays whenever the camera (projection) has changed
		public void Tick()
		{
			if (this.Tracer.Camera.HasChanged)
            {
				this.Tracer.Camera.RegenerateRays(this.Tracer.Scene);
				this.Tracer.Camera.HasChanged = false;
			}
		}

		// App draw; draw the debug view and cam view to the display
		public void DrawSplitViews()
        {
			// Fill the pixel-data
			this.Tracer.RenderDebugView();
			this.Tracer.RenderCameraView();

			// Draw the debug pixels
			this.Tracer.Display.DebugPixelsToTexImage2D();
			this.Tracer.Display.DrawQuadFill();

			// Draw the camera pixels
			this.Tracer.Display.CamPixelsToTextImage2D();
			this.Tracer.Display.DrawQuadFill();
		}

	}

}