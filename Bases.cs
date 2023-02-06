using OpenTK;

namespace RayTracer_JP
{

    // Abstact base class for a positionable; an object that can be positioned and moved.
    abstract public class Positionable
    {

        public Vector3 Position;
        

        public Positionable(Vector3 position)
        {
            this.Position = position;
        }


        //
        // Helper methods for calculations relative to the positionable's position
        //
        public Vector3 DirectionTo(Vector3 p)
            => (p - this.Position);

        public float DistanceTo(Vector3 p)
            => this.DirectionTo(p).Length;

        public Vector3 OrientationTo(Vector3 p)
            => this.DirectionTo(p).Normalized();

        public Vector3 TranslocatedBy(Vector3 direction)
            => (this.Position + direction);


        // Method to translocate a positionable in-place
        public virtual void TranslocateBy(Vector3 direction)
            => this.Position = this.TranslocatedBy(direction);

    }


    // Abstract base class for an orientable; a positionable with at least a forward-orientation
    // that can be modified
    abstract public class Orientable : Positionable
    {

        public Vector3 Orientation;


        public Orientable(Vector3 position, Vector3 orientation) : base(position)
        {
            this.Orientation = orientation;
        }


        // Helper method to return the extension of the orientables position
        // by a certain distance towards its forward-orientation
        public virtual Vector3 ExtendedBy(float distance)
            => this.TranslocatedBy(distance * this.Orientation);


        //
        // Methods to modify the orientation or position of this orientable in-place
        //
        public virtual void OrientateTo(Vector3 p)
            => this.Orientation = this.OrientationTo(p);

        public virtual void ExtendBy(float distance)
            => this.Position = this.ExtendedBy(distance);

    }

}
