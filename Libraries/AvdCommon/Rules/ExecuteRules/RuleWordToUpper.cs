namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleWordToUpper : ExecuteRule
    {
        public RuleWordToUpper()
        {
            Name = "Jedes Wort gro�schreiben";
            UniqueName = "WordToUpper";
        }

        public override bool HasParameter
        {
            get { return false; }
        }

        public override string Execute(string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }
    }
}