
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
//
using System;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	
	public class TimeElement : DateTimeElement {
		public TimeElement (string caption, DateTime date) : base (caption, date)
		{
		}
		
		public override string FormatDate (DateTime dt)
		{
			return GetDateWithKind (dt).ToLocalTime ().ToShortTimeString ();
		}
		
		public override UIDatePicker CreatePicker ()
		{
			var picker = base.CreatePicker ();
			picker.Mode = UIDatePickerMode.Time;
			return picker;
		}
	}
	
}
