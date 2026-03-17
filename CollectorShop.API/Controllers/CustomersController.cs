using CollectorShop.API.DTOs.Customers;
using CollectorShop.Domain.Entities;
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

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static CustomerAddressDto MapAddressToDto(CustomerAddress ca) => new()
    {
        Id = ca.Id,
        Label = ca.Label,
        Street = ca.Address.Street,
        City = ca.Address.City,
        State = ca.Address.State,
        Country = ca.Address.Country,
        ZipCode = ca.Address.ZipCode,
        IsDefault = ca.IsDefault,
        IsBillingAddress = ca.IsBillingAddress,
        IsShippingAddress = ca.IsShippingAddress
    };

    [HttpGet("me")]
    public async Task<ActionResult<CustomerDto>> GetCurrentCustomer()
    {
        var userId = GetUserId();
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
        var userId = GetUserId();
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

    // ─── Address endpoints ───────────────────────────────────────────

    [HttpGet("me/addresses")]
    public async Task<ActionResult<List<CustomerAddressDto>>> GetAddresses()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        var addresses = await _unitOfWork.Customers.GetAddressesByCustomerIdAsync(customer.Id);
        return Ok(addresses.Select(MapAddressToDto).ToList());
    }

    [HttpPost("me/addresses")]
    public async Task<ActionResult<CustomerAddressDto>> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        Address address;
        try
        {
            address = new Address(request.Street, request.City, request.State, request.Country, request.ZipCode);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        // If this address is marked as default, unset current default
        if (request.IsDefault)
        {
            var existing = await _unitOfWork.Customers.GetAddressesByCustomerIdAsync(customer.Id);
            foreach (var a in existing.Where(a => a.IsDefault))
            {
                a.UnsetAsDefault();
            }
        }

        var customerAddress = new CustomerAddress(
            customer.Id,
            request.Label,
            address,
            request.IsDefault,
            request.IsBillingAddress,
            request.IsShippingAddress
        );

        await _unitOfWork.Customers.AddAddressAsync(customerAddress);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAddresses), MapAddressToDto(customerAddress));
    }

    [HttpPut("me/addresses/{addressId:guid}")]
    public async Task<ActionResult<CustomerAddressDto>> UpdateAddress(Guid addressId, [FromBody] UpdateAddressRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        var customerAddress = await _unitOfWork.Customers.GetAddressByIdAsync(addressId);
        if (customerAddress == null || customerAddress.CustomerId != customer.Id)
        {
            return NotFound("Address not found");
        }

        Address address;
        try
        {
            address = new Address(request.Street, request.City, request.State, request.Country, request.ZipCode);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        // Handle default flag changes
        if (request.IsDefault && !customerAddress.IsDefault)
        {
            var existing = await _unitOfWork.Customers.GetAddressesByCustomerIdAsync(customer.Id);
            foreach (var a in existing.Where(a => a.IsDefault && a.Id != addressId))
            {
                a.UnsetAsDefault();
            }
            customerAddress.SetAsDefault();
        }
        else if (!request.IsDefault && customerAddress.IsDefault)
        {
            customerAddress.UnsetAsDefault();
        }

        customerAddress.UpdateLabel(request.Label);
        customerAddress.UpdateAddress(address);
        customerAddress.UpdateFlags(request.IsBillingAddress, request.IsShippingAddress);

        await _unitOfWork.SaveChangesAsync();

        return Ok(MapAddressToDto(customerAddress));
    }

    [HttpDelete("me/addresses/{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        var customerAddress = await _unitOfWork.Customers.GetAddressByIdAsync(addressId);
        if (customerAddress == null || customerAddress.CustomerId != customer.Id)
        {
            return NotFound("Address not found");
        }

        _unitOfWork.Customers.RemoveAddress(customerAddress);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}
