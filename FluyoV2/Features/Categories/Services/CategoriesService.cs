using FluyoV2.Constants;
using FluyoV2.Features.Categories.Dtos;

namespace FluyoV2.Features.Categories.Services;

public class CategoriesService
{
    private readonly List<CategoryResponse> _categories =
    [
        // INCOME
        new() { Name = "Trabajo", Type = TransactionTypes.Income, Icon = "briefcase" },
        new() { Name = "Préstamo", Type = TransactionTypes.Income, Icon = "hand-holding-dollar" },
        new() { Name = "Cosas de valor", Type = TransactionTypes.Income, Icon = "gem" },
        new() { Name = "Ventas", Type = TransactionTypes.Income, Icon = "cart-shopping" },
        new() { Name = "Regalos", Type = TransactionTypes.Income, Icon = "gift" },

        // EXPENSE
        new() { Name = "Vivienda", Type = TransactionTypes.Expense, Icon = "house" },
        new() { Name = "Comida", Type = TransactionTypes.Expense, Icon = "utensils" },
        new() { Name = "Servicios", Type = TransactionTypes.Expense, Icon = "bolt" },
        new() { Name = "Transporte", Type = TransactionTypes.Expense, Icon = "car" },
        new() { Name = "Salud", Type = TransactionTypes.Expense, Icon = "heart-pulse" },
        new() { Name = "Educación", Type = TransactionTypes.Expense, Icon = "graduation-cap" },
        new() { Name = "Ropa", Type = TransactionTypes.Expense, Icon = "shirt" },
        new() { Name = "Entretenimiento", Type = TransactionTypes.Expense, Icon = "film" },
        new() { Name = "Mascotas", Type = TransactionTypes.Expense, Icon = "paw" },
        new() { Name = "Caprichos", Type = TransactionTypes.Expense, Icon = "star" },
        new() { Name = "Préstamos", Type = TransactionTypes.Expense, Icon = "money-bill" },
        new() { Name = "Regalos", Type = TransactionTypes.Expense, Icon = "gift" },
        new() { Name = "Otros", Type = TransactionTypes.Expense, Icon = "ellipsis" }
    ];

    public List<CategoryResponse> GetAll()
    {
        return _categories;
    }

    public List<CategoryResponse> GetIncome()
    {
        return _categories
            .Where(x => x.Type == TransactionTypes.Income)
            .ToList();
    }

    public List<CategoryResponse> GetExpense()
    {
        return _categories
            .Where(x => x.Type == TransactionTypes.Expense)
            .ToList();
    }
}