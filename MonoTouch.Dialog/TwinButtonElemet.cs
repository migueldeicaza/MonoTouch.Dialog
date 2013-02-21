using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	/// <summary>
	/// Twin button identifier enum
	/// </summary>
	public enum TwinButton
	{
		Left,
		Right
	}

	/// <summary>
	/// Twin button elemet.
	/// </summary>
	public class TwinButtonElemet : Element
	{
		const string ElementKey = "TwinElementKey";
		UIButton rightButton;
		UIButton leftButton;
		Action<TwinButton> tapped;
		string leftButtonTitle;
		string rightButtonTitle;

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoTouch.Dialog.TwinButtonElemet"/> class.
		/// </summary>
		/// <param name="leftButtonTitle">Left button title.</param>
		/// <param name="rightButtonTitle">Right button title.</param>
		/// <param name="tapped">Twin button tapped handler </param>
		public TwinButtonElemet (string leftButtonTitle, string rightButtonTitle, Action<TwinButton> tapped):base("Twin Button Element")
		{
			this.leftButtonTitle = leftButtonTitle;
			this.rightButtonTitle = rightButtonTitle;
			this.tapped = tapped;
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (ElementKey);
			if (cell == null) {

				cell = new UITableViewCell (UITableViewCellStyle.Default, ElementKey);

				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				cell.BackgroundView = new UIView (RectangleF.Empty);
			
				float buttonHeight = cell.ContentView.Bounds.Height;
				const float buttonWidth = 155;
			
				leftButton = new UIButton (UIButtonType.RoundedRect);
				leftButton.Frame = new RectangleF (0, 0, buttonWidth, buttonHeight);
				leftButton.AutoresizingMask = UIViewAutoresizing.All;
				leftButton.SetTitle (leftButtonTitle, UIControlState.Normal);
				leftButton.TouchUpInside += delegate {
					tapped (TwinButton.Left);
				};
			
				rightButton = new UIButton (UIButtonType.RoundedRect);
				rightButton.Frame = new RectangleF (165, 0, buttonWidth, buttonHeight);
				rightButton.AutoresizingMask = UIViewAutoresizing.All;
				rightButton.SetTitle (rightButtonTitle, UIControlState.Normal);
				rightButton.TouchUpInside += delegate {
					tapped (TwinButton.Right);
				};

				cell.ContentView.AddSubviews (new []{leftButton, rightButton});
			}

			return cell;
		}

	}
}

