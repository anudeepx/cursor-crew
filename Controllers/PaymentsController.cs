using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Data;
using RetailOrderingWebsite.DTOs;
using RetailOrderingWebsite.DTOs.Payments;
using RetailOrderingWebsite.Services;

namespace RetailOrderingWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly PaymentStoreService _paymentStoreService;

    public PaymentsController(AppDbContext dbContext, PaymentStoreService paymentStoreService)
    {
        _dbContext = dbContext;
        _paymentStoreService = paymentStoreService;
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> Create(PaymentCreateRequestDto request)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId);
        if (order is null)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail("Order not found."));
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<PaymentResponseDto>.Fail("Invalid token."));
        }

        if (!User.IsInRole("Admin") && order.UserId != userId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PaymentResponseDto>.Fail("Access denied."));
        }

        var payment = _paymentStoreService.Create(request.OrderId);
        return Ok(ApiResponse<PaymentResponseDto>.Ok(payment, "Payment intent created."));
    }

    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> Verify(PaymentVerifyRequestDto request)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId);
        if (order is null)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail("Order not found."));
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<PaymentResponseDto>.Fail("Invalid token."));
        }

        if (!User.IsInRole("Admin") && order.UserId != userId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PaymentResponseDto>.Fail("Access denied."));
        }

        var payment = _paymentStoreService.Verify(request.OrderId, request.TransactionId);
        return Ok(ApiResponse<PaymentResponseDto>.Ok(payment, "Payment verified."));
    }

    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> GetPayment(int orderId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail("Order not found."));
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<PaymentResponseDto>.Fail("Invalid token."));
        }

        if (!User.IsInRole("Admin") && order.UserId != userId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PaymentResponseDto>.Fail("Access denied."));
        }

        var payment = _paymentStoreService.Get(orderId);
        if (payment is null)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail("Payment not found."));
        }

        return Ok(ApiResponse<PaymentResponseDto>.Ok(payment));
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
