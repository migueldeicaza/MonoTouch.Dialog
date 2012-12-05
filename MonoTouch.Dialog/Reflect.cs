//
// Reflect.cs: Creates Element classes from an instance
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//   
// Changes:
//   2012-12-05, Added CustomElementAttribute and integrations (Reinder Kamphorst, reinder@toubab.nl)
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
		public EntryAttribute () : this (null) { }

		public EntryAttribute (string placeholder)
		{
			Placeholder = placeholder;
		}

		public string Placeholder;
		public UIKeyboardType KeyboardType;
		public UITextAutocorrectionType AutocorrectionType;
		public UITextAutocapitalizationType AutocapitalizationType;
		public UITextFieldViewMode ClearButtonMode;
	}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class DateAttribute : Attribute { }
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class TimeAttribute : Attribute { }
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class CheckboxAttribute : Attribute {}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class MultilineAttribute : Attribute {}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class HtmlAttribute : Attribute {}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class SkipAttribute : Attribute {}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class PasswordAttribute : EntryAttribute {
		public PasswordAttribute (string placeholder) : base (placeholder) {}
	}
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class AlignmentAttribute : Attribute {
		public AlignmentAttribute (UITextAlignment alignment) {
			Alignment = alignment;
		}
		public UITextAlignment Alignment;
	}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class RadioSelectionAttribute : Attribute {
		public string Target;
		public RadioSelectionAttribute (string target) 
		{
			Target = target;
		}
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

	public class RangeAttribute : Attribute {
		public RangeAttribute (float low, float high)
		{
			Low = low;
			High = high;
		}
		public float Low, High;
		public bool ShowCaption;
	}

	/// <summary>
	/// Make your own custom element attributes for the reflection api
	/// by deriving from this class. 
	/// </summary>
	/// <remarks>
	/// For example, if you implement this class as e.g. MyElementAttribute,
	/// specifying how to create an instance of MyElement (CreateElement) and 
	/// specifying how to fetch its value (GetValue), you will be able to
	/// decorate class fields / properties with [MyElement] and have them rendered
	/// through the MonoTouch.Dialog reflection api.
	/// </remarks>
	public abstract class CustomElementAttribute : Attribute {
		/// <summary>
		/// Creates a new element of the type this CustomElementAttribute is made for
		/// </summary>
		/// <returns>
		/// A newly instantiated element that is initialized with given memberValue.
		/// </returns>
		/// <param name='caption>
		/// Caption to display on the created element.
		/// </param>			
		/// <param name='forMember'>
		/// MemberInfo instance reflecting the member to create the element for.
		/// </param>
		/// <param name='memberType'>
		/// Type of the reflected member. 
		/// </param>
		/// <param name='memberValue'>
		/// Value to initialize the new element with. The type of memberValue will be memberType.
		/// </param>
		/// <param name='attributes'>
		/// All the <seealso cref="Attribute"/>s that were defined on this member. 
		/// With these, you can define custon behavior for your element for other attributes,
		/// e.g. <seealso cref="OnTapAttribute"/>.
		/// </param>
		public abstract Element CreateElement(string caption, MemberInfo forMember, Type memberType, object memberValue, object[] attributes);

		/// <summary>
		/// Fetch the value of an element that was returned by <see cref="CreateElement"/>.
		/// </summary>
		/// <returns>
		/// The value that is held by given element. 
		/// </returns>
		/// <param name='element'>
		/// Element to fetch the value for. This element was previously created with <see cref="CreateElement"/>.
		/// </param>
		/// <param name='resultType'>
		/// Type of the object to return. Even tough the return type of this method is <see cref="object"/>, 
		/// it is the responsibility of this method to make sure the return value can be
		/// directly cast to the specified resultType (i.e., it should not need extra conversion through,
		/// for example, the <see cref="Convert"/> class).
		/// </param>
		public abstract object GetValue(Element element, Type resultType);
	}
	
	public class BindingContext : IDisposable {
		public RootElement Root;
		Dictionary<Element,MemberAndInstance> mappings;

		class MemberAndInstance {
			public MemberAndInstance (MemberInfo mi, object o, CustomElementAttribute customInfo, Type memberType)
			{
				Member = mi;
				Obj = o;
				CustomInfo = customInfo;
				MemberType = memberType;
			}

			public MemberInfo Member;
			public object Obj;
			public CustomElementAttribute CustomInfo;
			public Type MemberType;
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

			Root = new RootElement (title);
			Populate (callbacks, o, Root);
		}
		
		void Populate (object callbacks, object o, RootElement root)
		{
			MemberInfo last_radio_index = null;
			var members = o.GetType ().GetMembers (BindingFlags.DeclaredOnly | BindingFlags.Public |
							       BindingFlags.NonPublic | BindingFlags.Instance);

			Section section = null;
			
			foreach (var mi in members){
				Type mType = GetTypeForMember (mi);

				if (mType == null)
					continue;

				CustomElementAttribute customElementAttr = null;
				string caption = null;
				object [] attrs = mi.GetCustomAttributes (false);
				bool skip = false;
				foreach (var attr in attrs){
					if (attr is SkipAttribute || attr is System.Runtime.CompilerServices.CompilerGeneratedAttribute) {
						skip = true;
						break;
					} else if (attr is CaptionAttribute) {
						caption = ((CaptionAttribute) attr).Caption;
					} else if (attr is SectionAttribute) {
						if (section != null)
							root.Add (section);
						var sa = attr as SectionAttribute;
						section = new Section (sa.Caption, sa.Footer);
					} else if (attr is CustomElementAttribute){
						customElementAttr = attr as CustomElementAttribute;
					}

				}
				if (skip)
					continue;
				
				if (caption == null)
					caption = MakeCaption (mi.Name);
				
				if (section == null)
					section = new Section ();
				
				Element element = null;
				if (customElementAttr != null) {
					element = customElementAttr.CreateElement(caption, mi, mType, GetValue(mi, o), attrs);
				} else if (mType == typeof (string)){
					PasswordAttribute pa = null;
					AlignmentAttribute align = null;
					EntryAttribute ea = null;
					object html = null;
					NSAction invoke = null;
					bool multi = false;
					
					foreach (object attr in attrs){
						if (attr is PasswordAttribute)
							pa = attr as PasswordAttribute;
						else if (attr is EntryAttribute)
							ea = attr as EntryAttribute;
						else if (attr is MultilineAttribute)
							multi = true;
						else if (attr is HtmlAttribute)
							html = attr;
						else if (attr is AlignmentAttribute)
							align = attr as AlignmentAttribute;
						
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
					
					string value = (string) GetValue (mi, o);
					if (pa != null)
						element = new EntryElement (caption, pa.Placeholder, value, true);
					else if (ea != null)
						element = new EntryElement (caption, ea.Placeholder, value) { KeyboardType = ea.KeyboardType, AutocapitalizationType = ea.AutocapitalizationType, AutocorrectionType = ea.AutocorrectionType, ClearButtonMode = ea.ClearButtonMode };
					else if (multi)
						element = new MultilineElement (caption, value);
					else if (html != null)
						element = new HtmlElement (caption, value);
					else {
						var selement = new StringElement (caption, value);
						element = selement;
						
						if (align != null)
							selement.Alignment = align.Alignment;
					}
					
					if (invoke != null)
						((StringElement) element).Tapped += invoke;
				} else if (mType == typeof (float)){
					var floatElement = new FloatElement (null, null, (float) GetValue (mi, o));
					floatElement.Caption = caption;
					element = floatElement;
					
					foreach (object attr in attrs){
						if (attr is RangeAttribute){
							var ra = attr as RangeAttribute;
							floatElement.MinValue = ra.Low;
							floatElement.MaxValue = ra.High;
							floatElement.ShowCaption = ra.ShowCaption;
						}
					}
				} else if (mType == typeof (bool)){
					bool checkbox = false;
					foreach (object attr in attrs){
						if (attr is CheckboxAttribute)
							checkbox = true;
					}
					
					if (checkbox)
						element = new CheckboxElement (caption, (bool) GetValue (mi, o));
					else
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
						
						CaptionAttribute ca = Attribute.GetCustomAttribute(fi, typeof(CaptionAttribute)) as CaptionAttribute;
						csection.Add (new RadioElement (ca != null ? ca.Caption : MakeCaption (fi.Name)));
						idx++;
					}
					
					element = new RootElement (caption, new RadioGroup (null, selected)) { csection };
				} else if (mType == typeof (UIImage)){
					element = new ImageElement ((UIImage) GetValue (mi, o));
				} else if (typeof (System.Collections.IEnumerable).IsAssignableFrom (mType)){
					var csection = new Section ();
					int count = 0;
					
					if (last_radio_index == null)
						throw new Exception ("IEnumerable found, but no previous int found");
					foreach (var e in (IEnumerable) GetValue (mi, o)){
						csection.Add (new RadioElement (e.ToString ()));
						count++;
					}
					int selected = (int) GetValue (last_radio_index, o);
					if (selected >= count || selected < 0)
						selected = 0;
					element = new RootElement (caption, new MemberRadioGroup (null, selected, last_radio_index)) { csection };
					last_radio_index = null;
				} else if (typeof (int) == mType){
					foreach (object attr in attrs){
						if (attr is RadioSelectionAttribute){
							last_radio_index = mi;
							break;
						}
					}
				} else {
					var nested = GetValue (mi, o);
					if (nested != null){
						var newRoot = new RootElement (caption);
						Populate (callbacks, nested, newRoot);
						element = newRoot;
					}
				}
				
				if (element == null)
					continue;
				section.Add (element);
				mappings [element] = new MemberAndInstance (mi, o, customElementAttr, mType);
			}
			root.Add (section);
		}
		
		class MemberRadioGroup : RadioGroup {
			public MemberInfo mi;
			
			public MemberRadioGroup (string key, int selected, MemberInfo mi) : base (key, selected)
			{
				this.mi = mi;
			}
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
				CustomElementAttribute customInfo = dk.Value.CustomInfo;
				Type memberType = dk.Value.MemberType;
				Element element = dk.Key;
				MemberInfo mi = dk.Value.Member;
				object obj = dk.Value.Obj;

				if (customInfo != null) 
					SetValue (mi, obj, customInfo.GetValue(element, memberType)); 
				else if (element is DateTimeElement)
					SetValue (mi, obj, ((DateTimeElement) element).DateValue);
				else if (element is FloatElement)
					SetValue (mi, obj, ((FloatElement) element).Value);
				else if (element is BooleanElement)
					SetValue (mi, obj, ((BooleanElement) element).Value);
				else if (element is CheckboxElement)
					SetValue (mi, obj, ((CheckboxElement) element).Value);
				else if (element is EntryElement){
					var entry = (EntryElement) element;
					entry.FetchValue ();
					SetValue (mi, obj, entry.Value);
				} else if (element is ImageElement)
					SetValue (mi, obj, ((ImageElement) element).Value);
				else if (element is RootElement){
					var re = element as RootElement;
					if (re.group as MemberRadioGroup != null){
						var group = re.group as MemberRadioGroup;
						SetValue (group.mi, obj, re.RadioSelected);
					} else if (re.group as RadioGroup != null){
						var mType = GetTypeForMember (mi);
						var fi = mType.GetFields (BindingFlags.Public | BindingFlags.Static) [re.RadioSelected];
						
						SetValue (mi, obj, fi.GetValue (null));
					}
				}
			}
		}
	}
}
