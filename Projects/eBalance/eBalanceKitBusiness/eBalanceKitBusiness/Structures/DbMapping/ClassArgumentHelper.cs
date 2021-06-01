using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using DbAccess;

namespace eBalanceKitBusiness.Structures.DbMapping {

    public struct ArgumentSearchQuestion {
        public ArgumentSearchQuestion(string className, string propertyName, string[] attributeNames) {
            ClassName = className;
            PropertyName = propertyName;
            AttributeNames = attributeNames;
        }

        public string ClassName;

        public string PropertyName;

        public string[] AttributeNames;
    }

    public class ClassArgumentHelper {
        private static ClassArgumentHelper _instance;

        private ClassArgumentHelper() { _cachedResults = new Dictionary<ArgumentSearchQuestion, List<object>>(); }

        public static ClassArgumentHelper Instance {
            get { return _instance ?? (_instance = new ClassArgumentHelper()); }
        }

        private readonly Dictionary<ArgumentSearchQuestion, List<object>> _cachedResults;

        public List<object> GetColumnAttribute(ArgumentSearchQuestion question) {
            // goes there only if the question is the same instance.
            if (_cachedResults.ContainsKey(question)) {
                return _cachedResults[question];
            }
            // if the question's class name and property name is the same as one of the previous question's, we can check if 
            // all the questioned attributes are available. If available we can return with the cached values.
            foreach (KeyValuePair<ArgumentSearchQuestion, List<object>> cachedResult in _cachedResults) {
                if (cachedResult.Key.ClassName != question.ClassName ||
                    cachedResult.Key.PropertyName != question.PropertyName)
                    continue;
                bool containsEvery = true;
                List<object> ret = new List<object>();
                foreach (string questionAttributeName in question.AttributeNames) {
                    bool found = false;
                    int i = 0;
                    foreach (string cachedAttributeName in cachedResult.Key.AttributeNames) {
                        if (cachedAttributeName == questionAttributeName) {
                            ret.Add(cachedResult.Value[i]);
                            found = true;
                            break;
                        }
                        i++;
                    }
                    if (!found) {
                        containsEvery = false;
                        break;
                    }
                }
                if (containsEvery) {
                    return ret;
                }
            }
            // no cached results. Make a new one.
            Type classType = Type.GetType(question.ClassName);
            // didn't find the class
            if (classType == null) {
#if DEBUG
                Debug.Fail("didn't find class");
#endif
// ReSharper disable HeuristicUnreachableCode
                _cachedResults[question] = null;
                return null;
// ReSharper restore HeuristicUnreachableCode
            }
            PropertyInfo propertyInfo = classType.GetProperty(question.PropertyName);
            // didn't find the property
            if (propertyInfo == null) {
#if DEBUG
                Debug.Fail("didn't find property");
#endif
// ReSharper disable HeuristicUnreachableCode
                _cachedResults[question] = null;
                return null;
// ReSharper restore HeuristicUnreachableCode
            }
            Type dbColumnAttributeType = typeof (DbColumnAttribute);
            object[] propertyAttribute = propertyInfo.GetCustomAttributes(dbColumnAttributeType, false);
            _cachedResults[question] = new List<object>();
            foreach (string properyName in question.AttributeNames) {
                _cachedResults[question].Add(
                    propertyAttribute[0].GetType().GetProperty(properyName).GetValue(propertyAttribute[0], null));
            }
            return _cachedResults[question];
        }
    }
}
