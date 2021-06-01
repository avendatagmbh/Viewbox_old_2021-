namespace Utils
{
    public class NamedEnum
    {
        public NamedEnum(object value, string name)
        {
            Value = value;
            Name = name;
        }

        public object Value { get; set; }
        public string Name { get; set; }
    }
}