//
// This cell does not perform cell recycling, do not use as
// sample code for new elements. 
//
using System;
using System.Drawing;
using System.Threading;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	public class LoadMoreElement : Element, IElementSizing
	{
		public string NormalCaption { get; set; }
		public string LoadingCaption { get; set; }
		
		Action<LoadMoreElement> tapped = null;
		
		UITableViewCell cell;
		UIActivityIndicatorView activityIndicator;
		UILabel caption;
		UIFont font;
		
		public LoadMoreElement (string normalCaption, string loadingCaption, Action<LoadMoreElement> tapped, UIFont font, UIColor textColor) : base ("")
		{
			this.NormalCaption = normalCaption;
			this.LoadingCaption = loadingCaption;
			this.tapped = tapped;
			this.font = font;
			
			cell = new UITableViewCell (UITableViewCellStyle.Default, "loadMoreElement");
			
			activityIndicator = new UIActivityIndicatorView () {
				ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray,
				Hidden = true
			};
			activityIndicator.StopAnimating ();
			
			caption = new UILabel () {
				Font = font,
				Text = this.NormalCaption,
				TextColor = textColor,
				TextAlignment = UITextAlignment.Center
			};
			
			Layout ();
			
			cell.AddSubview (caption);
			cell.AddSubview (activityIndicator);
		}
		
		public bool Animating {
			get {
				return activityIndicator.IsAnimating;
			}
			set {
				if (value){
					caption.Text = LoadingCaption;
					activityIndicator.Hidden = false;
					activityIndicator.StartAnimating ();
				} else {
					activityIndicator.StopAnimating ();
					activityIndicator.Hidden = true;
					caption.Text = NormalCaption;
				}
				Layout ();
			}
		}
				
		public override UITableViewCell GetCell (UITableView tv)
		{
			Layout ();
			return cell;
		}
				
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			tableView.DeselectRow (path, true);
			
			Animating = true;
			Layout ();
			
			if (tapped != null)
				tapped (this);
		}
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return 0.1042f * UIScreen.MainScreen.Bounds.Height;
		}
		
		void Layout ()
		{
			float h = UIScreen.MainScreen.Bounds.Height;
			float width = UIScreen.MainScreen.Bounds.Width;
			
			
			float captionHeight = 0.0625f * h;			
			float topPadding = 0.02083f * h;
			float itemPadding = 0.01042f * h;
						
			var size = cell.StringSize (caption.Text, font, width - captionHeight - topPadding, UILineBreakMode.TailTruncation);
			
			float center = width / 2;
			
			if (!activityIndicator.Hidden)
			{
				activityIndicator.Frame = new RectangleF (center - (size.Width / 2) - captionHeight, topPadding, captionHeight, captionHeight);
				caption.Frame = new RectangleF (activityIndicator.Frame.Right + itemPadding, topPadding, size.Width, captionHeight);
			}
			else
			{
				caption.Frame = new RectangleF (center - (size.Width / 2), topPadding, size.Width, captionHeight);
			}
		}
	}
}

