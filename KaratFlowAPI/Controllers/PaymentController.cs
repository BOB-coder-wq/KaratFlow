using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KaratFlowAPI.Models;
using KaratFlowAPI.Services;

namespace KaratFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IKaratFlowService _karatFlowService;

        public PaymentController(IKaratFlowService karatFlowService)
        {
            _karatFlowService = karatFlowService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendPayment([FromBody] PaymentRequest request)
        {
            try
            {
                // Get current user
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Get current user's primary account
                var currentUserAccounts = await _karatFlowService.GetUserAccountsAsync(currentUserId);
                var fromAccount = currentUserAccounts.FirstOrDefault();
                if (fromAccount == null)
                {
                    return BadRequest(new { message = "No account found for current user" });
                }

                // Get recipient
                var recipient = await _karatFlowService.GetUserByUsernameAsync(request.RecipientUsername);
                if (recipient == null)
                {
                    return BadRequest(new { message = "Recipient not found" });
                }

                var recipientAccounts = await _karatFlowService.GetUserAccountsAsync(recipient.Id!);
                var toAccount = recipientAccounts.FirstOrDefault();
                if (toAccount == null)
                {
                    return BadRequest(new { message = "Recipient has no account" });
                }

                // Check balance
                if (fromAccount.Balance < request.Amount)
                {
                    return BadRequest(new { message = "Insufficient balance" });
                }

                // Process payment
                var success = await _karatFlowService.ProcessPaymentAsync(
                    fromAccount.Id!, 
                    toAccount.Id!, 
                    request.Amount, 
                    request.Description ?? $"Payment to {request.RecipientUsername}"
                );

                if (success)
                {
                    // Get updated account info
                    var updatedFromAccount = await _karatFlowService.GetAccountByNumberAsync(fromAccount.AccountNumber);
                    
                    return Ok(new PaymentResponse
                    {
                        Success = true,
                        Message = $"Payment of {request.Amount:N0} Karats sent to {request.RecipientUsername}",
                        Transaction = new Transaction
                        {
                            FromAccountId = fromAccount.Id!,
                            ToAccountId = toAccount.Id!,
                            Amount = request.Amount,
                            Description = request.Description ?? $"Payment to {request.RecipientUsername}",
                            Type = TransactionType.Payment,
                            Status = TransactionStatus.Completed,
                            CreatedAt = DateTime.UtcNow,
                            ProcessedAt = DateTime.UtcNow,
                            FromAccount = updatedFromAccount
                        }
                    });
                }
                else
                {
                    return BadRequest(new PaymentResponse
                    {
                        Success = false,
                        Message = "Payment processing failed"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaymentResponse
                {
                    Success = false,
                    Message = $"Payment error: {ex.Message}"
                });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistory([FromQuery] int limit = 50)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userAccounts = await _karatFlowService.GetUserAccountsAsync(currentUserId);
                var primaryAccount = userAccounts.FirstOrDefault();
                if (primaryAccount == null)
                {
                    return BadRequest(new { message = "No account found" });
                }

                var transactions = await _karatFlowService.GetTransactionsAsync(primaryAccount.Id!, limit);
                
                return Ok(new { 
                    transactions = transactions,
                    account = primaryAccount,
                    totalTransactions = transactions.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving transactions: {ex.Message}" });
            }
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userAccounts = await _karatFlowService.GetUserAccountsAsync(currentUserId);
                var primaryAccount = userAccounts.FirstOrDefault();
                if (primaryAccount == null)
                {
                    return BadRequest(new { message = "No account found" });
                }

                return Ok(new {
                    balance = primaryAccount.Balance,
                    accountNumber = primaryAccount.AccountNumber,
                    currency = primaryAccount.Currency
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving balance: {ex.Message}" });
            }
        }

        [HttpPost("nfc")]
        public async Task<IActionResult> ProcessNFCPayment([FromBody] NFCPaymentRequest request)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userAccounts = await _karatFlowService.GetUserAccountsAsync(currentUserId);
                var receivingAccount = userAccounts.FirstOrDefault();
                if (receivingAccount == null)
                {
                    return BadRequest(new { message = "No account found" });
                }

                // Process NFC payment
                var success = await _karatFlowService.ProcessNFCPaymentAsync(
                    request.CardNumber, 
                    request.Amount, 
                    receivingAccount.Id!
                );

                if (success)
                {
                    // Get updated account info
                    var updatedAccount = await _karatFlowService.GetAccountByNumberAsync(receivingAccount.AccountNumber);
                    
                    return Ok(new PaymentResponse
                    {
                        Success = true,
                        Message = $"NFC payment of {request.Amount:N0} Karats received",
                        Transaction = new Transaction
                        {
                            FromAccountId = "NFC_CARD",
                            ToAccountId = receivingAccount.Id!,
                            Amount = request.Amount,
                            Description = "NFC Card Payment",
                            Type = TransactionType.NFC,
                            Status = TransactionStatus.Completed,
                            CreatedAt = DateTime.UtcNow,
                            ProcessedAt = DateTime.UtcNow,
                            ToAccount = updatedAccount
                        }
                    });
                }
                else
                {
                    return BadRequest(new PaymentResponse
                    {
                        Success = false,
                        Message = "NFC payment failed - card may be invalid or limit exceeded"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaymentResponse
                {
                    Success = false,
                    Message = $"NFC payment error: {ex.Message}"
                });
            }
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetUserAccounts()
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var accounts = await _karatFlowService.GetUserAccountsAsync(currentUserId);
                
                return Ok(new { 
                    accounts = accounts,
                    totalAccounts = accounts.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving accounts: {ex.Message}" });
            }
        }
    }
}
