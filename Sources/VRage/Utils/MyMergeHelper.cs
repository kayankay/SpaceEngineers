﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VRage.Utils
{
    /// <summary>
    /// Helper class for merge funcionality. Performs comparison between
    /// source and other values and set on self if value is different
    /// </summary>
    public static class MyMergeHelper
    {
        public static void Merge<T>(T self, T source, T other)
            where T : class
        {
            Debug.Assert(self != null, "Self must be non null");
            Debug.Assert(source != null, "Source must be non null");
            Debug.Assert(other != null, "Ohter must be non null");

            object self_ = self;
            object source_ = source;
            object other_ = other;
            MergeInternal(typeof(T), ref self_, ref source_, ref other_);
        }

        public static void Merge<T>(ref T self, ref T source, ref T other)
            where T : struct
        {
            // Perform boxing now of value type
            object self_ = self;
            object source_ = source;
            object other_ = other;
            MergeInternal(typeof(T), ref self_, ref source_, ref other_);
            self = (T)self_;
        }

        private static void MergeInternal(Type type, ref object self, ref object source, ref object other)
        {
            if (self == null)
                self = Activator.CreateInstance(type);

            if (source == null)
                source = Activator.CreateInstance(type);

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object valueSource = field.GetValue(source);
                object valueOther = field.GetValue(other);
                if (valueSource == valueOther) // Ref check only
                    continue;

                bool equals = false;
                if (IsPrimitive(field.FieldType) && !(equals = valueSource.Equals(valueOther))
                    || valueSource != null && valueOther == null)
                {
                    field.SetValue(self, valueSource);
                }
                else if (!equals)// valueOther != null
                {
                    object valueSelf = field.GetValue(self);

                    MergeInternal(field.FieldType, ref valueSelf, ref valueSource, ref valueOther);
                    field.SetValue(self, valueSelf);
                }
            }
        }

        private static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type == typeof(String) || type == typeof(Type);
        }
    }
}
