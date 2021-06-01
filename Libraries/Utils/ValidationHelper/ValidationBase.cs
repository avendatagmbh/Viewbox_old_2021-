using System.Collections.Generic;

namespace Utils.ValidationHelper
{
    public class ValidationBase
    {
        public readonly Dictionary<string, string> Errors;

        public ValidationBase()
        {
            Errors = new Dictionary<string, string>();
        }

        public void AddError(string propertyName, string message)
        {
            if (!Errors.ContainsKey(propertyName))
            {
                Errors[propertyName] = message;
            }
        }

        public void RemoveErrors(string propertyName)
        {
            Errors.Remove(propertyName);
        }

        public string GetErrorMessageForProperty(string propertyName)
        {
            string message;
            Errors.TryGetValue(propertyName, out message);
            return message;
        }

        public bool HasErrors()
        {
            return Errors.Count != 0;
        }
    }
}