//
// This cell does not perform cell recycling, do not use as
// sample code for new elements. 
//
using System;
using System.Drawing;
using System.Threading;

using CoreFoundation;
using Foundation;
using UIKit;
using CoreGraphics;
using ObjCRuntime;

namespace MonoTouch.Dialog
{
	public partial class LoadMoreElement : Element, IElementSizing
	{
		static NSString key = new NSString ("LoadMoreElement");
		public string NormalCaption { get; set; }
		public string LoadingCaption { get; set; }
		public UIColor TextColor { get; set; }
		public UIColor BackgroundColor { get; set; }
		public event Action<LoadMoreElement> Tapped = null;
		public UIFont Font;
		public float? Height;
		UITextAlignment alignment = UITextAlignment.Center;
		bool animating;
		
		public LoadMoreElement () : base ("")
		{
		}
#if NET6_0 && !NET7_0
		public LoadMoreElement (string normalCaption, string loadingCaption, Action<LoadMoreElement> tapped) : this (normalCaption, loadingCaption, tapped, UIFont.BoldSystemFontOfSize (new NFloat (16)), UIColor.Black)
#else
		public LoadMoreElement (string normalCaption, string loadingCaption, Action<LoadMoreElement> tapped) : this (normalCaption, loadingCaption, tapped, UIFont.BoldSystemFontOfSize (16), UIColor.Black)
#endif
		{
		}
		
		public LoadMoreElement (string normalCaption, string loadingCaption, Action<LoadMoreElement> tapped, UIFont font, UIColor textColor) : base ("")
		{
			NormalCaption = normalCaption;
			LoadingCaption = loadingCaption;
			Tapped += tapped;
			Font = font;
			TextColor = textColor;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key);
			UIActivityIndicatorView activityIndicator;
			UILabel caption;
			
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, key);
			
				activityIndicator = new UIActivityIndicatorView () {
					ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray,
					Tag = 1
				};
				caption = new UILabel () {
					AdjustsFontSizeToFitWidth = false,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
					Tag = 2
				};
				cell.ContentView.AddSubview (caption);
				cell.ContentView.AddSubview (activityIndicator);
			} else {
				activityIndicator = cell.ContentView.ViewWithTag (1) as UIActivityIndicatorView;
				caption = cell.ContentView.ViewWithTag (2) as UILabel;
			}
			if (Animating){
				caption.Text = LoadingCaption;
				activityIndicator.Hidden = false;
				activityIndicator.StartAnimating ();
			} else {
				caption.Text = NormalCaption;
				activityIndicator.Hidden = true;
				activityIndicator.StopAnimating ();
			}
			if (BackgroundColor != null){
				cell.ContentView.BackgroundColor = BackgroundColor ?? UIColor.Clear;
			} else {
				cell.ContentView.BackgroundColor = null;
			}
			caption.BackgroundColor = UIColor.Clear;
			caption.TextColor = TextColor ?? UIColor.Black;
#if NET6_0 && !NET7_0
			caption.Font = Font ?? UIFont.BoldSystemFontOfSize (new NFloat (16));
#else
			caption.Font = Font ?? UIFont.BoldSystemFontOfSize (16);
#endif
			caption.TextAlignment = Alignment;
			Layout (cell, activityIndicator, caption);
			return cell;
		}
		
		public bool Animating {
			get {
				return animating;
			}
			set {
				if (animating == value)
					return;
				animating = value;
				var cell = GetActiveCell ();
				if (cell == null)
					return;
				var activityIndicator = cell.ContentView.ViewWithTag (1) as UIActivityIndicatorView;
				var caption = cell.ContentView.ViewWithTag (2) as UILabel;
				if (value){
					caption.Text = LoadingCaption;
					activityIndicator.Hidden = false;
					activityIndicator.StartAnimating ();
				} else {
					activityIndicator.StopAnimating ();
					activityIndicator.Hidden = true;
					caption.Text = NormalCaption;
				}
				Layout (cell, activityIndicator, caption);
			}
		}
				
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			tableView.DeselectRow (path, true);
			
			if (Animating)
				return;
			
			if (Tapped != null){
				Animating = true;
				Tapped (this);
			}
		}
		
		CGSize GetTextSize (string text)
		{
#if NET6_0 && !NET7_0
			return new NSString (text).StringSize (Font, (float)UIScreen.MainScreen.Bounds.Width.Value, UILineBreakMode.TailTruncation);
#else
			return new NSString (text).StringSize (Font, (float)UIScreen.MainScreen.Bounds.Width, UILineBreakMode.TailTruncation);
#endif
		}
		
		const int pad = 10;
		const int isize = 20;
		
		public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
#if NET6_0 && !NET7_0
			return new NFloat (Height ?? GetTextSize (Animating ? LoadingCaption : NormalCaption).Height.Value + 2*pad);
#else
			return Height ?? GetTextSize (Animating ? LoadingCaption : NormalCaption).Height + 2*pad;
#endif
		}
		
		void Layout (UITableViewCell cell, UIActivityIndicatorView activityIndicator, UILabel caption)
		{
			var sbounds = cell.ContentView.Bounds;

			var size = GetTextSize (Animating ? LoadingCaption : NormalCaption);
			
			if (!activityIndicator.Hidden)
#if NET6_0 && !NET7_0
				activityIndicator.Frame = new CGRect (new NFloat ((sbounds.Width.Value-size.Width.Value)/2-isize*2), new NFloat (pad), new NFloat (isize), new NFloat (isize));
#else
				activityIndicator.Frame = new CGRect ((sbounds.Width-size.Width)/2-isize*2, pad, isize, isize);
#endif

#if NET6_0 && !NET7_0
			caption.Frame = new CGRect (new NFloat (10), new NFloat (pad), new NFloat (sbounds.Width.Value-20), new NFloat (size.Height.Value));
#else
			caption.Frame = new CGRect (10, pad, sbounds.Width-20, size.Height);
#endif
		}
		
		public UITextAlignment Alignment { 
			get { return alignment; } 
			set { alignment = value; }
		}
		public UITableViewCellAccessory Accessory { get; set; }
	}
}

