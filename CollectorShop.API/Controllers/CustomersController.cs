using CollectorShop.API.DTOs.Customers;
using CollectorShop.Domain.Interfaces;
using CollectorShop.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("me")]
    public async Task<ActionResult<CustomerDto>> GetCurrentCustomer()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        return Ok(new CustomerDto
        {
            Id = customer.Id,
            Email = customer.Email.Value,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber?.Value,
            IsActive = customer.IsActive,
            IsEmailVerified = customer.IsEmailVerified,
            LastLoginAt = customer.LastLoginAt,
            CreatedAt = customer.CreatedAt
        });
    }

    [HttpPut("me")]
    public async Task<ActionResult<CustomerDto>> UpdateCurrentCustomer([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            try
            {
                phoneNumber = new PhoneNumber(request.PhoneNumber);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        customer.UpdateProfile(request.FirstName, request.LastName, phoneNumber);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new CustomerDto
        {
            Id = customer.Id,
            Email = customer.Email.Value,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber?.Value,
            IsActive = customer.IsActive,
            IsEmailVerified = customer.IsEmailVerified,
            LastLoginAt = customer.LastLoginAt,
            CreatedAt = customer.CreatedAt
        });
    }

    [HttpGet("me/addresses")]
    public async Task<ActionResult<List<CustomerAddressDto>>> GetAddresses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        var addresses = customer.Addresses.Select(a => new CustomerAddressDto
        {
            Street = a.Street,
            City = a.City,
            State = a.State,
            Country = a.Country,
            ZipCode = a.ZipCode
        }).ToList();

        return Ok(addresses);
    }
}
