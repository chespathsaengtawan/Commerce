using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace ShopInstallment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(ApplicationDbContext context, IConfiguration config, ILogger<WebhooksController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Omise Webhook Endpoint
        /// ตั้งค่า URL นี้ใน Omise Dashboard: https://yourdomain.com/api/webhooks/omise
        /// </summary>
        [HttpPost("omise")]
        public async Task<IActionResult> OmiseWebhook()
        {
            try
            {
                // อ่าน request body
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                _logger.LogInformation("Received Omise webhook: {Body}", body);

                // ตรวจสอบ signature (ความปลอดภัย)
                var signature = Request.Headers["X-Omise-Signature"].FirstOrDefault();
                if (!string.IsNullOrEmpty(signature))
                {
                    var webhookSecret = _config["Omise:WebhookSecret"];
                    if (!string.IsNullOrEmpty(webhookSecret) && !ValidateSignature(body, signature, webhookSecret))
                    {
                        _logger.LogWarning("Invalid webhook signature");
                        return Unauthorized(new { error = "Invalid signature" });
                    }
                }

                // Parse JSON
                var webhookEvent = JsonSerializer.Deserialize<OmiseWebhookEvent>(body);
                if (webhookEvent == null)
                {
                    return BadRequest(new { error = "Invalid webhook data" });
                }

                // จัดการตาม event type
                var result = webhookEvent.Key switch
                {
                    "charge.complete" => await HandleChargeComplete(webhookEvent),
                    "charge.create" => await HandleChargeCreate(webhookEvent),
                    "charge.update" => await HandleChargeUpdate(webhookEvent),
                    "charge.capture" => await HandleChargeCapture(webhookEvent),
                    "refund.create" => await HandleRefundCreate(webhookEvent),
                    "transfer.create" => await HandleTransferCreate(webhookEvent),
                    "transfer.update" => await HandleTransferUpdate(webhookEvent),
                    _ => await HandleUnknownEvent(webhookEvent)
                };

                if (result)
                {
                    return Ok(new { message = "Webhook processed successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Failed to process webhook" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Omise webhook");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private bool ValidateSignature(string body, string signature, string secret)
        {
            try
            {
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
                var computedSignature = Convert.ToBase64String(hash);
                return signature == computedSignature;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> HandleChargeComplete(OmiseWebhookEvent webhookEvent)
        {
            _logger.LogInformation("Processing charge.complete event");

            var transactionId = webhookEvent.Data?.Id;
            if (string.IsNullOrEmpty(transactionId))
            {
                return false;
            }

            // หา Payment จาก TransactionId
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for transaction: {TransactionId}", transactionId);
                return false;
            }

            // อัปเดตสถานะ Payment
            if (payment.Status != "Completed")
            {
                payment.Status = "Completed";
                payment.PaymentDate = DateTime.UtcNow;

                // อัปเดต Order ถ้าชำระครบแล้ว
                if (payment.Order != null)
                {
                    payment.Order.PaidAmount += payment.Amount;
                    payment.Order.RemainingAmount = payment.Order.TotalAmount - payment.Order.PaidAmount;

                    if (payment.Order.RemainingAmount <= 0)
                    {
                        payment.Order.Status = "Paid";
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment {PaymentId} marked as completed", payment.Id);
            }

            return true;
        }

        private async Task<bool> HandleChargeCreate(OmiseWebhookEvent webhookEvent)
        {
            _logger.LogInformation("Processing charge.create event");
            // บันทึก log หรือส่งแจ้งเตือน
            return await Task.FromResult(true);
        }

        private async Task<bool> HandleChargeUpdate(OmiseWebhookEvent webhookEvent)
        {
            _logger.LogInformation("Processing charge.update event");
            
            var transactionId = webhookEvent.Data?.Id;
            var status = webhookEvent.Data?.Status;

            if (string.IsNullOrEmpty(transactionId))
            {
                return false;
            }

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
            if (payment != null)
            {
                // อัปเดตสถานะตาม Omise status
                if (status == "failed")
                {
                    payment.Status = "Failed";
                }
                else if (status == "expired")
                {
                    payment.Status = "Expired";
                }
                else if (status == "successful")
                {
                    payment.Status = "Completed";
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }

        private async Task<bool> HandleChargeCapture(OmiseWebhookEvent webhookEvent)
        {
            _logger.LogInformation("Processing charge.capture event");
            return await HandleChargeComplete(webhookEvent);
        }

        private async Task<bool> HandleRefundCreate(OmiseWebhookEvent webhookEvent)
        {
            _logger.LogInformation("Processing refund.create event");

            var transactionId = webhookEvent.Data?.ChargeId;
            var refundAmount = webhookEvent.Data?.Amount ?? 0;

            if (string.IsNullOrEmpty(transactionId))
            {
                return false;
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

            if (payment != null)
            {
                payment.Status = "Refunded";

                // อัปเดต Order
                if (payment.Order != null)
                {
                    payment.Order.PaidAmount -= (refundAmount / 100m); // Omise ใช้ satang
                    payment.Order.RemainingAmount = payment.Order.TotalAmount - payment.Order.PaidAmount;
                    payment.Order.Status = "Refunded";
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }

        private async Task<bool> HandleTransferCreate(OmiseWebhookEvent webhookEvent)
        {
            _logger.LogInformation("Processing transfer.create event - ระบบโอนเงินให้ผู้ขาย");
            // TODO: อัปเดตสถานะการโอนเงินให้ผู้ขาย
            return await Task.FromResult(true);
        }

        private async Task<bool> HandleTransferUpdate(OmiseWebhookEvent webhookEvent)
        {
            _logger.LogInformation("Processing transfer.update event");
            return await Task.FromResult(true);
        }

        private async Task<bool> HandleUnknownEvent(OmiseWebhookEvent webhookEvent)
        {
            _logger.LogInformation("Unknown webhook event: {EventType}", webhookEvent.Key);
            return await Task.FromResult(true);
        }
    }

    // Omise Webhook Event Model (Simplified)
    public class OmiseWebhookEvent
    {
        public string? Key { get; set; } // event type: charge.complete, refund.create, etc.
        public OmiseWebhookData? Data { get; set; }
    }

    public class OmiseWebhookData
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public string? Status { get; set; }
        public long? Amount { get; set; } // satang (1/100 baht)
        public string? ChargeId { get; set; }
        public string? Currency { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
