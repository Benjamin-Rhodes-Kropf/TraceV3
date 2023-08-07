#region Includes
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
#endregion

namespace TS.DoubleSlider
{
    [RequireComponent(typeof(Slider))]
    public class SingleSlider : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [SerializeField] private Label _label;

        private Slider _slider;

        public bool IsEnabled
        {
            get { return _slider.interactable; }
            set { _slider.interactable = value; }

        }
        public float Value
        {
            get { return _slider.value; }
            set
            {
                _slider.value = value;
                _slider.onValueChanged.Invoke(_slider.value);

                UpdateLabel();
            }
        }
        public bool WholeNumbers
        {
            get { return _slider.wholeNumbers; }
            set { _slider.wholeNumbers = value; }
        }

        #endregion

        private void Awake()
        {
            if (!TryGetComponent<Slider>(out _slider))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing Slider Component");
#endif
            }
        }

        private float _previousValue;
        private float _previousTime;
        [SerializeField] private float _smoothingFactor = 5.0f;
        public float Velocity { get; private set; }

        public void Update()
        {
            float currentTime = Time.time;
            float currentValue = Value;
            float rawVelocity = 0;

            if (currentTime != _previousTime)
            {
                rawVelocity = (currentValue - _previousValue) / (currentTime - _previousTime);
            }

            // Update the velocity using exponential moving average for smoothing
            Velocity = (rawVelocity * (_smoothingFactor / (_smoothingFactor + 1))) +
                       (Velocity * (1 / (_smoothingFactor + 1)));

            _previousValue = currentValue;
            _previousTime = currentTime;
            
            //Debug.Log("Vel" + Velocity);
        }

        public void Setup(float value, float minValue, float maxValue, UnityAction<float> valueChanged)
        {
            _slider.minValue = minValue;
            _slider.maxValue = maxValue;

            _slider.value = value;
            _slider.onValueChanged.AddListener(Slider_OnValueChanged);
            _slider.onValueChanged.AddListener(valueChanged);
        }

        private void Slider_OnValueChanged(float arg0)
        {
            UpdateLabel();
        }

        protected virtual void UpdateLabel()
        {
            if (_label == null) { return; }
            _label.Text = Value.ToString();
        }
    }
}