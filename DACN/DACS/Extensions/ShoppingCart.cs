using DACS.Models;

namespace DACS.Extention
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new
        List<CartItem>();
        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId ==
            item.ProductId);
            if (existingItem != null)
            {
                existingItem.Khoiluong += item.Khoiluong;
            }
            else
            {
                Items.Add(item);
            }
        }
        public void RemoveItem(string productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }
        // Các phương thức khác...
    }
}
