using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using System.Drawing;
using System.Linq;

namespace MonoTouch.Dialog
{
	
	/// <summary>
	///  Used to display a slider on the screen, also showing the current value in the caption.
	/// </summary>
	public class FloatCaptionElement : Element {
		public bool ShowCaption;
		public float Value;
		public float MinValue, MaxValue;
		static NSString skey = new NSString ("FloatElement");
		UIImage Left, Right;
		UISlider slider;

		public FloatCaptionElement (float value, string caption = null) : this (null, null, value)
		{
			this.Caption = caption;
			ShowCaption = true;
		}
		public FloatCaptionElement (UIImage left, UIImage right, float value) : base (null)
		{
			Left = left;
			Right = right;
			MinValue = 0;
			MaxValue = 1;
			Value = value;
		}
		
		protected override NSString CellKey {
			get {
				return skey;
			}
		}
		
		private void UpdateCell(UITableViewCell cell, bool setValue = true)
		{
			SizeF captionSize = new SizeF (0, 0);
			if (Caption != null && ShowCaption){
				cell.TextLabel.Text = EffectiveCaption;
				captionSize = cell.TextLabel.StringSize (MaxCaption, UIFont.FromName (cell.TextLabel.Font.Name, UIFont.LabelFontSize));
				captionSize.Width += 10; // Spacing
			}

			var rect = new RectangleF (10f + captionSize.Width, 12f, 280f - captionSize.Width, 7f);
			if (slider == null || slider.Superview == null){
				slider = new UISlider (rect){
					BackgroundColor = UIColor.Clear,
					MinValue = this.MinValue,
					MaxValue = this.MaxValue,
					Continuous = true,
					Value = this.Value,
					Tag = 1
				};

				cell.ContentView.AddSubview (slider);
			}
			else 
			{
				if(setValue)
				{
					slider.Value = Value;
				}
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
			{
				if(slider != null)
				{
					RemoveTag (cell, 1);
					slider = null;
				}
			}
			UpdateCell(cell);

			slider.ValueChanged += delegate {
				Value = slider.Value;
				UpdateCell(cell, false);
			};

			if(!cell.ContentView.Subviews.Contains(slider))
			{
				cell.ContentView.AddSubview (slider);
			}

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
		
		public string NumberFormat = null;
		
		public string EffectiveCaption
		{
			get
			{
				if(NumberFormat == null)
				{
					return Caption;
				}
				else
				{
					return Caption + " = " + Value.ToString(NumberFormat);
				}
			}
		}
		public string MaxCaption
		{
			get
			{
				if(NumberFormat == null)
				{
					return Caption;
				}
				else
				{
					return Caption + " = " + (MaxValue - 0.1f).ToString(NumberFormat);
				}
			}
		}
	}
}

