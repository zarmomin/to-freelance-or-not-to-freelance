using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BlazorApp.Shared
{
    public class WageIncidentals {
        public WageIncidentals() : this(31, 5, 100000.0) {}

        public WageIncidentals(int age, int weeksOfHolidays, double salaryBase) {
            this.Contractor = new WageIncidentalsContractor(age, weeksOfHolidays);
            this.Salaried = new WageIncidentalsSalaried(age);
        }

        public WageIncidentalsContractor Contractor {get;}
        public WageIncidentalsSalaried Salaried {get;}

    }
    public class IncomeTaxApproximation
    {
        public IncomeTaxApproximation(double salaryGross, double salaryNet, double incomeTax)
        {
            this.SalaryGross = salaryGross;
            this.SalaryNet = salaryNet;
            this.IncomeTax = incomeTax;
        }
        public double SalaryGross { get; }
        public double SalaryNet { get; }
        public double IncomeTax { get; }
        public double GetIncomeTaxRatio()
        {
            return IncomeTax / SalaryGross;
        }
    }

    public class IncomeTaxApproximations
    {
        private List<IncomeTaxApproximation> approximations;
        public IncomeTaxApproximations()
        {
            this.approximations = new List<IncomeTaxApproximation>() {
                new IncomeTaxApproximation(60000, 6184, 53618),
                new IncomeTaxApproximation(60000.00, 6184.00,53816.00),
                new IncomeTaxApproximation(70000.00, 7928.00,62072.00),
                new IncomeTaxApproximation(80000.00,9825.00,70175.00),
                new IncomeTaxApproximation(90000.00,11785.00,78215.00),
                new IncomeTaxApproximation(100000.00,13749.00,86251.00),
                new IncomeTaxApproximation(110000.00,15807.00,94193.00),
                new IncomeTaxApproximation(120000.00,17987.00,102013.00),
                new IncomeTaxApproximation(130000.00,20167.00,109833.00),
                new IncomeTaxApproximation(140000.00,22397.00,117603.00),
                new IncomeTaxApproximation(150000.00,24798.00,125202.00),
                new IncomeTaxApproximation(160000.00,27195.00,132805.00),
                new IncomeTaxApproximation(170000.00,29589.00,140411.00),
            };
        }
        public double getClosestNet(double salaryGross)
        {
            var closest = this.approximations.OrderBy(x => Math.Abs(x.SalaryGross - salaryGross)).First();
            return closest.SalaryNet;
        }

        public double getClosestGross(double salaryNet)
        {
            var closest = this.approximations.OrderBy(x => Math.Abs(x.SalaryNet - salaryNet)).First();
            return closest.SalaryGross;
        }
    }


    public class IWageIncidentals
    {
        private static readonly HttpClient client = new HttpClient();
        private const string baseUrl = "https://webcalc.services.zh.ch";

        public async Task<double> GetRatesAsync(int year, double income)
        {
            string url = "/ZH-Web-Calculators/calculators/INCOME_ASSETS/calculate";

            var requestData = new
            {
                isLiabilityLessThanAYear = false,
                hasTaxSeparation = false,
                hasQualifiedInvestments = false,
                taxYear = year.ToString(),
                liabilityBegin = (string)null,
                liabilityEnd = (string)null,
                name = "",
                maritalStatus = "single",
                taxScale = "BASIC",
                religionP1 = "OTHERS",
                religionP2 = "OTHERS",
                municipality = "261",
                taxableIncome = income.ToString(),
                ascertainedTaxableIncome = (string)null,
                qualifiedInvestmentsIncome = (string)null,
                taxableAssets = "0",
                ascertainedTaxableAssets = (string)null,
                withholdingTax = "0"
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync(new Uri(baseUrl + url), content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Bad response");
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(responseContent);
            double totalTaxRate = data.totalCantonalTax.value;

            return totalTaxRate;
        }

        public virtual string Name { get; }

        public double SalaryBase { get; set; }

        public virtual double VAT { get; }

        public virtual double AhvIvEo { get; }

        public virtual double ALV { get; }

        public virtual double BVG { get; set; }

        public virtual double AccidentInsurance { get; }

        public virtual double SicknessDailyRate { get; }

        public virtual double SalaryTaxable { get; set; }

        public virtual double IncomeTax { get; set; }

        public virtual int WeeksOfHolidays { get; set; }

        public virtual double SalaryNet { get; set; }

        public virtual double getHolidayRate()
        {
            return 0.0;
        }

        public async Task<bool> UpdateSalaryBase(double salaryBase)
        {
            this.SalaryBase = salaryBase;
            this.SalaryTaxable = this.SalaryBase * (1 - this.AhvIvEo) * (1 - this.ALV) * (1 - this.BVG) * (1 - this.AccidentInsurance) * (1 - this.SicknessDailyRate);
            Task<double> incomeTax = this.GetRatesAsync(2023, this.SalaryTaxable);
            this.IncomeTax = await incomeTax;
            this.SalaryNet = (this.SalaryTaxable - this.IncomeTax) * (1 - getHolidayRate());
            return true;
        }

        public virtual bool UpdateSalaryNet(double salaryNet) { return true; }

        public double CalculateIncomeTaxFromNet(double salaryAfterTax) {return 0.0;}
    }

    public class WageIncidentalsSalaried : IWageIncidentals
    {
        public WageIncidentalsSalaried(int age)
        {
            age = (age < 18) ? 18 : (age > 70) ? 70 : age;
            if (age < 25)
            {
                BVG = 0.0;
            }
            else if (age >= 25 && age < 34)
            {
                BVG = 0.07 / 2;
            }
            else if (age >= 34 && age < 45)
            {
                BVG = 0.1 / 2;
            }
            else if (age >= 45 && age < 55)
            {
                BVG = 0.15 / 2;
            }
            else if (age >= 55 && age < 65)
            {
                BVG = 0.18 / 2;
            }
        }
        public override String Name { get; } = "Salaried";
        public override double VAT { get; } = 0.0;
        public override double AhvIvEo { get; } = 0.053;
        public override double ALV { get; } = 0.011;

        public override double BVG { get; set; } = 0.0;

        public override double AccidentInsurance { get; } = 0.0952;

        public override double SicknessDailyRate { get; } = 0;

        public override double SalaryTaxable { get; set; } = 0;

        public override double IncomeTax { get; set; } = 0;

        public override int WeeksOfHolidays { get; set; } = 5;

        public override double SalaryNet { get; set; } = 0;
    }

    public class WageIncidentalsContractor : IWageIncidentals
    {
        public WageIncidentalsContractor(int age, int weeksOfHolidays)
        {
            WeeksOfHolidays = (weeksOfHolidays < 4) ? 4 : (weeksOfHolidays > 6) ? 6 : weeksOfHolidays; ;
            age = (age < 18) ? 18 : (age > 70) ? 70 : age;

            if (age < 25)
            {
                BVG = 0.013;
            }
            else if (age >= 25 && age < 34)
            {
                BVG = 0.092;
            }
            else if (age >= 34 && age < 45)
            {
                BVG = 0.137;
            }
            else if (age >= 45 && age < 55)
            {
                BVG = 0.204;
            }
            else if (age >= 55 && age < 65)
            {
                BVG = 0.231;
            }
            else if (age >= 65)
            {
                BVG = 0.107;
            }
        }

        public override String Name { get; } = "Contractor";
        public override double VAT { get; } = 0.77;
        public override double AhvIvEo { get; } = 0.1;
        public override double ALV { get; } = 0;

        public override double BVG { get; set; } = 0.0;

        public override double AccidentInsurance { get; } = 0.32 + 0.0952;

        public override double SicknessDailyRate { get; } = 0.015;

        public override double SalaryTaxable { get; set; } = 0;

        public override double IncomeTax { get; set; } = 0;

        public override int WeeksOfHolidays { get; set; }

        public override double SalaryNet { get; set; } = 0;

        public override double getHolidayRate()
        {
            if (this.WeeksOfHolidays == 4)
            {
                return 0.083;
            }
            else if (this.WeeksOfHolidays == 5)
            {
                return 0.1064;
            }
            else if (this.WeeksOfHolidays == 6)
            {
                return 0.1304;
            }
            else { return 0.0; }
        }
    }
}
