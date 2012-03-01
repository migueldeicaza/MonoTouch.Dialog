using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	public class PickerElement : StyledStringElement
	{
		private bool becomeResponder;
		private UILabel replacementLabel;
		private IEnumerable<String> datasource;
		
		private static readonly UIFont defaultFont = UIFont.FromName("Helvetica", 16);
		private static readonly NSString cellkey = new NSString("PickerCell");
		
		public PickerElement(string caption, string value, IEnumerable<string> datasource) : base(caption, value)
		{
			this.datasource = datasource;
		}
		
		protected override NSString CellKey
		{
			get 
			{ 
				return cellkey; 
			}
		}
		
		protected virtual UILabel CreateReplacementLabel(RectangleF frame)
		{
			int selectedIdx = datasource.Select((a, i) => (a.Equals(Value)) ? i : -1).Max();
			
			var label = new UIPickerLabel(frame, CreatePicker(), selectedIdx)
			       	{
						BackgroundColor = UIColor.Clear,
			       		AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin,
			       		Text = Value ?? String.Empty,
			       		Tag = 1,
						Font = SubtitleFont ?? defaultFont,
						AdjustsFontSizeToFitWidth = true,
						TextAlignment = UITextAlignment.Right,
						TextColor = DetailColor ?? UIColor.FromRGB(56, 84, 135),
			       	};
			
			return label;
		}
		
		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(CellKey);
			if (cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			}
			else
				RemoveTag(cell, 1);
			
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			if (replacementLabel == null)
			{
				SizeF size = this.ComputeEntryPosition(tv, cell);
				float yOffset = (cell.ContentView.Bounds.Height - size.Height)/2 - 1;
				float width = cell.ContentView.Bounds.Width - size.Width;
				
				replacementLabel = CreateReplacementLabel(new RectangleF(size.Width + 5, yOffset, width - 10, size.Height));
			}
			
			if (becomeResponder)
			{
				replacementLabel.BecomeFirstResponder();
				becomeResponder = false;
			}
			
			cell.TextLabel.Text = Caption;
			cell.ContentView.AddSubview(replacementLabel);
			
			return cell;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			
			if (disposing)
			{
				if(replacementLabel != null)
				{
					replacementLabel.Dispose();
					replacementLabel = null;
				}
			}
		}

		public virtual UIPickerView CreatePicker()
		{
			var picker = new UIPickerView(RectangleF.Empty)
			             	{
			             		AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
			             	};
		
			picker.Model = new PickerModel(datasource, (s) => UpdateValue(s));
			picker.ShowSelectionIndicator = true;
			
			return picker;
		}
		
		private SizeF ComputeEntryPosition(UITableView tv, UITableViewCell cell)
		{
			var s = this.Parent as Section;
			if (s.EntryAlignment.Width != 0)
				return s.EntryAlignment;

			// If all EntryElements have a null Caption, align UITextField with the Caption
			// offset of normal cells (at 10px).
			var max = new SizeF(-15, tv.StringSize("M", defaultFont).Height);
			foreach (var ee in s.Elements)
			{
				if (ee.Caption != null)
				{
					var size = tv.StringSize(ee.Caption, defaultFont);
					if (size.Width > max.Width)
						max = size;
				}
			}
			s.EntryAlignment = new SizeF(25 + Math.Min(max.Width, 160), max.Height);
			
			return s.EntryAlignment;
		}
		
		private void UpdateValue(string newValue){
			
			if (newValue == Value)
				return;
			
			replacementLabel.Text = Value = newValue;
		}
		
		private class PickerModel: UIPickerViewModel 
		{
			IEnumerable<string> datasource;
			Action<string> callback;
			
			public PickerModel (IEnumerable<string> datasource, Action<String> callback)
			{
				this.callback = callback;
				this.datasource = datasource;
			}
			
			public override int GetComponentCount (UIPickerView v)
            {
                return 1;
            }
    
            public override int GetRowsInComponent (UIPickerView pickerView, int component)
            {
                return datasource.Count ();
            }
    
            public override string GetTitle (UIPickerView picker, int row, int component)
            {
                return datasource.ElementAt(row);
            }
    
            public override void Selected (UIPickerView picker, int row, int component)
            {
                callback(datasource.ElementAt(row));
            }
		}
		
		private class UIPickerLabel: UILabel 
		{
			UIPickerView inputView;
			int selectedIdx;
			UIToolbar inputAccessoryView;
			
			public UIPickerLabel (RectangleF frame, UIPickerView picker, int selectedIdx): base(frame)
			{
				this.selectedIdx = selectedIdx;
				this.inputView = picker;
				UserInteractionEnabled = true;
				
				inputAccessoryView = new UIToolbar(new RectangleF(0, 0, 320, 40));
				inputAccessoryView.Items = new UIBarButtonItem[] {
					new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
					new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate {
						ResignFirstResponder();
					})
				};
			}

			public override void TouchesEnded (NSSet touches, UIEvent evt)
			{
				inputView.Select(selectedIdx, 0, false);
				BecomeFirstResponder();
				
				base.TouchesEnded (touches, evt);
			}
			
			public override bool CanBecomeFirstResponder 
			{
				get 
				{
					return true;
				}
			}
			
			public override UIView InputAccessoryView 
			{
				get 
				{
					return inputAccessoryView;
				}
			}
			
			public override UIView InputView 
			{
				get 
				{
					return inputView;
				}
			}
			
			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					if (inputView != null)
					{
						inputView.Dispose();
						inputView = null;
					}
					
					if (inputAccessoryView != null)
					{
						inputAccessoryView.Dispose();
						inputAccessoryView = null;
					}
				}
			}
		}
	}
}

