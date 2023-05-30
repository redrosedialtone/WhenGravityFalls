﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Nebula.Input;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.CodeAnalysis;

namespace Nebula.Main
{
    public enum InputID
    {
        LeftMouseButton = 0,
        RightMouseButton = 1,
        Left = 2,
        Right = 3,
        Up = 4,
        Down = 5,
        Scroll = 6,
        Shift = 7,
        Lock = 8
    }

    public class MouseButtonActionState
    {
        public PointerEventData buttonData;
        public ButtonState buttonState;
        public Point mousePosition;

        public bool PressedThisFrame()
        {
            return buttonState == ButtonState.Pressed;
        }

        public bool ReleasedThisFrame()
        {
            return buttonState == ButtonState.Released;
        }

        public bool Active() => PressedThisFrame();
    }

    public class PointerEventData
    {
        public IPointerDownHandler pressedEvent;
        public IPointerUpHandler releaseEvent;
        public IPointerClickHandler clickEvent;
        public IPointerEnterHandler enterEvent;
        public IPointerExitHandler exitEvent;

        public Point pressPosition;
        public bool elligibleForClick;
        public int clickCount;
        public float clickTime;
        public float delta;

    }

    public interface IInputData
    {
        public InputID ID { get; }
        public bool Active { get; }
    }

    public struct InputActionState
    {
        public InputID ID;
        public ButtonState State;

        public static bool operator ==(InputActionState a, InputActionState b) =>
            a.State == b.State;

        public static bool operator !=(InputActionState a, InputActionState b) =>
            a.State != b.State;

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if(obj != null && obj is InputActionState b)
            {
                return b.State == this.State;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }
    }

    public struct InputRangeState
    {
        public InputID ID;
        public float State;

        public static bool operator ==(InputRangeState a, InputRangeState b) =>
            a.State == b.State;

        public static bool operator !=(InputRangeState a, InputRangeState b) =>
            a.State != b.State;

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj != null && obj is InputRangeState b)
            {
                return b.State == this.State;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }
    }

    public class InputActionData : IInputData
    {
        public InputID ID { get; private set; }
        public InputActionState Current;
        public InputActionState Previous;

        public InputActionData(InputID ID)
        {
            this.ID = ID;
        }

        public bool Active => Pressed();
        public bool PressedThisFrame => Current.State == ButtonState.Pressed && Previous.State == ButtonState.Released;

        public bool Pressed()
        {
            return Current.State == ButtonState.Pressed;
        }

        public bool Released()
        {
            return Current.State == ButtonState.Released;
        }
    }

    public class InputRangeData : IInputData
    {
        public InputID ID { get; private set; }
        public InputRangeState Current;
        public InputRangeState Previous;

        public InputRangeData(InputID ID)
        {
            this.ID = ID;
        }

        public bool Active => true;
    }

    public class Input : IControl
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("INPUT");
        public static Input Access;

        private List<IPointerEventListener> PointerListeners;

        public MouseState PreviousMousePointerEventData;
        public MouseState MousePointerEventData;

        private MouseButtonActionState leftClickButtonData;
        private MouseButtonActionState rightClickButtonData;
        private MouseButtonActionState middleClickButtonData;

        private KeyboardInputMap kbInput;
        public static DefaultCtxt DefaultCtxt;

        public void Create(Runtime game)
        {
            Access = this;
            PointerListeners = new List<IPointerEventListener>();
        }

        public void Draw(GameTime gameTime)
        {

        }

        public void Initialise()
        {
            leftClickButtonData = new MouseButtonActionState();
            leftClickButtonData.buttonData = new PointerEventData();
            rightClickButtonData = new MouseButtonActionState();
            rightClickButtonData.buttonData = new PointerEventData();
            middleClickButtonData = new MouseButtonActionState();
            middleClickButtonData.buttonData = new PointerEventData();

            SetupInputs();

            kbInput = new KeyboardInputMap();

            DefaultCtxt = new DefaultCtxt();
        }

        public void LoadContent()
        {

        }

        public void UnloadContent()
        {

        }

        public void Update(GameTime gameTime)
        {
            ProcessMouseData();

            foreach (var input in kbInput.MapActions())
            {
                var data = Data(input.ID);
                if(data is InputActionData actionData)
                {
                    actionData.Previous = actionData.Current;
                    actionData.Current = input;

                    if (actionData.Current != actionData.Previous)
                    {
                        log.Debug($"{input.ID.ToString()}::{actionData.Current.State.ToString()}");
                    }
                } 
            }
            foreach (var range in kbInput.MapRanges())
            {
                var data = Data(range.ID);
                if (data is InputRangeData rangeData)
                {
                    rangeData.Previous = rangeData.Current;
                    rangeData.Current = range;

                    if (rangeData.Current != rangeData.Previous)
                    {
                        log.Debug($"{range.ID.ToString()}::{rangeData.Current.State.ToString()}");
                    }
                }
            }

            DefaultCtxt.ProcessActions(gameTime);
        }
        // Mouse Actions
        #region Mouse
        public static void AddPointerEventListener(IPointerEventListener Listener)
        {
            Access.PointerListeners.Add(Listener);
            log.Debug("Adding Listener.. " + Access.PointerListeners.Count);
        }

        private void ProcessMouseData()
        {
            PreviousMousePointerEventData = MousePointerEventData;
            MousePointerEventData = Mouse.GetState();

            leftClickButtonData.buttonState = MousePointerEventData.LeftButton;
            leftClickButtonData.mousePosition = MousePointerEventData.Position;
            rightClickButtonData.buttonState = MousePointerEventData.RightButton;
            rightClickButtonData.mousePosition = MousePointerEventData.Position;
            middleClickButtonData.buttonState = MousePointerEventData.MiddleButton;
            middleClickButtonData.mousePosition = MousePointerEventData.Position;

            List<IPointerEventListener> listenersIntersectingCursor = new List<IPointerEventListener>();
            foreach (var listener in PointerListeners)
            {
                var eventListener = listener.Intersect(MousePointerEventData.Position);
                if (eventListener != null)
                {
                    listenersIntersectingCursor.Add(eventListener);
                }
            }

            IPointerEventListener[] Events = listenersIntersectingCursor.ToArray();
            ProcessMouseButton(leftClickButtonData, Events);
            ProcessMouseButton(rightClickButtonData, Events);
            ProcessMouseButton(middleClickButtonData, Events);

            ProcessMouseOver(leftClickButtonData, Events);
        }

        private void ProcessMouseButton(MouseButtonActionState Data, IPointerEventListener[] Events)
        {
            PointerEventData pointerData = Data.buttonData;
            bool Pressed = Data.PressedThisFrame();
            bool Released = Data.ReleasedThisFrame();
            if (!Pressed && !Released)
            {
                return;
            }
            if (Pressed)
            {
                pointerData.elligibleForClick = true;
                pointerData.delta = 0;
                pointerData.pressPosition = Data.mousePosition;

                IPointerDownHandler pointerDownExecuted = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(Events, Data, ExecuteEvents.pointerDown);
                IPointerClickHandler clickEvent = ExecuteEvents.GetEventListener<IPointerClickHandler>(Events, Data, ExecuteEvents.pointerClick);


                float dT = Time.deltaTime;
                if (clickEvent != null && clickEvent == pointerData.clickEvent)
                {
                    float timeSinceLastClick = dT - pointerData.clickTime;
                    if (timeSinceLastClick < 0.3F)
                    {
                        pointerData.clickCount++;
                    }
                    else
                    {
                        pointerData.clickCount = 1;
                    }
                    pointerData.clickTime = dT;
                }
                else
                {
                    pointerData.clickCount = 0;
                }

                pointerData.pressedEvent = pointerDownExecuted;
                pointerData.clickEvent = clickEvent;

            }
            if (Released)
            {
                IPointerUpHandler pointerUpExecuted = ExecuteEvents.ExecuteHierarchy<IPointerUpHandler>(Events, Data, ExecuteEvents.pointerUp);
                IPointerClickHandler clickEvent = ExecuteEvents.GetEventListener<IPointerClickHandler>(Events, Data, ExecuteEvents.pointerClick);

                if (pointerData.elligibleForClick && clickEvent == pointerData.clickEvent)
                {
                    ExecuteEvents.ExecuteHierarchy<IPointerClickHandler>(Events, Data, ExecuteEvents.pointerClick);
                }
                else
                {
                    pointerData.clickEvent = null;
                }

                pointerData.elligibleForClick = false;
                pointerData.pressedEvent = null;
                pointerData.releaseEvent = pointerUpExecuted;
            }
        }

        private void ProcessMouseOver(MouseButtonActionState Data, IPointerEventListener[] Events)
        {
            PointerEventData pointerData = Data.buttonData;
            IPointerEnterHandler enterEvent = ExecuteEvents.GetEventListener<IPointerEnterHandler>(Events, Data, ExecuteEvents.EventHandle.PointerEnter);
            IPointerExitHandler exitEvent = ExecuteEvents.GetEventListener<IPointerExitHandler>(Events, Data, ExecuteEvents.EventHandle.PointerExit);

            if (pointerData.enterEvent != enterEvent)
            {
                if (pointerData.exitEvent != null)
                {
                    ExecuteEvents.ExecuteEvent(pointerData.exitEvent, Data, ExecuteEvents.EventHandle.PointerExit);
                }
                ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(Events, Data, ExecuteEvents.EventHandle.PointerExit);
                if (enterEvent != null)
                {
                    ExecuteEvents.ExecuteHierarchy<IPointerEnterHandler>(Events, Data, ExecuteEvents.EventHandle.PointerEnter);
                }
            }

            pointerData.enterEvent = enterEvent;
            pointerData.exitEvent = exitEvent;
        }
        #endregion

        //Keyboard Actions

        private Dictionary<InputID, IInputData> Inputs = new Dictionary<InputID, IInputData>();

        private void SetupInputs()
        {
            Inputs.Add(InputID.Up, new InputActionData(InputID.Up));
            Inputs.Add(InputID.Left, new InputActionData(InputID.Left));
            Inputs.Add(InputID.Right, new InputActionData(InputID.Right));
            Inputs.Add(InputID.Down, new InputActionData(InputID.Down));
            Inputs.Add(InputID.Scroll, new InputRangeData(InputID.Scroll));
            Inputs.Add(InputID.Shift, new InputActionData(InputID.Shift));
            Inputs.Add(InputID.Lock, new InputActionData(InputID.Lock));
        }

        public static bool Active(InputID ID) => Access.Instance_Active(ID);

        public bool Instance_Active(InputID ID)
        {
            var data = Instance_Data(ID);
            if (data != null)
            {
                return data.Active;
            }
            return false;
        }

        public static IInputData Data(InputID ID) => Access.Instance_Data(ID);

        public IInputData Instance_Data(InputID ID)
        {
            if (Inputs.TryGetValue(ID, out IInputData data))
            {
                return data;
            }
            return null;
        }
    }
}
