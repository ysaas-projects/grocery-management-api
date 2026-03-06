namespace grocery_management.Models
{
    public class Sale
    {
        public int SaleId { get; set; }

        public int FirmId { get; set; }

        public string InvoiceNumber { get; set; }

        public decimal Subtotal { get; set; }

        public decimal GST { get; set; }

        public decimal Total { get; set; }

        public string PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<SaleItem> Items { get; set; }
    }


    public class CreateSaleDto
    {
        public decimal Subtotal { get; set; }

        public decimal GST { get; set; }

        public decimal Total { get; set; }

        public string PaymentMethod { get; set; }

        public List<CreateSaleItemDto> Items { get; set; }
    }

}
