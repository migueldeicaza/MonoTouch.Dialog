using System;

using CoreGraphics;
using ObjCRuntime;

namespace MonoTouch.Dialog
{
	public static class GraphicsUtil {
		
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
		public static CGPath MakeRoundedRectPath (CGRect rect, nfloat radius)
		{
			nfloat minx = rect.Left;
#if NET6_0 && !NET7_0
			nfloat midx = new NFloat (rect.Left.Value + (rect.Width.Value)/2);
#else
			nfloat midx = rect.Left + (rect.Width)/2;
#endif
			nfloat maxx = rect.Right;
			nfloat miny = rect.Top;
#if NET6_0 && !NET7_0
			nfloat midy = new NFloat (rect.Y.Value+rect.Size.Height.Value/2);
#else
			nfloat midy = rect.Y+rect.Size.Height/2;
#endif
			nfloat maxy = rect.Bottom;

			var path = new CGPath ();
			path.MoveToPoint (minx, midy);
			path.AddArcToPoint (minx, miny, midx, miny, radius);
			path.AddArcToPoint (maxx, miny, maxx, midy, radius);
			path.AddArcToPoint (maxx, maxy, midx, maxy, radius);
			path.AddArcToPoint (minx, maxy, minx, midy, radius);		
			path.CloseSubpath ();
			
			return path;
        }
		
#if NET6_0 && !NET7_0
		public static CGPath MakeRoundedRectPath (CGRect rect, float radius)
		{
			return MakeRoundedRectPath (rect, new NFloat (radius));
		}
#endif

		public static void FillRoundedRect (CGContext ctx, CGRect rect, nfloat radius)
		{
				var p = GraphicsUtil.MakeRoundedRectPath (rect, radius);
				ctx.AddPath (p);
				ctx.FillPath ();
		}

#if NET6_0 && !NET7_0
		public static void FillRoundedRect (CGContext ctx, CGRect rect, float radius)
		{
				FillRoundedRect (ctx, rect, new NFloat (radius));
		}
#endif

		public static CGPath MakeRoundedPath (float size, float radius)
		{
			float hsize = size/2;
			
			var path = new CGPath ();
			path.MoveToPoint (size, hsize);
			path.AddArcToPoint (size, size, hsize, size, radius);
			path.AddArcToPoint (0, size, 0, hsize, radius);
			path.AddArcToPoint (0, 0, hsize, 0, radius);
			path.AddArcToPoint (size, 0, size, hsize, radius);
			path.CloseSubpath ();
			
			return path;
		}
	}
}

