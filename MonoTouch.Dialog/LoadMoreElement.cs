using System;
using System.Drawing;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	public class LoadMoreElement : Element, IElementSizing
	{
		public string NormalCaption
		{
			get;set;
		}
		
		public string LoadingCaption
		{
			get;set;
		}
		
		NSAction tapped = null;
		
		UITableViewCell cell;
		UIActivityIndicatorView activityIndicator;
		UILabel caption;
		UIFont font;
		
		public LoadMoreElement (string normalCaption, string loadingCaption, NSAction tapped, UIFont font, UIColor textColor) : base("")
		{
			this.NormalCaption = normalCaption;
			this.LoadingCaption = loadingCaption;
			this.tapped = tapped;
			this.font = font;
			
			cell = new UITableViewCell(UITableViewCellStyle.Default, "loadMoreElement");
			
			if (this.tapped == null)
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			
			activityIndicator = new UIActivityIndicatorView();
			activityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray;
			activityIndicator.Hidden = true;
			activityIndicator.StopAnimating();
			
			caption = new UILabel();
			caption.Font = font;
			caption.Text = this.NormalCaption;
			caption.TextColor = textColor;
			caption.TextAlignment = UITextAlignment.Center;
			
			Layout();
			
			cell.AddSubview(caption);
			cell.AddSubview(activityIndicator);
			
									
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			Layout();
			return cell;
		}
				
		public override void Selected (MonoTouch.Dialog.DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			tableView.DeselectRow(path, true);
		
			if (this.tapped != null)
			{
				caption.Text = this.LoadingCaption;
				activityIndicator.Hidden = false;
				activityIndicator.StartAnimating();
				Layout();
				
				System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(Tapped));
			}
		}
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return 0.1042f * UIScreen.MainScreen.Bounds.Height;
		}
		
		
		void Tapped(object state)
		{
			if (tapped != null)
				tapped();
			
			FinishedLoading();
		}
		
		void Layout()
		{
			float h = UIScreen.MainScreen.Bounds.Height;
			float width = UIScreen.MainScreen.Bounds.Width;
			width = cell.ContentView.Bounds.Width;
			
			
			float captionHeight = 0.04166f * h;			
			float topPadding = 0.03125f * h;
			float itemPadding = 0.01042f * h;
						
			var size = cell.StringSize(caption.Text, font, width - captionHeight - topPadding, UILineBreakMode.TailTruncation);
			
			float center = width / 2;
			
			if (!activityIndicator.Hidden)
			{
				activityIndicator.Frame = new RectangleF(center - (size.Width / 2) - captionHeight, topPadding, captionHeight, captionHeight);
				caption.Frame = new RectangleF(activityIndicator.Frame.Right + itemPadding, topPadding, size.Width, captionHeight);
			}
			else
			{
				caption.Frame = new RectangleF(center - (size.Width / 2), topPadding, size.Width, captionHeight);
			}
		}
		
		void FinishedLoading()
		{
			this.cell.BeginInvokeOnMainThread(delegate {
				activityIndicator.StopAnimating();
				activityIndicator.Hidden = true;
				caption.Text = this.NormalCaption;
				
				Layout();
			});
		}
	}
}

