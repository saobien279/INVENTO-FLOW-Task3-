namespace InventoFlow.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        // Quan hệ 1-N với User (Sen đã có bảng User từ TaskFlow)
        public int UserId { get; set; }

        // Một đơn hàng có nhiều món hàng chi tiết
        public ICollection<OrderItem> ?OrderItems { get; set; }
    }
}

