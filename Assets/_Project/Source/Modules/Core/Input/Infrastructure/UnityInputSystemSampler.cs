using System;
using Input.Application;
using Input.Domain;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input.Infrastructure
{
    /// <summary>
    /// Input System implementation of <see cref="IInputSampler"/> (keyboard, mouse, gamepad).
    /// </summary>
    public sealed class UnityInputSystemSampler : IInputSampler, IDisposable
    {
        private readonly InputActionMap _map;
        private readonly InputAction _move;
        private readonly InputAction _look;
        private readonly InputAction _fire;
        private readonly InputAction _aim;

        private const float MouseAxisScale = 0.05f;

        public UnityInputSystemSampler()
        {
            _map = new InputActionMap("Basketball");

            _move = _map.AddAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick");

            _look = _map.AddAction("Look", InputActionType.Value, "<Mouse>/delta");
            _look.AddBinding("<Gamepad>/rightStick");

            _fire = _map.AddAction("Fire", InputActionType.Button, "<Mouse>/leftButton");
            _fire.AddBinding("<Keyboard>/space");
            _fire.AddBinding("<Gamepad>/buttonSouth");

            _aim = _map.AddAction("Aim", InputActionType.Button, "<Mouse>/rightButton");
            _aim.AddBinding("<Gamepad>/leftTrigger");

            _map.Enable();
        }

        public void Dispose()
        {
            _map?.Disable();
            _map?.Dispose();
        }

        public float SampleAxis(string axisName)
        {
            switch (axisName)
            {
                case "Horizontal":
                    return _move.ReadValue<Vector2>().x;
                case "Vertical":
                    return _move.ReadValue<Vector2>().y;
                case "Mouse X":
                    return _look.ReadValue<Vector2>().x * MouseAxisScale;
                case "Mouse Y":
                    return _look.ReadValue<Vector2>().y * MouseAxisScale;
                default:
                    return 0f;
            }
        }

        public bool SampleButton(string buttonName)
        {
            return buttonName switch
            {
                "Fire1" => _fire.IsPressed(),
                "Fire2" => _aim.IsPressed(),
                _ => false
            };
        }

        public bool SampleButtonDown(string buttonName)
        {
            return buttonName switch
            {
                "Fire1" => _fire.WasPressedThisFrame(),
                "Fire2" => _aim.WasPressedThisFrame(),
                _ => false
            };
        }

        public bool SampleButtonUp(string buttonName)
        {
            return buttonName switch
            {
                "Fire1" => _fire.WasReleasedThisFrame(),
                "Fire2" => _aim.WasReleasedThisFrame(),
                _ => false
            };
        }

        public int TouchCount
        {
            get
            {
                var ts = Touchscreen.current;
                if (ts == null)
                    return 0;
                var n = 0;
                for (var i = 0; i < ts.touches.Count; i++)
                {
                    if (ts.touches[i].press.isPressed)
                        n++;
                }

                return n;
            }
        }

        public void SampleTouch(int index, out float x, out float y, out InputTouchPhase phase)
        {
            phase = InputTouchPhase.Began;
            x = y = 0f;
            var ts = Touchscreen.current;
            if (ts == null || index < 0 || index >= ts.touches.Count)
                return;
            var touch = ts.touches[index];
            var pos = touch.position.ReadValue();
            x = pos.x;
            y = pos.y;
            var u = touch.phase.ReadValue();
            phase = (InputTouchPhase)(int)u;
        }

        public void SamplePointerPosition(out float x, out float y)
        {
            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                var p = Touchscreen.current.primaryTouch.position.ReadValue();
                x = p.x;
                y = p.y;
            }
            else if (Mouse.current != null)
            {
                var p = Mouse.current.position.ReadValue();
                x = p.x;
                y = p.y;
            }
            else
            {
                x = y = 0f;
            }
        }
    }
}
