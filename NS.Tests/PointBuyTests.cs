namespace NS.Tests;

public sealed class PointBuyTests
{
    [Theory]
    [InlineData(8, 0)]
    [InlineData(10, 2)]
    [InlineData(13, 5)]
    [InlineData(14, 7)]
    [InlineData(15, 9)]
    public void CostOf_ReturnsTableCost(int score, int expected)
    {
        Assert.Equal(expected, PointBuy.CostOf(score));
    }

    [Fact]
    public void CostOf_ThrowsForOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PointBuy.CostOf(16));
    }

    [Fact]
    public void TotalCost_SumsAllFourScores()
    {
        // 14(7) + 13(5) + 12(4) + 8(0) = 16
        var scores = new AbilityScores(Dexterity: 14, Intelligence: 13, Strength: 12, Will: 8);
        Assert.Equal(16, PointBuy.TotalCost(scores));
    }

    [Fact]
    public void IsValid_TrueWhenWithinBudgetAndRange()
    {
        // 15(9) + 14(7) + 13(5) + 13(5) = 26 <= 27
        Assert.True(PointBuy.IsValid(new AbilityScores(15, 14, 13, 13)));
    }

    [Fact]
    public void IsValid_FalseWhenOverBudget()
    {
        // 15(9) + 15(9) + 15(9) + 8(0) = 27 ok; bump Will to 9 -> 28 over budget
        Assert.False(PointBuy.IsValid(new AbilityScores(15, 15, 15, 9)));
    }

    [Fact]
    public void IsValid_FalseWhenOutOfRange()
    {
        Assert.False(PointBuy.IsValid(new AbilityScores(7, 10, 10, 10)));
    }
}
