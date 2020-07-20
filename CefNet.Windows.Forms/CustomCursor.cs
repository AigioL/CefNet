﻿using CefNet.WinApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CefNet.Windows.Forms
{
	/// <summary>
	/// Represents the image used to paint the mouse pointer.
	/// </summary>
	public sealed class CustomCursor
	{
		private readonly IntPtr _cursorHandle;
		private Cursor _cursor;

		/// <summary>
		/// Creates new instance of the <see cref="Cursor"/> class.
		/// </summary>
		/// <param name="cursorInfo">The cursor information.</param>
		/// <returns>A new <see cref="Cursor"/> that this method creates.</returns>
		public static Cursor Create(ref CefCursorInfo cursorInfo)
		{
			CefSize size = cursorInfo.Size;
			Bitmap bitmap = null;
			IntPtr iconHandle = IntPtr.Zero;
			try
			{
				if (size.Width > 0 && size.Height > 0 && cursorInfo.Buffer != IntPtr.Zero)
				{
					bitmap = new Bitmap(size.Width, size.Height, 4 * size.Width, System.Drawing.Imaging.PixelFormat.Format32bppArgb, cursorInfo.Buffer);
					iconHandle = bitmap.GetHicon();
					if (iconHandle != IntPtr.Zero && NativeMethods.GetIconInfo(iconHandle, out ICONINFO iconInfo))
					{
						iconInfo.Hotspot = cursorInfo.Hotspot;
						iconInfo.IsIcon = false;
						IntPtr cursorHandle = NativeMethods.CreateIconIndirect(ref iconInfo);
						if (cursorHandle == IntPtr.Zero)
							return Cursors.Default;

						return new CustomCursor(cursorHandle)._cursor;
					}
				}
			}
			catch (AccessViolationException) { throw; }
			catch { }
			finally
			{
				if (iconHandle != IntPtr.Zero)
					NativeMethods.DestroyIcon(iconHandle);
				bitmap?.Dispose();
			}
			return Cursors.Default;
		}

		private CustomCursor(IntPtr cursorHandle)
		{
			_cursorHandle = cursorHandle;
			_cursor = new Cursor(cursorHandle) { Tag = this };
		}

		~CustomCursor()
		{
			if (Interlocked.Exchange(ref _cursor, null) != null)
			{
				NativeMethods.DestroyIcon(_cursorHandle);
			}
		}

	}
}
