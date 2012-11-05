using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.Dialog.Utilities;
using MonoTouch.CoreAnimation;


namespace MonoTouch.Dialog
{
	
	public class StyledRadioElement : StyledStringElement {
		public string Group;
		internal int RadioIdx;
		
		public StyledRadioElement (string caption, string group) : base (caption)
		{
			Group = group;
		}
		
		public StyledRadioElement (string caption) : base (caption)
		{
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);			
			var root = (RootElement) Parent.Parent;
			
			if (!(root.group is RadioGroup))
				throw new Exception ("The RootElement's Group is null or is not a RadioGroup");
			
			bool selected = RadioIdx == ((RadioGroup)(root.group)).Selected;
			cell.Accessory = selected ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
			
			return cell;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			RootElement root = (RootElement) Parent.Parent;
			if (RadioIdx != root.RadioSelected){
				UITableViewCell cell;
				var selectedIndex = root.PathForRadio (root.RadioSelected);
				if (selectedIndex != null) {
					cell = tableView.CellAt (selectedIndex);
					if (cell != null)
						cell.Accessory = UITableViewCellAccessory.None;
				}				
				cell = tableView.CellAt (indexPath);
				if (cell != null)
					cell.Accessory = UITableViewCellAccessory.Checkmark;
				root.RadioSelected = RadioIdx;
			}
			
			base.Selected (dvc, tableView, indexPath);
		}
	}

}

