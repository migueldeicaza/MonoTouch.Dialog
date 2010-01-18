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

			var root = new RootElement ("Settings") {
				new Section (){
					new BooleanElement ("Airplane Mode", false),
					new RootElement ("Notifications", 0, 0) {
						new Section (null, "Turn off Notifications to disable Sounds\n" +
							     "Alerts and Home Screen Badges for the\napplications below."){
							new BooleanElement ("Notifications", false)
						}
					}},
				new Section (){
					new RootElement ("Sounds"){
						new Section ("Silent") {
							new BooleanElement ("Vibrate", true),
						},
						new Section ("Ring") {
							new BooleanElement ("Vibrate", true),
							new FloatElement (null, null, 0.8f),
							new RootElement ("Ringtone", new RadioGroup (0)){
								new Section ("Custom"){
									new RadioElement ("Circus Music"),
									new RadioElement ("True Blood"),
								},
								new Section ("Standard"){
									new RadioElement ("Marimba"),
									new RadioElement ("Alarm"),
									new RadioElement ("Ascending"),
									new RadioElement ("Bark"),
									new RadioElement ("Xylophone")
								}
							},
							new RootElement ("New Text Message", new RadioGroup (3)){
								new Section (){
									new RadioElement ("None"),
									new RadioElement ("Tri-tone"),
									new RadioElement ("Chime"),
									new RadioElement ("Glass"),
									new RadioElement ("Horn"),
									new RadioElement ("Bell"),
									new RadioElement ("Electronic")
								}
							},
							new BooleanElement ("New Voice Mail", false),
							new BooleanElement ("New Mail", false),
							new BooleanElement ("Sent Mail", true),
							new BooleanElement ("Calendar Alerts", true),
							new BooleanElement ("Lock Sounds", true),
							new BooleanElement ("Keyboard Clicks", false)
						}
					}
				},
				new Section () {
					new HtmlElement ("About", "http://monotouch.net")
				}
			};
				
			var dv = new DialogViewController (root);
			navigation.PushViewController (dv, false);
			
			window.MakeKeyAndVisible ();
			
			return true;
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}
