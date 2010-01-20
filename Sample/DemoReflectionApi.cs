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
		public bool AccountEnabled;
				
	[Section ("Account", "Your credentials")]
		
		[Entry ("Enter your login name")]
		public string Login;
		
		[Password ("Enter your password")]
		public string Password;
		
	[Section ("Time Editing")]
		
		public DateTime Appointment;
		
		[Date]
		public DateTime Birthday;
		
		[Time]
		public DateTime Alarm;
		
	[Section ("Enumerations")]
		
		[Caption ("Favorite CLR type")]
		public TypeCode FavoriteType;
	}
	
	public partial class AppDelegate 
	{
		Settings s;
		
		public void DemoReflectionApi ()
		{	
			if (s == null){
				s = new Settings () {
					AccountEnabled = true,
					Login = "postmater@localhost.com",
					Appointment = DateTime.Now,
					Birthday = new DateTime (1980, 6, 24),
					Alarm = new DateTime (2000, 1, 1, 7, 30, 0, 0),
					FavoriteType = TypeCode.Int32
				};
			}
			var bc = new BindingContext (s, "Settings");
			
			var dv = new DialogViewController (bc.Root, true);
			
			// When the view goes out of screen, we fetch the data.
			dv.ViewDissapearing += delegate {
				// This reflects the data back to the object instance
				bc.Fetch ();
				
				// Manly way of dumping the data.
				Console.WriteLine ("Current status:");
				Console.WriteLine (
				    "AccountEnabled: {0}\n" +
				    "Login:          {1}\n" +
				    "Password:       {2}\n" +
				    "Appointment:    {3}\n" +
				    "Birthday:       {4}\n" +
				    "Alarm:          {5}\n" +
				    "Favorite Type:  {6}\n", 
				    s.AccountEnabled, s.Login, s.Password, 
				    s.Appointment, s.Birthday, s.Alarm, s.FavoriteType);
			};
			navigation.PushViewController (dv, true);	
		}
	}
}