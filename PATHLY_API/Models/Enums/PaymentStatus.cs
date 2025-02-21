namespace PATHLY_API.Models.Enums
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Cancelled
    }
}
//[HttpPost("complete-payment")]
//public async Task<IActionResult> CompletePayment([FromBody] CompletePaymentRequest request)
//{
//	try
//	{
//		bool success = await _paymentService.CompletePayPalPaymentAsync(request.OrderId);
//		return Ok(new { Success = success });
//	}
//	catch (KeyNotFoundException ex)
//	{
//		return NotFound(ex.Message);
//	}
//}
