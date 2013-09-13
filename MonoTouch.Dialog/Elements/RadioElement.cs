
//
// Elements.cs: defines the various components of our view
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010, Novell, Inc.
//
// Code licensed under the MIT X11 license
//
using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	
	public class RadioElement : StringElement {
		public string Group;
		internal int RadioIdx;
		
		public RadioElement (string caption, string group) : base (caption)
		{
			Group = group;
		}
				
		public RadioElement (string caption) : base (caption)
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

			if (OnSelected != null)
				OnSelected (this, EventArgs.Empty);

			base.Selected (dvc, tableView, indexPath);
		}

		public event EventHandler<EventArgs> OnSelected;
	}
	
}
