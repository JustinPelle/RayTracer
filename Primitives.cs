using System;
using OpenTK;

namespace RayTracer_JP
{

    // Primitive ABC (abstract base class) which is an orientable with a color and indication of whether
    // it is a mirror-reflected primitive. Enforces method implementation of ray-intersection, getting
    // the outward-normal at a certain position, and color at certain position.
    abstract public class Primitive : Orientable
    {
        public Vector3 Color;
        public bool IsMirror;

        public Primitive(Vector3 pos, Vector3 ori, Vector3 c, bool isMirror) : base(pos, ori)
        {
            this.Color = c;
            this.IsMirror = isMirror;
        }

        abstract public Intersection IntersectWithRay(Ray ray);

        abstract public Vector3 OutwardNormalAt(Vector3 pos);

        abstract public Vector3 GetColorAt(Vector3 pos);

    }


    // Class for the instantiating sphere-primitives. Can be mirror-reflected, but does not implement any textures.
    public class Sphere : Primitive
    {
        public float Radius;


        public Sphere(Vector3 pos, Vector3 ori, float r, Vector3 c, bool isMirror) : base(pos, ori, c, isMirror)
        {
            this.Radius = r;
        }


        // Checks whether this primitive intersects with a ray, and returns an intersection if so.
        // Algorithm reference: from Lecture 5. ray-tracing.
        public override Intersection IntersectWithRay(Ray ray)
        {
            Vector3 centered = this.Position - ray.Position;
            float distance = Vector3.Dot(centered, ray.Orientation);
            float transDistance2 = (centered - (distance * ray.Orientation)).LengthSquared,
                r2 = this.Radius * this.Radius;
            if (transDistance2 > r2) return null;
            distance -= (float)(Math.Sqrt(r2 - transDistance2));
            if (distance < 0) return null;
            return new Intersection(ray.ExtendedBy(distance), distance, this);
        }

        // Gets the outward-normal at a certain position of this primitive (assumed to be a point of intersection).
        public override Vector3 OutwardNormalAt(Vector3 pos)
            => this.OrientationTo(pos);

        // Gets the color of this primitive at certain position of this primitive (assumed to be a point of intersection).
        public override Vector3 GetColorAt(Vector3 pos)
            => this.Color;

    }


    // Class for the instantiating sphere-primitives. Can be mirror-reflected
    // and supports the option of having a checker-board texture.
    class Plane : Primitive
    {

        public bool HasCheckersTexture;


        public Plane(Vector3 pos, Vector3 ori, Vector3 c, bool isMirror, bool hasCheckersTexture) : base(pos, ori, c, isMirror) 
        {
            this.HasCheckersTexture = hasCheckersTexture;
        }


        // Checks whether this primitive intersects with a ray, and returns an intersection if so.
        public override Intersection IntersectWithRay(Ray ray)
        {
            float upperTerm = Vector3.Dot((this.Position - ray.Position), this.Orientation),
                lowerTerm = Vector3.Dot(ray.Orientation, this.Orientation);
            if (lowerTerm == 0) return null;
            float dist = upperTerm / lowerTerm;
            if (dist < 0) return null;
            return new Intersection(ray.ExtendedBy(dist), dist, this);
        }


        // Gets the outward-normal at a certain position of this primitive (assumed to be a point of intersection).
        public override Vector3 OutwardNormalAt(Vector3 pos)
            => this.Orientation;


        // Gets the color of this primitive at certain position of this primitive (assumed to be a point of intersection).
        public override Vector3 GetColorAt(Vector3 pos)
            => this.HasCheckersTexture
                ? this.Color * this.checkersTextureColorAt(pos)
                : this.Color;

        // Gets the checkers-texture color of a certain point on the plane, relative to the direction of the plane's position.
        private Vector3 checkersTextureColorAt(Vector3 pos)
        {
            Vector3 direction = this.DirectionTo(pos),
                N = this.Orientation;
            
            // Alternative approach to calculating u and v for the texture
            // tries to correct for the fact that the textured plane is not bound to fully align with the x,y-plane.
            int u = (int)(Math.Sqrt(direction.X * direction.X + direction.Z * direction.Z) / (float)Math.Sqrt(N.X * N.X + N.Z * N.Z)),
                v = (int)(Math.Sqrt(direction.Y * direction.Y + direction.Z * direction.Z) / (float)Math.Sqrt(N.Y * N.Y + N.Z * N.Z));
            return this.Color * (float)((u + v) & 1);
        }

    }

}
