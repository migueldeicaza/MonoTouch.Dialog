using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
namespace MonoTouch.Dialog
{
	[Register("RotatingViewController")]
	public abstract partial class RotatingViewController : UIViewController
	{
		public UIView PortraitView {get;set;}
		public UIView LandscapeLeftView {get;set;}
		public UIView LandscapeRightView {get;set;}

		public NSObject notificationObserver;

		public RotatingViewController (IntPtr handle) : base(handle)
		{
		
		}

		[Export("initWithCoder:")]
		public RotatingViewController (NSCoder coder) : base(coder)
		{
		}

		public RotatingViewController (string nibName, NSBundle bundle) : base(nibName, bundle)
		{
		}

		public RotatingViewController () :base ()
		{
			
		}
		public override void ViewDidLoad ()
		{
			SetView();
		}

		public override void ViewWillAppear (bool animated)
		{
			SetView();
		}
		private void _showView(UIView view){
			_removeAllViews();
			view.Frame = this.View.Frame;
			View.AddSubview(view);

		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			if (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft)
				return true;
			else if (toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
				return true;
			return false;
		}
		
		public abstract void SetupNavBar();

		private void SetView()
		{
			
			Console.WriteLine(InterfaceOrientation);
			switch (this.InterfaceOrientation){

				case  UIInterfaceOrientation.Portrait:
					_showView(PortraitView);
					break;

				case UIInterfaceOrientation.LandscapeLeft:
					_showView(LandscapeLeftView);
					break;
				case UIInterfaceOrientation.LandscapeRight:
					_showView(LandscapeRightView);
					break;
			}
			SetupNavBar();
		}
			
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			Console.WriteLine("rotated! "+UIDevice.CurrentDevice.Orientation);
			SetView();
		}
		

		private void _removeAllViews(){
			PortraitView.RemoveFromSuperview();
			LandscapeLeftView.RemoveFromSuperview();
			LandscapeRightView.RemoveFromSuperview();
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

	}

}

