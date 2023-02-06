using System;
using OpenTK;

namespace RayTracer_JP
{

    // An instantiating class for the scene which encapsulates the present
    // primitive-objects and lights, along with further specifics.
	public class Scene
	{

		public Primitive[] Primitives;
		public Light[] Lights;

		public int BgColor;

		private float ambientLightIntensity = 0.2f;


		public Scene(Primitive[] prims, Light[] lights, Vector3 bgColor)
		{
			this.Primitives = prims;
			this.Lights = lights;
			this.BgColor = Vec3Color.ToIntRGB(bgColor);
		}


        // Intersect all the primitive objects in the scene with a certain ray and save
        // the intersection in the ray if it is closer than the previously known one.
		public void IntersectWithRay(Ray ray)
		{
			foreach (Primitive prim in this.Primitives) 
            {
				Intersection isection = prim.IntersectWithRay(ray);
				ray.Intersection = ray.IsCloserIntersection(isection) ? isection : ray.Intersection;
			}
		}

        // Calculates the ambient light intensity modified color at a certain intersection.
		public Vector3 AmbientLightAt(Primitive prim, Vector3 isect_pos)
			=> prim.GetColorAt(isect_pos) * this.ambientLightIntensity;

	}



    // Instantiating class for a positionable light.
    public class Light : Positionable
    {
        public Vector3 Color;
        
        private Vector3 glossColorFactor = Vec3Color.GrayScale(0.1f);


        public Light(Vector3 pos, Vector3 c) : base(pos)
        {
            this.Color = c;
        }


        // Creates and intersects a shadow-ray from this light to a certain intersection.
        public Ray CreateIntersectShadowray(Scene scene, Intersection isection)
        {
            Ray shadowRay = new Ray(this.Position, this.OrientationTo(isection.Position));
            scene.IntersectWithRay(shadowRay);
            return shadowRay;
        }

        // Calculates and returns the diffusion-modified color of an intersection produced by a certain shadow- and primary ray.
        public Vector3 CalculateDiffuseColorAt(Intersection isect, Ray shadowRay, Vector3 rayPos)
        {
            // Calculate the distance-attenuaded light shading.
            Vector3 attenuatedColor = this.Color / (float)Math.Pow(shadowRay.Intersection.Distance, 1.0f);

            // Calculate the diffused color shading.
            Vector3 N = -isect.Intersectee.OutwardNormalAt(isect.Position),
                L = this.OrientationTo(isect.Position);
            Vector3 diffusedColor = isect.Intersectee.GetColorAt(isect.Position) * Math.Max(0, Vector3.Dot(L, N));

            // Calculate the gloss shading.
            Vector3 V = isect.OrientationTo(rayPos),
                R = (L - (2f * Vector3.Dot(L, N) * N)).Normalized();
            Vector3 glossyColor = glossColorFactor * (float)Math.Pow(Math.Max(0, Vector3.Dot(V, R)), 2);

            return attenuatedColor * (diffusedColor + glossyColor);
        }

    }

}
