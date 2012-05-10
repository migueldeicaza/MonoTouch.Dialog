using System;

namespace MonoTouch.Dialog
{
	public abstract class PropertyStore<TType> {
		
		public TType Value {
			get {
				return GetValue();
			}
			set {
				SetValue(value);
			}
		}
		
		protected abstract void SetValue(TType value);
		protected abstract TType GetValue();
		
		public static implicit operator TType(PropertyStore<TType> val) {
			return val.Value;
		}
	}
	
	public class UnlinkedPropertyStore<TType> : PropertyStore<TType> {
		TType _value;
		
		public UnlinkedPropertyStore()
			:this(default(TType))
		{
		}
		
		public UnlinkedPropertyStore(TType initialValue) {
			_value = initialValue;
		}

		#region implemented abstract members of MonoTouch.Dialog.PropertyStore[TType]
		protected override void SetValue (TType value)
		{
			_value = value;
		}

		protected override TType GetValue ()
		{
			return _value;
		}
		#endregion
	}
	
	public static class PropertyStoreExtensions {
		public static UnlinkedPropertyStore<TType> CreateUnlinked<TType>(this TType initialValue) {
			return new MonoTouch.Dialog.UnlinkedPropertyStore<TType>(initialValue);
		}
	}
}

