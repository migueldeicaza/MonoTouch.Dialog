using System;
using MonoTouch.Dialog;

namespace Sample
{
	public partial class AppDelegate
	{

		public void DemoTwinButton ()
		{
			var root = new RootElement ("Contact")
			{
				new Section("Prashant C"){
					new StringElement("mobile", "+91 1234-567890"),
					new StringElement("home", "+91 1234-567890"),
				},
				new Section ()
				{
					new StringElement("email", "pvc@outlook.com")
				},
				new Section(){
					new TwinButtonElemet("Send Message", "FaceTime", Tapped),
				},
				new Section(){
					new TwinButtonElemet("Share Contact", "Add to Favourites", Tapped)
				}
			};

			var dvc = new DialogViewController (root, true);
			navigation.PushViewController (dvc, true);
		}

		void Tapped (TwinButton button)
		{
			Console.WriteLine (button);
		}
	}
	
}

