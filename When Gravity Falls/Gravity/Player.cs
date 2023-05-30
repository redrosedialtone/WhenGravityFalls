using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nebula;
using Nebula.Base;
using Nebula.Main;
using Nebula.Systems;
using System;

namespace Gravity
{
    public class Player : Manager, IDefaultCtxt, IDrawGizmos
    {
        public Vector2 Position { get => Body.Position; set => Body.Position = value; }
        public float Rotation { get => Body.Rotation; set => Body.Rotation = value; }
        public Rectangle PlayerBounds { get => Body.Collision.Bounds; }

        private TextureData PlayerTexture { get; set; }
        private SpriteFont _spriteFont;

        private PhysicsBody Body;

        private ActorGizmo PlayerColliderGizmo;

        public override void Init()
        {
            var _texture = Resources.Load<Texture2D>("Entities/Textures/Player");
            PlayerTexture = new TextureData(_texture);
            PlayerTexture.SetOrigin(new Vector2(_texture.Width/2, _texture.Height/2));
            Body = new PhysicsBody();
            Position = Vector2.Zero;
            PlayerTexture.SetPosition(Position);

            Input.DefaultCtxt.OnMovement += OnMovementAxis;
            Input.DefaultCtxt.OnScroll += OnZoom;

            Body.SetBounds(new Vector2(-(_texture.Width-256)/2,-_texture.Height/2), 256, 512);
            _spriteFont = Resources.Load<SpriteFont>("FONT/Constantina");

            Graphics.AddGizmo(this);
            SetDrawGizmo(true);

            PlayerColliderGizmo = new ActorGizmo(Body);
            PlayerColliderGizmo.SetDrawGizmo(true);

            base.Init();
        }



        public override void Tick()
        {
            base.Tick();

            ApplyVelocity();

            UpdatePlayer();
            Graphics.DrawTexture(PlayerTexture);
        }

        private void ApplyVelocity()
        {
            Position = Position + Body.CurrentVelocity();
            Rotation = Rotation + Body.AngularVelocity;
        }

        private void UpdatePlayer()
        {
            Body.Position = Position;
            PlayerTexture.SetPosition(Position);
            PlayerTexture.SetRotation(Rotation);

        }

        #region Input


        public void OnMovementAxis(Vector2 movementAxis)
        {
            Body.ApplyForce(movementAxis);
        }

        public void OnZoom(float zoomDelta)
        {

        }


        public void OnLock(bool locked)
        {

        }

        #endregion

        #region Gizmos

        public bool DrawGizmo { get; private set; }

        public void SetDrawGizmo(bool drawGizmo)
        {
            DrawGizmo = drawGizmo;
        }

        public void DrawGizmos(SpriteBatch Batch)
        {
            if (DrawGizmo)
            {
                /*Vector2 TL = Camera.WorldToScreen(new Vector2(PlayerBounds.Left, PlayerBounds.Top));
                Vector2 TR = Camera.WorldToScreen(new Vector2(PlayerBounds.Right, PlayerBounds.Top));
                Vector2 BR = Camera.WorldToScreen(new Vector2(PlayerBounds.Right, PlayerBounds.Bottom));
                Vector2 BL = Camera.WorldToScreen(new Vector2(PlayerBounds.Left, PlayerBounds.Bottom));

                Graphics.DrawLine(Batch, TL, TR, Color.Magenta);
                Graphics.DrawLine(Batch, TR, BR, Color.Magenta);
                Graphics.DrawLine(Batch, BR, BL, Color.Magenta);
                Graphics.DrawLine(Batch, BL, TL, Color.Magenta);*/

                var fps = string.Format("Jerry");
                Batch.DrawString(_spriteFont, fps, Camera.WorldToScreen(new Vector2(Position.X+256,Position.Y + 50)), Color.Yellow);

                fps = string.Format("Player: ({0:0},{1:0})", this.Position.X, this.Position.Y);
                Batch.DrawString(_spriteFont, fps, new Vector2(1, 45), Color.Yellow);

                /*Rectangle nextFrame = Body.NextFrame();

                Vector2 TL = Camera.WorldToScreen(new Vector2(nextFrame.Left, nextFrame.Top));
                Vector2 TR = Camera.WorldToScreen(new Vector2(nextFrame.Right, nextFrame.Top));
                Vector2 BR = Camera.WorldToScreen(new Vector2(nextFrame.Right, nextFrame.Bottom));
                Vector2 BL = Camera.WorldToScreen(new Vector2(nextFrame.Left, nextFrame.Bottom));
                Graphics.DrawLine(Batch, TL, TR, Color.Magenta);
                Graphics.DrawLine(Batch, TR, BR, Color.Magenta);
                Graphics.DrawLine(Batch, BR, BL, Color.Magenta);
                Graphics.DrawLine(Batch, BL, TL, Color.Magenta);*/
            }          
        }

        #endregion

    }
}
