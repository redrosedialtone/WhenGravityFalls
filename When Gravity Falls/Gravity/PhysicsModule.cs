using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nebula.Base;
using Nebula.Main;
using Nebula.Systems;
using System;
using System.Collections.Generic;
using System.Net;

namespace Gravity
{
    public class PhysicsModule : Manager
    {
        #region Static
        private static PhysicsModule instance;
        public static PhysicsModule Get => instance;

        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("PHYSICS");
        #endregion
        public List<ICollision> Bodies;
        public List<PhysicsBody> Actors;

        private CameraViewportCollider _temp;

        public PhysicsModule()
        {
            instance = this;
            Bodies = new List<ICollision>();
            Actors = new List<PhysicsBody>();
        }

        public override void Init()
        {
            log.Info("> Physics Init..");
            _temp = new CameraViewportCollider();


            base.Init();
        }

        public override void Tick()
        {
            base.Tick();
            CheckActors();
        }

        private void CheckActors()
        {
            if (Actors == null || Actors.Count == 0) return;
            foreach (var actor in Actors)
            {
                if (actor.Active)
                {
                    Rectangle nextFrame = actor.NextFrame();
                    ICollision[] collisions = IntersectsWith(actor.Collision, nextFrame);

                    if (collisions != null && collisions.Length > 0)
                    {

                        Collision(actor, collisions[0]);
                    }
                }
            }
        }

        private void Collision(PhysicsBody Actor, ICollision Body)
        {
            log.Info("Collision Detected!!");
            if (Body.Static)
            {
                StaticCollision(Actor, Body);
            }
            else
            {

            }
        }

        private void StaticCollision(PhysicsBody Actor, ICollision StaticBody)
        {
            Vector2 reflection = ReflectionAngle(Actor, StaticBody);
            Actor.Velocity = reflection;
        }

        private Vector2 ReflectionAngle(PhysicsBody movable, ICollision immovable)
        {

            Vector2 velocity = movable.Velocity;

            Vector2 TL = new Vector2(immovable.Bounds.Left, immovable.Bounds.Top);
            Vector2 BR = new Vector2(immovable.Bounds.Right, immovable.Bounds.Bottom);
            Vector2 line = TL - BR;

            Vector2 normal = new Vector2(line.Y, -line.X);
            normal.Normalize();

            Vector2 richochet = velocity - 2 * Vector2.Dot(velocity, normal) * normal;

            float criticalAngle = MathHelper.ToDegrees((float)(Math.PI - Math.Atan2(richochet.Y - normal.Y, richochet.X - normal.X)));
            float sharpness = 0.5f + ((0.1f + (criticalAngle - 90.0f)) / 90.0f) / 2;
            log.Info($"Collision at angle of {criticalAngle} / {sharpness}");

            return richochet / movable.Mass;
        }

        private ICollision[] IntersectsWith(ICollision source, Rectangle cast)
        {
            List<ICollision> results = new List<ICollision>();
            foreach (var body in Bodies)
            {
                if (body.Active && body != source)
                {
                    if (body.CollidesWith(source, cast))
                    {
                        results.Add(body);
                    }      
                }
            }
            return results.ToArray();
        }

        public static void AddCollisionBody(ICollision body)
        {
            instance.Instance_AddCollisionBody(body);
        }

        private void Instance_AddCollisionBody(ICollision body)
        {
            log.Trace("Adding Physics Body...");
            Bodies.Add(body);
        }

        public static void AddActor(PhysicsBody body)
        {
            instance.Instance_AddActor(body);
        }

        private void Instance_AddActor(PhysicsBody body)
        {
            log.Trace("Adding Physics Actor...");
            Actors.Add(body);
        }
    }

    public class CameraViewportCollider
    {
        public StaticCollisionBody LeftWall;
        public StaticCollisionBody RightWall;
        public StaticCollisionBody TopWall;
        public StaticCollisionBody BottomWall;

        private ColliderGizmo LeftCollider;
        private ColliderGizmo RightCollider;
        private ColliderGizmo TopCollider;
        private ColliderGizmo BottomCollider;

        public CameraViewportCollider()
        {
            Rectangle bounds = Camera.Get.Bounds;
            Vector2 TL = Camera.ScreenToWorld(new Vector2(bounds.Left + 5, bounds.Top + 5));
            Vector2 TR = Camera.ScreenToWorld(new Vector2(bounds.Right - 5, bounds.Top + 5));
            Vector2 BR = Camera.ScreenToWorld(new Vector2(bounds.Right - 5, bounds.Bottom - 5));
            Vector2 BL = Camera.ScreenToWorld(new Vector2(bounds.Left + 5, bounds.Bottom - 5));

            TopWall = new StaticCollisionBody();
            TopWall.SetBounds(TL, (int)(TR.X - TL.X), 1);

            RightWall = new StaticCollisionBody();
            RightWall.SetBounds(TR, 1, (int)(BR.Y - TR.Y));

            BottomWall = new StaticCollisionBody();
            BottomWall.SetBounds(BL, (int)(BR.X - BL.X), 1);

            LeftWall = new StaticCollisionBody();
            LeftWall.SetBounds(TL, 1, (int)(BL.Y - TL.Y));

            LeftCollider = new ColliderGizmo(LeftWall);
            LeftCollider.SetDrawGizmo(true);

            RightCollider = new ColliderGizmo(RightWall);
            RightCollider.SetDrawGizmo(true);

            TopCollider = new ColliderGizmo(TopWall);
            TopCollider.SetDrawGizmo(true);

            BottomCollider = new ColliderGizmo(BottomWall);
            BottomCollider.SetDrawGizmo(true);
        }
    }

    public class ActorGizmo : IDrawGizmos
    {
        public bool DrawGizmo { get; private set; }
        private PhysicsBody Body;

        public ActorGizmo(PhysicsBody Collider)
        {
            Graphics.AddGizmo(this);
            Body = Collider;
        }

        public void DrawGizmos(SpriteBatch Batch)
        {
            if (DrawGizmo)
            {
                Rectangle nextFrame = Body.NextFrame();

                Vector2 TL = Camera.WorldToScreen(new Vector2(nextFrame.Left, nextFrame.Top));
                Vector2 TR = Camera.WorldToScreen(new Vector2(nextFrame.Right, nextFrame.Top));
                Vector2 BR = Camera.WorldToScreen(new Vector2(nextFrame.Right, nextFrame.Bottom));
                Vector2 BL = Camera.WorldToScreen(new Vector2(nextFrame.Left, nextFrame.Bottom));

                Graphics.DrawLine(Batch, TL, TR, Color.Magenta);
                Graphics.DrawLine(Batch, TR, BR, Color.Magenta);
                Graphics.DrawLine(Batch, BR, BL, Color.Magenta);
                Graphics.DrawLine(Batch, BL, TL, Color.Magenta);
            }
        }

        public void SetDrawGizmo(bool drawGizmo)
        {
            DrawGizmo = drawGizmo;
        }
    }

    public class ColliderGizmo : IDrawGizmos
    {
        public bool DrawGizmo { get; private set; }
        private ICollision Body;

        public ColliderGizmo(ICollision Collider)
        {
            Graphics.AddGizmo(this);
            Body = Collider;
        }

        public void DrawGizmos(SpriteBatch Batch)
        {
            if (DrawGizmo)
            {
                Rectangle nextFrame = Body.Bounds;

                Vector2 TL = Camera.WorldToScreen(new Vector2(nextFrame.Left, nextFrame.Top));
                Vector2 TR = Camera.WorldToScreen(new Vector2(nextFrame.Right, nextFrame.Top));
                Vector2 BR = Camera.WorldToScreen(new Vector2(nextFrame.Right, nextFrame.Bottom));
                Vector2 BL = Camera.WorldToScreen(new Vector2(nextFrame.Left, nextFrame.Bottom));

                Graphics.DrawLine(Batch, TL, TR, Color.Magenta);
                Graphics.DrawLine(Batch, TR, BR, Color.Magenta);
                Graphics.DrawLine(Batch, BR, BL, Color.Magenta);
                Graphics.DrawLine(Batch, BL, TL, Color.Magenta);
            }
        }

        public void SetDrawGizmo(bool drawGizmo)
        {
            DrawGizmo = drawGizmo;
        }
    }
}
