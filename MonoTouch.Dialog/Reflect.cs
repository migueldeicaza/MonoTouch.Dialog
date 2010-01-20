//
// Reflect.cs: Creates Element classes from an instance
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010, Novell, Inc.
//
// Code licensed under the MIT X11 license
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class EntryAttribute : Attribute {
		public EntryAttribute (string placeholder)
		{
			Placeholder = placeholder;
		}
		public string Placeholder;
	}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class DateAttribute : Attribute { }
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class TimeAttribute : Attribute { }
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class PasswordAttribute : EntryAttribute {
		public PasswordAttribute (string placeholder) : base (placeholder) {}
	}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class OnTapAttribute : Attribute {
		public OnTapAttribute (string method)
		{
			Method = method;
		}
		public string Method;
	}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class CaptionAttribute : Attribute {
		public CaptionAttribute (string caption)
		{
			Caption = caption;
		}
		public string Caption;
	}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class SectionAttribute : Attribute {
		public SectionAttribute () {}
		
		public SectionAttribute (string caption)
		{
			Caption = caption;
		}
			
		public SectionAttribute (string caption, string footer)
		{
			Caption = caption;
			Footer = footer;
		}
		public string Caption, Footer;
	}

	public class BindingContext : IDisposable {
		public RootElement Root;
		Dictionary<Element,MemberAndInstance> mappings;
			
		class MemberAndInstance {
			public MemberAndInstance (MemberInfo mi, object o)
			{
				Member = mi;
				Obj = o;
			}
			public MemberInfo Member;
			public object Obj;
		}
		
		static object GetValue (MemberInfo mi, object o)
		{
			var fi = mi as FieldInfo;
			if (fi != null)
				return fi.GetValue (o);
			var pi = mi as PropertyInfo;
			
			var getMethod = pi.GetGetMethod ();
			return getMethod.Invoke (o, new object [0]);
		}

		static void SetValue (MemberInfo mi, object o, object val)
		{
			var fi = mi as FieldInfo;
			if (fi != null){
				fi.SetValue (o, val);
				return;
			}
			var pi = mi as PropertyInfo;
			var setMethod = pi.GetSetMethod ();
			setMethod.Invoke (o, new object [] { val });
		}
			
		static string MakeCaption (string name)
		{
			var sb = new StringBuilder (name.Length);
			bool nextUp = true;
			
			foreach (char c in name){
				if (nextUp){
					sb.Append (Char.ToUpper (c));
					nextUp = false;
				} else {
					if (c == '_'){
						sb.Append (' ');
						continue;
					}
					if (Char.IsUpper (c))
						sb.Append (' ');
					sb.Append (c);
				}
			}
			return sb.ToString ();
		}

		// Returns the type for fields and properties and null for everything else
		static Type GetTypeForMember (MemberInfo mi)
		{				
			if (mi is FieldInfo)
				return ((FieldInfo) mi).FieldType;
			else if (mi is PropertyInfo)
				return ((PropertyInfo) mi).PropertyType;
			return null;
		}
		
		public BindingContext (object callbacks, object o, string title)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
			
			mappings = new Dictionary<Element,MemberAndInstance> ();
			var members = o.GetType ().GetMembers (BindingFlags.DeclaredOnly | BindingFlags.Public |
							       BindingFlags.NonPublic | BindingFlags.Instance);

			Root = new RootElement (title);
			Section section = null;
			
			foreach (var mi in members){
				Type mType = GetTypeForMember (mi);

				if (mType == null)
					continue;

				string caption = null;
				object [] attrs = mi.GetCustomAttributes (false);
				foreach (var attr in attrs){
					if (attr is CaptionAttribute)
						caption = ((CaptionAttribute) attr).Caption;
					else if (attr is SectionAttribute){
						if (section != null)
							Root.Add (section);
						var sa = attr as SectionAttribute;
						section = new Section (sa.Caption, sa.Footer);
					}
				}
				if (caption == null)
					caption = MakeCaption (mi.Name);
				
				if (section == null)
					section = new Section ();
				
				Element element = null;
				if (mType == typeof (string)){
					PasswordAttribute pa = null;
					EntryAttribute ea = null;
					NSAction invoke = null;
					
					foreach (object attr in attrs){
						if (attr is PasswordAttribute)
							pa = attr as PasswordAttribute;
						else if (attr is EntryAttribute)
							ea = attr as EntryAttribute;
						
						if (attr is OnTapAttribute){
							string mname = ((OnTapAttribute) attr).Method;
							
							if (callbacks == null){
								throw new Exception ("Your class contains [OnTap] attributes, but you passed a null object for `context' in the constructor");
							}
							
							var method = callbacks.GetType ().GetMethod (mname);
							if (method == null)
								throw new Exception ("Did not find method " + mname);
							invoke = delegate {
								method.Invoke (method.IsStatic ? null : callbacks, new object [0]);
							};
						}
					}
					
					if (pa != null)
						element = new EntryElement (caption, pa.Placeholder, (string) GetValue (mi, o), true);
					else if (ea != null)
						element = new EntryElement (caption, ea.Placeholder, (string) GetValue (mi, o));
					else
						element = new StringElement (caption, (string) GetValue (mi, o));
					if (invoke != null)
						((StringElement) element).Tapped += invoke;
				} else if (mType == typeof (float)){
					element = new FloatElement (null, null, (float) GetValue (mi, o));
				} else if (mType == typeof (bool)){
					element = new BooleanElement (caption, (bool) GetValue (mi, o));
				} else if (mType == typeof (DateTime)){
					var dateTime = (DateTime) GetValue (mi, o);
					bool asDate = false, asTime = false;
					
					foreach (object attr in attrs){
						if (attr is DateAttribute)
							asDate = true;
						else if (attr is TimeAttribute)
							asTime = true;
					}
					
					if (asDate)
						element = new DateElement (caption, dateTime);
					else if (asTime)
						element = new TimeElement (caption, dateTime);
					else
						 element = new DateTimeElement (caption, dateTime);
				} else if (mType.IsEnum){
					var csection = new Section ();
					ulong evalue = Convert.ToUInt64 (GetValue (mi, o), null);
					int idx = 0;
					int selected = 0;
					
					foreach (var fi in mType.GetFields (BindingFlags.Public | BindingFlags.Static)){
						ulong v = Convert.ToUInt64 (GetValue (fi, null));
						
						if (v == evalue)
							selected = idx;
						
						csection.Add (new RadioElement (fi.Name));
						idx++;
					}
					
					element = new RootElement (caption, new RadioGroup (null, selected)) { csection };
				}
				
				if (element == null)
					continue;
				section.Add (element);
				mappings [element] = new MemberAndInstance (mi, o);
			}
			Root.Add (section);
		}
		
		public void Dispose ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing){
				foreach (var element in mappings.Keys){
					element.Dispose ();
				}
				mappings = null;
			}
		}
		
		public void Fetch ()
		{
			foreach (var dk in mappings){
				Element element = dk.Key;
				MemberInfo mi = dk.Value.Member;
				object obj = dk.Value.Obj;
				
				if (element is DateTimeElement)
					SetValue (mi, obj, ((DateTimeElement) element).DateValue);
				else if (element is FloatElement)
					SetValue (mi, obj, ((FloatElement) element).Value);
				else if (element is BooleanElement)
					SetValue (mi, obj, ((BooleanElement) element).Value);
				else if (element is EntryElement)
					SetValue (mi, obj, ((EntryElement) element).Value);
				else if (element is RootElement){
					var re = element as RootElement;
					if (re.radio != null){
						var mType = GetTypeForMember (mi);
						var fi = mType.GetFields (BindingFlags.Public | BindingFlags.Static) [re.RadioSelected];
						
						SetValue (mi, obj, fi.GetValue (null));
					}
				}
			}
		}
	}
}
