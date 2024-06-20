using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;

namespace SMGI.Common
{
    //If you try using IDictionary with the PropertyGrid control, the results aren't spectacular:
    //Here's how to do it properly.
    //The first thing that you need to figure out is that when you associate an object with the PropertyGrid, it asks for that object's type descriptor, and then asks that about which properties it supports.
    //To fake out the type descriptor, we'll need some kind of adapter object. It'll need to implement ICustomTypeDescriptor:

    class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        Dictionary<string, ParameterInfo> _dictionary;

        public DictionaryPropertyGridAdapter(Dictionary<string, ParameterInfo> d)
        {
            _dictionary = d;
        }
        //Three of the ICustomTypeDescriptor methods are never called by the property grid, but we'll stub them out properly anyway:

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }
        //Then there's a whole slew of methods that are called by PropertyGrid, but we don't need to do anything interesting in them:

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }
        //Then the interesting bit. We simply iterate over the IDictionary, creating a property descriptor for each entry:

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            ArrayList properties = new ArrayList();
            foreach (var e in _dictionary)
            {
                properties.Add(new DictionaryPropertyDescriptor(_dictionary, e.Key));
            }

            PropertyDescriptor[] props =
                (PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

            return new PropertyDescriptorCollection(props);
        }
    }

    //Of course, now we need to implement the DictionaryPropertyDescriptor class:

    public class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        //PropertyDescriptor provides 3 constructors. We want the one that takes a string and an array of attributes:

        Dictionary<string, ParameterInfo> _dictionary;
        string _key;

        internal DictionaryPropertyDescriptor(Dictionary<string, ParameterInfo> d, string key)
            : base(key.ToString(), null)
        {
            _dictionary = d;
            _key = key;
        }

        public override string DisplayName
        {
            get
            {
                return _dictionary[_key].Name;
            }
        }

        //The attributes are used by PropertyGrid to organise the properties into categories, to display help text and so on. We don't bother with any of that at the moment, so we simply pass null.

        //The first interesting member is the PropertyType property. We just get the object out of the dictionary and ask it:

        public override Type PropertyType
        {
            get { return _dictionary[_key].Value.GetType(); }
        }
        //If you knew that all of your values were strings, for example, you could just return typeof(string).

        //Then we implement SetValue and GetValue:

        public override void SetValue(object component, object value)
        {
            _dictionary[_key].Value = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[_key].Value;
        }
        //The component parameter passed to these two methods is whatever value was returned from ICustomTypeDescriptor.GetPropertyOwner. If it weren't for the fact that we need the dictionary object in PropertyType, we could avoid using the _dictionary member, and just grab it using this mechanism.

        //And that's it for interesting things. The rest of the class looks like this:

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
        public override string Category
        {
            get
            {
                return _dictionary[_key].Category;
            }
        }
        public override string Description
        {
            get
            {
                return _dictionary[_key].Info;
            }
        }
    }
    //Then you can just use it like this:

    //private void Form1_Load(object sender, System.EventArgs e)
    //{
    //    IDictionary d = new Hashtable();
    //    d["Hello"] = "World";
    //    d["Meaning"] = 42;
    //    d["Shade"] = Color.ForestGreen;

    //    propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(d);
    //}
}
