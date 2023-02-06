using OpenTK;

namespace RayTracer_JP
{

    // Instantiating class for usage as ray as an orientable.
    // Is practically used for primary-, secondary and shadow-rays.
    public class Ray : Orientable
    {
        public Intersection Intersection = null;


        public Ray(Vector3 pos, Vector3 ori) : base(pos, ori) { }


        // Checks whether the supplied intersection is closer than the currently known intersection.
        public bool IsCloserIntersection(Intersection isection)
            => isection != null && (this.Intersection == null || isection.Distance < this.Intersection.Distance);

        // Updates the vector in-place
        public void Update(Vector3 pos, Vector3 ori)
        {
            this.Position = pos;
            this.Orientation = ori;
        }

    }


    // Instantiating class for an intersection, holding information about
    // its position, distance, object-of-intersection, secondary- and shadowrays,
    // and found color, if applicable.
    public class Intersection : Positionable
    {

        public float Distance;
        public Primitive Intersectee;

        public Ray[] ShadowRays;
        public Ray SecondaryRay;

        public int Color = 0x000000;


        public Intersection(Vector3 pos, float isectionDistance, Primitive intersectee) : base(pos)
        {
            this.Distance = isectionDistance;
            this.Intersectee = intersectee;
        }


        // Checks whether another intersection intersects the same object as this one.
        public bool IntersectsAtEqualObject(Intersection other)
            => (other != null && other.Intersectee == this.Intersectee);

        // Checks whether another intersection intersects at approx. the same distance as this one.
        public bool IntersectsAtEqualDistance(float dist, float error = 0.01f)
            => (this.Distance >= (dist - error));


        // Calculates the illuminated color of this intersection and stores it in-place.
        public void CalculateIlluminatedColor(Scene scene, Vector3 rayPos, int maxReflections)
        {
            if (!this.Intersectee.IsMirror)
                this.CalculateDiffuseColor(scene, rayPos);
            else if (0 < maxReflections)
                this.CalculateReflectiveColor(scene, rayPos, maxReflections);
        }

        // Calculates the color of this intersection; assumes the intersectee is a diffuse object.
        private void CalculateDiffuseColor(Scene scene, Vector3 rayPos)
        {
            // Set the ambient (lower bound) color of the scene and make array of shadow rays.
            Vector3 c = scene.AmbientLightAt(this.Intersectee, this.Position);
            this.ShadowRays = new Ray[scene.Lights.Length];

            // Go through all the lights in the scene, make a shadow-ray for each and intersect
            // it with the scene. Calculate and add the diffuse-color of the intersection if the shadow-ray
            // was found to intersect the same objectat approx. the same distance as the primary ray. 
            int i = 0;
            foreach (Light light in scene.Lights)
            {
                Ray shadowRay = this.ShadowRays[i] = light.CreateIntersectShadowray(scene, this);
                if (shadowRay == null
                    || !this.IntersectsAtEqualObject(shadowRay.Intersection)
                    || !shadowRay.Intersection.IntersectsAtEqualDistance(light.DistanceTo(this.Position)))
                    continue;
                c += light.CalculateDiffuseColorAt(this, shadowRay, rayPos);
            }

            // Convert the color to int-format, after limiting the r,g,b values between 0f < val < 1f.
            this.Color = Vec3Color.ToIntRGB(new Vector3(
                MathHelpers.FloatLimitBetween(c.X, 0f, 1f),
                MathHelpers.FloatLimitBetween(c.Y, 0f, 1f),
                MathHelpers.FloatLimitBetween(c.Z, 0f, 1f)
            ));
        }

        // Calculates the color of this intersection; assumes the intersectee is a reflective object.
        private void CalculateReflectiveColor(Scene scene, Vector3 ray_pos, int maxReflections)
        {
            // Calculate the reflection-direction by orientation from intersection 
            // to ray-position and outward-normal at the intersection.
            Vector3 V = -this.OrientationTo(ray_pos),
                N = this.Intersectee.OutwardNormalAt(this.Position);
            Vector3 reflectionDirection = V - 2f * Vector3.Dot(V, N) * N;

            // Create a secondary ray, intersect it with the scene, and calculate its intersection-color when applicable
            this.SecondaryRay = new Ray(this.Position, reflectionDirection);
            scene.IntersectWithRay(this.SecondaryRay);
            if (this.SecondaryRay.Intersection != null)
            {
                this.SecondaryRay.Intersection.CalculateIlluminatedColor(scene, this.SecondaryRay.Position, --maxReflections);

                // The final color of this intersection is determined by the color of its object-of-intersection,
                // multiplied by the reflective color from the intersection of the secondary ray.
                Vector3 isectiveColor = this.Intersectee.GetColorAt(this.Position),
                    reflectiveColor = Vec3Color.FromIntRGB(this.SecondaryRay.Intersection.Color);
                this.Color = Vec3Color.ToIntRGB(isectiveColor * reflectiveColor);
            }
        }

    }

}
