
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
//
using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	
	/// <summary>
	///   This element can be used to insert an arbitrary UIView
	/// </summary>
	/// <remarks>
	///   There is no cell reuse here as we have a 1:1 mapping
	///   in this case from the UIViewElement to the cell that
	///   holds our view.
	/// </remarks>
	public class UIViewElement : Element, IElementSizing {
		static int count;
		public UIView ContainerView;
		NSString key;
		protected UIView View;
		public CellFlags Flags;
		UIEdgeInsets insets;

		public UIEdgeInsets Insets { 
			get {
				return insets;
			}
			set {
				var viewFrame = View.Frame;
				var dx = value.Left - insets.Left;
				var dy = value.Top - insets.Top;
				var ow = insets.Left + insets.Right;
				var oh = insets.Top + insets.Bottom;
				var w = value.Left + value.Right;
				var h = value.Top + value.Bottom;

				ContainerView.Frame = new RectangleF (0, 0, ContainerView.Frame.Width + w - ow, ContainerView.Frame.Height + h -oh);
				viewFrame.X += dx;
				viewFrame.Y += dy;
				View.Frame = viewFrame;

				insets = value;

				// Height changed, notify UITableView
				if (dy != 0 || h != oh)
					GetContainerTableView ().ReloadData ();
				
			}
		}

		public enum CellFlags {
			Transparent = 1,
			DisableSelection = 2
		}

		/// <summary>
		///   Constructor
		/// </summary>
		/// <param name="caption">
		/// The caption, only used for RootElements that might want to summarize results
		/// </param>
		/// <param name="view">
		/// The view to display
		/// </param>
		/// <param name="transparent">
		/// If this is set, then the view is responsible for painting the entire area,
		/// otherwise the default cell paint code will be used.
		/// </param>
		public UIViewElement (string caption, UIView view, bool transparent, UIEdgeInsets insets) : base (caption) 
		{
			this.insets = insets;
			var oframe = view.Frame;
			var frame = oframe;
			frame.Width += insets.Left + insets.Right;
			frame.Height += insets.Top + insets.Bottom;

			ContainerView = new UIView (frame);
			if ((Flags & CellFlags.Transparent) != 0)
				ContainerView.BackgroundColor = UIColor.Clear;

			if (insets.Left != 0 || insets.Top != 0)
				view.Frame = new RectangleF (insets.Left + frame.X, insets.Top + frame.Y, frame.Width, frame.Height);

			ContainerView.AddSubview (view);
			this.View = view;
			this.Flags = transparent ? CellFlags.Transparent : 0;
			key = new NSString ("UIViewElement" + count++);
		}
		
		public UIViewElement (string caption, UIView view, bool transparent) : this (caption, view, transparent, UIEdgeInsets.Zero)
		{
		}

		protected override NSString CellKey {
			get {
				return key;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				if ((Flags & CellFlags.Transparent) != 0){
					cell.BackgroundColor = UIColor.Clear;
					
					// 
					// This trick is necessary to keep the background clear, otherwise
					// it gets painted as black
					//
					cell.BackgroundView = new UIView (RectangleF.Empty) { 
						BackgroundColor = UIColor.Clear 
					};
				}
				if ((Flags & CellFlags.DisableSelection) != 0)
					cell.SelectionStyle = UITableViewCellSelectionStyle.None;

				if (Caption != null)
					cell.TextLabel.Text = Caption;
				cell.ContentView.AddSubview (ContainerView);
			} 
			return cell;
		}
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return ContainerView.Bounds.Height+1;
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing){
				if (View != null){
					View.Dispose ();
					View = null;
				}
			}
		}
	}
	
}
