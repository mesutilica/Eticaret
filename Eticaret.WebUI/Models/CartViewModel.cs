using Eticaret.Core.Entities;

namespace Eticaret.WebUI.Models
{
    public class CartViewModel
    {
        public List<CartLine>? CartLines { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
