using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	public abstract class OwnerDrawnElement : Element, IElementSizing
	{		
		public string CellReuseIdentifier
		{
			get;set;	
		}
		
		public UITableViewCellStyle Style
		{
			get;set;	
		}
		
		public OwnerDrawnElement (UITableViewCellStyle style, string cellIdentifier) : base(null)
		{
			this.CellReuseIdentifier = cellIdentifier;
			this.Style = style;
		}
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return Height(tableView.Bounds);
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			OwnerDrawnCell cell = tv.DequeueReusableCell(this.CellReuseIdentifier) as OwnerDrawnCell;
			
			if (cell == null)
			{
				cell = new OwnerDrawnCell(this, this.Style, this.CellReuseIdentifier);
<<<<<<< HEAD
				cell.Update();
			}

=======
			}
			else
			{
				cell.Element = this;
			}
			
			cell.Update();
>>>>>>> b301c4059a63479ccc119679e9c3aef3d385f734
			return cell;
		}	
		
		public abstract void Draw(RectangleF bounds, CGContext context, UIView view);
		
		public abstract float Height(RectangleF bounds);
		
		class OwnerDrawnCell : UITableViewCell
		{
			OwnerDrawnCellView view;
			
			public OwnerDrawnCell(OwnerDrawnElement element, UITableViewCellStyle style, string cellReuseIdentifier) : base(style, cellReuseIdentifier)
			{
<<<<<<< HEAD
				view = new OwnerDrawnCellView(element);
				ContentView.Add(view);
			}
=======
				Element = element;
			}
			
			public OwnerDrawnElement Element
			{
				get {
					return view.Element;
				}
				set {
					if (view == null)
					{
						view = new OwnerDrawnCellView (value);
						ContentView.Add (view);
					}
					else
					{
						view.Element = value;
					}
				}
			}
				
			
>>>>>>> b301c4059a63479ccc119679e9c3aef3d385f734

			public void Update()
			{
				SetNeedsDisplay();
<<<<<<< HEAD
			}		
	
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
=======
				view.SetNeedsDisplay();
			}		
	
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
>>>>>>> b301c4059a63479ccc119679e9c3aef3d385f734
				
				view.Frame = ContentView.Bounds;
			}
		}
		
		class OwnerDrawnCellView : UIView
		{				
			OwnerDrawnElement element;
			
			public OwnerDrawnCellView(OwnerDrawnElement element)
			{
<<<<<<< HEAD
				this.element = element;	
=======
				this.element = element;
			}
			
			
			public OwnerDrawnElement Element
			{
				get { return element; }
				set {
					element = value; 
				}
>>>>>>> b301c4059a63479ccc119679e9c3aef3d385f734
			}
			
			public void Update()
			{
				SetNeedsDisplay();
			
			}
			
			public override void Draw (RectangleF rect)
			{
				CGContext context = UIGraphics.GetCurrentContext();
				element.Draw(rect, context, this);
			}
		}
	}
}

