using System;
using OpenTK;

namespace RayTracer_JP
{

    // Class for the camera of the raytracer, which handles the viewing-rays and projection
    // and its orientation for the rendering of the cam's current perspective.
    public class Camera : Orientable
    {

        public ProjectionSurface Projection;
        public Ray[,] ViewRays;

        public Vector3 Orientation2, Orientation3;
        public bool HasChanged = false;

        private int maxReflectionBounces = 25;
        private float theta, phi,
            fov,
            projectionDistance,
            movementSpeed = 2.5f,
            rotationSpeed = (float)Math.PI / 720f;


        public Camera(Vector3 pos, Vector3 ori, Vector3 ori2, float projectionDistance, int projectionWidth, int projectionHeight) : base(pos, ori)
        {
            this.Orientation2 = ori2;
            this.Orientation3 = Vector3.Cross(this.Orientation, ori2);
            this.Projection = new ProjectionSurface(this, distance: projectionDistance, width: projectionWidth, height: projectionHeight);
            this.theta = MathHelpers.CarthToPolarTheta(ori);
            this.phi = MathHelpers.CarthToPolarPhi(ori);
            this.projectionDistance = projectionDistance;
            this.fov = this.DistanceToFOV(projectionDistance);
        }


        //
        // Public methods for transversally moving the camera- (and projection)'s position.
        //
        public void MoveLeftBy(float dt)
            => this.TranslocateBy(-this.movementSpeed * dt * this.Orientation3);

        public void MoveRightBy(float dt)
            => this.TranslocateBy(this.movementSpeed * dt * this.Orientation3);

        public void MoveForwardBy(float dt)
            => this.TranslocateBy(this.movementSpeed * dt * this.Orientation);

        public void MoveBackwardBy(float dt)
            => this.TranslocateBy(this.movementSpeed * dt * this.Orientation);


        //
        // Public methods for rotating the camera- (and projection)'s orientation.
        //
        public void RotateAroundZAxis(float dPhi)
            => this.changePerspectiveOrientation(this.theta, this.phi + (this.rotationSpeed * dPhi));

        public void RotateAroundXAxis(float dTheta)
            => this.changePerspectiveOrientation(this.theta + (this.rotationSpeed * dTheta), this.phi);


        //
        // Public method for zooming in by changing the projection's distance to the camera.
        //
        public void Zoom(float dA)
            => this.ChangeFOV(MathHelper.RadiansToDegrees(this.fov) + dA);


        // Changes the field-of-view (within 0<a<180 degrees bounds) of the projection
        // by altering to the corresponding projection-distance, and propagating this to the projection
        public void ChangeFOV(float a)
        {
            if (0 < a && a < 179.99f)
            {
                this.fov = MathHelper.DegreesToRadians(a);
                this.projectionDistance = this.FOVToDistance(this.fov);
                this.Projection.UpdateProjection(this, this.projectionDistance);
                this.HasChanged = true;
            }
        }

        //
        // Helper-methods for computing the field-of-view by projection distance, and vice versa
        //
        public float FOVToDistance(float rad)
            => this.Projection.RightDirection.Length / (2f * (float)Math.Tan(rad / 2f));

        public float DistanceToFOV(float dist)
            => 2f * (float)Math.Atan(this.Projection.RightDirection.Length / (2f * dist));


        //
        // Overrides translocationg and rotation of 'Orientable' base-class,
        // which will propagate to the projection-surface and indicate camera change (follow-up: regenerate rays).
        //
        public override void TranslocateBy(Vector3 direction)
        {
            base.TranslocateBy(direction);
            this.Projection.TranslocateBy(direction);
            this.HasChanged = true;
        }

        public override void OrientateTo(Vector3 p)
        {
            Vector3 newOri = this.OrientationTo(p);
            float newTheta = MathHelpers.CarthToPolarTheta(newOri),
                newPhi = MathHelpers.CarthToPolarPhi(newOri);
            this.changePerspectiveOrientation(newTheta, newPhi);

        }

        // Changes the normal of the camera given polar coordinates of the new
        // forward-vector (this.orientation), assuming the radius stays 1.
        private void changePerspectiveOrientation(float newTheta, float newPhi)
        {
            // Calculate the theta-change an theta and phi of current upward normal-vector.
            float dTheta = newTheta - this.theta;
            float ori2Theta = MathHelpers.CarthToPolarTheta(this.Orientation2),
                ori2Phi = MathHelpers.CarthToPolarPhi(this.Orientation2);

            // Set the forward-vector to the new theta and phi coordinates,
            // the upward-vector theta modified by the change in theta
            // and the right vector as the cross-product between those two.
            this.Orientation = MathHelpers.PolarToCarth(1f, newTheta, newPhi).Normalized();
            this.Orientation2 = MathHelpers.PolarToCarth(1f, ori2Theta + dTheta, ori2Phi).Normalized();
            this.Orientation3 = Vector3.Cross(this.Orientation, this.Orientation2).Normalized();

            this.theta = newTheta;
            this.phi = newPhi;

            // Propagate the update to the projection-screen and indicate camera-change
            this.Projection.UpdateProjection(this, this.projectionDistance);
            this.HasChanged = true;
        }



        // Generates rays from the camera and intersects the scene with them,
        // given the current specifics of the projection-screen
        public void GenerateRays(Scene scene)
        {
            // Make the 2D ray-array ("the array of rayed ray arrays") 
            int w = this.Projection.Width, h = this.Projection.Height;
            this.ViewRays = new Ray[h, w];

            // Iterate through all pixel-points on the projection-screen, starting at p0
            Vector3 p_ij, p0 = this.Projection.GetProjectionStartPoint();
            for (int i = 0; i < h; i++) 
            {
                for (int j = 0; j < w; j++) 
                {
                    // Caltulate pixel-point p_ij, create and a ray to it, intersect ray with scene and calculate its color when applicable
                    p_ij = p0 + this.Projection.GetProjectionOffsetPoint((float)i, (float)j);
                    this.ViewRays[i, j] = this.createRayTo(p_ij);
                    scene.IntersectWithRay(this.ViewRays[i, j]);
                    if (this.ViewRays[i, j].Intersection != null)
                        this.ViewRays[i, j].Intersection.CalculateIlluminatedColor(scene, this.ViewRays[i, j].Position, this.maxReflectionBounces);
                }
            }
        }

        // Re-generates rays from the camera and intersects the scene with them,
        // given the current specifics of the projection-screen.
        // Differs from 'GenerateRays' by updating rays in-place whenever the 
        // dimensions of the projection-surface haven't changed
        public void RegenerateRays(Scene scene)
        {
            // Check whether projection-screen dimensions have changed; if so - do generate rays normally
            int h = this.ViewRays.GetUpperBound(0), w = this.ViewRays.GetUpperBound(1);
            if (h != this.Projection.Height|| w != this.Projection.Width) 
            {
                this.GenerateRays(scene);
                return;
            }

            // Iterate through all pixel-points on the projection-screen, starting at p0
            Vector3 p_ij, p0 = this.Projection.GetProjectionStartPoint();
            for (int i = 0; i < h; i++) 
            {
                for (int j = 0; j < w; j++) 
                {
                    // Caltulate pixel-point p_ij, create and a ray to it, intersect ray with scene and calculate its color when applicable
                    p_ij = p0 + this.Projection.GetProjectionOffsetPoint((float)i, (float)j);
                    this.ViewRays[i, j].Update(this.Position, this.OrientationTo(p_ij));
                    scene.IntersectWithRay(this.ViewRays[i, j]);
                    if (this.ViewRays[i, j].Intersection != null)
                        this.ViewRays[i, j].Intersection.CalculateIlluminatedColor(scene, this.ViewRays[i, j].Position, this.maxReflectionBounces);
                }
            }
        }

        // Helper-method for creating a ray from the camera to a certain point p
        private Ray createRayTo(Vector3 p)
            => new Ray(this.Position, this.OrientationTo(p));

    }



    // Class for holding the specifics of the projection surface, onto which the camera shoots its rays
    public class ProjectionSurface : Orientable
    {

        public int Width, Height;
        public Vector3 LeftUpperPosition, 
            DownDirection, RightDirection;


        public ProjectionSurface(Camera cam, float distance, int width, int height) : base(cam.ExtendedBy(distance), cam.Orientation)
        {
            this.Width = width;
            this.Height = height;
            this.UpdateProjection(cam, projectionDistance: distance);
        }

        // Overrides 'Orientable' translocation by also modifying the position of the left-upper corner
        public override void TranslocateBy(Vector3 direction)
        {
            base.TranslocateBy(direction);
            this.LeftUpperPosition += direction;
        }


        // Updates the projection-screen given the relevant camera and projection-distance
        public void UpdateProjection(Camera cam, float projectionDistance)
        {
            float a = this.Width / this.Height;
            this.Position = cam.Position + (projectionDistance * cam.Orientation);
            this.LeftUpperPosition = this.Position + cam.Orientation2 - (a * cam.Orientation3);
            this.RightDirection = (this.Position + cam.Orientation2 + (a * cam.Orientation3)) - this.LeftUpperPosition;
            this.DownDirection = (this.Position - cam.Orientation2 - (a * cam.Orientation3)) - this.LeftUpperPosition;
        }


        //
        // Helper methods for getting the starting pixel-point, and further i,j offsets, on the projection-screen
        //
        public Vector3 GetProjectionStartPoint()
            => this.LeftUpperPosition + (this.DownDirection / (2f * this.Height)) + (this.RightDirection / (2f * this.Width));

        public Vector3 GetProjectionOffsetPoint(float i, float j)
            => this.DownDirection * (i / this.Height) + this.RightDirection * (j / this.Width);

    }

}
