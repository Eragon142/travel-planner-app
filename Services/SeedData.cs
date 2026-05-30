using Microsoft.EntityFrameworkCore;
using TravelPlannerApp.Data;
using TravelPlannerApp.Models;

namespace TravelPlannerApp.Services;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        var auth = scope.ServiceProvider.GetRequiredService<AuthService>();
        var user = await auth.EnsureDemoUserAsync();
        if (await db.Trips.AnyAsync()) return;
        var trip = new Trip { Name = "Weekend w Rzymie", Destination = "Rzym", StartDate = DateTime.Today.AddDays(14), EndDate = DateTime.Today.AddDays(17), BudgetAmount = 2500, BudgetCurrency = "PLN" };
        trip.TripUsers.Add(new TripUser { UserId = user.Id, Role = TripRole.Owner });
        trip.Expenses.Add(new Expense { Amount = 250, Currency = "PLN", Category = "Transport", Description = "Bilet lotniczy", Date = DateTime.Today });
        trip.Locations.Add(new LocationPoint { Name = "Koloseum", Tag = "Zabytki", Latitude = 41.8902, Longitude = 12.4922, Description = "Zwiedzanie rano" });
        trip.PackingLists.Add(new PackingList { Name = "Podstawowa lista", Items = new List<PackingItem> { new() { Name = "Paszport", Quantity = 1 }, new() { Name = "Ładowarka", Quantity = 1, IsPacked = true } } });
        db.Trips.Add(trip);
        await db.SaveChangesAsync();
    }
}
