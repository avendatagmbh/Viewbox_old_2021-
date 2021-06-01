using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace EasyDocExtraction.Helper
{
    public class ReflectionHelper
    {
        public static TConverter CreateInstance<TConverter>(Type[] types, object[] parameters)
        {
            ConstructorInfo converterCtor = typeof(TConverter).GetConstructor(types);

            if (converterCtor == null) throw new Exception("No matching constructor for type " + typeof(TConverter).Name + " takes parameters: " + String.Join(",", (from p in parameters select p.GetType().Name)));

            return (TConverter)converterCtor.Invoke(parameters);
        }
        /// <summary>
        /// Instanciate the constructor that corresponds to the parameters passed
        /// </summary>
        /// <typeparam name="TConverter"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static TConverter CreateInstance<TConverter>(params object[] parameters) 
        {
            return CreateInstance<TConverter>(parameters.ToList().ConvertAll<Type>(p => p.GetType()).ToArray(), parameters);
        }

        /// <summary>
        /// Helper method to display object properties.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string GetPropertiesWithValues(object o)
        {
            return "{ "+ string.Join(", ", (from p in o.GetType().GetProperties() select p.Name + "=" + p.GetValue(o, null)).ToArray()) + " }"; 
        }

        public static int GetMaxLengthAttributeFromProperty<T>(string propertyName) 
        {
            PropertyInfo property = typeof(T).GetProperty(propertyName);
            foreach (var a in property.GetCustomAttributes(true)) {
                var maxLengthAttribute = a as MaxLengthAttribute;
                if (maxLengthAttribute != null)
                    return maxLengthAttribute.Length;
            }
            return 0;
        }


    }
}
