//
// Sample showing the core Element-based API to create a dialog
//
using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace Sample
{
	class Settings {
		[Section]
		bool AirplaneMode;
		
		[Section ("Data Entry")]
		
		[Entry ("Enter your login name")]
		string Login;
		
		[Caption ("Password"), Password ("Enter your password")]
		string passwd;
	}
	
	public partial class AppDelegate 
	{
		public void DemoReflectionApi ()
		{	
			var s = new Settings ();
			var bc = new BindingContext (s, "Settings");
			
			var dv = new DialogViewController (bc.Root);
			navigation.PushViewController (dv, false);	
		}
	}
}