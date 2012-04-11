using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.GameKit;

namespace MonoTouch.Dialog
{
	public class GameKitElement : Element
	{
		static NSString mkey = new NSString ("GameKitElement");
		GKLocalPlayer lp;
		
		public GameKitElement ( string aCaption ) : base(aCaption)
		{
			Authenticate();
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (mkey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, mkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			cell.TextLabel.Text = Caption;
			return cell;
		}
		
		static bool NetworkActivity {
			set {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = value;
			}
		}	
		
		private void DoAuthentication()
		{
			try 				
			{
				var osVersion = UIDevice.CurrentDevice.SystemVersion;
				if(osVersion.Contains("."))
				if(osVersion.IndexOf(".") != osVersion.LastIndexOf("."))
				{
					var parts = osVersion.Split(char.Parse("."));
					osVersion = parts[0] + "." + parts[1];
				}
				
				if (double.Parse(osVersion) > 4.1)
				{
					
					lp = GKLocalPlayer.LocalPlayer;
			        if (lp != null)
					{
						lp.Authenticate( delegate(NSError error) 
						                	{  							              
												try 
												{
													if ( error != null )
													{
														Console.WriteLine(error);
													}
													else
													{
														
													}
												} 
												finally 
												{
													
												}
											}
						                );
					}
				}
			}
			catch (Exception ex) 
			{
				Console.WriteLine(ex.Message);
			}
		}
		
		public void Authenticate()
		{
			
			// Register to receive the GKPlayerAuthenticationDidChangeNotificationName so we are notified when 
			// Authentication changes
			NSNotificationCenter.DefaultCenter.AddObserver( new NSString("GKPlayerAuthenticationDidChangeNotificationName"), (notification) => {   
        													    if (lp !=null && lp.Authenticated)
																{
																	Console.WriteLine("Authenticated");
																}
														    	else
																{
														        	Console.WriteLine("No Longer Authenticated");
																}
	    													});	
			
			DoAuthentication();
		}
	}
}
