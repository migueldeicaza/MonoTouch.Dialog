
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
	/// <summary>
	/// Base class for all elements in MonoTouch.Dialog
	/// </summary>
	public class Element : IDisposable {
		/// <summary>
		///  Handle to the container object.
		/// </summary>
		/// <remarks>
		/// For sections this points to a RootElement, for every
		/// other object this points to a Section and it is null
		/// for the root RootElement.
		/// </remarks>
		public Element Parent;
		
		/// <summary>
		///  The caption to display for this given element
		/// </summary>
		public string Caption;
		
		/// <summary>
		///  Initializes the element with the given caption.
		/// </summary>
		/// <param name="caption">
		/// The caption.
		/// </param>
		public Element (string caption)
		{
			this.Caption = caption;
		}	
		
		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
		}
		
		static NSString cellkey = new NSString ("xx");
		/// <summary>
		/// Subclasses that override the GetCell method should override this method as well
		/// </summary>
		/// <remarks>
		/// This method should return the key passed to UITableView.DequeueReusableCell.
		/// If your code overrides the GetCell method to change the cell, you must also 
		/// override this method and return a unique key for it.
		/// 
		/// This works in most subclasses with a couple of exceptions: StringElement and
		/// various derived classes do not use this setting as they need a wider range
		/// of keys for different uses, so you need to look at the source code for those
		/// if you are trying to override StringElement or StyledStringElement.
		/// </remarks>
		protected virtual NSString CellKey { 
			get {
				return cellkey;
			}
		}
		
		/// <summary>
		/// Gets a UITableViewCell for this element.   Can be overridden, but if you 
		/// customize the style or contents of the cell you must also override the CellKey 
		/// property in your derived class.
		/// </summary>
		public virtual UITableViewCell GetCell (UITableView tv)
		{
			return new UITableViewCell (UITableViewCellStyle.Default, CellKey);
		}
		
		static protected void RemoveTag (UITableViewCell cell, int tag)
		{
			var viewToRemove = cell.ContentView.ViewWithTag (tag);
			if (viewToRemove != null)
				viewToRemove.RemoveFromSuperview ();
		}
		
		/// <summary>
		/// Returns a summary of the value represented by this object, suitable 
		/// for rendering as the result of a RootElement with child objects.
		/// </summary>
		/// <returns>
		/// The return value must be a short description of the value.
		/// </returns>
		public virtual string Summary ()
		{
			return "";
		}
		
		/// <summary>
		/// Invoked when the given element has been deslected by the user.
		/// </summary>
		/// <param name="dvc">
		/// The <see cref="DialogViewController"/> where the deselection took place
		/// </param>
		/// <param name="tableView">
		/// The <see cref="UITableView"/> that contains the element.
		/// </param>
		/// <param name="path">
		/// The <see cref="NSIndexPath"/> that contains the Section and Row for the element.
		/// </param>
		public virtual void Deselected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
		}
		
		/// <summary>
		/// Invoked when the given element has been selected by the user.
		/// </summary>
		/// <param name="dvc">
		/// The <see cref="DialogViewController"/> where the selection took place
		/// </param>
		/// <param name="tableView">
		/// The <see cref="UITableView"/> that contains the element.
		/// </param>
		/// <param name="path">
		/// The <see cref="NSIndexPath"/> that contains the Section and Row for the element.
		/// </param>
		public virtual void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
		}

		/// <summary>
		/// If the cell is attached will return the immediate RootElement
		/// </summary>
		public RootElement GetImmediateRootElement ()
		{
				var section = Parent as Section;
                if (section == null)
                    section = this as Section;
				if (section == null)
					return null;
				return section.Parent as RootElement;
		}
		
		/// <summary>
		/// Returns the UITableView associated with this element, or null if this cell is not currently attached to a UITableView
		/// </summary>
		public UITableView GetContainerTableView ()
		{
			var root = GetImmediateRootElement ();
			if (root == null)
				return null;
			return root.TableView;
		}
		
		/// <summary>
		/// Returns the currently active UITableViewCell for this element, or null if the element is not currently visible
		/// </summary>
		public UITableViewCell GetActiveCell ()
		{
			var tv = GetContainerTableView ();
			if (tv == null)
				return null;
			var path = IndexPath;
			if (path == null)
				return null;
			return tv.CellAt (path);
		}
		
		/// <summary>
		///  Returns the IndexPath of a given element.   This is only valid for leaf elements,
		///  it does not work for a toplevel RootElement or a Section of if the Element has
		///  not been attached yet.
		/// </summary>
		public NSIndexPath IndexPath { 
			get {
				var section = Parent as Section;
				if (section == null)
					return null;
				var root = section.Parent as RootElement;
				if (root == null)
					return null;
				
				int row = 0;
				foreach (var element in section.Elements){
					if (element == this){
						int nsect = 0;
						foreach (var sect in root.Sections){
							if (section == sect){
								return NSIndexPath.FromRowSection (row, nsect);
							}
							nsect++;
						}
					}
					row++;
				}
				return null;
			}
		}
		
		/// <summary>
		///   Method invoked to determine if the cell matches the given text, never invoked with a null value or an empty string.
		/// </summary>
		public virtual bool Matches (string text)
		{
			if (Caption == null)
				return false;
			return Caption.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1;
		}
	}
	
}
