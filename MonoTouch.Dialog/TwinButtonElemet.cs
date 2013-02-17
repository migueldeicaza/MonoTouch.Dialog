using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	public class TwinButtonElemet : Element
	{
		Tuple<string, string> _captions;
		const string _elementKey = "TwinElementKey";

		public TwinButtonElemet (string leftButton, string rightButton):base("Twin Elements")
		{
			_captions = new Tuple<string, string>(leftButton, rightButton);

		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (_elementKey);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, _elementKey);

				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				cell.BackgroundView = new UIView(RectangleF.Empty);

				 float buttonHeight = cell.ContentView.Bounds.Height;
				const float buttonWidth = 155;

				UIButton btn = new UIButton (UIButtonType.RoundedRect);
				btn.Frame = new RectangleF(0,0,buttonWidth, buttonHeight);
				btn.AutoresizingMask = UIViewAutoresizing.All;
				btn.SetTitle (_captions.Item1, UIControlState.Normal);

				UIButton btn1 = new UIButton (UIButtonType.RoundedRect);
				btn1.Frame = new RectangleF (165, 0, buttonWidth, buttonHeight);
				btn1.AutoresizingMask = UIViewAutoresizing.All;
				btn1.SetTitle (_captions.Item2, UIControlState.Normal);
				cell.ContentView.AddSubviews (new []{btn, btn1});
			}

			return cell;
		}

	}


}

