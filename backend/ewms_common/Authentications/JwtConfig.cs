namespace ewms_common.Authentications
{
    public class TokenAuthentication
    {
        public string Issuer { get; set; } = null!;
        public string SecretSercurityKey { get; set; } = null!;
        public int AccessTokenExpirationDay { get; set; } = 1;
        public int RefeshTokenExpirationDay { get; set; } = 10;
    }
}
