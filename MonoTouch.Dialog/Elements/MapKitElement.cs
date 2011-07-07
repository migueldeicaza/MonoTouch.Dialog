using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;

namespace MonoTouch.Dialog
{
	public class MapKitElement : Element
	{
		static NSString mkey = new NSString ("MapKitElement");
		public MKMapView MapView = null;
		private MKMapViewDelegate mkViewDelegate = null;
		private MKAnnotation[] mkAnnotationObjects;
		
		public MapKitElement ( string aCaption, MKMapViewDelegate aMapViewDelegate, MKAnnotation[] aMKAnnotationObjects ) : base(aCaption)
		{
			mkViewDelegate = aMapViewDelegate;
			mkAnnotationObjects = aMKAnnotationObjects;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (mkey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, mkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			cell.TextLabel.Text = Caption;
			return cell;
		}
		
		static bool NetworkActivity {
			set {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = value;
			}
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var vc = new MapKitViewController (this) {
				Autorotate = dvc.Autorotate
			};
			MapView = new MKMapView(){				
				BackgroundColor = UIColor.White,				
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
			};
			MapView.Frame = new System.Drawing.RectangleF(UIScreen.MainScreen.ApplicationFrame.Left, UIScreen.MainScreen.ApplicationFrame.Top - 20, UIScreen.MainScreen.ApplicationFrame.Right, UIScreen.MainScreen.ApplicationFrame.Bottom - 20);
			MapView.Delegate = mkViewDelegate;			
			
			if ( mkAnnotationObjects != null )
			{
				MapView.AddAnnotation(mkAnnotationObjects);
			}
			
			MapView.WillStartLoadingMap += delegate(object sender, EventArgs e) {
				NetworkActivity = true;
			};
			
			MapView.MapLoaded += delegate(object sender, EventArgs e) {
				NetworkActivity = false;
			};
			
			MapView.LoadingMapFailed += delegate(object sender, NSErrorEventArgs e) {			
				// Display an error of sorts
			};
			
			vc.NavigationItem.Title = Caption;
			vc.View.AddSubview(MapView);
			
			dvc.ActivateController (vc);
		}
		
		// We use this class to dispose the MapKit control when it is not
		// in use, as it could be a bit of a pig, and we do not want to
		// wait for the GC to kick-in.
		class MapKitViewController : UIViewController {
			MapKitElement container;
			
			public MapKitViewController (MapKitElement container) : base ()
			{
				this.container = container;
			}
			
			public override void ViewWillDisappear (bool animated)
			{
				base.ViewWillDisappear (animated);
				
				NetworkActivity = false;
				if (container.MapView == null)
					return;

				container.MapView.Dispose();
				container.MapView = null;
			}
			
			public bool Autorotate { get; set; }
			
			public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
			{
				return Autorotate;
			}
		}
	}
}