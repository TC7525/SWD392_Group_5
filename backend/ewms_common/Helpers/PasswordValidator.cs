namespace ewms_common.Helpers
{
    public class PasswordValidator
    {
        public int MinLength { get; set; } = 6;
        public bool RequireDigit { get; set; } = false;
        public bool RequireLowercase { get; set; } = false;
        public bool RequireUppercase { get; set; } = false;
        public bool RequireNonLetterOrDigit { get; set; } = false;

        public PasswordValidator()
        {
        }

        public PasswordValidator(int minLength, bool requireDigit, bool requireLowercase, bool requireUppercase, bool requireNonLetterOrDigit)
        {
            MinLength = minLength;
            RequireDigit = requireDigit;
            RequireLowercase = requireLowercase;
            RequireUppercase = requireUppercase;
            RequireNonLetterOrDigit = requireNonLetterOrDigit;
        }

        public bool Validate(string password)
        {
            if (password.Length < MinLength) return false;
            if (RequireDigit && !password.Any(char.IsDigit)) return false;
            if (RequireLowercase && !password.Any(char.IsLower)) return false;
            if (RequireUppercase && !password.Any(char.IsUpper)) return false;
            if (RequireNonLetterOrDigit && !password.Any(c => !char.IsLetterOrDigit(c))) return false;
            return true;
        }
    }
}
