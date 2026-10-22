using Microsoft.EntityFrameworkCore;
using LeafWeighing.Domain.Entities;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Infrastructure.Data;

namespace LeafWeighing.Infrastructure.Repositories;

public class RegLeafCountRepository : GenericRepository<RegLeafCount>, IRegLeafCountRepository
{
	private readonly BoughtLeafDbContext _boughtLeafContext;

	public RegLeafCountRepository(BoughtLeafDbContext context) : base(context)
	{
		_boughtLeafContext = context;
	}

	public async Task<IEnumerable<string>> GetDistinctRoutesAsync()
	{
		var routes = await _boughtLeafContext.RegLeafCount
			.Where(x => x.Route != null && x.Route != "" && x.Route != "null")
			.Select(x => x.Route)
			.Distinct()
			.ToListAsync();

		return routes.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r!.Trim())!;
	}

	public async Task<RegLeafCount> AddLeafCountAsync(RegLeafCount leafCount)
	{
		leafCount.LogTime = DateTime.Now;
		await _boughtLeafContext.RegLeafCount.AddAsync(leafCount);
		await _boughtLeafContext.SaveChangesAsync();
		return leafCount;
	}

	public async Task<IEnumerable<RegLeafCount>> GetLeafCountHistoryAsync(string? month, string? route, DateTime? startDate, DateTime? endDate)
	{
		var query = _boughtLeafContext.RegLeafCount.AsQueryable();

		if (!string.IsNullOrEmpty(month))
		{
			query = query.Where(x => x.Month == month);
		}

		if (!string.IsNullOrEmpty(route))
		{
			var trimmedRoute = route.Trim();
			query = query.Where(x => x.Route != null && x.Route.Trim() == trimmedRoute);
		}

		if (startDate.HasValue)
		{
			query = query.Where(x => x.LogTime >= startDate.Value);
		}

		if (endDate.HasValue)
		{
			query = query.Where(x => x.LogTime <= endDate.Value);
		}

		return await query.OrderByDescending(x => x.LogTime).ToListAsync();
	}
}