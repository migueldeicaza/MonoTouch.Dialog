using System;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	partial class DialogViewController
	{
		/// <summary>
		/// Activates a nested view controller from the DialogViewController.   
		/// If the view controller is hosted in a UINavigationController it
		/// will push the result.   Otherwise it will show it as a modal
		/// dialog
		/// </summary>
		public void ActivateController (UIViewController controller, bool AnimatePageTurn)
		{
			dirty = true;
			var parent = ParentViewController;
			var nav = parent as UINavigationController;
			
			// We can not push a nav controller into a nav controller
			if (nav != null && !(controller is UINavigationController))
				nav.PushViewController (controller, AnimatePageTurn);
			else
				PresentModalViewController (controller, AnimatePageTurn);
		}
	}
}

