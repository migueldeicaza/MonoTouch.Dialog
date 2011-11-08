using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.IO;

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
			
			var p = Path.GetFullPath ("background.png");

			var menu = new RootElement ("Demos"){
				new Section ("Element API"){
					new StringElement ("iPhone Settings Sample", DemoElementApi),
					new StringElement ("Dynamically load data", DemoDynamic),
					new StringElement ("Add/Remove demo", DemoAddRemove),
					new StringElement ("Assorted cells", DemoDate),
					new StyledStringElement ("Styled Elements", DemoStyled) { BackgroundUri = new Uri ("file://" + p) },
					new StringElement ("Load More Sample", DemoLoadMore),
					new StringElement ("Row Editing Support", DemoEditing),
					new StringElement ("Advanced Editing Support", DemoAdvancedEditing),
					new StringElement ("Owner Drawn Element", DemoOwnerDrawnElement),
				},
				new Section ("Container features"){
					new StringElement ("Pull to Refresh", DemoRefresh),
					new StringElement ("Headers and Footers", DemoHeadersFooters),
					new StringElement ("Root Style", DemoContainerStyle),
					new StringElement ("Index sample", DemoIndex),
				},
				new Section ("Auto-mapped", footer){
					new StringElement ("Reflection API", DemoReflectionApi)
				},
			};

			//
			// Create our UI and add it to the current toplevel navigation controller
			// this will allow us to have nice navigation animations.
			//
			var dv = new DialogViewController (menu) {
				Autorotate = true
			};
			navigation.PushViewController (dv, true);				
			
			window.MakeKeyAndVisible ();
			
			// On iOS5 we use the new window.RootViewController, on older versions, we add the subview
			if (UIDevice.CurrentDevice.CheckSystemVersion (5, 0))
				window.RootViewController = navigation;	
			else
				window.AddSubview (navigation.View);			
			
			return true;
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}
