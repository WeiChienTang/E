using ERPCore2.Data.Entities;
using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Components.Pages.Payroll
{
    public class PayrollRowViewModel
    {
        public Employee Employee { get; set; } = null!;
        public PayrollRecord? Record { get; set; }
    }
}
