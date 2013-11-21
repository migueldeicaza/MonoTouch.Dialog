using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

#if !HAVE_NATIVE_TYPES
using nint = global::System.Int32;
using nuint = global::System.UInt32;
using nfloat = global::System.Single;

using CGSize = global::System.Drawing.SizeF;
using CGPoint = global::System.Drawing.PointF;
using CGRect = global::System.Drawing.RectangleF;
#endif

namespace MonoTouch.Dialog
{
	public class ActivityElement : UIViewElement, IElementSizing {
		public ActivityElement () : base ("", new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray), false)
		{
			var sbounds = UIScreen.MainScreen.Bounds;			
			var uia = View as UIActivityIndicatorView;
			
			uia.StartAnimating ();
			
			var vbounds = View.Bounds;
			View.Frame = new CGRect ((sbounds.Width-vbounds.Width)/2, 4, vbounds.Width, vbounds.Height + 0);
			View.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin;
		}
		
		public bool Animating {
			get {
				return ((UIActivityIndicatorView) View).IsAnimating;
			}
			set {
				var activity = View as UIActivityIndicatorView;
				if (value)
					activity.StartAnimating ();
				else
					activity.StopAnimating ();
			}
		}
		
		nfloat IElementSizing.GetHeight (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			return base.GetHeight (tableView, indexPath)+ 8;
		}
		
	}
}

