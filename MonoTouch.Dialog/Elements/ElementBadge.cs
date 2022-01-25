//
// ElementBadge.cs: defines the Badge Element.
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010, Novell, Inc.
//
// Code licensed under the MIT X11 license
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using UIKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

using NSAction = global::System.Action;

namespace MonoTouch.Dialog
{	
	/// <summary>
	///    This element can be used to show an image with some text
	/// </summary>
	/// <remarks>
	///    The font can be configured after the element has been created
	///    by assignign to the Font property;   If you want to render
	///    multiple lines of text, set the MultiLine property to true.
	/// 
	///    If no font is specified, it will default to Helvetica 17.
	/// 
	///    A static method MakeCalendarBadge is provided that can 
	///    render a calendar badge like the iPhone OS.   It will compose
	///    the text on top of the image which is expected to be 57x57
	/// </remarks>
	public partial class BadgeElement : Element, IElementSizing {
		static NSString ckey = new NSString ("badgeKey");
		public event NSAction Tapped;
		public UILineBreakMode LineBreakMode = UILineBreakMode.TailTruncation;
		public UIViewContentMode ContentMode = UIViewContentMode.Left;
		public int Lines = 1;
		public UITableViewCellAccessory Accessory = UITableViewCellAccessory.None;
		UIImage image;
		UIFont font;
	
		public BadgeElement (UIImage badgeImage, string cellText)
			: this (badgeImage, cellText, null)
		{
		}

		public BadgeElement (UIImage badgeImage, string cellText, NSAction tapped) : base (cellText)
		{
			if (badgeImage == null)
				throw new ArgumentNullException ("badgeImage");
			
			image = badgeImage;
			if (tapped != null)
				Tapped += tapped;
		}		
	
		public UIFont Font {
			get {
				if (font == null)
#if NET6_0 && !NET7_0
					font = UIFont.FromName ("Helvetica", new NFloat (17f));
#else
					font = UIFont.FromName ("Helvetica", 17f);
#endif
				return font;
			}
			set {
				if (font != null)
					font.Dispose ();
				font = value;
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (ckey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, ckey) {
					SelectionStyle = UITableViewCellSelectionStyle.Blue
				};
			}
			cell.Accessory = Accessory;
			var tl = cell.TextLabel;
			tl.Text = Caption;
			tl.Font = Font;
			tl.LineBreakMode = LineBreakMode;
			tl.Lines = Lines;
			tl.ContentMode = ContentMode;
			
			cell.ImageView.Image = image;
			
			return cell;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
#if NET6_0 && !NET7_0
			CGSize size = new CGSize (new NFloat (tableView.Bounds.Width.Value - 40), NFloatHelpers.MaxValue);
			nfloat height = new NFloat (Caption.StringSize (Font, size, LineBreakMode).Height.Value + 10);
#else
			CGSize size = new CGSize (tableView.Bounds.Width - 40, nfloat.MaxValue);
			nfloat height = Caption.StringSize (Font, size, LineBreakMode).Height + 10;
#endif
			
			// Image is 57 pixels tall, add some padding
#if NET6_0 && !NET7_0
			return new NFloat (Math.Max (height.Value, 63));
#else
			return (nfloat)Math.Max (height, 63);
#endif
		}

		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			if (Tapped != null)
				Tapped ();
			tableView.DeselectRow (path, true);
		}
		
		public static UIImage MakeCalendarBadge (UIImage template, string smallText, string bigText)
		{
			using (var cs = CGColorSpace.CreateDeviceRGB ()){
				using (var context = new CGBitmapContext (IntPtr.Zero, 57, 57, 8, 57*4, cs, CGImageAlphaInfo.PremultipliedLast)){
					//context.ScaleCTM (0.5f, -1);
					context.TranslateCTM (0, 0);
					context.DrawImage (new CGRect (0, 0, 57, 57), template.CGImage);
					context.SetFillColor (1, 1, 1, 1);
					
					context.SelectFont ("Helvetica", 10f, CGTextEncoding.MacRoman);
					
					// Pretty lame way of measuring strings, as documented:
					var start = context.TextPosition.X;					
					context.SetTextDrawingMode (CGTextDrawingMode.Invisible);
					context.ShowText (smallText);
#if NET6_0 && !NET7_0
					var width = new NFloat (context.TextPosition.X.Value - start.Value);
#else
					var width = context.TextPosition.X - start;
#endif
					
					context.SetTextDrawingMode (CGTextDrawingMode.Fill);
#if NET6_0 && !NET7_0
					context.ShowTextAtPoint ((57-(float) width.Value)/2, 46, smallText);
#else
					context.ShowTextAtPoint ((57-width)/2, 46, smallText);
#endif
					
					// The big string
					context.SelectFont ("Helvetica-Bold", 32, CGTextEncoding.MacRoman);					
					start = context.TextPosition.X;
					context.SetTextDrawingMode (CGTextDrawingMode.Invisible);
					context.ShowText (bigText);
#if NET6_0 && !NET7_0
					width = new NFloat (context.TextPosition.X.Value - start.Value);
#else
					width = context.TextPosition.X - start;
#endif
					
					context.SetFillColor (0, 0, 0, 1);
					context.SetTextDrawingMode (CGTextDrawingMode.Fill);
#if NET6_0 && !NET7_0
					context.ShowTextAtPoint ((57-(float) width.Value)/2, 9, bigText);
#else
					context.ShowTextAtPoint ((57-width)/2, 9, bigText);
#endif
					
					context.StrokePath ();
				
					return UIImage.FromImage (context.ToImage ());
				}
			}
		}
	}
}
