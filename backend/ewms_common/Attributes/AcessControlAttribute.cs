namespace ewms_common.Attributes
{
    public class AcessControlAttribute : Attribute
    {
        public bool Allowed { get; set; }
        public AcessControlAttribute(bool allowed)
        {
            Allowed = allowed;
        }
    }
}
