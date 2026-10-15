using System.Collections.Generic;

namespace CommUnityHub.Models.ViewModels
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; } = new();
        public IEnumerable<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
