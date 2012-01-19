using System;
using System.IO;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Sample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UINavigationController navigation;
		
		const string footer = 
			"These show the two sets of APIs\n" +
			"available in MonoTouch.Dialogs";
		
		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			navigation = new UINavigationController();

			var Last = new DateTime (2010, 10, 7);
			Console.WriteLine (Last);
			
			var p = Path.GetFullPath ("background.png");
			window.AddSubview (navigation.View);

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

			var dv = new DialogViewController (menu) {
				Autorotate = true
			};
			navigation.PushViewController (dv, true);				
			window.RootViewController = navigation;
			window.MakeKeyAndVisible ();
			
			return true;
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}