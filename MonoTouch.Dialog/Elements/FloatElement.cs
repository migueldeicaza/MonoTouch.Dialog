
//
// Elements.cs: defines the various components of our view
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010, Novell, Inc.
//
// Code licensed under the MIT X11 license
//
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	
	/// <summary>
	///  Used to display a slider on the screen.
	/// </summary>
	public class FloatElement : Element {
		public bool ShowCaption;
		public float Value;
		public float MinValue, MaxValue;
		static NSString skey = new NSString ("FloatElement");
		//UIImage Left, Right;
		UISlider slider;
		
		public FloatElement (UIImage left, UIImage right, float value) : base (null)
		{
			//Left = left;
			//Right = right;
			MinValue = 0;
			MaxValue = 1;
			Value = value;
		}
		
		protected override NSString CellKey {
			get {
				return skey;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag (cell, 1);

			SizeF captionSize = new SizeF (0, 0);
			if (Caption != null && ShowCaption){
				cell.TextLabel.Text = Caption;
				captionSize = cell.TextLabel.StringSize (Caption, UIFont.FromName (cell.TextLabel.Font.Name, UIFont.LabelFontSize));
				captionSize.Width += 10; // Spacing
			}

			if (slider == null){
				slider = new UISlider (new RectangleF (10f + captionSize.Width, 12f, 280f - captionSize.Width, 7f)){
					BackgroundColor = UIColor.Clear,
					MinValue = this.MinValue,
					MaxValue = this.MaxValue,
					Continuous = true,
					Value = this.Value,
					Tag = 1
				};
				slider.ValueChanged += delegate {
					Value = slider.Value;
				};
			} else {
				slider.Value = Value;
			}
			
			cell.ContentView.AddSubview (slider);
			return cell;
		}

		public override string Summary ()
		{
			return Value.ToString ();
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (slider != null){
					slider.Dispose ();
					slider = null;
				}
			}
		}		
	}
	
}
