using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;

namespace EnhancedScrollerDemos.SuperSimpleDemo
{
    /// <summary>
    /// This is the view of our cell which handles how the cell looks.
    /// </summary>
    public class CellView : EnhancedScrollerCellView
    {
        /// <summary>
        /// A reference to the UI Text element to display the cell data
        /// </summary>
        public ContactView _View;


        /// <summary>
        /// This function just takes the Demo data and displays it
        /// </summary>
        /// <param name="data"></param>
        public void SetData(Data data)
        {
#if UNITY_EDITOR
            _View.ContactInfoUpdate(data._Name,data._ContactNumber,data._Sprite);
#elif UNITY_IPHONE
            _View.UpdateContactInfo(data._Contact);
#endif
        }
    }
}