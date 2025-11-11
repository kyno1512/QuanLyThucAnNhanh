using System;
using System.Collections.Generic;

namespace QuanLyThucAnNhanh.DTOs
{
    public class OrderSummaryDto
    {
        public long OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
    }

    public class OrderItemDto
    {
        public long MonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; } = string.Empty;
    }

    public class OrderCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }

    public class OrderDetailDto : OrderSummaryDto
    {
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public OrderCustomerDto Customer { get; set; } = new();
        public List<OrderItemDto> Items { get; set; } = new();
    }
}


