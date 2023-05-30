using Microsoft.Xna.Framework;
using System;

namespace Nebula.Main
{
	public interface IInputContext
	{

	}

	public abstract class InputContext : IInputContext
	{

	}

	public class DefaultCtxt : InputContext
	{
		public float CAMERA_SENSITIVTY = 750.0f;

		public Action<Vector2> OnMovement;
		public Action<float> OnScroll;
		public Action<bool> OnLock;
        private bool _movementUpdate = false;

		private bool Lock = false;

		public void ProcessActions(GameTime time)
		{
			bool _active = false;
			bool _oldLock = Lock;
			float mouseX = 0.0F;
			float mouseY = 0.0F;
			if (Input.Active(InputID.Right))
			{
				mouseX += 1.0f;
				_active = true;
			}

			if (Input.Active(InputID.Left))
			{
				mouseX -= 1.0f;
                _active = true;
            }

			if (Input.Active(InputID.Up))
			{
				mouseY -= 1.0f;
                _active = true;
            }

			if (Input.Active(InputID.Down))
			{
				mouseY += 1.0f;
                _active = true;
            }

			var _lockData = Input.Data(InputID.Lock);
			if (_lockData != null && _lockData is InputActionData _data)
			{
				if (_data.PressedThisFrame) Lock = !Lock;
			}

			if (_active) _movementUpdate = true;
			if (_movementUpdate)
			{
				bool _shift = Input.Active(InputID.Shift);
                Vector2 cameraMovement = new Vector2(mouseX, mouseY);
				if(cameraMovement != Vector2.Zero) cameraMovement.Normalize();
				cameraMovement = cameraMovement * Time.deltaTime * CAMERA_SENSITIVTY * (_shift ? CAMERA_SENSITIVTY * 2 : 1);

                OnMovement?.Invoke(cameraMovement);
            }
			if(!_active) _movementUpdate = false;
			if(_oldLock != Lock)
			{
				OnLock?.Invoke(Lock);
			}

			float scrollY = 0.0f;
			if(Input.Active(InputID.Scroll))
			{
				InputRangeData data = (InputRangeData)Input.Data(InputID.Scroll);
				scrollY += data.Current.State;
			}

			if (scrollY != 0)
			{
				scrollY = scrollY * Time.deltaTime * 0.02f;
				OnScroll?.Invoke(scrollY);
			}
			
		}
	}

	public interface IDefaultCtxt
	{
		public void OnMovementAxis(Vector2 movementAxis);
		public void OnZoom(float zoomDelta);
		public void OnLock(bool locked);
	}


}
