using System;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    protected readonly ProductDbContext _context;
    protected readonly DbSet<Product> _dbSet;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<Product>();
    }

    public async Task<Product> AddAsync(Product product)
    {
        await _dbSet.AddAsync(product);
        return product;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Product?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.ProductName == name);
    }
}

