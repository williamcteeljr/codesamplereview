ora.DX = {
    DD_Edit: {
        ListBox: {
            textSeparator: ", ",

            IndexChange: function (listBoxName) {
                var lb = ASPxClientControl.GetControlCollection().GetByName(listBoxName);
                var lbList = ASPxClientControl.GetControlCollection().GetByName(listBoxName + '_List');
                var selectedItems = lb.GetSelectedValues();
                var valueList = lbList.SetText(ora.DX.DD_Edit.ListBox.GetSelectedItemsText(selectedItems));
            },

            GetSelectedItemsText: function (items) {
                var texts = [];
                for (var i = 0; i < items.length; i++) {
                    texts.push(items[i]);
                }
                return texts.join(ora.DX.DD_Edit.ListBox.textSeparator);
            }
        },

		GridListBox: {
			textSeparator: ",",

			IndexChange: function (gridName, listBoxName, filterCol) {
				var lb = ASPxClientControl.GetControlCollection().GetByName(listBoxName);
				var grid = ASPxClientControl.GetControlCollection().GetByName(gridName);
				var lbList = ASPxClientControl.GetControlCollection().GetByName(listBoxName + '_List');

				var selectedItems = lb.GetSelectedValues();
				var valueList = lbList.SetText(ora.DX.DD_Edit.GridListBox.GetSelectedItemsText(selectedItems));
				grid.AutoFilterByColumn(filterCol, ora.DX.DD_Edit.GridListBox.GetSelectedItemsText(selectedItems));
			},

			GetSelectedItemsText: function (items) {
				var texts = [];
				for (var i = 0; i < items.length; i++) {
					texts.push(items[i]);
				}
				return texts.join(ora.DX.DD_Edit.GridListBox.textSeparator);
			}
		}
	}
}