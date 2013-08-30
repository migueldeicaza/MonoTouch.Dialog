
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
// TODO: StyledStringElement: merge with multi-line?
// TODO: StyledStringElement: add image scaling features?
// TODO: StyledStringElement: add sizing based on image size?
// TODO: Move image rendering to StyledImageElement, reason to do this: the image loader would only be imported in this case, linked out otherwise
//
using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.Dialog.Utilities;

namespace MonoTouch.Dialog
{
	
	/// <summary>
	///   A version of the StringElement that can be styled with a number of formatting 
	///   options and can render images or background images either from UIImage parameters 
	///   or by downloading them from the net.
	/// </summary>
	public class StyledStringElement : StringElement, IImageUpdated, IColorizeBackground {
		static NSString [] skey = { new NSString (".1"), new NSString (".2"), new NSString (".3"), new NSString (".4") };
		
		public StyledStringElement (string caption) : base (caption) {}
		public StyledStringElement (string caption, NSAction tapped) : base (caption, tapped) {}
		public StyledStringElement (string caption, string value) : base (caption, value) 
		{
			style = UITableViewCellStyle.Value1;	
		}
		public StyledStringElement (string caption, string value, UITableViewCellStyle style) : base (caption, value) 
		{ 
			this.style = style;
		}
		
		protected UITableViewCellStyle style;
		public event NSAction AccessoryTapped;
		public UIFont Font;
		public UIFont SubtitleFont;
		public UIColor TextColor;
		public UILineBreakMode LineBreakMode = UILineBreakMode.WordWrap;
		public int Lines = 0;
		public UITableViewCellAccessory Accessory = UITableViewCellAccessory.None;
		
		// To keep the size down for a StyleStringElement, we put all the image information
		// on a separate structure, and create this on demand.
		ExtraInfo extraInfo;
		
		class ExtraInfo {
			public UIImage Image; // Maybe add BackgroundImage?
			public UIColor BackgroundColor, DetailColor;
			public Uri Uri, BackgroundUri;
		}

		ExtraInfo OnImageInfo ()
		{
			if (extraInfo == null)
				extraInfo = new ExtraInfo ();
			return extraInfo;
		}
		
		// Uses the specified image (use this or ImageUri)
		public UIImage Image {
			get {
				return extraInfo == null ? null : extraInfo.Image;
			}
			set {
				OnImageInfo ().Image = value;
				extraInfo.Uri = null;
			}
		}
		
		// Loads the image from the specified uri (use this or Image)
		public Uri ImageUri {
			get {
				return extraInfo == null ? null : extraInfo.Uri;
			}
			set {
				OnImageInfo ().Uri = value;
				extraInfo.Image = null;
			}
		}
		
		// Background color for the cell (alternative: BackgroundUri)
		public UIColor BackgroundColor {
			get {
				return extraInfo == null ? null : extraInfo.BackgroundColor;
			}
			set {
				OnImageInfo ().BackgroundColor = value;
				extraInfo.BackgroundUri = null;
			}
		}
		
		public UIColor DetailColor {
			get {
				return extraInfo == null ? null : extraInfo.DetailColor;
			}
			set {
				OnImageInfo ().DetailColor = value;
			}
		}
		
		// Uri for a Background image (alternatiev: BackgroundColor)
		public Uri BackgroundUri {
			get {
				return extraInfo == null ? null : extraInfo.BackgroundUri;
			}
			set {
				OnImageInfo ().BackgroundUri = value;
				extraInfo.BackgroundColor = null;
			}
		}
			
		protected virtual string GetKey (int style)
		{
			return skey [style];
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var key = GetKey ((int) style);
			var cell = tv.DequeueReusableCell (key);
			if (cell == null){
				cell = new UITableViewCell (style, key);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			PrepareCell (cell);
			return cell;
		}
		
		protected void PrepareCell (UITableViewCell cell)
		{
			cell.Accessory = Accessory;
			var tl = cell.TextLabel;
			tl.Text = Caption;
			tl.TextAlignment = Alignment;
			tl.TextColor = TextColor ?? UIColor.Black;
			tl.Font = Font ?? UIFont.BoldSystemFontOfSize (17);
			tl.LineBreakMode = LineBreakMode;
			tl.Lines = Lines;	
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
			
			if (extraInfo == null){
				ClearBackground (cell);
			} else {
				var imgView = cell.ImageView;
				UIImage img;

				if (imgView != null) {
					if (extraInfo.Uri != null)
						img = ImageLoader.DefaultRequestImage (extraInfo.Uri, this);
					else if (extraInfo.Image != null)
						img = extraInfo.Image;
					else 
						img = null;
					imgView.Image = img;
				}

				if (cell.DetailTextLabel != null)
					cell.DetailTextLabel.TextColor = extraInfo.DetailColor ?? UIColor.Gray;
			}
				
			if (cell.DetailTextLabel != null){
				cell.DetailTextLabel.Lines = Lines;
				cell.DetailTextLabel.LineBreakMode = LineBreakMode;
				cell.DetailTextLabel.Font = SubtitleFont ?? UIFont.SystemFontOfSize (14);
				cell.DetailTextLabel.TextColor = (extraInfo == null || extraInfo.DetailColor == null) ? UIColor.Gray : extraInfo.DetailColor;
			}
		}	
	
		void ClearBackground (UITableViewCell cell)
		{
			cell.BackgroundColor = UIColor.White;
			cell.TextLabel.BackgroundColor = UIColor.Clear;
		}

		void IColorizeBackground.WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
			if (extraInfo == null){
				ClearBackground (cell);
				return;
			}
			
			if (extraInfo.BackgroundColor != null){
				cell.BackgroundColor = extraInfo.BackgroundColor;
				cell.TextLabel.BackgroundColor = UIColor.Clear;
			} else if (extraInfo.BackgroundUri != null){
				var img = ImageLoader.DefaultRequestImage (extraInfo.BackgroundUri, this);
				cell.BackgroundColor = img == null ? UIColor.White : UIColor.FromPatternImage (img);
				cell.TextLabel.BackgroundColor = UIColor.Clear;
			} else 
				ClearBackground (cell);
		}

		void IImageUpdated.UpdatedImage (Uri uri)
		{
			if (uri == null || extraInfo == null)
				return;
			var root = GetImmediateRootElement ();
			if (root == null || root.TableView == null)
				return;
			root.TableView.ReloadRows (new NSIndexPath [] { IndexPath }, UITableViewRowAnimation.None);
		}	
		
		internal void AccessoryTap ()
		{
			NSAction tapped = AccessoryTapped;
			if (tapped != null)
				tapped ();
		}
	}
	
}
