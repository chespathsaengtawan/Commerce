using System.ComponentModel.DataAnnotations;

namespace ShopInstallment.Models
{
    public class SellerPaymentSetting
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string SellerId { get; set; } = string.Empty;

        public bool AllowPromptPay { get; set; } = true;
        public bool AllowBillPayment { get; set; } = true;
        public bool AllowTrueWallet { get; set; } = true;
        public bool AllowBankTransfer { get; set; } = true;
        public bool AllowCoin { get; set; } = true;

        // Fees by method (% or fixed)
        public decimal PromptPayFeePercent { get; set; } = 0m;
        public decimal BillPaymentFeePercent { get; set; } = 0m;
        public decimal TrueWalletFeePercent { get; set; } = 0m;
        public decimal BankTransferFeePercent { get; set; } = 0m;
        public decimal CoinFeePercent { get; set; } = 0m;
    }
}
