/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Drawing;

namespace MediaPortal.Layouts
{
	public class RingLayout : ILayout
	{
		#region Constructors

		public RingLayout() : this(0, 0)
		{
		}

		public RingLayout(int spacing) : this(spacing, spacing)
		{
		}
		
		public RingLayout(int horizontalSpacing, int verticalSpacing)
		{
			_spacing.Width = horizontalSpacing;
			_spacing.Height = verticalSpacing;
		}

		#endregion Constructors

		#region Methods

		public void Arrange(ILayoutComposite composite)
		{
			Point l = composite.Location;
			Rectangle r = new Rectangle();
			Margins m = composite.Margins;

			int index = 0;

			foreach(ILayoutComponent child in composite.Children)
			{
				if(child.Visible == false)
					continue;

				double angle = (++index * 2 * Math.PI) / composite.Children.Count;

				r.Size = child.Size;
				r.X = l.X + m.Left + _spacing.Width + ((_size.Width - m.Width - (_spacing.Width * 2)) / 2) + (int)(Math.Sin(angle) * _radius) - (r.Width / 2);
				r.Y = l.Y + m.Top + _spacing.Height + ((_size.Height - m.Height - (_spacing.Height * 2)) / 2) - (int)(Math.Cos(angle) * _radius) - (r.Height / 2);

				child.Arrange(r);
			}
		}

		public Size Measure(ILayoutComposite composite, Size availableSize)
		{
			int w = 0;
			int h = 0;

			foreach(ILayoutComponent child in composite.Children)
			{
				if(child.Visible == false)
					continue;

				Size s = child.Measure();

				w = Math.Max(w, s.Width);
				h = Math.Max(h, s.Height);
			}

			Margins m = composite.Margins;

			_radius = (Math.Min(w + _spacing.Width * composite.Children.Count, h + _spacing.Height * composite.Children.Count) / 2);
			_radius -= Math.Max(w, h) / 2;
			_size.Width = (int)(2 * _radius) - w + m.Width;
			_size.Height = (int)(2 * _radius) - h + m.Height;
			
			return _size;
		}

		#endregion Methods
		
		#region Properties

		public Size Size
		{
			get { return _size; }
		}

		public Size Spacing
		{
			get { return _spacing; }
			set { if(Size.Equals(_spacing, value) == false) _spacing = value; }
		}

		#endregion Properties

		#region Fields

		Size						_size = Size.Empty;
		Size						_spacing = Size.Empty;
		double						_radius = 0;

		#endregion Fields
	}
}
