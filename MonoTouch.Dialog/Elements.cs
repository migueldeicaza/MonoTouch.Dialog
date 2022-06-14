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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using UIKit;
using CoreGraphics;
using Foundation;
using CoreAnimation;
using MonoTouch.Dialog.Utilities;

using NSAction = global::System.Action;

namespace MonoTouch.Dialog
{
	/// <summary>
	/// Base class for all elements in MonoTouch.Dialog
	/// </summary>
	public partial class Element : IDisposable {
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

	public abstract partial class BoolElement : Element {
		bool val;
		public virtual bool Value {
			get {
				return val;
			}
			set {
				bool emit = val != value;
				val = value;
				if (emit && ValueChanged != null)
					ValueChanged (this, EventArgs.Empty);
			}
		}
		public event EventHandler ValueChanged;
		
		public BoolElement (string caption, bool value) : base (caption)
		{
			val = value;
		}
		
		public override string Summary ()
		{
			return val ? "On".GetText () : "Off".GetText ();
		}		
	}
	
	/// <summary>
	/// Used to display switch on the screen.
	/// </summary>
	public partial class BooleanElement : BoolElement {
		static NSString bkey = new NSString ("BooleanElement");
#if !__TVOS__
		UISwitch sw;
#endif // !__TVOS__

		static UIFont font = UIFont.BoldSystemFontOfSize(17);

		public BooleanElement (string caption, bool value) : base (caption, value)
		{  }
		
		public BooleanElement (string caption, bool value, string key) : base (caption, value)
		{  }
		
		protected override NSString CellKey {
			get {
				return bkey;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
#if __TVOS__
			var cell = ConfigCell (base.GetCell (tv));
#else
			if (sw == null){
				sw = new UISwitch (){
					BackgroundColor = UIColor.Clear,
					Tag = 1,
					On = Value
				};
				sw.AddTarget (delegate {
					Value = sw.On;
				}, UIControlEvent.ValueChanged);
			} else
				sw.On = Value;
			
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				cell.TextLabel.Font = font;
			} else
				RemoveTag (cell, 1);
		
			cell.TextLabel.Text = Caption;
			cell.AccessoryView = sw;
#endif // !__TVOS__
			return cell;
		}

#if __TVOS__
		UITableViewCell ConfigCell (UITableViewCell cell)
		{
			cell.Accessory = Value ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
			cell.TextLabel.Text = Caption;
			return cell;
		}

		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			Value = !Value;
			var cell = tableView.CellAt (path);
			ConfigCell (cell);
			base.Selected (dvc, tableView, path);
		}
#endif // !__TVOS__

		protected override void Dispose (bool disposing)
		{
			if (disposing){
#if !__TVOS__
				if (sw != null){
					sw.Dispose ();
					sw = null;
				}
#endif // !__TVOS__
			}
		}
		
		public override bool Value {
			get {
				return base.Value;
			}
			set {
				 base.Value = value;
#if __TVOS__
				// Not sure what to do here
#else
				if (sw != null)
					sw.On = value;
#endif  // !__TVOS__
			}
		}
	}
	
	/// <summary>
	///  This class is used to render a string + a state in the form
	/// of an image.  
	/// </summary>
	/// <remarks>
	/// It is abstract to avoid making this element
	/// keep two pointers for the state images, saving 8 bytes per
	/// slot.   The more derived class "BooleanImageElement" shows
	/// one way to implement this by keeping two pointers, a better
	/// implementation would return pointers to images that were 
	/// preloaded and are static.
	/// 
	/// A subclass only needs to implement the GetImage method.
	/// </remarks>
	public abstract partial class BaseBooleanImageElement : BoolElement {
		static NSString key = new NSString ("BooleanImageElement");
		
		public class TextWithImageCellView : UITableViewCell {
			const int fontSize = 17;
			static UIFont font = UIFont.BoldSystemFontOfSize (fontSize);
			BaseBooleanImageElement parent;
			UILabel label;
			UIButton button;
			const int ImageSpace = 32;
			const int Padding = 8;
	
			public TextWithImageCellView (BaseBooleanImageElement parent_) : base (UITableViewCellStyle.Value1, parent_.CellKey)
			{
				parent = parent_;
				label = new UILabel () {
					TextAlignment = UITextAlignment.Left,
					Text = parent.Caption,
					Font = font,
					BackgroundColor = UIColor.Clear
				};
				button = UIButton.FromType (UIButtonType.Custom);
				button.TouchDown += delegate {
					parent.Value = !parent.Value;
					UpdateImage ();
					if (parent.Tapped != null)
						parent.Tapped ();
				};
				ContentView.Add (label);
				ContentView.Add (button);
				UpdateImage ();
			}

			void UpdateImage ()
			{
				button.SetImage (parent.GetImage (), UIControlState.Normal);
			}
			
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				var full = ContentView.Bounds;
				var frame = full;
				frame.Height = 22;
				frame.X = Padding;
				frame.Y = (full.Height-frame.Height)/2;
				frame.Width -= ImageSpace+Padding;
				label.Frame = frame;
				
				button.Frame = new CGRect (full.Width-ImageSpace, -3, ImageSpace, 48);
			}
			
			public void UpdateFrom (BaseBooleanImageElement newParent)
			{
				parent = newParent;
				UpdateImage ();
				label.Text = parent.Caption;
				SetNeedsDisplay ();
			}
		}
	
		public BaseBooleanImageElement (string caption, bool value)
			: base (caption, value)
		{
		}
		
		public event NSAction Tapped;
		
		protected abstract UIImage GetImage ();
		
		protected override NSString CellKey {
			get {
				return key;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey) as TextWithImageCellView;
			if (cell == null)
				cell = new TextWithImageCellView (this);
			else
				cell.UpdateFrom (this);
			return cell;
		}
	}
	
	public partial class BooleanImageElement : BaseBooleanImageElement {
		UIImage onImage, offImage;
		
		public BooleanImageElement (string caption, bool value, UIImage onImage, UIImage offImage) : base (caption, value)
		{
			this.onImage = onImage;
			this.offImage = offImage;
		}
		
		protected override UIImage GetImage ()
		{
			if (Value)
				return onImage;
			else
				return offImage;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			onImage = offImage = null;
		}
	}
	
	/// <summary>
	///  Used to display a slider on the screen.
	/// </summary>
	public partial class FloatElement : Element {
		public bool ShowCaption;
		public float Value;
		public float MinValue, MaxValue;
		static NSString skey = new NSString ("FloatElement");
		//UIImage Left, Right;
#if !__TVOS__
		// There is no UISlider in tvOS, so make this read-only for now.
		UISlider slider;
#endif // !__TVOS__

		public FloatElement (float value) : this (null, null, value)
		{
		}
		
		public FloatElement (UIImage left, UIImage right, float value) : base (null)
		{
			//Left = left;
			//Right = right;
			MinValue = 0;
			MaxValue = 1;
			Value = value;
		}
		
		protected override NSString CellKey {
			get {
				return skey;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag (cell, 1);

#if __TVOS__
			if (Caption != null && ShowCaption)
				cell.TextLabel.Text = Caption;

			cell.DetailTextLabel.Text = string.Format ("[{0}...{2}]: {1}]", MinValue, Value, MaxValue);
#else
			CGSize captionSize = new CGSize (0, 0);
			if (Caption != null && ShowCaption){
				cell.TextLabel.Text = Caption;
				captionSize = Caption.StringSize (UIFont.FromName (cell.TextLabel.Font.Name, UIFont.LabelFontSize));
				captionSize.Width += 10; // Spacing
			}
			if (slider == null){
				slider = new UISlider (new CGRect (10f + captionSize.Width, UIDevice.CurrentDevice.CheckSystemVersion (7, 0) ? 18f : 12f, cell.ContentView.Bounds.Width - 20 - captionSize.Width, 7f)) {
					BackgroundColor = UIColor.Clear,
					MinValue = this.MinValue,
					MaxValue = this.MaxValue,
					Continuous = true,
					Value = this.Value,
					Tag = 1,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth
				};
				slider.ValueChanged += delegate {
					Value = slider.Value;
				};
			} else {
				slider.Value = Value;
			}
			
			cell.ContentView.AddSubview (slider);
#endif // __TVOS__
			return cell;
		}

		public override string Summary ()
		{
			return Value.ToString ();
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
#if !__TVOS__
				if (slider != null){
					slider.Dispose ();
					slider = null;
				}
#endif // !__TVOS__
			}
		}		
	}

	/// <summary>
	///  Used to display a cell that will launch a web browser when selected.
	/// </summary>
	public partial class HtmlElement : Element {
		NSUrl nsUrl;
		static NSString hkey = new NSString ("HtmlElement");
#if !__TVOS__
		// There is no UIWebView in tvOS, so we can't launch anything.
		UIWebView web;
#endif // !__TVOS__
		
		public HtmlElement (string caption, string url) : base (caption)
		{
			Url = url;
		}
		
		public HtmlElement (string caption, NSUrl url) : base (caption)
		{
			nsUrl = url;
		}
		
		protected override NSString CellKey {
			get {
				return hkey;
			}
		}
		public string Url {
			get {
				return nsUrl.ToString ();
			}
			set {
				nsUrl = new NSUrl (value);
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			cell.TextLabel.Text = Caption;
			return cell;
		}

		static bool NetworkActivity {
			set {
#if !__TVOS__
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = value;
#endif // !__TVOS__
			}
		}
		
#if !__TVOS__
		// We use this class to dispose the web control when it is not
		// in use, as it could be a bit of a pig, and we do not want to
		// wait for the GC to kick-in.
		class WebViewController : UIViewController {
#pragma warning disable 414
			HtmlElement container;
#pragma warning restore 414
			
			public WebViewController (HtmlElement container) : base ()
			{
				this.container = container;
			}
			
			public bool Autorotate { get; set; }

			public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
			{
				return Autorotate;
			}
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			int i = 0;
			var vc = new WebViewController (this) {
				Autorotate = dvc.Autorotate
			};

			web = new UIWebView (UIScreen.MainScreen.Bounds) {
				BackgroundColor = UIColor.White,
				ScalesPageToFit = true,
				AutoresizingMask = UIViewAutoresizing.All
			};
			web.LoadStarted += delegate {
				// this is called several times and only one UIActivityIndicatorView is needed
				if (i++ == 0) {
					var indicator = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.White);
					vc.NavigationItem.RightBarButtonItem = new UIBarButtonItem (indicator);
					indicator.StartAnimating ();
				}
				NetworkActivity = true;
			};
			web.LoadFinished += delegate {
				if (--i == 0) {
					// we stopped loading, remove indicator and dispose of UIWebView
					vc.NavigationItem.RightBarButtonItem = null;
					web.StopLoading ();
					web.Dispose ();
				}
				NetworkActivity = false;
			};
			web.LoadError += (webview, args) => {
				NetworkActivity = false;
				vc.NavigationItem.RightBarButtonItem = null;
				if (web != null)
					web.LoadHtmlString (
						String.Format ("<html><center><font size=+5 color='red'>{0}:<br>{1}</font></center></html>",
						"An error occurred:".GetText (), args.Error.LocalizedDescription), null);
			};
			vc.NavigationItem.Title = Caption;
			
			vc.View.AutosizesSubviews = true;
			vc.View.AddSubview (web);
			
			dvc.ActivateController (vc);
			web.LoadRequest (NSUrlRequest.FromUrl (nsUrl));
		}
#endif // !__TVOS__
	}

	/// <summary>
	///   The string element can be used to render some text in a cell 
	///   that can optionally respond to tap events.
	/// </summary>
	public partial class StringElement : Element {
		static NSString skey = new NSString ("StringElement");
		static NSString skeyvalue = new NSString ("StringElementValue");
		public UITextAlignment Alignment = UITextAlignment.Left;
		public string Value;
		
		public StringElement (string caption) : base (caption) {}
		
		public StringElement (string caption, string value) : base (caption)
		{
			this.Value = value;
		}
		
		public StringElement (string caption,  NSAction tapped) : base (caption)
		{
			Tapped += tapped;
		}
		
		public event NSAction Tapped;
				
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (Value == null ? skey : skeyvalue);
			cell = null;
			if (cell == null){
				cell = new UITableViewCell (Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Value1, Value == null ? skey : skeyvalue);
				cell.SelectionStyle = (Tapped != null) ? UITableViewCellSelectionStyle.Blue : UITableViewCellSelectionStyle.None;
			}
			cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
			
			return cell;
		}

		public override string Summary ()
		{
			return Value ?? Caption;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			if (Tapped != null)
				Tapped ();
			tableView.DeselectRow (indexPath, true);
		}
		
		public override bool Matches (string text)
		{
			return (Value != null ? Value.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1: false) || base.Matches (text);
		}
	}
	
	/// <summary>
	///   A version of the StringElement that can be styled with a number of formatting 
	///   options and can render images or background images either from UIImage parameters 
	///   or by downloading them from the net.
	/// </summary>
	public partial class StyledStringElement : StringElement, IImageUpdated, IColorizeBackground
	{
		static NSString[] skey = { new NSString(".1"), new NSString(".2"), new NSString(".3"), new NSString(".4"), new NSString(".5") };

		public StyledStringElement(string caption) : base(caption) { }
		public StyledStringElement(string caption, NSAction tapped) : base(caption, tapped) { }
		public StyledStringElement(string caption, string value) : base(caption, value)
		{
			style = UITableViewCellStyle.Value1;	
		}
		public StyledStringElement (string caption, string value, UITableViewCellStyle style) : base (caption, value) 
		{ 
			this.style = style;
		}
		
		protected UITableViewCellStyle style;
		public event NSAction AccessoryTapped;
		private UIFont font;
		private UIFont subtitleFont;
		public UIColor TextColor;
		public UILineBreakMode LineBreakMode = UILineBreakMode.WordWrap;
		public int Lines = 0;
		/// <summary>
		/// Used to center the Image when Caption is also centered.
		/// </summary>
		public bool IsImageCentered;
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

		public UIFont SubtitleFont
		{
			get
			{
				if (subtitleFont == null)
				{
					subtitleFont = UIFont.SystemFontOfSize(17);
				}
				return subtitleFont;
			}
			set => subtitleFont = value;
		}
		public UIFont Font
		{
			get
			{
				if (font == null)
				{
					font = UIFont.BoldSystemFontOfSize(17);
				}
				return font;
			}
			set => font = value;
		}

        public class UICenteredImageTableViewCell : UITableViewCell
        {
			public UICenteredImageTableViewCell(StyledStringElement element, UITableViewCellStyle style, string reuseIdentifier)
				: base(style, reuseIdentifier)
			{
				Element = element;
			}

            public StyledStringElement Element { get; }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

				var captionSize = Element.Caption.StringSize(Element.Font, TextLabel.Bounds.Size, Element.LineBreakMode).Width;
				var x = (ContentView.Bounds.Size.Width - captionSize) / 2 - 6;
				ImageView.Center = new CGPoint(x, ContentView.Bounds.Size.Height / 2);
			}
        }

        protected virtual string GetKey (int style)
		{
			return skey [style];
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			bool centeredImage = false;
			if ((extraInfo?.Image != null || extraInfo?.Uri != null)
				&& IsImageCentered
				&& Alignment == UITextAlignment.Center)
			{
				centeredImage = true;
			}
			var key = GetKey(centeredImage ? 4 : (int)style);
			var cell = centeredImage ? null : tv.DequeueReusableCell(key);
			if (cell == null)
			{
				cell = centeredImage
					? new UICenteredImageTableViewCell(this, style, key)
					: new UITableViewCell(style, key);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			PrepareCell(cell);
			return cell;
		}
		
		protected void PrepareCell (UITableViewCell cell)
		{
			cell.Accessory = Accessory;
			var tl = cell.TextLabel;
			tl.Text = Caption;
			tl.TextAlignment = Alignment;
			tl.TextColor = TextColor ?? UIColor.Black;
			tl.Font = Font;
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
				cell.DetailTextLabel.Font = SubtitleFont ?? UIFont.SystemFontOfSize (17);
				cell.DetailTextLabel.TextColor = (extraInfo == null || extraInfo.DetailColor == null) ? UIColor.Gray : extraInfo.DetailColor;
			}
		}	
	
		void ClearBackground (UITableViewCell cell)
		{
			cell.BackgroundColor = UITableViewCell.Appearance.BackgroundColor;
			cell.TextLabel.BackgroundColor = UIColor.Clear;
		}

		void IColorizeBackground.WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
			if (extraInfo == null){
				ClearBackground (cell);
				return;
			}
			
			if (extraInfo.BackgroundColor != null){
				cell.TextLabel.BackgroundColor = UIColor.Clear;
				cell.BackgroundColor = extraInfo.BackgroundColor;
			} else if (extraInfo.BackgroundUri != null){
				var img = ImageLoader.DefaultRequestImage (extraInfo.BackgroundUri, this);
				cell.TextLabel.BackgroundColor = UIColor.Clear;
				cell.BackgroundColor = img == null ? UIColor.White : UIColor.FromPatternImage (img);
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

#if __TVOS__
	internal static class Helper {

		static public CGSize StringSize (this string self, UIFont font)
		{
			using (var str = (NSString) self) {
				return str.GetSizeUsingAttributes (new UIStringAttributes ()
				{
					Font = font,
				});
			}
		}

		static public CGSize StringSize (this string self, UIFont font, CGSize constrainedToSize, UILineBreakMode lineBreakMode)
		{
			using (var str = (NSString) self) {
				return StringSize (str, font, constrainedToSize, lineBreakMode);
			}
		}

		static public CGSize StringSize (this NSString self, UIFont font, CGSize constrainedToSize, UILineBreakMode lineBreakMode)
		{
			return self.GetBoundingRect (constrainedToSize, NSStringDrawingOptions.UsesLineFragmentOrigin, new UIStringAttributes ()
			{
				Font = font,
				ParagraphStyle = new NSMutableParagraphStyle ()
				{
					LineBreakMode = lineBreakMode,
				},
			}, null).Size;
		}

		static public CGSize StringSize (this NSString self, UIFont font, float forWidth, UILineBreakMode lineBreakMode)
		{
			return StringSize (self, font, new CGSize (forWidth, nfloat.MaxValue), lineBreakMode);
		}

		static public void DrawString (this string self, CGRect rect, UIFont font)
		{
			using (var str = (NSString) self) {
				NSStringDrawing.DrawString (str, rect, new UIStringAttributes ()
				{
					Font = font,
				});
			}
		}

		static public void DrawString (this string self, CGPoint point, float width, UIFont font, UILineBreakMode lineBreakMode)
		{
			using (var str = (NSString) self) {
				NSStringDrawing.DrawString (str, point, new UIStringAttributes ()
				{
					Font = font,
					ParagraphStyle = new NSMutableParagraphStyle ()
					{
						LineBreakMode = lineBreakMode,
					},
				});
			}
		}

		static public void DrawString (this string self, CGRect rect, UIFont font, UILineBreakMode lineBreakMode, UITextAlignment alignment)
		{
			using (var str = (NSString) self) {
				NSStringDrawing.DrawString (str, rect, new UIStringAttributes ()
				{
					Font = font,
					ParagraphStyle = new NSMutableParagraphStyle ()
					{
						LineBreakMode = lineBreakMode,
						Alignment = alignment,
					},
				});
			}
		}
	}
#endif
	
	public partial class StyledMultilineElement : StyledStringElement, IElementSizing {
		public StyledMultilineElement (string caption) : base (caption) {}
		public StyledMultilineElement (string caption, string value) : base (caption, value) {}
		public StyledMultilineElement (string caption, NSAction tapped) : base (caption, tapped) {}
		public StyledMultilineElement (string caption, string value, UITableViewCellStyle style) : base (caption, value) 
		{ 
			this.style = style;
		}

		public virtual nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			float margin = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 40f : 110f;
			CGSize maxSize = new CGSize (tableView.Bounds.Width - margin, float.MaxValue);
			
			if (this.Accessory != UITableViewCellAccessory.None)
				maxSize.Width -= 20;

			string c = Caption;
			string v = Value;
			// ensure the (multi-line) Value will be rendered inside the cell when no Caption is present
			if (String.IsNullOrEmpty (c))
				c = " ";

			var captionFont = Font;
			var captionSize = c.StringSize(captionFont, maxSize, LineBreakMode);
			var height = captionSize.Height;
			
			if (!String.IsNullOrEmpty (v)) {
				var subtitleFont = SubtitleFont;
				if (this.style == UITableViewCellStyle.Subtitle) {
					height += v.StringSize (subtitleFont, maxSize, LineBreakMode).Height;
				} else {
					maxSize.Width -= captionSize.Width;
					var vheight = v.StringSize (subtitleFont, maxSize, LineBreakMode).Height;
					if (vheight > height)
						height = vheight;
				}
			}
			
			return (nfloat)Math.Max(height + 10, 43);
		}
	}
	
	public partial class ImageStringElement : StringElement {
		static NSString skey = new NSString ("ImageStringElement");
		UIImage image;
		public UITableViewCellAccessory Accessory { get; set; }
		
		public ImageStringElement (string caption, UIImage image) : base (caption)
		{
			this.image = image;
			this.Accessory = UITableViewCellAccessory.None;
		}

		public ImageStringElement (string caption, string value, UIImage image) : base (caption, value)
		{
			this.image = image;
			this.Accessory = UITableViewCellAccessory.None;
		}
		
		public ImageStringElement (string caption,  NSAction tapped, UIImage image) : base (caption, tapped)
		{
			this.image = image;
			this.Accessory = UITableViewCellAccessory.None;
		}
		
		protected override NSString CellKey {
			get {
				return skey;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Subtitle, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			
			cell.Accessory = Accessory;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;
			
			cell.ImageView.Image = image;
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
			
			return cell;
		}
		
	}
	
	/// <summary>
	///   This interface is implemented by Element classes that will have
	///   different heights
	/// </summary>
	public interface IElementSizing {
		nfloat GetHeight (UITableView tableView, NSIndexPath indexPath);
	}
	
	/// <summary>
	///   This interface is implemented by Elements that needs to update
	///   their cells Background properties just before they are displayed
	///   to the user.   This is an iOS 3 requirement to properly render
	///   a cell.
	/// </summary>
	public interface IColorizeBackground {
		void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath);
	}
	
	public partial class MultilineElement : StringElement, IElementSizing {
		public MultilineElement (string caption) : base (caption)
		{
		}
		
		public MultilineElement (string caption, string value) : base (caption, value)
		{
		}
		
		public MultilineElement (string caption, NSAction tapped) : base (caption, tapped)
		{
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);				
			var tl = cell.TextLabel;
			tl.LineBreakMode = UILineBreakMode.WordWrap;
			tl.Lines = 0;

			return cell;
		}
		
		public virtual nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			float margin = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 40f : 110f;
			CGSize size = new CGSize (tableView.Bounds.Width - margin, float.MaxValue);
			UIFont font = UIFont.BoldSystemFontOfSize (17);
			string c = Caption;
			// ensure the (single-line) Value will be rendered inside the cell
			if (String.IsNullOrEmpty (c) && !String.IsNullOrEmpty (Value))
				c = " ";
			return c.StringSize (font, size, UILineBreakMode.WordWrap).Height + 10;
		}
	}
	
	public partial class RadioElement : StringElement {
		public string Group;
		internal int? RadioIdx;
		
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
				root.RadioSelected = RadioIdx.Value;
			}
			
			base.Selected (dvc, tableView, indexPath);
		}
	}
	
	public partial class CheckboxElement : StringElement {
		public new bool Value;
		public string Group;
		
		public CheckboxElement (string caption) : base (caption) {}
		public CheckboxElement (string caption, bool value) : base (caption)
		{
			Value = value;
		}
		
		public CheckboxElement (string caption, bool value, string group) : this (caption, value)
		{
			Group = group;
		}
		
		UITableViewCell ConfigCell (UITableViewCell cell)
		{
			cell.Accessory = Value ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
			return cell;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			return  ConfigCell (base.GetCell (tv));
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			Value = !Value;
			var cell = tableView.CellAt (path);
			ConfigCell (cell);
			base.Selected (dvc, tableView, path);
		}

	}
	
	public partial class ImageElement : Element {
		public UIImage Value;
		static CGRect rect = new CGRect (0, 0, dimx, dimy);
		static NSString ikey = new NSString ("ImageElement");
		UIImage scaled;

		// There's no UIImagePickerController in tvOS (and I couldn't find any suitable replacement either).
#if !__TVOS__
		UIPopoverController popover;
		
		// Apple leaks this one, so share across all.
		static UIImagePickerController picker;
#endif // !__TVOS__
		
		// Height for rows
		const int dimx = 48;
		const int dimy = 43;
		
		// radius for rounding
		const int rad = 10;
		
		static UIImage MakeEmpty ()
		{
			using (var cs = CGColorSpace.CreateDeviceRGB ()){
				using (var bit = new CGBitmapContext (IntPtr.Zero, dimx, dimy, 8, 0, cs, CGImageAlphaInfo.PremultipliedFirst)){
					bit.SetStrokeColor (1, 0, 0, 0.5f);
					bit.FillRect (new CGRect (0, 0, dimx, dimy));
					
					return UIImage.FromImage (bit.ToImage ());
				}
			}
		}
		
		UIImage Scale (UIImage source)
		{
			UIGraphics.BeginImageContext (new CGSize (dimx, dimy));
			var ctx = UIGraphics.GetCurrentContext ();
		
			var img = source.CGImage;
			ctx.TranslateCTM (0, dimy);
			if (img.Width > img.Height)
				ctx.ScaleCTM (1, (nfloat)(-img.Width/dimy));
			else
				ctx.ScaleCTM ((nfloat)img.Height/dimx, -1);

			ctx.DrawImage (rect, source.CGImage);
			
			var ret = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return ret;
		}
		
		public ImageElement (UIImage image) : base ("")
		{
			if (image == null){
				Value = MakeEmpty ();
				scaled = Value;
			} else {
				Value = image;			
				scaled = Scale (Value);
			}
		}
		
		protected override NSString CellKey {
			get {
				return ikey;
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
			}
			
			if (scaled == null)
				return cell;
			
			Section psection = Parent as Section;
			bool roundTop = psection.Elements [0] == this;
			bool roundBottom = psection.Elements [psection.Elements.Count-1] == this;
			
			using (var cs = CGColorSpace.CreateDeviceRGB ()){
				using (var bit = new CGBitmapContext (IntPtr.Zero, dimx, dimy, 8, 0, cs, CGImageAlphaInfo.PremultipliedFirst)){
					// Clipping path for the image, different on top, middle and bottom.
					if (roundBottom){
						bit.AddArc (rad, rad, rad, (float) Math.PI, (float) (3*Math.PI/2), false);
					} else {
						bit.MoveTo (0, rad);
						bit.AddLineToPoint (0, 0);
					}
					bit.AddLineToPoint (dimx, 0);
					bit.AddLineToPoint (dimx, dimy);
					
					if (roundTop){
						bit.AddArc (rad, dimy-rad, rad, (float) (Math.PI/2), (float) Math.PI, false);
						bit.AddLineToPoint (0, rad);
					} else {
						bit.AddLineToPoint (0, dimy);
					}
					bit.Clip ();
					bit.DrawImage (rect, scaled.CGImage);
					
					cell.ImageView.Image = UIImage.FromImage (bit.ToImage ());
				}
			}			
			return cell;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (scaled != null){
					scaled.Dispose ();
					Value.Dispose ();
					scaled = null;
					Value = null;
				}
			}
			base.Dispose (disposing);
		}

#if !__TVOS__
		class MyDelegate : UIImagePickerControllerDelegate {
			ImageElement container;
			UITableView table;
			NSIndexPath path;
			
			public MyDelegate (ImageElement container, UITableView table, NSIndexPath path)
			{
				this.container = container;
				this.table = table;
				this.path = path;
			}
			
			public override void FinishedPickingImage (UIImagePickerController picker, UIImage image, NSDictionary editingInfo)
			{
				container.Picked (image);
				table.ReloadRows (new NSIndexPath [] { path }, UITableViewRowAnimation.None);
			}
		}
		
		void Picked (UIImage image)
		{
			Value = image;
			scaled = Scale (image);
			currentController.DismissModalViewController (true);
		}
		
		UIViewController currentController;
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			if (picker == null)
				picker = new UIImagePickerController ();
			picker.Delegate = new MyDelegate (this, tableView, path);
			
			switch (UIDevice.CurrentDevice.UserInterfaceIdiom){
			case UIUserInterfaceIdiom.Pad:
				CGRect useRect;
				popover = new UIPopoverController (picker);
				var cell = tableView.CellAt (path);
				if (cell == null)
					useRect = rect;
				else
					useRect = cell.Frame;
				popover.PresentFromRect (useRect, dvc.View, UIPopoverArrowDirection.Any, true);
				break;
				
			default:
			case UIUserInterfaceIdiom.Phone:
				dvc.ActivateController (picker);
				break;
			}
			currentController = dvc;
		}
#endif // !__TVOS__
	}
	
	/// <summary>
	/// An element that can be used to enter text.
	/// </summary>
	/// <remarks>
	/// This element can be used to enter text both regular and password protected entries. 
	///     
	/// The Text fields in a given section are aligned with each other.
	/// </remarks>
	public partial class EntryElement : Element {
		/// <summary>
		///   The value of the EntryElement
		/// </summary>
		public virtual string Value
		{
			get
			{
				if (entry == null)
				{
					return val;
				}
				var newValue = entry.Text;
				if (newValue == val || val == null && newValue == "")
				{
					return val;
				}
				val = newValue;
				OnChanged();
				return val;
			}
			set
			{
				val = value;
				if (entry != null)
				{
					entry.Text = value;
				}
			}
		}
		protected string val;

		protected void OnChanged()
        {
			Changed?.Invoke(this, EventArgs.Empty);
        }

		/// <summary>
		/// The key used for reusable UITableViewCells.
		/// </summary>
		static NSString entryKey = new NSString ("EntryElement");
		protected virtual NSString EntryKey {
			get {
				return entryKey;
			}
		}

		/// <summary>
		/// The type of keyboard used for input, you can change
		/// this to use this for numeric input, email addressed,
		/// urls, phones.
		/// </summary>
		public virtual UIKeyboardType KeyboardType {
			get {
				return keyboardType;
			}
			set {
				keyboardType = value;
				if (entry != null)
					entry.KeyboardType = value;
			}
		}

		/// <summary>
		/// The type of Return Key that is displayed on the
		/// keyboard, you can change this to use this for
		/// Done, Return, Save, etc. keys on the keyboard
		/// </summary>
		public virtual UIReturnKeyType? ReturnKeyType {
			get {
				return returnKeyType;
			}
			set {
				returnKeyType = value;
				if (entry != null && returnKeyType.HasValue)
					entry.ReturnKeyType = returnKeyType.Value;
			}
		}

		/// <summary>
		/// The default value for this property is <c>false</c>. If you set it to <c>true</c>, the keyboard disables the return key when the text entry area contains no text. As soon as the user enters any text, the return key is automatically enabled.
		/// </summary>
		public virtual bool EnablesReturnKeyAutomatically {
			get {
				return enablesReturnKeyAutomatically;
			}
			set {
				enablesReturnKeyAutomatically = value;
				if (entry != null)
					entry.EnablesReturnKeyAutomatically = value;
			}
		}

		public virtual UITextAutocapitalizationType AutocapitalizationType {
			get {
				return autocapitalizationType;	
			}
			set { 
				autocapitalizationType = value;
				if (entry != null)
					entry.AutocapitalizationType = value;
			}
		}

		public virtual UITextAutocorrectionType AutocorrectionType { 
			get { 
				return autocorrectionType;
			}
			set { 
				autocorrectionType = value;
				if (entry != null)
					entry.AutocorrectionType = value;
			}
		}

		public virtual UITextFieldViewMode ClearButtonMode { 
			get { 
				return clearButtonMode;
			}
			set { 
				clearButtonMode = value;
				if (entry != null)
					entry.ClearButtonMode = value;
			}
		}

		public virtual UITextAlignment TextAlignment {
			get {
				return textalignment;
			}
			set{
				textalignment = value;
				if (entry != null) {
					entry.TextAlignment = textalignment;
				}
			}
		}

		public bool AlignEntryWithAllSections { get; set; }

		public bool NotifyChangedOnKeyStroke { get; set; }

		/// <summary>
		/// Add a toolbar with Done button to hide keyboard on iPhone.
		/// </summary>
		public bool HideKeyboardWithDoneButton { get; set; }

		/// <summary>
		/// Show space char in the end of right aligned entry input.
		/// </summary>
		public bool VisibleSpaceEnding { get; set; }

		protected UITextAlignment textalignment = UITextAlignment.Left;
		protected UIKeyboardType keyboardType = UIKeyboardType.Default;
		protected UIReturnKeyType? returnKeyType = null;
		protected bool enablesReturnKeyAutomatically = false;
		protected UITextAutocapitalizationType autocapitalizationType = UITextAutocapitalizationType.Sentences;
		protected UITextAutocorrectionType autocorrectionType = UITextAutocorrectionType.Default;
		UITextFieldViewMode clearButtonMode = UITextFieldViewMode.Never;
		protected bool isPassword;
		protected bool becomeResponder;
		UITextField entry;
		string placeholder;
		protected static UIFont font = UIFont.BoldSystemFontOfSize (17);

		public event EventHandler Changed;
		public event Func<bool> ShouldReturn;
		public EventHandler EntryStarted {get;set;}
		public EventHandler EntryEnded {get;set;}
		/// <summary>
		/// Constructs an EntryElement with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display when no value is set.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		public EntryElement (string caption, string placeholder, string value) : base (caption)
		{ 
			Value = value;
			this.placeholder = placeholder;
		}
		
		/// <summary>
		/// Constructs an EntryElement for password entry with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use.
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display when no value is set.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		/// <param name="isPassword">
		/// True if this should be used to enter a password.
		/// </param>
		public EntryElement (string caption, string placeholder, string value, bool isPassword) : base (caption)
		{
			Value = value;
			this.isPassword = isPassword;
			this.placeholder = placeholder;
		}

		public override string Summary ()
		{
			return Value;
		}

		protected const float MaxEntryLabelWidth = 160;
		protected const float EntryLabelXOffset = 42;

		// 
		// Computes the X position for the entry by aligning all the entries in the Section
		//
		protected virtual CGSize ComputeEntryPosition(UITableViewCell cell)
		{
			nfloat maxWidth = 10 - EntryLabelXOffset; // If all EntryElements have a null Caption, align UITextField with the Caption offset of normal cells (at 10px).
			nfloat maxHeight = font.LineHeight;

			// Determine if we should calculate accross all sections or just the current section.
			var sections = AlignEntryWithAllSections ? (Parent.Parent as RootElement).Sections : (new[] { Parent as Section }).AsEnumerable();

			foreach (var s in sections)
			{
				foreach (var e in s.Elements)
				{
                    if (e is EntryElement ee
                        && !string.IsNullOrEmpty(ee.Caption))
                    {
                        var size = ee.Caption.StringSize(font);
                        maxWidth = (nfloat)Math.Max(size.Width, maxWidth);
                        maxHeight = (nfloat)Math.Max(size.Height, maxHeight);
                    }
                }
			}

			return new CGSize(EntryLabelXOffset + (nfloat)Math.Min(maxWidth, MaxEntryLabelWidth), maxHeight);
		}

		static readonly NSString passwordKey = new NSString ("EntryElement+Password");
		static readonly NSString cellkey = new NSString ("EntryElement");
		
		protected override NSString CellKey
		{
			get
			{
				return isPassword ? passwordKey : cellkey;
			}
		}

		protected class EntryCell : UITableViewCell
		{
			public UITextField Entry;
			readonly EntryElement Element;

			public EntryCell(EntryElement element, string key) : base(UITableViewCellStyle.Default, key)
			{
				Element = element;

				Entry = new UITextField()
				{
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin,
					Placeholder = element.placeholder ?? "",
					SecureTextEntry = element.isPassword,
					Text = element.Value ?? "",
					Tag = 1,
					TextAlignment = element.textalignment,
					ClearButtonMode = element.ClearButtonMode
				};
				ContentView.AddSubview(Entry);

				SelectionStyle = UITableViewCellSelectionStyle.None;
				TextLabel.Font = font;
			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				var size = Element.ComputeEntryPosition(this);

				var f = TextLabel.Frame;
				f.Y = (44 - font.LineHeight) / 2;
				f.Height = font.LineHeight;
				f.Width = size.Width - EntryLabelXOffset + 5;
				TextLabel.Frame = f;

				var yOffset = (ContentView.Bounds.Height - size.Height) / 2;
				var width = ContentView.Bounds.Width - size.Width;
				if (Element.textalignment == UITextAlignment.Right)
				{
					width -= 10;
				}
				Entry.Frame = new CGRect(size.Width, yOffset, width, size.Height);
			}
		}

		private EntryCell cell;
		public override UITableViewCell GetCell(UITableView tv)
		{
			if (cell == null)
			{
				cell = new EntryCell(this, CellKey);
			}
			cell.TextLabel.Text = Caption;

			if (entry == null) {
				entry = cell.Entry;
				entry.EditingChanged += delegate {
					if (VisibleSpaceEnding 
						&& textalignment == UITextAlignment.Right
						&& entry.Text.EndsWith(' '))
                    {
						entry.Text = entry.Text.Substring(0, entry.Text.Length - 1) + '\u00a0';
                    }
					if (NotifyChangedOnKeyStroke) 
					{
						FetchValue();
					}
				};
				entry.ValueChanged += delegate {
					FetchValue ();
				};
				entry.Ended += delegate {					
					FetchValue ();
					if (EntryEnded != null) {
						EntryEnded (this, null);
					}
				};
				entry.ShouldReturn += delegate {
					
					if (ShouldReturn != null)
						return ShouldReturn ();
					
					RootElement root = GetImmediateRootElement ();
					EntryElement focus = null;
					
					if (root == null)
						return true;
					
					foreach (var s in root.Sections) {
						foreach (var e in s.Elements) {
							if (e == this) {
								focus = this;
							} else if (focus != null && e is EntryElement) {
								focus = e as EntryElement;
								break;
							}
						}
						
						if (focus != null && focus != this)
							break;
					}
					
					if (focus != this)
						focus.BecomeFirstResponder (true);
					else 
						focus.ResignFirstResponder (true);
					
					return true;
				};
				entry.Started += delegate {
					EntryElement self = null;
					
					if (EntryStarted != null) {
						EntryStarted (this, null);
					}
					
					if (!returnKeyType.HasValue) {
						var returnType = UIReturnKeyType.Default;
						
						foreach (var e in (Parent as Section).Elements) {
							if (e == this)
								self = this;
							else if (self != null && e is EntryElement)
								returnType = UIReturnKeyType.Next;
						}
						entry.ReturnKeyType = returnType;
					} else
						entry.ReturnKeyType = returnKeyType.Value;
					
					tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, true);
				};
				cell.ContentView.AddSubview (entry);
			}

			entry.InputAccessoryView = GetEntryToolbar(tv);
			entry.KeyboardType = KeyboardType;
			entry.EnablesReturnKeyAutomatically = EnablesReturnKeyAutomatically;
			entry.AutocapitalizationType = AutocapitalizationType;
			entry.AutocorrectionType = AutocorrectionType;

			if (becomeResponder)
			{
				entry.BecomeFirstResponder();
				becomeResponder = false;
			}

			return cell;
		}

		protected virtual UIToolbar GetEntryToolbar(UITableView tv)
        {
            if (!HideKeyboardWithDoneButton)
            {
                return null;
            }

            return GetHideKeyboardToolbar(tv);
        }

        public static UIToolbar GetHideKeyboardToolbar(UITableView tv)
        {
			if (UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone)
			{
				return null;
			}

            var toolbar = new UIToolbar()
            {
                Frame = new CGRect(0, 0, tv.Frame.Width, 30)
            };
            var flexSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            var doneBtn = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) => tv.EndEditing(true));
            toolbar.SetItems(new UIBarButtonItem[] { flexSpace, doneBtn }, false);
            toolbar.SizeToFit();
            return toolbar;
        }

		/// <summary>
		///  Copies the value from the UITextField in the EntryElement to the
		///  Value property and raises the Changed event if necessary.
		/// </summary>
		public virtual void FetchValue()
		{
			if (entry == null)
			{
				return;
			}

			var newValue = entry.Text;
			if (VisibleSpaceEnding)
			{
				newValue = newValue?.Replace('\u00a0', ' ');
			}
			if (newValue == Value)
			{
				return;
			}

			Value = newValue;

			OnChanged();
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (entry != null){
					entry.Dispose ();
					entry = null;
				}
			}
		}

		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			BecomeFirstResponder(true);
			tableView.DeselectRow (indexPath, true);
		}
		
		public override bool Matches (string text)
		{
			return (Value != null ? Value.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1: false) || base.Matches (text);
		}
		
		/// <summary>
		/// Makes this cell the first responder (get the focus)
		/// </summary>
		/// <param name="animated">
		/// Whether scrolling to the location of this cell should be animated
		/// </param>
		public virtual void BecomeFirstResponder (bool animated)
		{
			becomeResponder = true;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (GetResponder() != null){
				GetResponder().BecomeFirstResponder ();
				becomeResponder = false;
			}
		}

		public virtual void ResignFirstResponder (bool animated)
		{
			becomeResponder = false;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (GetResponder() != null)
				GetResponder().ResignFirstResponder ();
		}

		protected virtual UIResponder GetResponder()
        {
			return entry;
        }
	}
	
	public partial class DateTimeElement : StringElement {
		public DateTime DateValue;
		// There's no UIDatePicker for tvOS, so this is a read-only element for now
#if !__TVOS__
		public UIDatePicker datePicker;
#endif
		public int MinuteInterval = 1;
#pragma warning disable 67 // The event 'X' is never used
		public event Action<DateTimeElement> DateSelected;
#pragma warning restore 67
		public UIColor BackgroundColor = (UIDevice.CurrentDevice.CheckSystemVersion (7, 0)) ? UIColor.White : UIColor.Black;
		
		protected internal NSDateFormatter fmt = new NSDateFormatter () {
			DateStyle = NSDateFormatterStyle.Short
		};

		public DateTimeElement (string caption, DateTime date) : base (caption)
		{
			DateValue = date;
			Value = FormatDate (date);
		}	
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			Value = FormatDate (DateValue);
			var cell = base.GetCell (tv);
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			return cell;
		}
 
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing){
				if (fmt != null){
					fmt.Dispose ();
					fmt = null;
				}
#if !__TVOS__
				if (datePicker != null){
					datePicker.Dispose ();
					datePicker = null;
				}
#endif // !__TVOS__
			}
		}
		
		protected DateTime GetDateWithKind (DateTime dt)
		{
			if (dt.Kind == DateTimeKind.Unspecified)
				return DateTime.SpecifyKind (dt, DateTimeKind.Local);

			return dt;
		}
		
		public virtual string FormatDate (DateTime dt)
		{
			dt = GetDateWithKind (dt);
			return fmt.ToString ((NSDate) dt) + " " + dt.ToLocalTime ().ToShortTimeString ();
		}
		
#if !__TVOS__
		public virtual UIDatePicker CreatePicker ()
		{
			var picker = new UIDatePicker (CGRect.Empty){
				AutoresizingMask = UIViewAutoresizing.FlexibleMargins,
				Mode = UIDatePickerMode.DateAndTime,
				Date = (NSDate) GetDateWithKind(DateValue),
				MinuteInterval = MinuteInterval
			};
			return picker;
		}
		                                                                                                                                                                                                                                                            
		class MyViewController : UIViewController {
			DateTimeElement container;
			
			public MyViewController (DateTimeElement container)
			{
				this.container = container;
			}
			
			public override void ViewWillDisappear (bool animated)
			{
				base.ViewWillDisappear (animated);
				container.DateValue = (DateTime) container.datePicker.Date;
				if (container.DateSelected != null)
					container.DateSelected (container);
			}
			
			public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
			{
				base.DidRotate (fromInterfaceOrientation);
				container.datePicker.Center = this.View.Center;
			}
			
			public bool Autorotate { get; set; }
			
			public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
			{
				return Autorotate;
			}
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var vc = new MyViewController (this) {
				Autorotate = dvc.Autorotate
			};
			datePicker = CreatePicker ();
			                            
			vc.View.BackgroundColor = BackgroundColor;
			vc.View.AddSubview (datePicker);
			dvc.ActivateController (vc);

			datePicker.Center = vc.View.Center;
		}
#endif // !__TVOS__                                                                                                                     
	}
	
	public partial class DateElement : DateTimeElement {
		public DateElement (string caption, DateTime date) : base (caption, date)
		{
			fmt.DateStyle = NSDateFormatterStyle.Medium;
		}
		
		public override string FormatDate (DateTime dt)
		{
			return fmt.ToString ((NSDate) GetDateWithKind (dt));
		}
		
#if !__TVOS__
		public override UIDatePicker CreatePicker ()
		{
			var picker = base.CreatePicker ();
			picker.Mode = UIDatePickerMode.Date;
			return picker;
		}
#endif // !__TVOS__
	}
	
	public partial class TimeElement : DateTimeElement {
		public TimeElement (string caption, DateTime date) : base (caption, date)
		{
		}
		
		public override string FormatDate (DateTime dt)
		{
			return GetDateWithKind (dt).ToLocalTime ().ToShortTimeString ();
		}
		
#if !__TVOS__
		public override UIDatePicker CreatePicker ()
		{
			var picker = base.CreatePicker ();
			picker.Mode = UIDatePickerMode.Time;
			picker.MinuteInterval = MinuteInterval;
			return picker;
		}
#endif // !__TVOS__
	}
	
	/// <summary>
	///   This element can be used to insert an arbitrary UIView
	/// </summary>
	/// <remarks>
	///   There is no cell reuse here as we have a 1:1 mapping
	///   in this case from the UIViewElement to the cell that
	///   holds our view.
	/// </remarks>
	public partial class UIViewElement : Element, IElementSizing {
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

				ContainerView.Frame = new CGRect (0, 0, ContainerView.Frame.Width + w - ow, ContainerView.Frame.Height + h -oh);
				viewFrame.X += dx;
				viewFrame.Y += dy;
				View.Frame = viewFrame;

				insets = value;

				// Height changed, notify UITableView
				if (dy != 0 || h != oh)
					GetContainerTableView ().ReloadData ();
				
			}
		}

		[Flags]
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
				view.Frame = new CGRect (insets.Left + frame.X, insets.Top + frame.Y, frame.Width, frame.Height);

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
					cell.BackgroundView = new UIView (CGRect.Empty) { 
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
		
		public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
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
	
	/// <summary>
	/// Sections contain individual Element instances that are rendered by MonoTouch.Dialog
	/// </summary>
	/// <remarks>
	/// Sections are used to group elements in the screen and they are the
	/// only valid direct child of the RootElement.    Sections can contain
	/// any of the standard elements, including new RootElements.
	/// 
	/// RootElements embedded in a section are used to navigate to a new
	/// deeper level.
	/// 
	/// You can assign a header and a footer either as strings (Header and Footer)
	/// properties, or as UIViews to be shown (HeaderView and FooterView).   Internally
	/// this uses the same storage, so you can only show one or the other.
	/// </remarks>
	public class Section : Element, IEnumerable {
		object header, footer;
		public List<Element> Elements = new List<Element> ();
				
		// X corresponds to the alignment, Y to the height of the password
		public CGSize EntryAlignment;

		public bool SearchFiltered;

		/// <summary>
		///  Constructs a Section without header or footers.
		/// </summary>
		public Section () : base (null) {}
		
		/// <summary>
		///  Constructs a Section with the specified header
		/// </summary>
		/// <param name="caption">
		/// The header to display
		/// </param>
		public Section (string caption) : base (caption)
		{
		}
		
		/// <summary>
		/// Constructs a Section with a header and a footer
		/// </summary>
		/// <param name="caption">
		/// The caption to display (or null to not display a caption)
		/// </param>
		/// <param name="footer">
		/// The footer to display.
		/// </param>
		public Section (string caption, string footer) : base (caption)
		{
			Footer = footer;
		}

		public Section (UIView header) : base (null)
		{
			HeaderView = header;
		}
		
		public Section (UIView header, UIView footer) : base (null)
		{
			HeaderView = header;
			FooterView = footer;
		}
		
		/// <summary>
		///    The section header, as a string
		/// </summary>
		public string Header {
			get {
				return header as string;
			}
			set {
				header = value;
			}
		}
		
		/// <summary>
		/// The section footer, as a string.
		/// </summary>
		public string Footer {
			get {
				return footer as string;
			}
			
			set {
				footer = value;
			}
		}
		
		/// <summary>
		/// The section's header view.  
		/// </summary>
		public UIView HeaderView {
			get {
				return header as UIView;
			}
			set {
				header = value;
			}
		}
		
		/// <summary>
		/// The section's footer view.
		/// </summary>
		public UIView FooterView {
			get {
				return footer as UIView;
			}
			set {
				footer = value;
			}
		}

		void OnElementsChanged()
		{
			if (!SearchFiltered && Parent is RootElement root)
			{
				root.Prepare(true);
			}
		}

		/// <summary>
		/// Adds a new child Element to the Section
		/// </summary>
		/// <param name="element">
		/// An element to add to the section.
		/// </param>
		/// <param name="animate">
		/// Should action be animated on UI.
		/// </param>
		public void Add(Element element, bool animate = true)
		{
			if (element == null)
			{
				return;
			}

			Elements.Add(element);
			element.Parent = this;

			OnElementsChanged();

			if (animate && Parent != null)
			{
				InsertVisual(Elements.Count - 1, UITableViewRowAnimation.None, 1);
			}
		}

		/// <summary>
		/// Adds a new child RootElement to the Section. This only exists to fix a compiler breakage when the mono 3.0 mcs is used.
		/// </summary>
		/// <param name="element">
		/// An element to add to the section.
		/// </param>
		public void Add (RootElement element)
		{
			Add ((Element)element);
		}

		/// <summary>
		///    Add version that can be used with LINQ
		/// </summary>
		/// <param name="elements">
		/// An enumerable list that can be produced by something like:
		///    from x in ... select (Element) new MyElement (...)
		/// </param>
		public int AddAll (IEnumerable<Element> elements)
		{
			int count = 0;
			foreach (var e in elements){
				Add (e);
				count++;
			}
			return count;
		}
		
		/// <summary>
		///    This method is being obsoleted, use AddAll to add an IEnumerable<Element> instead.
		/// </summary>
		[Obsolete ("Please use AddAll since this version will not work in future versions of MonoTouch when we introduce 4.0 covariance")]
		public int Add (IEnumerable<Element> elements)
		{
			return AddAll (elements);
		}
		
		/// <summary>
		/// Use to add a UIView to a section, it makes the section opaque, to
		/// get a transparent one, you must manually call UIViewElement
		public void Add (UIView view)
		{
			if (view == null)
				return;
			Add (new UIViewElement (null, view, false));
		}

		/// <summary>
		///   Adds the UIViews to the section.
		/// </summary>
		/// <param name="views">
		/// An enumarable list that can be produced by something like:
		///    from x in ... select (UIView) new UIFoo ();
		/// </param>
		public void Add (IEnumerable<UIView> views)
		{
			foreach (var v in views)
				Add (v);
		}
		
		/// <summary>
		/// Inserts a series of elements into the Section using the specified animation
		/// </summary>
		/// <param name="idx">
		/// The index where the elements are inserted
		/// </param>
		/// <param name="anim">
		/// The animation to use
		/// </param>
		/// <param name="newElements">
		/// A series of elements.
		/// </param>
		public void Insert (int idx, UITableViewRowAnimation anim, params Element [] newElements)
		{
			if (newElements == null)
				return;
			
			int pos = idx;
			foreach (var e in newElements){
				Elements.Insert (pos++, e);
				e.Parent = this;
			}

			OnElementsChanged();

			var root = Parent as RootElement;
			if (Parent != null && root.TableView != null){
				if (anim == UITableViewRowAnimation.None)
					root.TableView.ReloadData ();
				else
					InsertVisual (idx, anim, newElements.Length);
			}
		}

		public int Insert(int idx, UITableViewRowAnimation anim, IEnumerable<Element> newElements)
		{
			if (newElements == null)
				return 0;

			int pos = idx;
			int count = 0;
			foreach (var e in newElements)
			{
				Elements.Insert(pos++, e);
				e.Parent = this;
				count++;
			}

			OnElementsChanged();

			var root = Parent as RootElement;
			if (root?.TableView != null)
			{
				InsertVisual(idx, anim, count);
			}
			return count;
		}
		
		/// <summary>
		/// Inserts a single RootElement into the Section using the specified animation
		/// </summary>
		/// <param name="idx">
		/// The index where the elements are inserted
		/// </param>
		/// <param name="anim">
		/// The animation to use
		/// </param>
		/// <param name="newElements">
		/// A series of elements.
		/// </param>
		public void Insert (int idx, UITableViewRowAnimation anim, RootElement newElement)
		{
			Insert (idx, anim, (Element) newElement);
		}

		void InsertVisual (int idx, UITableViewRowAnimation anim, int count)
		{
			var root = Parent as RootElement;
			
			if (root?.TableView == null)
				return;
			
			int sidx = root.IndexOf (this);
			var paths = new NSIndexPath [count];
			for (int i = 0; i < count; i++)
				paths [i] = NSIndexPath.FromRowSection (idx+i, sidx);
			
			root.TableView.InsertRows (paths, anim);
		}
		
		public void Insert (int index, params Element [] newElements)
		{
			Insert (index, UITableViewRowAnimation.None, newElements);
		}
		
		public void Remove (Element e)
		{
			if (e == null)
				return;
			for (int i = Elements.Count; i > 0;){
				i--;
				if (Elements [i] == e){
					RemoveRange (i, 1);
					return;
				}
			}
		}
		
		public void Remove (int idx)
		{
			RemoveRange (idx, 1);
		}
		
		/// <summary>
		/// Removes a range of elements from the Section
		/// </summary>
		/// <param name="start">
		/// Starting position
		/// </param>
		/// <param name="count">
		/// Number of elements to remove from the section
		/// </param>
		public void RemoveRange (int start, int count)
		{
			RemoveRange (start, count, UITableViewRowAnimation.Fade);
		}

		/// <summary>
		/// Remove a range of elements from the section with the given animation
		/// </summary>
		/// <param name="start">
		/// Starting position
		/// </param>
		/// <param name="count">
		/// Number of elements to remove form the section
		/// </param>
		/// <param name="anim">
		/// The animation to use while removing the elements
		/// </param>
		public void RemoveRange (int start, int count, UITableViewRowAnimation anim)
		{
			if (start < 0 || start >= Elements.Count)
				return;
			if (count == 0)
				return;
			
			var root = Parent as RootElement;
			
			if (start+count > Elements.Count)
				count = Elements.Count-start;
			
			Elements.RemoveRange (start, count);

			OnElementsChanged();

			if (root == null || root.TableView == null)
				return;
			
			int sidx = root.IndexOf (this);
			var paths = new NSIndexPath [count];
			for (int i = 0; i < count; i++)
				paths [i] = NSIndexPath.FromRowSection (start+i, sidx);
			root.TableView.DeleteRows (paths, anim);
		}
		
		/// <summary>
		/// Enumerator to get all the elements in the Section.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/>
		/// </returns>
		public IEnumerator GetEnumerator ()
		{
			foreach (var e in Elements)
				yield return e;
		}

		public int Count {
			get {
				return Elements.Count;
			}
		}

		public Element this [int idx] {
			get {
				return Elements [idx];
			}
		}

		public void Clear ()
		{
			if (Elements != null){
				foreach (var e in Elements)
					e.Dispose ();
			}
			Elements = new List<Element> ();

			var root = Parent as RootElement;
			if (root != null && root.TableView != null)
				root.TableView.ReloadData ();
		}
				
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				Parent = null;
				Clear ();
				Elements = null;
			}
			base.Dispose (disposing);
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = new UITableViewCell (UITableViewCellStyle.Default, "");
			cell.TextLabel.Text = "Section was used for Element";
			
			return cell;
		}
	}
	
	/// <summary>
	/// Used by root elements to fetch information when they need to
	/// render a summary (Checkbox count or selected radio group).
	/// </summary>
	public class Group {
		public string Key;
		public Group (string key)
		{
			Key = key;
		}
	}
	/// <summary>
	/// Captures the information about mutually exclusive elements in a RootElement
	/// </summary>
	public class RadioGroup : Group {
		int selected;

		public event NSAction SelectedChanged;

		public virtual int Selected
		{
			get { return selected; }
			set
			{
				if (selected != value)
				{
					selected = value;
					SelectedChanged?.Invoke();
				}
			}
		}

		public string NotSelectedCaption { get; set; }

		public RadioGroup(string key, int selected, string notSelectedCaption = null) : base(key)
		{
			this.selected = selected;
			NotSelectedCaption = notSelectedCaption;
		}

		public RadioGroup(int selected, string notSelectedCaption = null) : this(null, selected, notSelectedCaption)
		{ }
	}

	/// <summary>
	///    RootElements are responsible for showing a full configuration page.
	/// </summary>
	/// <remarks>
	///    At least one RootElement is required to start the MonoTouch.Dialogs
	///    process.   
	/// 
	///    RootElements can also be used inside Sections to trigger
	///    loading a new nested configuration page.   When used in this mode
	///    the caption provided is used while rendered inside a section and
	///    is also used as the Title for the subpage.
	/// 
	///    If a RootElement is initialized with a section/element value then
	///    this value is used to locate a child Element that will provide
	///    a summary of the configuration which is rendered on the right-side
	///    of the display.
	/// 
	///    RootElements are also used to coordinate radio elements.  The
	///    RadioElement members can span multiple Sections (for example to
	///    implement something similar to the ring tone selector and separate
	///    custom ring tones from system ringtones).
	/// 
	///    Sections are added by calling the Add method which supports the
	///    C# 4.0 syntax to initialize a RootElement in one pass.
	/// </remarks>
	public partial class RootElement : Element, IEnumerable, IEnumerable<Section> {
		static NSString rkey1 = new NSString("RootElement1");
		static NSString rkey2 = new NSString("RootElement2");
		int summarySection, summaryElement;
		internal Group group;
		private bool searchable;
		public bool UnevenRows;
		public Func<RootElement, UIViewController> createOnSelected;
		public UITableView TableView;
		public string Value;
		static UIFont font = UIFont.BoldSystemFontOfSize(17);

		// This is used to indicate that we need the DVC to dispatch calls to
		// WillDisplayCell so we can prepare the color of the cell before 
		// display
		public bool NeedColorUpdate;

		/// <summary>
		///  Initializes a RootSection with a caption
		/// </summary>
		/// <param name="caption">
		///  The caption to render.
		/// </param>
		/// <param name="searchable">
		/// Turn on Search for elements.
		/// </param>
		public RootElement(string caption, bool searchable = false) : base(caption)
		{
			summarySection = -1;
			Sections = new List<Section>();
			this.searchable = searchable;
		}

		/// <summary>
		///  Initializes a RootSection with a caption
		/// </summary>
		/// <param name="caption">
		///  The caption to render.
		/// </param>
		/// <param name="value">
		///  The value to render.
		/// </param>
		/// <param name="searchable">
		/// Turn on Search for elements.
		/// </param>
		public RootElement(string caption, string value, bool searchable = false) : this(caption, searchable)
		{
			Value = value;
		}

		/// <summary>
		/// Initializes a RootSection with a caption and a callback that will
		/// create the nested UIViewController that is activated when the user
		/// taps on the element.
		/// </summary>
		/// <param name="caption">
		///  The caption to render.
		/// </param>
		public RootElement(string caption, Func<RootElement, UIViewController> createOnSelected) : base(caption)
		{
			summarySection = -1;
			this.createOnSelected = createOnSelected;
			Sections = new List<Section>();
		}

		/// <summary>
		///   Initializes a RootElement with a caption with a summary fetched from the specified section and leement
		/// </summary>
		/// <param name="caption">
		/// The caption to render cref="System.String"/>
		/// </param>
		/// <param name="section">
		/// The section that contains the element with the summary.
		/// </param>
		/// <param name="element">
		/// The element index inside the section that contains the summary for this RootSection.
		/// </param>
		/// <param name="searchable">
		/// Turn on Search for elements.
		/// </param>
		public RootElement(string caption, int section, int element, bool searchable = false) : this(caption, searchable)
		{
			summarySection = section;
			summaryElement = element;
		}

		/// <summary>
		/// Initializes a RootElement that renders the summary based on the radio settings of the contained elements. 
		/// </summary>
		/// <param name="caption">
		/// The caption to ender
		/// </param>
		/// <param name="group">
		/// The group that contains the checkbox or radio information.  This is used to display
		/// the summary information when a RootElement is rendered inside a section.
		/// </param>
		/// <param name="searchable">
		/// Turn on Search for elements.
		/// </param>
		public RootElement(string caption, Group group, bool searchable = false) : this(caption, searchable)
		{
			this.group = group;
		}

		private string title;

		/// <summary>
		/// The title for DialogViewController. <see cref="Caption"/> is default Title.
		/// </summary>
		public string Title { get => !string.IsNullOrEmpty(title) ? title : Caption; set => title = value; }

		internal List<Section> Sections { get; set; } = new List<Section>();

		private List<Section> sectionsOriginal;

		internal List<Section> SectionsOriginal { get => sectionsOriginal ?? Sections; set => sectionsOriginal = value; }

		internal List<Section> SectionsSearchFiltered
		{
			set
			{
				if (value is null)
				{
					Sections = SectionsOriginal;
					SectionsOriginal = null;
				}
				else
				{
					if (sectionsOriginal is null)
					{
						SectionsOriginal = Sections;
					}
					Sections = value;
				}
			}
		}

		internal NSIndexPath PathForRadio (int idx)
		{
			RadioGroup radio = group as RadioGroup;
			if (radio == null)
				return null;
			
			uint section = 0;
			foreach (Section s in Sections){
				uint row = 0;
				
				foreach (Element e in s.Elements){
					var element = e as RadioElement;
					if (element == null)
						continue;
					
					if (element.RadioIdx == idx){
						return NSIndexPath.Create(section, row); 
					}
					row++;
				}
				section++;
			}
			return null;
		}
		
		public int Count { 
			get {
				return Sections.Count;
			}
		}

		public Section this [int idx] {
			get {
				return Sections [idx];
			}
		}
		
		internal int IndexOf (Section target)
		{
			int idx = 0;
			foreach (Section s in Sections){
				if (s == target)
					return idx;
				idx++;
			}
			return -1;
		}

		public void Prepare(bool refreshRadioIndexes = false)
		{
			int current = 0;
			foreach (var s in SectionsOriginal)
			{
				foreach (var e in s.Elements)
				{
					if (e is RadioElement re && (refreshRadioIndexes || !re.RadioIdx.HasValue))
					{
						re.RadioIdx = current++;
					}
					if (UnevenRows == false && e is IElementSizing)
					{
						UnevenRows = true;
					}
					if (NeedColorUpdate == false && e is IColorizeBackground)
					{
						NeedColorUpdate = true;
					}
				}
			}
		}
		
		/// <summary>
		/// Adds a new section to this RootElement
		/// </summary>
		/// <param name="section">
		/// The section to add, if the root is visible, the section is inserted with no animation
		/// </param>
		public void Add (Section section)
		{
			if (section == null)
				return;
			
			Sections.Add (section);
			section.Parent = this;
			if (TableView == null)
				return;
			
			TableView.InsertSections (MakeIndexSet (Sections.Count-1, 1), UITableViewRowAnimation.None);
		}

		//
		// This makes things LINQ friendly;  You can now create RootElements
		// with an embedded LINQ expression, like this:
		// new RootElement ("Title") {
		//     from x in names
		//         select new Section (x) { new StringElement ("Sample") }
		//
		public void Add (IEnumerable<Section> sections)
		{
			foreach (var s in sections)
				Add (s);
		}
		
		NSIndexSet MakeIndexSet (int start, int count)
		{
			NSRange range;
			range.Location = start;
			range.Length = count;
			return NSIndexSet.FromNSRange (range);
		}
		
		/// <summary>
		/// Inserts a new section into the RootElement
		/// </summary>
		/// <param name="idx">
		/// The index where the section is added <see cref="System.Int32"/>
		/// </param>
		/// <param name="anim">
		/// The <see cref="UITableViewRowAnimation"/> type.
		/// </param>
		/// <param name="newSections">
		/// A <see cref="Section[]"/> list of sections to insert
		/// </param>
		/// <remarks>
		///    This inserts the specified list of sections (a params argument) into the
		///    root using the specified animation.
		/// </remarks>
		public void Insert (int idx, UITableViewRowAnimation anim, params Section [] newSections)
		{
			if (idx < 0 || idx > Sections.Count)
				return;
			if (newSections == null)
				return;
			
			if (TableView != null)
				TableView.BeginUpdates ();
			
			int pos = idx;
			foreach (var s in newSections){
				s.Parent = this;
				Sections.Insert (pos++, s);
			}
			
			if (TableView == null)
				return;
			
			TableView.InsertSections (MakeIndexSet (idx, newSections.Length), anim);
			TableView.EndUpdates ();
		}
		
		/// <summary>
		/// Inserts a new section into the RootElement
		/// </summary>
		/// <param name="idx">
		/// The index where the section is added <see cref="System.Int32"/>
		/// </param>
		/// <param name="newSections">
		/// A <see cref="Section[]"/> list of sections to insert
		/// </param>
		/// <remarks>
		///    This inserts the specified list of sections (a params argument) into the
		///    root using the Fade animation.
		/// </remarks>
		public void Insert (int idx, Section section)
		{
			Insert (idx, UITableViewRowAnimation.None, section);
		}
		
		/// <summary>
		/// Removes a section at a specified location
		/// </summary>
		public void RemoveAt (int idx)
		{
			RemoveAt (idx, UITableViewRowAnimation.Fade);
		}

		/// <summary>
		/// Removes a section at a specified location using the specified animation
		/// </summary>
		/// <param name="idx">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="anim">
		/// A <see cref="UITableViewRowAnimation"/>
		/// </param>
		public void RemoveAt (int idx, UITableViewRowAnimation anim)
		{
			if (idx < 0 || idx >= Sections.Count)
				return;
			
			Sections.RemoveAt (idx);
			
			if (TableView == null)
				return;
			
			TableView.DeleteSections (NSIndexSet.FromIndex (idx), anim);
		}
			
		public void Remove (Section s)
		{
			if (s == null)
				return;
			int idx = Sections.IndexOf (s);
			if (idx == -1)
				return;
			RemoveAt (idx, UITableViewRowAnimation.Fade);
		}
		
		public void Remove (Section s, UITableViewRowAnimation anim)
		{
			if (s == null)
				return;
			int idx = Sections.IndexOf (s);
			if (idx == -1)
				return;
			RemoveAt (idx, anim);
		}

		public void Clear ()
		{
			foreach (var s in Sections)
				s.Dispose ();
			Sections = new List<Section> ();
			if (TableView != null)
				TableView.ReloadData ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (Sections == null)
					return;
				
				TableView = null;
				Clear ();
				Sections = null;
			}
		}
		
		/// <summary>
		/// Enumerator that returns all the sections in the RootElement.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/>
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			foreach (var s in Sections)
				yield return s;
		}
		
		IEnumerator<Section> IEnumerable<Section>.GetEnumerator ()
		{
			foreach (var s in Sections)
				yield return s;
		}

		/// <summary>
		/// The currently selected Radio item in the whole Root.
		/// </summary>
		public int RadioSelected {
			get {
				var radio = group as RadioGroup;
				if (radio != null)
					return radio.Selected;
				return -1;
			}
			set {
				var radio = group as RadioGroup;
				if (radio != null)
					radio.Selected = value;
			}
		}
		
		public event NSAction RadioSelectedChanged
		{
			add
			{
				if (group is RadioGroup radio)
				{
					radio.SelectedChanged += value;
				}
			}
			remove
			{
				if (group is RadioGroup radio)
				{
					radio.SelectedChanged -= value;
				}
			}
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var summarized = summarySection != -1 || group != null || Value != null;
			var key = summarized ? rkey2 : rkey1;
			var cell = tv.DequeueReusableCell (key);
			if (cell == null){
				var style = summarized ? UITableViewCellStyle.Value1 : UITableViewCellStyle.Default;
				
				cell = new UITableViewCell (style, key);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
				cell.TextLabel.Font = font;
			} 
		
			cell.TextLabel.Text = Caption;
			if (cell.DetailTextLabel != null)
			{
				cell.DetailTextLabel.Text = Summary();
			}
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			return cell;
		}

		public Action<DialogViewController> PrepareDialogViewControllerCallback { get; set; }

		/// <summary>
		///    This method does nothing by default, but gives a chance to subclasses to
		///    customize the UIViewController before it is presented
		/// </summary>
		protected virtual void PrepareDialogViewController(DialogViewController dvc)
		{
			PrepareDialogViewControllerCallback?.Invoke(dvc);
		}
		
		/// <summary>
		/// Creates the UIViewController that will be pushed by this RootElement
		/// </summary>
		protected virtual UIViewController MakeViewController ()
		{
			if (createOnSelected != null)
				return createOnSelected (this);
			
			return new DialogViewController (this, true) {
				Autorotate = true,
				EnableSearch = searchable,
				SearchPlaceholder = SearchPlaceholder,
				SearchLabel = SearchLabel,
			};
		}
		
		public string SearchPlaceholder { get; set; }

		public string SearchLabel { get; set; }
        
        public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			tableView.DeselectRow (path, false);
			var newDvc = MakeViewController ();
			if (newDvc is DialogViewController ndvc)
			{
				PrepareDialogViewController(ndvc);
			}
			dvc.ActivateController (newDvc);
		}

        public override string Summary()
        {
			if (group is RadioGroup radio)
			{
				int selected = radio.Selected;
				Prepare();
				foreach (var s in SectionsOriginal)
				{
					foreach (var e in s.Elements)
					{
						if (e is RadioElement re && re.RadioIdx == selected)
						{
							return e.Summary();
						}
					}
				}

				if (radio.NotSelectedCaption != null)
				{
					return radio.NotSelectedCaption;
				}
			}
			else if (group != null)
			{
				int count = 0;

				foreach (var s in Sections)
				{
					foreach (var e in s.Elements)
					{
						if (e is CheckboxElement ce)
						{
							if (ce.Value)
								count++;
							continue;
						}
						if (e is BoolElement be)
						{
							if (be.Value)
								count++;
							continue;
						}
					}
				}
				return count.ToString();
			}
			else if (summarySection != -1 && summarySection < Sections.Count)
			{
				var s = Sections[summarySection];
				if (summaryElement < s.Elements.Count)
				{
					return s.Elements[summaryElement].Summary();
				}
			}
			else if (Value != null)
			{
				return Value;
			}
            return base.Summary();
        }

        public void Reload (Section section, UITableViewRowAnimation animation)
		{
			if (section == null)
				throw new ArgumentNullException ("section");
			if (section.Parent == null || section.Parent != this)
				throw new ArgumentException ("Section is not attached to this root");
			
			int idx = 0;
			foreach (var sect in Sections){
				if (sect == section){
					TableView.ReloadSections (new NSIndexSet ((uint) idx), animation);
					return;
				}
				idx++;
			}
		}
		
		public void Reload (Element element, UITableViewRowAnimation animation)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			var section = element.Parent as Section;
			if (section == null)
				throw new ArgumentException ("Element is not attached to this root");
			var root = section.Parent as RootElement;
			if (root == null)
				throw new ArgumentException ("Element is not attached to this root");
			var path = element.IndexPath;
			if (path == null)
				return;
			TableView.ReloadRows (new NSIndexPath [] { path }, animation);
		}
		
	}
}
