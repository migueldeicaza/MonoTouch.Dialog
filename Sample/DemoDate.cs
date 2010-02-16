//
// Shows how to add/remove cells dynamically.
//

using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace Sample
{
	public partial class AppDelegate
	{
		const string longString = 
			"Today a major announcement was done in my kitchen, when " +
			"I managed to not burn the onions while on the microwave";
		
		UIImage badgeImage;
		
		public void DemoDate ()
		{
			if (badgeImage == null)
				badgeImage = UIImage.FromFile ("caltemplate.png");
			
			var badgeSection = new Section ("Basic Badge Properties"){
				new BadgeElement (badgeImage, "New Movie Day") {
					Font = UIFont.FromName ("Helvetica", 36f)
				},
				new BadgeElement (badgeImage, "Valentine's Day"),
				
				new BadgeElement (badgeImage, longString) {
					MultiLine = true,
					Font = UIFont.FromName ("Helvetica", 12f)
				}
			};
			
			//
			// Use the MakeCalendarBadge API
			//
			var calendarSection = new Section ("Date sample"){
				new BadgeElement (
					BadgeElement.MakeCalendarBadge (badgeImage, "February", "14"),
				                  "Valentine's Day.   Do not forget to hug your sysadmin"){
					Font = UIFont.FromName ("Helvetica", 16f)
				}
			};
			
			var root = new RootElement ("Date sample") {
				calendarSection,
				badgeSection
			};
			var dvc = new DialogViewController (root, true);
			navigation.PushViewController (dvc, true);
		}
	}
}