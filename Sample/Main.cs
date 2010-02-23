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
		const string footer = 
			"These show the two sets of APIs\n" +
			"available in MonoTouch.Dialogs";
		
		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			var Last = new DateTime (2010, 10, 7);
			Console.WriteLine (Last);
			
			window.AddSubview (navigation.View);

			var menu = new RootElement ("Demos"){
				new Section ("Element API"){
					new StringElement ("iPhone Settings Sample", DemoElementApi),
					new StringElement ("Dynamically load data", DemoDynamic),
					new StringElement ("Add/Remove demo", DemoAddRemove),
					new StringElement ("Date cells", DemoDate),
				},
				new Section ("Auto-mapped", footer){
					new StringElement ("Reflection API", DemoReflectionApi)
				},
				new Section ("Other"){
					new StringElement ("Headers and Footers", DemoHeadersFooters)
				}
			};

			var dv = new DialogViewController (menu);
			navigation.PushViewController (dv, true);				
			
			window.MakeKeyAndVisible ();
			
			return true;
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}
