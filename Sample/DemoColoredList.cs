using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace Sample
{
	class ColoredViewController : DialogViewController {
		public ColoredViewController (RootElement root, bool pushing) : base (root, pushing)
		{
			Style = UITableViewStyle.Plain;
			EnableSearch = true;
			SearchPlaceholder = "Find item";
		}
	}

	public partial class AppDelegate
	{
		class CustomElement : StringElement
		{
			static UIColor EvenColor = UIColor.White;
			static UIColor UnevenColor = UIColor.FromRGB(245, 245, 245);

			public CustomElement(string caption) : base(caption)
			{}

			public override UITableViewCell GetCell (UITableView tv)
			{
				var cell = base.GetCell (tv);
				var color = IndexPath.Row % 2 == 0 ? EvenColor : UnevenColor;
				cell.TextLabel.BackgroundColor = color;
				cell.ContentView.BackgroundColor = color;
				return cell;
			}
		}

		public void DemoColoredList ()
		{
			var root = new RootElement ("Colored List Demo") {
				from sh in "ABCDEFGHIJKLMNOPQRSTUVWXYZ" 
					select new Section (sh + " - Section") {
						from filler in "12345" 
							select (Element) new CustomElement (sh + " - " + filler)
					}
			};
			var dvc = new ColoredViewController (root, true);
			navigation.PushViewController (dvc, true);
		}
	}
}

