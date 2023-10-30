namespace ApiCall.Tests;

using System.Diagnostics;
using BlazorApp.Shared;


public class WageIncidentalsTest
{
    [Fact]
    public async void TestSalaryUpdate()
    {
        WageIncidentals wageIncidentals = new WageIncidentals(31, 5, 100);
        bool result = await wageIncidentals.Contractor.UpdateSalaryBase(100000);
        Assert.True(result);
        Assert.True(Math.Abs(59184.0 - wageIncidentals.Contractor.SalaryNet) < 1);
    }
}