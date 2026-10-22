using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Domain.Calculations;

public static class LeafWeightCalculator
{

    // Gross in the database is the original measured weight. Apply these two
    // deductions once when returning Gross to the app; do not overwrite it.
    public static decimal CalculateAdjustedGross(TrLeafCollectionTemp row) =>
        (row.Gross ?? 0m)
        - (row.SuperDeduction ?? 0)
        - (row.NormalDeduction ?? 0);

    public static decimal CalculateAdjustedGrossTotal(IEnumerable<TrLeafCollectionTemp> rows) =>
        rows.Sum(CalculateAdjustedGross);

    public static decimal Calculate(TrLeafCollectionTemp row) =>
        CalculateAdjustedGross(row)
        - (row.BagWeight ?? 0m)
        - (row.Water ?? 0m)
        - (row.Coarse ?? 0m)
        - (row.Rejected ?? 0m)
        - (row.Boild ?? 0m);

    public static decimal CalculateTotal(IEnumerable<TrLeafCollectionTemp> rows) =>
        rows.Sum(Calculate);
}
