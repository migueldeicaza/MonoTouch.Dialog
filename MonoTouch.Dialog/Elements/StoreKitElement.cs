using System;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.StoreKit;

namespace MonoTouch.Dialog
{
	public class StoreKitElement : StringElement
	{
		static NSString mkey = new NSString ("StoreKitElement");
		NSString[] ProductIdentifiers; 
		
		public StoreKitElement( string aCaption, string[] aProductIdentifiers ) : base(aCaption)
		{
			ProductIdentifiers = aProductIdentifiers.Select(x=> new NSString(x)).ToArray();
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
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			/*var vc = new MapKitViewController (this) {
				Autorotate = dvc.Autorotate
			};*/
			
			NetworkActivity = true;
			if ( SKPaymentQueue.CanMakePayments )
			{
				// Let's do it
				NSSet productIdentifiers  = NSSet.MakeNSObjectSet<NSString>(/*new NSString[]{new NSString("com.savagesoftwaresolutions.murdermap.subscription.monthly"), new NSString("com.savagesoftwaresolutions.com.murdermap.monthly")}*/ ProductIdentifiers);
				var request = new SKProductsRequest(productIdentifiers);
				
				request.ReceivedResponse += delegate(object sender, SKProductsRequestResponseEventArgs e) 
				{
					var root = new RootElement (Caption);
					var dvStore = new DialogViewController (root, true);
					var storeSection = new Section();
					
					root.Add(storeSection);
			
					
					foreach (var product in e.Response.Products) 
					{
						storeSection.Add(new StringElement(product.LocalizedTitle +":"+ product.PriceLocale ) );
					}
					
					dvc.ActivateController (dvStore);
				};
				
				request.RequestFailed += delegate(object sender, SKRequestErrorEventArgs e) 
				{					
					using (var msg = new UIAlertView ("Request Failed", e.Error.ToString(), null, "Ok")){
						msg.Show ();
					}
				};
				
				request.RequestFinished += delegate(object sender, EventArgs e) {
					NetworkActivity = false;
				};				
				
				request.Start();
			}
			else
			{
				using (var msg = new UIAlertView ("Unabled to Make Payments", "We are unable to connect to the Apple Store at this moment, to process your payments.\n Please try again later.", null, "Ok")){
					msg.Show ();
				}
			}
			
			NetworkActivity = false;
		}
	}
}

