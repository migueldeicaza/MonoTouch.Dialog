using System;
using System.Drawing;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace Sample
{
	public partial class AppDelegate
	{
		public void DemoOwnerDrawnElement () 
		{
			var root = new RootElement("Owner Drawn") {
				new Section() {
					new SampleOwnerDrawnElement("Element 1"),
					new SampleOwnerDrawnElement("Element 2"),
					new SampleOwnerDrawnElement("Element 3"),
					new SampleOwnerDrawnElement("Element 4"),
					new SampleOwnerDrawnElement("Element 5"),
					new SampleOwnerDrawnElement("Element 6"),
					new SampleOwnerDrawnElement("Element 7"),
					new SampleOwnerDrawnElement("Element 8"),
					new SampleOwnerDrawnElement("Element 9"),
					new SampleOwnerDrawnElement("Element 10"),
				}
			};
			
			var dvc = new DialogViewController (root, true);
			navigation.PushViewController (dvc, true);
		}
	}
	
	
	/// <summary>
	/// This is an example of implementing the OwnerDrawnElement abstract class.
	/// It makes it very simple to create an element that you draw using CoreGraphics
	/// </summary>
	public class SampleOwnerDrawnElement : OwnerDrawnElement
	{
		CGGradient gradient;
		
		public SampleOwnerDrawnElement (string text) : base(UITableViewCellStyle.Default, "sampleOwnerDrawnElement")
		{
			this.Text = text;
			
			CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
			gradient = new CGGradient(
			    colorSpace,
			    new float[] { 0.95f, 0.95f, 0.95f, 1, 
							  0.85f, 0.85f, 0.85f, 1},
				new float[] { 0, 1 } );
		}
		
		public string Text
		{
			get;set;	
		}
		
		public override void Draw (RectangleF bounds, CGContext context, UIView view)
		{
			UIColor.White.SetFill();
			context.FillRect(bounds);
			
			context.DrawLinearGradient(gradient, new PointF(bounds.Left, bounds.Top), new PointF(bounds.Left, bounds.Bottom), CGGradientDrawingOptions.DrawsAfterEndLocation);
			
			UIColor.Black.SetColor();
			
			view.DrawString(this.Text, new RectangleF(10, 15, bounds.Width - 20, bounds.Height - 30), UIFont.BoldSystemFontOfSize(14.0f), UILineBreakMode.TailTruncation);
		}
		
		public override float Height (RectangleF bounds)
		{
			return 44.0f;
		}
	}
}

