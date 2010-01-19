using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace Sample
{
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args);
		}
	}

	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class AppDelegate : UIApplicationDelegate
	{
		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window.AddSubview (navigation.View);

			using (var alertView = new UIAlertView ("MonoTouch.Dialogs Sample", "Would you like use the Element API or the Unfinished Reflected API", null, "Element", "Unfinished")){
				alertView.Dismissed += delegate(object sender, UIButtonEventArgs e) {
					switch (e.ButtonIndex){
					case 0:
						DemoElementApi ();
						break;
						
					case 1:
						DemoReflectionApi ();
						break;
					}
				};
				alertView.Show ();
			}

			window.MakeKeyAndVisible ();
			
			return true;
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}
