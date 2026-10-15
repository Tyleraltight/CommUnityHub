namespace CommUnityHub.Models.Dtos
{
    public class CheckoutRequestDto
    {
        public int OrderHeaderId { get; set; }
        public string ReturnUrlBase { get; set; } = string.Empty;
    }
}
