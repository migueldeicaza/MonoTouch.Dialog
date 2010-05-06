using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	internal enum RefreshViewStatus {
		ReleaseToReload,
		PullToReload,
		Loading
	}
		
	internal class RefreshTableHeaderView : UIView {
		static UIImage arrow = UIImage.FromFileUncached ("Images/arrow.png");
		UIActivityIndicatorView activity;
		UILabel lastUpdateLabel, statusLabel;
		UIImageView arrowView;		
			
		public RefreshTableHeaderView (RectangleF rect) : base (rect)
		{
			BackgroundColor = new UIColor (0.88f, 0.9f, 0.92f, 1);
			lastUpdateLabel = new UILabel (new RectangleF (0, rect.Height - 30, 320, 20)){
				Font = UIFont.SystemFontOfSize (12f),
				TextColor = new UIColor (0.34f, 0.74f, 0.54f, 1),
				ShadowColor = UIColor.FromWhiteAlpha (0.9f, 1),
				ShadowOffset = new SizeF (0, 1),
				BackgroundColor = this.BackgroundColor,
				Opaque = true,
				TextAlignment = UITextAlignment.Center
			};
			AddSubview (lastUpdateLabel);
			
			statusLabel = new UILabel (new RectangleF (0, rect.Height-48, 320, 20)){
				Font = UIFont.BoldSystemFontOfSize (13),
				TextColor = lastUpdateLabel.TextColor,
				ShadowColor = lastUpdateLabel.ShadowColor,
				ShadowOffset = new SizeF (0, 1),
				BackgroundColor = this.BackgroundColor,
				Opaque = true,
				TextAlignment = UITextAlignment.Center,
			};
			AddSubview (statusLabel);
			SetStatus (RefreshViewStatus.PullToReload);
			
			arrowView = new UIImageView (new RectangleF (25, rect.Height - 65, 30, 55)){
				ContentMode = UIViewContentMode.ScaleAspectFill,
				Image = arrow,
			};
			arrowView.Layer.Transform = CATransform3D.MakeRotation ((float) Math.PI, 0, 0, 1);
			AddSubview (arrowView);
			
			activity = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray) {
				Frame = new RectangleF (25, rect.Height-38, 20, 20),
				HidesWhenStopped = true
			};
			AddSubview (activity);
		}
		
		RefreshViewStatus status = (RefreshViewStatus) (-1);
		
		public void SetStatus (RefreshViewStatus status)
		{
			if (this.status == status)
				return;
			
			string s = "Release to refresh";
	
			switch (status){
			case RefreshViewStatus.Loading:
				s = "Loading..."; 
				break;
				
			case RefreshViewStatus.PullToReload:
				s = "Pull down to refresh...";
				break;
			}
			statusLabel.Text = s;
		}
		
		public override void Draw (RectangleF rect)
		{
			var context = UIGraphics.GetCurrentContext ();
			context.DrawPath (CGPathDrawingMode.FillStroke);
			statusLabel.TextColor.SetStroke ();
			context.BeginPath ();
			context.MoveTo (0, Bounds.Height-1);
			context.AddLineToPoint (Bounds.Width, Bounds.Height-1);
			context.StrokePath ();
		}		
		
		public bool IsFlipped;
		
		public void Flip (bool animate)
		{
			UIView.BeginAnimations (null);
			UIView.SetAnimationDuration (animate ? .18f : 0);
			arrowView.Layer.Transform = IsFlipped 
				? CATransform3D.MakeRotation ((float)Math.PI, 0, 0, 1) 
				: CATransform3D.MakeRotation ((float)Math.PI * 2, 0, 0, 1);
				
			UIView.CommitAnimations ();
			IsFlipped = !IsFlipped;
		}
		
		DateTime lastUpdateTime;
		public DateTime LastUpdate {
			get {
				return lastUpdateTime;
			}
			set {
				if (value == lastUpdateTime)
					return;
				
				lastUpdateTime = value;
				if (value == DateTime.MinValue){
					lastUpdateLabel.Text = "Last Updated: never";
				} else 
					lastUpdateLabel.Text = String.Format ("Last Updated: {0:g}", value);
			}
		}
		
		public void SetActivity (bool active)
		{
			if (active){
				activity.StartAnimating ();
				arrowView.Hidden = true;
				SetStatus (RefreshViewStatus.Loading);
			} else {
				activity.StopAnimating ();
				arrowView.Hidden = false;
			}
		}	
	}
}