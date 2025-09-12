using System;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    protected readonly OrderDbContext _context;
    protected readonly DbSet<Order> _dbSet;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<Order>();
    }

    public async Task<Order> AddAsync(Order order)
    {
        await _dbSet.AddAsync(order);
        return order;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
