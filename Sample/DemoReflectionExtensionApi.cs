//
// Sample showing the Extension Reflection-based API to create a dialog
//
using System;
using System.Collections.Generic;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace Sample
{
	// Use the preserve attribute to inform the linker that even if I do not
	// use the fields, to not try to optimize them away.
	
	[Preserve (AllMembers=true)]
	class ExtensionSettings {
	
	[Section("Reflection Extension")]
		[Caption("Custom Data Type")]
		public CustomDataType custom;
	}

	public class CustomDataType {
		public int value;

		public override string ToString() {
			return value.ToString();
		}
	}

	class CustomElement : Element
	{
		static MonoTouch.Foundation.NSString key = new MonoTouch.Foundation.NSString("CustomElement");

		CustomControl ui;

		public CustomDataType Value
		{
			get { return new CustomDataType() { value = ui.Value }; }
			set { ui.Value = value.value; }
		}

		public CustomElement(string Caption, CustomDataType Value)
			: base(Caption)
		{
			ui = new CustomControl(new RectangleF(0, 0, 90, 20));
			this.Value = Value;
		}

		protected override MonoTouch.Foundation.NSString CellKey
		{
			get {
				return key;
			}
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(CellKey);
			if (cell == null) {
				cell = new UITableViewCell(UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag(cell, 1);

			cell.TextLabel.Text = Caption;
			cell.AccessoryView = ui;

			return cell;
		}
	}

	class BindingContextEx : BindingContext
	{
		public BindingContextEx(object callbacks, object o, string title) : base(callbacks, o, title) { }

		public override Element BuildElementForType(Type mType, string Caption, object Value, object[] attrs, object callbacks)
		{
			if (mType == typeof(CustomDataType)) {
				return new CustomElement(Caption, (CustomDataType)Value);
			}

			return base.BuildElementForType(mType, Caption, Value, attrs, callbacks);
		}

		public override bool FetchElementValue(Element element, out object Value)
		{
			if (element.GetType() == typeof(CustomElement)) {
				Value = ((CustomElement)element).Value;
				return true;
			}

			return base.FetchElementValue(element, out Value);
		}
	}

	class CustomControl : UIView
	{
		UIButton left, right;
		UILabel text;

		private int _Value;
		public int Value
		{
			get { return _Value; }
			set { _Value = value; text.Text = _Value.ToString();  }
		}

		public CustomControl(RectangleF frame)
			: base(frame)
		{
			BackgroundColor = UIColor.Clear;

			float controlwidth = frame.Width / 3f;
			RectangleF controlframe = new RectangleF(frame.X, frame.Y, controlwidth, frame.Height);

			left = UIButton.FromType(UIButtonType.RoundedRect);
			left.Frame = controlframe;
			left.SetTitle("+", UIControlState.Normal);
			left.TouchUpInside += delegate {
				Value++;
			};

			controlframe.Offset(controlwidth, 0);
			text = new UILabel(controlframe) {
				TextAlignment = UITextAlignment.Center,
				Text = Value.ToString()
			};
			text.Layer.BorderColor = UIColor.Black.CGColor;
			text.Layer.BorderWidth = 1f;

			controlframe.Offset(controlwidth, 0);
			right = UIButton.FromType(UIButtonType.RoundedRect);
			right.Frame = controlframe;
			right.SetTitle("-", UIControlState.Normal);
			right.TouchUpInside += delegate {
				Value--;
			};

			this.AddSubviews(left, text, right);
		}
	}
	
	public partial class AppDelegate 
	{
		ExtensionSettings ExtensionSettings;
		
		public void DemoReflectionExtensionApi ()
		{
			if (ExtensionSettings == null) {
				ExtensionSettings = new ExtensionSettings() {
					custom = new CustomDataType() {
						value = 1
					}
				};
			}

			var bc = new BindingContextEx(null, ExtensionSettings, "Settings");
			
			var dv = new DialogViewController (bc.Root, true);
			
			// When the view goes out of screen, we fetch the data.
			dv.ViewDissapearing += delegate {
				// This reflects the data back to the object instance
				bc.Fetch ();
				
				// Manly way of dumping the data.
				Console.WriteLine ("Current status:");
				Console.WriteLine (
				    "CustomDataType:  {0}\n",
					ExtensionSettings.custom.ToString()); 
			};
			navigation.PushViewController (dv, true);	
		}
	}
}