using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FuzzyLogic3
{
    public class BaseExample : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
        [SerializeField] private TextMeshProUGUI eMeshProUGUI;

        [SerializeField] private TextMeshProUGUI AccelerationXText;
        [SerializeField] private TextMeshProUGUI AccelerationYText;
        [SerializeField] private TextMeshProUGUI AccelerationZText;
        [SerializeField] private TextMeshProUGUI RotationXText;
        [SerializeField] private TextMeshProUGUI RotationYText;
        [SerializeField] private TextMeshProUGUI RotationZText;
        [SerializeField] private TextMeshProUGUI MagnitudeText;

        [SerializeField] private Toggle _toggle;

        private readonly FuzzyVariable _accelerometerX = new FuzzyVariable();
        private readonly FuzzyVariable _accelerometerY = new FuzzyVariable();
        private readonly FuzzyVariable _accelerometerZ = new FuzzyVariable();

        private readonly FuzzyVariable _gyroscopeCrossX = new FuzzyVariable();
        private readonly FuzzyVariable _gyroscopeCrossY = new FuzzyVariable();
        private readonly FuzzyVariable _gyroscopeCrossZ = new FuzzyVariable();

        private readonly FuzzyVariable _magnitude = new FuzzyVariable();

        private readonly FuzzyVariable _gestureType = new FuzzyVariable();

        private FuzzyLogicController _controller;

        private KalmanFilter _accXFilter = new KalmanFilter();
        private KalmanFilter _accYFilter = new KalmanFilter();
        private KalmanFilter _accZFilter = new KalmanFilter();
        private KalmanFilter _rotXFilter = new KalmanFilter();
        private KalmanFilter _rotYFilter = new KalmanFilter();
        private KalmanFilter _rotZFilter = new KalmanFilter();
        private KalmanFilter _magnFilter = new KalmanFilter();
        
        private Quaternion _initialRotation;
        private Vector3 _initialAcceleration;
        
        private void Awake()
        {
            Input.compensateSensors = true;
            Input.gyro.enabled = true;

            _toggle.onValueChanged.AddListener(PointerClick);
        }
        
        private void Start()
        {
            InitController();
            InitFuzzyVariables();
            InitRules();
        }

        private void FixedUpdate()
        {
            if (!_toggle.isOn) return;

            var accel = Input.gyro.userAcceleration;
            var acceleration = -(accel - _initialAcceleration);
            
            var accX = _accXFilter.Update(acceleration.x);
            var accY = _accYFilter.Update(acceleration.y);
            var accZ = _accZFilter.Update(acceleration.z);
            var magn = _magnFilter.Update(acceleration.sqrMagnitude);
            
            var quaternion = Input.gyro.attitude;

            float angleX = _rotXFilter.Update(GetAngle(_initialRotation, quaternion, Vector3.right));
            float angleY = _rotYFilter.Update(GetAngle(_initialRotation, quaternion, Vector3.up));
            float angleZ = _rotZFilter.Update(GetAngle(_initialRotation, quaternion, Vector3.forward));
            
            Debug.LogError("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
            Debug.LogError($"AccelerationX = {(double) accX}\n");
            Debug.LogError($"AccelerationY = {(double) accY}\n");
            Debug.LogError($"AccelerationZ = {(double) accZ}\n");
            Debug.LogError($"AngleX = {(double) angleX}\n");
            Debug.LogError($"AngleY = {(double) angleY}\n");
            Debug.LogError($"AngleZ = {(double) angleZ}\n");
            Debug.LogError($"Magnitude = {(double) magn}\n");
            
            AccelerationXText.SetText($"AccelerationX = {(double) accX}");
            AccelerationYText.SetText($"AccelerationY = {(double) accY}");
            AccelerationZText.SetText($"AccelerationZ = {(double) accZ}");
            RotationXText.SetText($"AngleX = {(double) angleX}");
            RotationYText.SetText($"AngleY = {(double) angleY}");
            RotationZText.SetText($"AngleZ = {(double) angleZ}");
            MagnitudeText.SetText($"Magnitude = {(double) magn}");

            _accelerometerX.SetInputValue(accX);
            _accelerometerY.SetInputValue(accY);
            _accelerometerZ.SetInputValue(accZ);
            _gyroscopeCrossX.SetInputValue(angleX);
            _gyroscopeCrossY.SetInputValue(angleY);
            _gyroscopeCrossZ.SetInputValue(angleZ);
            _magnitude.SetInputValue(magn);

            _controller.Evaluate();

            float result = _controller.GetOutputValue("GestureType");
            _textMeshProUGUI.SetText(RoundUp(result).ToString());
            eMeshProUGUI.SetText(((GestureType) RoundUp(result)).ToString());

            Debug.LogError("result is " + result);
        }

        private void InitFuzzyVariables()
        {
            InitGestureVariable();

            InitAccelerationXVariable();
            InitAccelerationYVariable();
            InitAccelerationZVariable();
            
            InitCrossXVariable();
            InitCrossYVariable();
            InitCrossZVariable();
            
            InitMagnitudeVariable();
            
            
            void InitGestureVariable()
            {
                _gestureType.AddFuzzySet("None",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-1f, -0.5f, 0f),
                        -1f, 0f));
                _gestureType.AddFuzzySet("StepBackward",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(0f, 0.5f,1f),
                        0f, 1f));
                _gestureType.AddFuzzySet("StepForward",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(1f, 1.5f, 2f),
                        1f, 2f));
                _gestureType.AddFuzzySet("Jump",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(2f, 2.5f, 3f),
                        2f, 3f));
                _gestureType.AddFuzzySet("StepLeft",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(3f, 3.5f, 4f),
                        3f, 4f));
                _gestureType.AddFuzzySet("StepRight",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(4f, 4.5f, 5f),
                        4f, 5f));
                _gestureType.AddFuzzySet("Shake",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(5f, 5.5f, 6f),
                        5f, 6f));
                _gestureType.AddFuzzySet("Squat",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(6f, 6.5f, 7f),
                        6f, 7f));
                _gestureType.AddFuzzySet("TurnLeft",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(7f, 7.5f, 8f),
                        7f, 8f));
                _gestureType.AddFuzzySet("TurnRight",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(8f, 8.5f, 9f),
                        8f, 9f));
            }
            
            void InitAccelerationXVariable()
            {
                _accelerometerX.AddFuzzySet("HighNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-1f, -0.375f, -0.3f),
                        -1f, -0.3f));
                _accelerometerX.AddFuzzySet("LowNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-0.3f, -0.175f, -0.1f),
                        -0.3f, -0.1f));
                _accelerometerX.AddFuzzySet("Zero",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-0.1f, 0f, 0.1f),
                        -0.1f, 0.1f));
                _accelerometerX.AddFuzzySet("LowPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(0.1f, 0.175f, 0.3f),
                        0.1f, 0.3f));
                _accelerometerX.AddFuzzySet("HighPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(0.3f, 0.375f, 1f),
                        0.3f, 1f));
            }
            void InitAccelerationYVariable()
            {
                _accelerometerY.AddFuzzySet("HighNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-1f, -0.375f, -0.3f),
                        -1f, -0.3f));
                _accelerometerY.AddFuzzySet("LowNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-0.3f, -0.175f, -0.1f),
                        -0.3f, -0.1f));
                _accelerometerY.AddFuzzySet("Zero",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-0.1f, 0f, 0.1f),
                        -0.1f, 0.1f));
                _accelerometerY.AddFuzzySet("LowPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(0.1f, 0.175f, 0.3f),
                        0.1f, 0.3f));
                _accelerometerY.AddFuzzySet("HighPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(0.3f, 0.375f, 1f),
                        0.3f, 1f));
            }
            void InitAccelerationZVariable()
            {
                _accelerometerZ.AddFuzzySet("HighNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-1f, -0.375f, -0.3f),
                        -1f, -0.3f));
                _accelerometerZ.AddFuzzySet("LowNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-0.3f, -0.175f, -0.1f),
                        -0.3f, -0.1f));
                _accelerometerZ.AddFuzzySet("Zero",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-0.1f, 0f, 0.1f),
                        -0.1f, 0.1f));
                _accelerometerZ.AddFuzzySet("LowPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(0.1f, 0.175f, 0.3f),
                        0.1f, 0.3f));
                _accelerometerZ.AddFuzzySet("HighPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(0.3f, 0.375f, 1f),
                        0.3f, 1f));
            }
            
            void InitCrossXVariable()
            {
                _gyroscopeCrossX.AddFuzzySet("HighNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-120f, -90f, -85f),
                        -120f, -85f));
                _gyroscopeCrossX.AddFuzzySet("LowNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-85f, -60f, -45f),
                        -85f, -45f));
                _gyroscopeCrossX.AddFuzzySet("Zero",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-45f, 0f, 45f),
                        -45f, 45f));
                _gyroscopeCrossX.AddFuzzySet("LowPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(45f, 60f, 85f),
                        45f, 85f));
                _gyroscopeCrossX.AddFuzzySet("HighPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(85, 90f, 120f),
                        85, 120f));
            }
            void InitCrossYVariable()
            {
                _gyroscopeCrossY.AddFuzzySet("HighNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-120f, -90f, -85f),
                        -120f, -85f));
                _gyroscopeCrossY.AddFuzzySet("LowNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-85f, -60f, -45f),
                        -85f, -45f));
                _gyroscopeCrossY.AddFuzzySet("Zero",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-45f, 0f, 45f),
                        -45f, 45f));
                _gyroscopeCrossY.AddFuzzySet("LowPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(45f, 60f, 85f),
                        45f, 85f));
                _gyroscopeCrossY.AddFuzzySet("HighPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(85, 90f, 120f),
                        85, 120f));
            }
            void InitCrossZVariable()
            {
                _gyroscopeCrossZ.AddFuzzySet("HighNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-120f, -90f, -85f),
                        -120f, -85f));
                _gyroscopeCrossZ.AddFuzzySet("LowNeg",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-85f, -60f, -45f),
                        -85f, -45f));
                _gyroscopeCrossZ.AddFuzzySet("Zero",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(-45f, 0f, 45f),
                        -45f, 45f));
                _gyroscopeCrossZ.AddFuzzySet("LowPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(45f, 60f, 85f),
                        45f, 85f));
                _gyroscopeCrossZ.AddFuzzySet("HighPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(85, 90f, 120f),
                        85, 120f));
            }

            void InitMagnitudeVariable()
            {
                _magnitude.AddFuzzySet("Zero",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(0f, 0.25f, 0.5f),
                        0f, 0.5f));
                _magnitude.AddFuzzySet("LowPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(0.5f, 0.75f, 1f),
                        0.5f, 1f));
                _magnitude.AddFuzzySet("HighPos",
                    new FuzzySet(FuzzySet.CreateTriangularFunction(1f, 10f, 25f),
                        1f, 25f));
            }
        }

        private void InitRules()
        {
            FuzzyRule stepForwardRule = new FuzzyRule(
                new[] { "AccelerometerX", "AccelerometerY", "AccelerometerZ" },
                new[] { "Zero", "HighPos", "Zero" },
                new[] { FuzzyOperator.And, FuzzyOperator.And },
                "GestureType", "StepForward");

            FuzzyRule stepBackwardRule = new FuzzyRule(
                new[] { "AccelerometerX", "AccelerometerY", "AccelerometerZ" },
                new[] { "Zero", "HighNeg", "Zero" },
                new[] { FuzzyOperator.And, FuzzyOperator.And },
                "GestureType", "StepBackward");
            
            FuzzyRule stepLeftRule = new FuzzyRule(
                new[] { "AccelerometerX", "AccelerometerY", "AccelerometerZ" },
                new[] { "HighNeg", "Zero", "Zero" },
                new[] { FuzzyOperator.And, FuzzyOperator.And },
                "GestureType", "StepLeft");
            
            FuzzyRule stepRightRule = new FuzzyRule(
                new[] { "AccelerometerX", "AccelerometerY", "AccelerometerZ" },
                new[] { "HighPos", "Zero", "Zero" },
                new[] { FuzzyOperator.And, FuzzyOperator.And },
                "GestureType", "StepRight");

            FuzzyRule jumpRule = new FuzzyRule(
                new[] { "AccelerometerX", "AccelerometerY", "AccelerometerZ" },
                new[] { "Zero", "Zero", "HighPos" },
                new[] { FuzzyOperator.And, FuzzyOperator.And },
                "GestureType", "Jump");

            FuzzyRule squatRule = new FuzzyRule(
                new[] { "AccelerometerX", "AccelerometerY", "AccelerometerZ" },
                new[] { "Zero", "Zero", "HighNeg" },
                new[] { FuzzyOperator.And, FuzzyOperator.And },
                "GestureType", "Squat");

            FuzzyRule turnLeftRule = new FuzzyRule(
                new[] { "GyroscopeCrossX", "GyroscopeCrossY", "GyroscopeCrossZ" },
                new[] { "HighNeg", "LowNeg","Zero" },
                new[] { FuzzyOperator.And, FuzzyOperator.And },
                "GestureType", "TurnLeft");
            
            FuzzyRule turnRightRule = new FuzzyRule(
                new[] { "GyroscopeCrossX", "GyroscopeCrossY", "GyroscopeCrossZ" },
                new[] { "HighPos", "LowPos","Zero" },
                new[] { FuzzyOperator.And, FuzzyOperator.And },
                "GestureType", "TurnRight");

            FuzzyRule shakeRule = new FuzzyRule(
                new[] { "Magnitude" },
                new[] { "HighPos" },
                null,
                "GestureType", "Shake");

            FuzzyRule noGestureRule = new FuzzyRule(
                new[] { "AccelerometerX", "AccelerometerY", "AccelerometerZ", "GyroscopeCrossX", "GyroscopeCrossY", "GyroscopeCrossZ", "Magnitude" },
                new[] { "Zero", "Zero", "Zero", "Zero", "Zero", "Zero", "Zero", },
                new[] { FuzzyOperator.And, FuzzyOperator.And, FuzzyOperator.And, FuzzyOperator.And, FuzzyOperator.And, FuzzyOperator.And },
                "GestureType", "None");
            
            _controller.AddRule(stepForwardRule);
            _controller.AddRule(stepBackwardRule);
            _controller.AddRule(stepRightRule);
            _controller.AddRule(stepLeftRule);
            _controller.AddRule(jumpRule);
            _controller.AddRule(squatRule);
            _controller.AddRule(turnRightRule);
            _controller.AddRule(turnLeftRule);
            _controller.AddRule(shakeRule);
            _controller.AddRule(noGestureRule);
        }

        private void InitController()
        {
            _controller = new FuzzyLogicController();
            _controller.AddInputVariable("AccelerometerX", _accelerometerX);
            _controller.AddInputVariable("AccelerometerY", _accelerometerY);
            _controller.AddInputVariable("AccelerometerZ", _accelerometerZ);
            _controller.AddInputVariable("GyroscopeCrossX", _gyroscopeCrossX);
            _controller.AddInputVariable("GyroscopeCrossY", _gyroscopeCrossY);
            _controller.AddInputVariable("GyroscopeCrossZ", _gyroscopeCrossZ);
            _controller.AddInputVariable("Magnitude", _magnitude);

            _controller.AddOutputVariable("GestureType", _gestureType);
        }

        private float GetAngle(Quaternion from, Quaternion to, Vector3 axis)
        {
            float angle = 0f;
            Vector3 cross = Vector3.Cross(from * axis, to * axis);
            if (cross.magnitude > 0.01f)
            {
                angle = Vector3.Angle(from * axis, to * axis);
                if (cross.z < 0) angle = -angle;
            }

            return -angle;
        }
        
        private void PointerClick(bool isOn)
        {
            if (isOn)
            {
                _initialRotation = Input.gyro.attitude;
                _initialAcceleration = Input.gyro.userAcceleration;
                Input.ResetInputAxes();

                _accXFilter = new KalmanFilter();
                _accYFilter = new KalmanFilter();
                _accZFilter = new KalmanFilter();
                _rotXFilter = new KalmanFilter();
                _rotYFilter = new KalmanFilter();
                _rotZFilter = new KalmanFilter();
                _magnFilter = new KalmanFilter();
            }
        }

        private float RoundUp(float value)
        {
            return value switch
            {
                > 0 => Mathf.FloorToInt(value),
                < 0 => -1,
                _ => value
            };
        }
    }
}