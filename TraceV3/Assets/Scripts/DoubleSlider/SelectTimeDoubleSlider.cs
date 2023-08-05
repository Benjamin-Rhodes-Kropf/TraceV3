#region Includes
using System;

using TMPro;

using UnityEngine;
#endregion

namespace TS.DoubleSlider
{
    public class SelectTimeDoubleSlider : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        [SerializeField] private DoubleSlider _slider;
        [SerializeField] private TextMeshProUGUI _labelMaxRange;
        [SerializeField] private TextMeshProUGUI _labelMinRange;


        [Header("Slider")] 
        [SerializeField] private int MaxValue = 500;
        
        #endregion

        private void OnEnable()
        {
            _slider.OnValueChanged.AddListener(SliderDouble_ValueChanged);
        }

        private void Start()
        { 
            _slider.Setup(0,MaxValue,20,80); 
        }

        private void SliderDouble_ValueChanged(float min, float max)
        {
            float higherValue = _slider.LowerValue;
            float higherIncrement = _slider.MaxIncrement;
            
            float lowerValue = _slider.HigherValue;
            float lowerIncrement = _slider.MinIncrement;
            
            _labelMaxRange.text = "max:" + Mathf.RoundToInt(higherValue) + "--" + higherIncrement;
            _labelMinRange.text = "min:" + Mathf.RoundToInt(lowerValue) + "--" +lowerIncrement;
            
            //offset date option
            // _labelMaxRange.text = GetMonthAndDay((int)higherValue + GetDayOfYear());
            // _labelMinRange.text = GetMonthAndDay((int)lowerValue + GetDayOfYear());

            //direct string option
            _labelMaxRange.text = GetSliderStringValue(higherValue);
            _labelMinRange.text = GetSliderStringValue(lowerValue);

            //offset skew option
            //_labelMaxRange.text = GetSliderTimeValue(higherValue);
            //_labelMinRange.text =  GetSliderTimeValue(lowerValue);
            
            
            if (lowerValue == 0 && lowerIncrement == 0)
            {
                _labelMinRange.text = "Now";
            }
            if (higherValue == MaxValue && higherIncrement == 0)
            {
                _labelMaxRange.text = "forever";
            }
        }

        //string option
        public static string GetSliderStringValue(float value)
        {
            string displayString = "";
            if (value < 100)
            {
                float hours = Map(value, 0, 100,1,24);
                if((int)hours == 1)
                    displayString = (int)hours + " hour";
                else
                    displayString = (int)hours + " hours";
            }else if (value < 200)
            {
                float days = Map(value, 100, 200,1,7);
                if((int)days == 1)
                    displayString = (int)days + " day";
                else
                    displayString = (int)days + " days";
            }else if (value < 300)
            {
                float weeks = Map(value, 200, 300,1,4);
                if((int)weeks == 1)
                    displayString = (int)weeks + " week";
                else
                    displayString = (int)weeks + " weeks";
            }else if (value < 400)
            {
                float months = Map(value, 300, 400,1,7);
                if((int)months == 1)
                    displayString = (int)months + " month";
                else
                    displayString = (int)months + " months";
            }else if (value < 500)
            {
                float months = Map(value, 400, 500,7,13);
                if((int)months == 1)
                    displayString = (int)months + " month";
                else if((int)months < 12)
                    displayString = (int)months + " months";
                else if (months >= 12)
                    displayString = 1 + " year";
            }
            return displayString;
        }
        
        public static string GetSliderTimeValue(float value)
        {
            //convert value to time
            float minutes = 0;
            float hours = 0;
            float days = 0;
            float weeks = 0;
            float months = 0;
            
            if (value < 100)
            {
                hours = Map(value, 0, 100,1,24);
            }else if (value < 200)
            {
                days = Map(value, 100, 200,1,7);
            }else if (value < 300)
            {
                weeks = Map(value, 200, 300,1,4);
            }else if (value < 400)
            {
                months = Map(value, 300, 400,1,7);
            }else if (value < 500)
            {
                months = Map(value, 400, 500,7,13);
            }
            
            //get time offset
            DateTime dateTime = new DateTime(DateTime.Now.Year, 1, 1).AddDays(GetDayOfYear());
            DateTime display = dateTime.AddMinutes(minutes + hours*60 + days *1440 + weeks*10080 + months * 43829.1f);
            

            return display.ToString();
            //structure string correctly
            // if (months > 0 || weeks > 0)
            // {
            //     return (dateTime.ToString("MMMM") + ", "+ dateTime.Day + ", "+ dateTime.Year);
            // }
            //
            // if (days > 0)
            // {
            //     return (dateTime.DayOfWeek + ", " + dateTime.Hour + dateTime.TimeOfDay);
            // }
            //
            // if (hours > 0)
            // {
            //     return (dateTime.Hour + " hours and " + dateTime.Minute + " minutes");
            // }
            //
            // return "now";
        }
        
        public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
        {
            // First, normalize the value to a 0-1 range
            float normalizedValue = (value - originalMin) / (originalMax - originalMin);

            // Then, scale and shift it to the new range
            float newValue = normalizedValue * (targetMax - targetMin) + targetMin;

            return newValue;
        }
        
        //getting text
        public static int GetDayOfYear()
        {
            DateTime now = DateTime.Now;
            DateTime firstDayOfYear = new DateTime(now.Year, 1, 1);
            return (now - firstDayOfYear).Days + 1;
        }
        public static string GetMonthAndDay(int dayOfYear)
        {
            DateTime dateTime = new DateTime(DateTime.Now.Year, 1, 1).AddDays(dayOfYear);
            string month = dateTime.ToString("MMMM");
            int day = dateTime.Day;
            int year = dateTime.Year;
            
            return(month + "." + day + "." + year);
        }
        
    }
}