namespace ewms_common.Authentications
{
    public class RefreshToken
    {
        public string token { get; set; } = null!;
        public DateTime expires { get; set; }
    }
}
