namespace ApiCall.Tests;

using System.Diagnostics;
using BlazorApp.Shared;


public class UnitTest1
{
    [Fact]
    public async void Test1()
    {
        WageIncidentals wageIncidentals = new WageIncidentals(31, 5, 100);
        bool result = await wageIncidentals.Contractor.UpdateSalaryBase(100000);
        Debug.WriteLine(wageIncidentals.Contractor.SalaryTaxable);
        Assert.True(result);
        Assert.True(Math.Abs(59184.0 - wageIncidentals.Contractor.SalaryNet) < 1);
    }
}