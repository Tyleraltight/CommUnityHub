namespace CommUnityHub.Models
{
    public static class SD
    {
        // Roles
        public const string Role_Admin = "Admin";
        public const string Role_Customer = "Customer";
        public const string Role_Provider = "Provider";

        // Order Status
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        // Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected";

        // Session
        public const string SessionCart = "SessionShoppingCart";
    }
}
