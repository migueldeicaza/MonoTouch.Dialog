using System;
using MonoTouch.CoreGraphics;
using System.Drawing;
namespace MonoTouch.Dialog
{
	public static class Graphics {
		
		/// <summary>
		///    Creates a path for a rectangle with rounded corners
		/// </summary>
		/// <param name="rect">
		/// The <see cref="RectangleF"/> rectangle bounds
		/// </param>
		/// <param name="radius">
		/// The <see cref="System.Single"/> size of the rounded corners
		/// </param>
		/// <returns>
		/// A <see cref="CGPath"/> that can be used to stroke the rounded rectangle
		/// </returns>
		public static CGPath MakeRoundedRectPath (RectangleF rect, float radius)
		{
			float size = rect.Width;
			float hsize = size/2;
			
			var path = new CGPath ();
			path.MoveToPoint (rect.Right, rect.Top+hsize);
			path.AddArcToPoint (rect.Right, rect.Bottom, rect.Left+hsize, rect.Bottom, radius);
			path.AddArcToPoint (rect.Left, rect.Bottom, rect.Left, rect.Top+hsize, radius);
			path.AddArcToPoint (rect.Left, rect.Top, rect.Left+hsize, rect.Top, radius);
			path.AddArcToPoint (rect.Right, rect.Top, rect.Right, rect.Top+hsize, radius);
			path.CloseSubpath ();
			
			return path;
        }
		
		public static void FillRoundedRect (CGContext ctx, RectangleF rect, float radius)
		{
				var p = Graphics.MakeRoundedRectPath (rect, 3);
				ctx.AddPath (p);
				ctx.FillPath ();
		}
	}
}

