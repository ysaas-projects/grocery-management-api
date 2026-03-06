namespace grocery_management.Models
{
    public class SaleItem
    {
        public int SaleItemId { get; set; }

        public int SaleId { get; set; }

        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total { get; set; }

        public Sale Sale { get; set; }

        public Product Product { get; set; }
    }


    public class CreateSaleItemDto
    {
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }
    }
}
