using GorillaNetworking;
using HarmonyLib;
using LightsCameraAction.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace LightsCameraAction.Interactions
{
    public class GestureTracker : MonoBehaviour
    {
        public static GestureTracker Instance;

        public static bool UseSteamInput;

        public List<InputTracker> inputs;

        public InputDevice leftController, rightController;

        public InputTracker<float> leftGrip;

        public InputTracker<bool> leftPrimary;

        public InputTracker<bool> leftSecondary;

        public InputTracker<bool> leftStick;

        public InputTracker<Vector2> leftStickAxis;

        public InputTracker<float> leftTrigger;

        public InputTracker<float> rightGrip;

        public InputTracker<bool> rightPrimary;

        public InputTracker<bool> rightSecondary;

        public InputTracker<bool> rightStick;

        public InputTracker<Vector2> rightStickAxis;

        public InputTracker<float> rightTrigger;

        private Traverse _poller, _analogPoller;

        public void Awake()
        {
            Instance = this;

            UseSteamInput = PlayFabAuthenticator.instance.platform.ToString().ToLower().Contains("steam");

            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            _poller = Traverse.Create(ControllerInputPoller.instance);
            _analogPoller = Traverse.Create(gameObject.AddComponent<AnalogStickInputProvider>());

            leftGrip = new InputTracker<float>(_poller.Field("leftControllerGripFloat"), XRNode.LeftHand);
            rightGrip = new InputTracker<float>(_poller.Field("rightControllerGripFloat"), XRNode.RightHand);

            leftTrigger = new InputTracker<float>(_poller.Field("leftControllerIndexFloat"), XRNode.LeftHand);
            rightTrigger = new InputTracker<float>(_poller.Field("rightControllerIndexFloat"), XRNode.RightHand);

            leftPrimary = new InputTracker<bool>(_poller.Field("leftControllerPrimaryButton"), XRNode.LeftHand);
            rightPrimary = new InputTracker<bool>(_poller.Field("rightControllerPrimaryButton"), XRNode.RightHand);

            leftSecondary = new InputTracker<bool>(_poller.Field("leftControllerSecondaryButton"), XRNode.LeftHand);
            rightSecondary = new InputTracker<bool>(_poller.Field("rightControllerSecondaryButton"), XRNode.RightHand);

            leftStick = new InputTracker<bool>(_analogPoller.Field("leftControllerStickButton"), XRNode.LeftHand);
            rightStick = new InputTracker<bool>(_analogPoller.Field("rightControllerStickButton"), XRNode.RightHand);

            leftStickAxis = new InputTracker<Vector2>(_analogPoller.Field("leftControllerStickAxis"), XRNode.LeftHand);
            rightStickAxis = new InputTracker<Vector2>(_analogPoller.Field("rightControllerStickAxis"), XRNode.RightHand);

            inputs =
            [
                leftGrip, rightGrip,
                leftTrigger, rightTrigger,
                leftPrimary, rightPrimary,
                leftSecondary, rightSecondary,
                leftStick, rightStick,
                leftStickAxis, rightStickAxis
            ];
        }

        public void Update()
        {
            UpdateValues();
        }

        public void OnDestroy()
        {
            //Logging.Debug("Gesture Tracker Destroy");
            Instance = null;
            ((MonoBehaviour)AccessTools.Field(_analogPoller.GetType(), "_root").GetValue(_analogPoller))?.Obliterate();
        }

        public void UpdateValues()
        {
            foreach (InputTracker input in inputs) input.UpdateValues();
        }

        public void HapticPulse(bool isLeft, float strength = 0.5f, float duration = 0.05f)
        {
            (isLeft ? this.leftController : this.rightController).SendHapticImpulse(0U, strength, duration);
        }

        public abstract class InputTracker
        {
            public string name;
            public XRNode node;
            public Action OnPressed, OnReleased;
            public bool pressed, wasPressed;
            public Quaternion quaternionValue;
            public Traverse traverse;
            public Vector3 vector3Value;

            public abstract void UpdateValues();
        }

        public class InputTracker<T> : InputTracker
        {
            public InputTracker(Traverse traverse, XRNode node)
            {
                this.traverse = traverse;
                this.node = node;
            }

            public T GetValue()
            {
                return traverse.GetValue<T>();
            }

            public override void UpdateValues()
            {
                wasPressed = pressed;

                if (typeof(T) == typeof(bool))
                    pressed = traverse.GetValue<bool>();
                else if (typeof(T) == typeof(float))
                    pressed = traverse.GetValue<float>() > .5f;

                if (!wasPressed && pressed)
                    OnPressed?.Invoke();
                if (wasPressed && !pressed)
                    OnReleased?.Invoke();
            }
        }

        public class AnalogStickInputProvider : MonoBehaviour
        {
            public static AnalogStickInputProvider Instance;

            public Vector2 rightControllerStickAxis, leftControllerStickAxis;
            public bool rightControllerStickButton, leftControllerStickButton;

            private InputDevice _leftController, _rightController;

            public void Awake()
            {
                Instance = this;

                _leftController = GestureTracker.Instance.leftController;
                _rightController = GestureTracker.Instance.rightController;
            }

            public void Update()
            {
                if (UseSteamInput)
                {
                    leftControllerStickButton = SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
                    leftControllerStickAxis = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.axis;

                    rightControllerStickButton = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
                    rightControllerStickAxis = SteamVR_Actions.gorillaTag_RightJoystick2DAxis.axis;

                    return;
                }

                _leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out leftControllerStickButton);
                _leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftControllerStickAxis);

                _rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out rightControllerStickButton);
                _rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightControllerStickAxis);
            }
        }
    }
}
