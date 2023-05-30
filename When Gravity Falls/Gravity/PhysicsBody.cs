using Microsoft.Xna.Framework;
using System;

namespace Gravity
{
	public class PhysicsBody
	{
        public Vector2 Position { get { return _position; } set { _position = value; Recalculate(); } }
        public Vector2 Velocity { get; set; }
        public float AngularVelocity { get; set; }
        public float Rotation { get; set; }
        public bool Active { get; private set; }
        public CollisionActor Collision => _collision;
        public float Mass { get; set; } = 5.0f;
        public bool CollideWithScreen { get; set; } = false;

        private CollisionActor _collision;
        private Vector2 _position;

        public PhysicsBody()
        {
            _collision = new CollisionActor();
            PhysicsModule.AddActor(this);
        }

        public void ApplyForce(Vector2 Force)
        {
            Velocity += Force / Mass;
            if (Velocity == Vector2.Zero) Active = false; else Active = true;
        }

        public void Tick()
        {

        }

        public void SetBounds(Vector2 Origin, int Width, int Height)
        {
            Collision.SetBounds(Origin, Width, Height);
        }

        public void Recalculate()
        {
            Collision.Position = Position;
            if (Velocity == Vector2.Zero) Active = false; else Active = true;
        }

        public Vector2 CurrentVelocity()
        {
            return Velocity;
        }

        public Rectangle NextFrame()
        {
            return Collision.NextFrame(Velocity);
        }

        public Rectangle FutureFrame(int Frames)
        {
            return Collision.FutureFrame(Velocity, Frames);
        }

        public bool CollidesWith(Rectangle rect) => Collision.Intersect(rect);
        public bool CollidesWith(PhysicsBody body) => Collision.Intersect(body.Collision, body.Collision.Bounds);
    }

    public interface ICollision
    {
        Vector2 Position { get; }
        Rectangle Bounds { get; }
        bool DirtyCollider { get; }
        bool Static { get; }
        bool Active { get; }

        Rectangle NextFrame(Vector2 Velocity);
        Rectangle FutureFrame(Vector2 Maximum, int Frames);

        bool CollidesWith(ICollision Body, Rectangle Cast);
        bool CollidesWith(Rectangle rect);

        bool Intersect(ICollision Body, Rectangle Cast);
        bool Contains(ICollision Body, Rectangle Cast);

        bool Intersect(Rectangle rect);
        bool Contains(Rectangle rect);
    }

    public abstract class BaseCollisionBody : ICollision
    {
        public Vector2 Position { get { return _position; } set { _position = value; Recalculate(); } }
        public Vector2 Origin { get; private set; }
        public bool DirtyCollider { get; private set; }
        public bool Active { get; private set; } = true;

        public float Width { get; private set; }
        public float Height { get; private set; }

        protected Vector2 _position;
        protected Rectangle _bounds;
        protected Rectangle _futureFrameBounds;
        protected bool _boundsDirty;

        public abstract bool Static { get; }

        public Rectangle Bounds
        {
            get
            {
                if (_boundsDirty)
                {
                    Vector2 pos = Position + Origin;
                    _bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)Width, (int)Height);
                }
                return _bounds;
            }
        }

        public BaseCollisionBody()
        {
            PhysicsModule.AddCollisionBody(this);
        }

        public void SetBounds(Vector2 Origin, int Width, int Height)
        {
            this.Origin = Origin; this.Width = Width; this.Height = Height;
            Recalculate();
        }

        public void Recalculate()
        {
            DirtyCollider = true;
            _boundsDirty = true;
        }

        public virtual Rectangle NextFrame(Vector2 Force)
        {
            Rectangle nextFrameBounds = Bounds;

            nextFrameBounds.X += (int)Force.X;
            nextFrameBounds.Y += (int)Force.Y;

            return nextFrameBounds;
        }

        public virtual Rectangle FutureFrame(Vector2 Maximum, int Frames = 1)
        {
            if (DirtyCollider)
            {
                _futureFrameBounds = Bounds;
                _futureFrameBounds.Width += (int)Maximum.X * Frames;
                _futureFrameBounds.Height += (int)Maximum.Y * Frames;
            }
            return _futureFrameBounds;
        }

        public bool CollidesWith(ICollision Body, Rectangle Cast)
        {
            return Intersect(Body, Cast);
        }

        public bool CollidesWith(Rectangle rect)
        {
            return Intersect(rect);
        }

        public bool Intersect(ICollision Body, Rectangle Cast) => Bounds.Intersects(Cast);
        public bool Contains(ICollision Body, Rectangle Cast) => Bounds.Contains(Cast);

        public bool Intersect(Rectangle rect) => Bounds.Intersects(rect);
        public bool Contains(Rectangle rect) => rect.Contains(Bounds);
    }

    public class StaticCollisionBody : BaseCollisionBody
    {
        public override bool Static => true;

        public override Rectangle NextFrame(Vector2 Force)
        {
            return Bounds;
        }

        public override Rectangle FutureFrame(Vector2 Maximum, int Frames = 1)
        {
            return Bounds;
        }
    }

    public class CollisionActor : BaseCollisionBody
    {
        public override bool Static => false;
    }
}
