using OpenTK;

namespace RayTracer_JP
{

	// The main class where the action happens; the ray-tracer.
	// Contains (has under command) the display, scene and camera
	// and delegates calls from the window to be computed by the specifics of these three.
	public class RayTracer
	{

		public DisplayFrame Display;
		public Camera Camera;
		public Scene Scene;



		public RayTracer(DisplayFrame display, Scene scene, Vector3 camPos, Vector3 camOri, Vector3 camOri2, float projectionDistance)
		{
			this.Display = display;
			this.Scene = scene;
			this.Camera = new Camera(camPos, camOri, camOri2, projectionDistance:projectionDistance, projectionWidth:display.Width, projectionHeight:display.Height);
			this.Camera.GenerateRays(scene);
		}


		// Renders the normal camera view by setting each viewing-ray's intersection color to its correspondig pixel.
		public void RenderCameraView()
        {
			this.Display.ClearCamPixels(this.Scene.BgColor);
			for (int i = 0; i<this.Display.Height; i++) 
			{
				for(int j = 0; j<this.Display.Width; j++) 
				{
					// Check each viewing ray for intersection, and if so, put intersection color to pixel of corresponding ray.
					if (this.Camera.ViewRays[i, j].Intersection != null)
						this.Display.PutCamPixel(i, j, this.Camera.ViewRays[i, j].Intersection.Color);
                }
            }
        }

		// Renders the debug view by drawing each relevant object of the ray tracer (top view)
		public void RenderDebugView()
        {
			this.Display.ClearDebugPixels(this.Display.DebugBgColor);
			// Draw primitive spheres.
			foreach (Primitive prim in this.Scene.Primitives) 
			{
				if (typeof(Sphere).IsInstanceOfType(prim)) 
				{
					Sphere sph = (Sphere)prim;
					this.Display.DrawDebugSphere(sph.Position, sph.Radius, sph.Color);
				}
            }
			// Draw middle primary-, secondary- and shadow-rays.
			this.drawAllDebugRays(nRays: 8);

			// Draw camera and projection-plane.
			this.Display.DrawDebugSphere(this.Camera.Position, 0.05f, Vec3Color.Black);
			this.Display.DrawDebugLine(this.Camera.Projection.LeftUpperPosition, this.Camera.Projection.LeftUpperPosition + this.Camera.Projection.RightDirection, Vec3Color.White);

			// Draw lights.
			foreach(Light light in this.Scene.Lights)
				this.Display.DrawDebugSphere(light.Position, 0.05f, Vec3Color.White);
		}


		// Draw the primary-, secondary- and shadow-rays to debug output
		private void drawAllDebugRays(int nRays)
		{
			int i = (int)(this.Display.Height / 2), j;
			Ray primaryRay;

			// Draw nRays amount of viewing-rays in the middle row (i), and eventual corresponding non-viewing rays.
			for (j = 0; j < this.Display.Width; j += (int)(this.Display.Width / nRays-1)) 
			{
				primaryRay = this.Camera.ViewRays[i, j];
				this.drawDebugRay(primaryRay, this.Display.DebugPrimaryRayColor);
				if (primaryRay.Intersection != null)
					this.drawDebugNonViewingRays(primaryRay.Intersection);
			}
		}

		// Draw the secondary- and shadow-rays of a primary ray's intersection to debug output.
		private void drawDebugNonViewingRays(Intersection isection)
        {
			// Draw the shadowrays if the object of the intersection is a mirror else secondary rays.
			if (!isection.Intersectee.IsMirror)
			{
				foreach (Ray shadowRay in isection.ShadowRays)
				{
					if (shadowRay == null)
						continue;
					this.drawDebugRay(shadowRay, this.Display.DebugShadowRayColor);
				}
			}
			else
			{
				Ray secondaryRay = isection.SecondaryRay;
				while (secondaryRay != null)
				{
					this.drawDebugRay(secondaryRay, this.Display.DebugSecondaryRayColor);
					if (secondaryRay.Intersection == null)
						break;
					secondaryRay = secondaryRay.Intersection.SecondaryRay;
				}
			}
		}

		// Draw a single ray to debug output.
		private void drawDebugRay(Ray ray, Vector3 rayColor)
		{
			if (ray.Intersection != null && ray.DistanceTo(ray.Intersection.Position) < 1000f)
				// Draw ray line up until the intersection.
				this.Display.DrawDebugLine(ray.Position, ray.Intersection.Position, rayColor);
			else
				// To far of a line!! In reality endless, but the DrawDebugLine method will do the clippings!
				this.Display.DrawDebugLine(ray.Position, ray.Position + (150f * ray.Orientation), Vec3Color.White);
		}

	}

}
