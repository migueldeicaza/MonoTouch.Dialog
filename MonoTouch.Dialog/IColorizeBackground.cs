
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
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	
	/// <summary>
	///   This interface is implemented by Elements that needs to update
	///   their cells Background properties just before they are displayed
	///   to the user.   This is an iOS 3 requirement to properly render
	///   a cell.
	/// </summary>
	public interface IColorizeBackground {
		void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath);
	}
	
}
