using MonoTouch.Dialog;

namespace Sample;

[Register("AppDelegate")]
public partial class AppDelegate : UIApplicationDelegate
{
	public override UIWindow? Window
	{
		get;
		set;
	}

	const string footer =
			"These show the two sets of APIs\n" +
			"available in MonoTouch.Dialogs";

	public UIWindow? window => Window;

	UINavigationController? navigation;

	public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
	{
		// create a new window instance based on the screen size
		Window = new UIWindow(UIScreen.MainScreen.Bounds);
		var p = Path.GetFullPath("Contents/Resources/background.png");
		var menu = new RootElement("Demos"){
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
					new StringElement ("UIViewElement insets", DemoInsets),
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

		var dv = new DialogViewController(menu)
		{
			Autorotate = true
		};

		navigation = new UINavigationController();
		navigation.PushViewController(dv, true);

		Window.RootViewController = navigation;

		// make the window visible
		Window.MakeKeyAndVisible();

		return true;
	}
}