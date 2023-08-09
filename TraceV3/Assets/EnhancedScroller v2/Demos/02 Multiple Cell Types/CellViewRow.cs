using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;

namespace EnhancedScrollerDemos.MultipleCellTypesDemo
{
    /// <summary>
    /// This is the view for the rows
    /// </summary>
    public class CellViewRow : CellView
    {
        /// <summary>
        /// An internal reference to the row data. We could have just
        /// used the base CellView's class member _data, but that would
        /// require us to cast it each time a row data field is needed.
        /// By referencing the row data, we can save some time accessing
        /// the fields.
        /// </summary>
        private RowData _rowData;

        /// <summary>
        /// Links to the UI fields
        /// </summary>
        public SendToFriendView _view;
        
        /// <summary>
        /// Override of the base class's SetData function. This links the data
        /// and updates the UI
        /// </summary>
        /// <param name="data"></param>
        public override void SetData(Data data)
        {
            // call the base SetData to link to the underlying _data
            base.SetData(data);

            // cast the data as rowData and store the reference
            _rowData = data as RowData;
            //Todo : Update UI Elements;
            _view.UpdateUIElements(_rowData._userData, _rowData._index, UpdateSelectionStatus);
        }

        private void UpdateSelectionStatus(bool isSelected)
        {
            Debug.Log("Username "+  _rowData._userData._textData + " :: Toggle Status ::"+ isSelected);
            _rowData._userData._isSelected = isSelected;
        }
        
    }
}