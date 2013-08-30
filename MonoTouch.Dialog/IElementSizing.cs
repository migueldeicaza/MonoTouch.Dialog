
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
	///   This interface is implemented by Element classes that will have
	///   different heights
	/// </summary>
	public interface IElementSizing {
		float GetHeight (UITableView tableView, NSIndexPath indexPath);
	}
	
}
