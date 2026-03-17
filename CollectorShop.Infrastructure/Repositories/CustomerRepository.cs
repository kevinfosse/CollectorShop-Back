using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using CollectorShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CollectorShop.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<Customer?> GetByIdWithOrdersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByIdWithAddressesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer != null)
        {
            var addresses = await _context.CustomerAddresses
                .Where(ca => ca.CustomerId == id)
                .ToListAsync(cancellationToken);
        }
        return customer;
    }

    public async Task<Customer?> GetByIdWithCartAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Cart)
                .ThenInclude(cart => cart!.Items)
                    .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByIdWithWishlistAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.WishlistItems)
                .ThenInclude(w => w.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByUserIdWithWishlistAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.WishlistItems)
                .ThenInclude(w => w.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    // Customer address operations
    public async Task<List<CustomerAddress>> GetAddressesByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses
            .Where(ca => ca.CustomerId == customerId)
            .OrderByDescending(ca => ca.IsDefault)
            .ThenBy(ca => ca.Label)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerAddress?> GetAddressByIdAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses
            .FirstOrDefaultAsync(ca => ca.Id == addressId, cancellationToken);
    }

    public async Task AddAddressAsync(CustomerAddress address, CancellationToken cancellationToken = default)
    {
        await _context.CustomerAddresses.AddAsync(address, cancellationToken);
    }

    public void RemoveAddress(CustomerAddress address)
    {
        _context.CustomerAddresses.Remove(address);
    }
}
