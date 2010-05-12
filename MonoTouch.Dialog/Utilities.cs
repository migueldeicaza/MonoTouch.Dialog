using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	public static class Utilities
	{
		public static void SuccessfulMessage ()
		{
			UIApplication.SharedApplication.InvokeOnMainThread (delegate {
				var msg = new UIAlertView ("Success", "Saved Succcessfully", null, "Ok");
				msg.Transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeTranslation (0f, 110f);
				msg.Show ();
			});
			
		}

		public static void UnsuccessfulMessage ()
		{
			UIApplication.SharedApplication.InvokeOnMainThread (delegate {
				var msg = new UIAlertView ("Error", "Save UnSuccessful, Please try again", null, "Ok");
				msg.Transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeTranslation (0f, 110f);
				msg.Show ();
			});
			
		}

		public static void UnsuccessfulMessage (string message)
		{
			UIApplication.SharedApplication.InvokeOnMainThread (delegate {
				var msg = new UIAlertView ("Error", message, null, "Ok");
				msg.Transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeTranslation (0f, 110f);
				msg.Show ();
			});
		}

		public static void MessageAndRedirect (string message, NSAction action)
		{
			UIApplication.SharedApplication.InvokeOnMainThread (delegate {
				var msg = new UIAlertView ("Info", message, null, "Ok");
				msg.Clicked += delegate { UIApplication.SharedApplication.InvokeOnMainThread (action); };
				msg.Transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeTranslation (0f, 110f);
				msg.Show ();
			});
			
		}
		
		public static int GenerateRandomNumber()
		{
			System.Random rand = new System.Random();
			
			return rand.Next(100000,999999);
		}
		
	}
	
}
