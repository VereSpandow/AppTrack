using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDOC.Helpers
{
    public class MyViewEngine : RazorViewEngine
    {
        public MyViewEngine()
        {
            var newLocationFormat = new[]
                                    {
                                        "~/Views/Shared/{0}.cshtml",
                                    };

            PartialViewLocationFormats = PartialViewLocationFormats.Union(newLocationFormat).ToArray();
        }
    }

    public class Fixtures
    {
        public List<Location> GetLocations()
        {
            return new List<Location> { 
            new Location () {  Id = 2, Name = "Location Downtown 2"},
            new Location () {  Id = 3, Name = "Location Airport 3"},
            new Location () {  Id = 4, Name = "Location City 4"}
            };
        }

        public Location GetActiveLocation()
        {
            return new Location() { Id = 1, Name = "Location1" };
        }

        public HealthIndicator GetHealthIndicator()
        {
            Random r = new Random();
            return (HealthIndicator)((r.Next(100) % 4) + 1);
        }

        public DashBoardSummaryData GetDashBoardSummaryData(PeriodType period)
        {
            return new DashBoardSummaryData()
            {
                Sales = new CmpSummaryData() { AvgValue = (int)period * 1235, StoreValue = (int)period * 1103 },
                Transactions = new CmpSummaryData() { AvgValue = (int)period * 24, StoreValue = (int)period * 32 },
                AVGTicket = new CmpSummaryData() { AvgValue = (int)period * 39.61, StoreValue = (int)period * 45.95 },
                Customers = new CmpSummaryData() { AvgValue = (int)period * 27, StoreValue = (int)period * 21 }
            };
        }

        public HomeFavoritesViewModel GetDashBoardData(PeriodType period)
        {
            DateTime now = DateTime.Today;
            var dataSalesSummary = this.GetSalesSummaryData(period);
            var mostMoney = new DataWithoutHealth<SalesCalendarData> { data = this.GetSalesCalendarData(period, 100) };
            mostMoney.data.Sales.Defs = new Dictionary<string, string>() { { "title", "HIGHEST SALES" }, { "type", "highest-sales" } };
            mostMoney.data.Transactions.Defs = new Dictionary<string, string>() { { "title", "HIGHEST NUMBER OF TRANSACTIONS" }, { "type", "highest-transactions" } };
            mostMoney.data.AVGTicket.Defs = new Dictionary<string, string>() { { "title", "HIGHEST AVERAGE TICKET" }, { "type", "highest-ticket" } };
            var leastMoney = new DataWithoutHealth<SalesCalendarData> { data = this.GetSalesCalendarData(period, 20) };
            leastMoney.data.Sales.Defs = new Dictionary<string, string>() { { "title", "LOWEST SALES" }, { "type", "lowest-sales" } };
            leastMoney.data.Transactions.Defs = new Dictionary<string, string>() { { "title", "LOWEST NUMBER OF TRANSACTIONS" }, { "type", "lowest-transactions" } };
            leastMoney.data.AVGTicket.Defs = new Dictionary<string, string>() { { "title", "LOWEST AVERAGE TICKET" }, { "type", "lowest-ticket" } };
            List<TransactionData> dataTransactionSummary = this.GetTransactionSummaryData(new TransactionSummaryViewModel()
            {
                dateStart = now.AddDays(-10),
                dateEnd = now,
                amountStart = -200,
                amountEnd = 200,
                cardID = "VSDFYU"
            });
            return new HomeFavoritesViewModel()
            {
                summaryData = this.GetDashBoardSummaryData(period),
                customersActive = new DataWithHealth<CustomersActivityData> { health = this.GetHealthIndicator(), data = this.GetCustomersActivityData(period) },
                customersTotal = new DataWithHealth<CustomersShoppingData> { health = this.GetHealthIndicator(), data = this.GetCustomersShoppingData(period) },
                baseGrowth = new DataWithHealth<CustomersSummaryData> { health = this.GetHealthIndicator(), data = this.GetCustomersSummaryData(period) },
                baseTrends = new DataWithHealth<CustomersSummaryData> { health = this.GetHealthIndicator(), data = this.GetCustomersSummaryData(period) },
                salesGrowth = new DataWithHealth<SalesSummaryData> { health = this.GetHealthIndicator(), data = dataSalesSummary },
                salesTransactions = new DataWithHealth<SalesSummaryData> { health = this.GetHealthIndicator(), data = dataSalesSummary },
                salesAVGTicket = new DataWithHealth<SalesSummaryData> { health = this.GetHealthIndicator(), data = dataSalesSummary },
                mostMoney = mostMoney,
                leastMoney = leastMoney,
                transactionSummary = dataTransactionSummary,
                cmpSales = new DataWithHealth<CompareData<SalesSummaryData>> { health = HealthIndicator.Up, data = this.GetCmpSalesSummaryData(period) },
                cmpCustomers = new DataWithHealth<CompareData<CustomersSummaryData>> { health = HealthIndicator.Down, data = this.GetCmpCustomersSummaryData(period) },
                cmpActiveCustomers = new DataWithHealth<CompareData<CustomersSegmentSummaryData>>
                {
                    health = HealthIndicator.Up,
                    data = this.GetCmpActiveCustomersSummaryData(period)
                },
            };
        }

        public CustomersSummaryData GetCustomersSummaryData(PeriodType period, int seed = 3)
        {
            Random r = new Random(seed);
            int ratio = (int)period * 3;
            return new CustomersSummaryData()
            {
                Total = new SummaryData() { Value = r.Next((int)(2.1 * ratio), (int)(3.5 * ratio)), Percent = r.Next(4, 8) },
                New = new SummaryData() { Value = r.Next((int)(1.5 * ratio), (int)(1.9 * ratio)), Percent = r.Next(-10, 20) },
                Repeat = new SummaryData() { Value = r.Next((int)(0.6 * ratio), (int)(1.8 * ratio)), Percent = r.Next(2, 25) },
            };
        }

        public CustomersSegmentSummaryData GetCustomersSegmentSummaryData(PeriodType period, int seed = 3)
        {
            Random r = new Random(seed);
            int ratio = (int)period * 3;
            return new CustomersSegmentSummaryData()
            {
                Top = new SummaryData() { Value = r.Next((int)(2.1 * ratio), (int)(3.5 * ratio)), Percent = r.Next(4, 8) },
                Medium = new SummaryData() { Value = r.Next((int)(1.5 * ratio), (int)(1.9 * ratio)), Percent = r.Next(-10, 20) },
                Low = new SummaryData() { Value = r.Next((int)(0.6 * ratio), (int)(1.8 * ratio)), Percent = r.Next(2, 25) },
            };
        }

        public List<CustomerBaseChartData> GetCustomersChartData(PeriodType period)
        {
            List<DateTime> dates = this.GetDates(period);
            List<CustomerBaseChartData> data = new List<CustomerBaseChartData>();
            Random r = new Random();
            dates.ForEach(d => data.Add(new CustomerBaseChartData { Date = d, New = r.Next(4000), Repeat = r.Next(50, 8000), Total = 0 }));
            data.ForEach(d => d.Total = d.New + d.Repeat);
            return data;
        }

        public CustomersShoppingData GetCustomersShoppingData(PeriodType period)
        {
            switch (period)
            {
                case PeriodType.MonthToDate:
                    return new CustomersShoppingData()
                    {
                        Total = new SummaryData() { Value = 21, Percent = 4 },
                        Active = new SummaryData() { Value = 15, Percent = 15 },
                        Dormant = new SummaryData() { Value = 6, Percent = -2 },
                    };
                case PeriodType.QuarterToDate:
                    return new CustomersShoppingData()
                    {
                        Total = new SummaryData() { Value = 42, Percent = 4 },
                        Active = new SummaryData() { Value = 12, Percent = 6 },
                        Dormant = new SummaryData() { Value = 30, Percent = 5 },
                    };
                case PeriodType.Rolling13Months:
                    return new CustomersShoppingData()
                    {
                        Total = new SummaryData() { Value = 53, Percent = 4 },
                        Active = new SummaryData() { Value = 52, Percent = 15 },
                        Dormant = new SummaryData() { Value = 1, Percent = -2 },
                    };
                case PeriodType.YearToDate:
                default:
                    return new CustomersShoppingData()
                    {
                        Total = new SummaryData() { Value = 210, Percent = 40 },
                        Active = new SummaryData() { Value = 150, Percent = -10 },
                        Dormant = new SummaryData() { Value = 60, Percent = 23 },
                    };
            }
        }

        public CustomersActivityData GetCustomersActivityData(PeriodType period)
        {
            Random r = new Random();
            CustomersSegmentsData top = new CustomersSegmentsData()
            {
                Defs = new Dictionary<string, string>() { { "sales", ">$10" }, { "transactions", ">6" }, { "type", "top" } },
                Spenders = new SummaryData() { Value = r.Next((int)period * 100), Percent = r.Next(-100, 100) },
                Transactors = new SummaryData() { Value = r.Next((int)period * 100), Percent = r.Next(-100, 100) },
                Sales = new SummaryData() { Value = r.Next((int)period * 200), Percent = r.Next(1, 100) },
                Transactions = new SummaryData() { Value = r.Next((int)period * 100), Percent = r.Next(1, 100) },
                AVGTicket = new SummaryData() { Value = r.Next((int)period * 100), Percent = r.Next(1, 100) },
                AVGTransaction = new SummaryData() { Value = r.Next((int)period * 100), Percent = r.Next(1, 100) },
            };
            CustomersSegmentsData medium = new CustomersSegmentsData()
            {
                Defs = new Dictionary<string, string>() { { "sales", "$6-$10" }, { "transactions", "3-6" }, { "type", "medium" } },
                Spenders = new SummaryData() { Value = r.Next((int)period * 80), Percent = r.Next(-100, 100) },
                Transactors = new SummaryData() { Value = r.Next((int)period * 80), Percent = r.Next(-100, 100) },
                Sales = new SummaryData() { Value = r.Next((int)period * 100), Percent = r.Next(1, 100) },
                Transactions = new SummaryData() { Value = r.Next((int)period * 80), Percent = r.Next(1, 100) },
                AVGTicket = new SummaryData() { Value = r.Next((int)period * 80), Percent = r.Next(1, 100) },
                AVGTransaction = new SummaryData() { Value = r.Next((int)period * 80), Percent = r.Next(1, 100) },
            };
            CustomersSegmentsData low = new CustomersSegmentsData()
            {
                Defs = new Dictionary<string, string>() { { "sales", "<$6" }, { "transactions", "<3" }, { "type", "low" } },
                Spenders = new SummaryData() { Value = r.Next((int)period * 50), Percent = r.Next(-100, 100) },
                Transactors = new SummaryData() { Value = r.Next((int)period * 50), Percent = r.Next(-100, 100) },
                Sales = new SummaryData() { Value = r.Next((int)period * 60), Percent = r.Next(1, 100) },
                Transactions = new SummaryData() { Value = r.Next((int)period * 40), Percent = r.Next(1, 100) },
                AVGTicket = new SummaryData() { Value = r.Next((int)period * 20), Percent = r.Next(1, 100) },
                AVGTransaction = new SummaryData() { Value = r.Next((int)period * 20), Percent = r.Next(1, 100) },
            };
            return new CustomersActivityData()
            {
                Top = top,
                Medium = medium,
                Low = low,
            };
        }

        public List<CustomersMixData> GetCustomersMixData(PeriodType period)
        {
            Random r = new Random();
            List<CustomersMixData> data = new List<CustomersMixData>();
            int iCustomers = r.Next(20, (int)period * 50);
            for (int i = 0; i < iCustomers; i++)
            {
                CustomersMixData d = new CustomersMixData() { Spend = r.Next((int)period * 300), Transactions = r.Next((int)period * 50) };
                data.Add(d);
            }
            return data;
        }

        public SalesSummaryData GetSalesSummaryData(PeriodType period, int seed = 3)
        {
            Random r = new Random(seed);
            int ratio = (int)period * 10;
            return new SalesSummaryData()
            {
                Sales = new SummaryData() { Value = r.Next((int)(2.1 * ratio), (int)(3.5 * ratio)), Percent = r.Next(4, 8) },
                Transactions = new SummaryData() { Value = r.Next((int)(1.5 * ratio), (int)(1.9 * ratio)), Percent = r.Next(-10, 20) },
                AVGTicket = new SummaryData() { Value = r.Next((int)(0.6 * ratio), (int)(1.8 * ratio)), Percent = r.Next(13, 25) },
            };
        }

        public List<DateTime> GetDates(PeriodType period)
        {
            int iDates = 0;
            List<DateTime> dates = new List<DateTime>();
            DateTime now = DateTime.Today;
            switch (period)
            {
                case PeriodType.MonthToDate:
                    iDates = now.Day - 1;
                    for (DateTime day = now.AddDays(1 - now.Day); day < now; day = day.AddDays(1))
                    {
                        dates.Add(day);
                    }
                    break;
                case PeriodType.QuarterToDate:
                    int quarter = ((now.Month - 1) / 3) + 1;
                    for (DateTime day = new DateTime(now.Year, (quarter - 1) * 3 + 1, 1); day < now; day = day.AddDays(7))
                    {
                        dates.Add(day);
                    }
                    break;
                case PeriodType.Rolling13Months:
                    for (DateTime day = now.AddMonths(-13); day < now; day = day.AddMonths(1))
                    {
                        dates.Add(day);
                    }
                    break;
                case PeriodType.YearToDate:
                default:
                    for (DateTime day = new DateTime(now.Year, 1, 1); day < now; day = day.AddMonths(1))
                    {
                        dates.Add(day);
                    }
                    break;
            }
            return dates;
        }

        public List<ChartData> GetChartData(PeriodType period, int baseValue)
        {
            List<DateTime> dates = this.GetDates(period);
            List<ChartData> data = new List<ChartData>();
            Random r = new Random();
            dates.ForEach(d => data.Add(new ChartData { Date = d, Value = r.Next(5, baseValue) * r.Next(1, (int)period) }));
            return data;
        }

        public SalesCalendarData GetSalesCalendarData(PeriodType period, int baseValue)
        {
            Random r = new Random();
            DateTime now = DateTime.Today;
            return new SalesCalendarData()
            {
                Sales = new CalendarData()
                {
                    Defs = null,
                    StartTime = new DateTime(now.Year, now.Month, r.Next(1, 7), r.Next(10, 16), 0, 0),
                    EndTime = new DateTime(now.Year, now.Month, 1, r.Next(16, 23), 0, 0),
                    Value = r.Next(5, (int)period * baseValue),
                },
                Transactions = new CalendarData()
                {
                    Defs = null,
                    StartTime = new DateTime(now.Year, now.Month, r.Next(1, 7), r.Next(10, 16), 0, 0),
                    EndTime = new DateTime(now.Year, now.Month, 1, r.Next(16, 23), 0, 0),
                    Value = r.Next(5, (int)period * baseValue * 3),
                },
                AVGTicket = new CalendarData()
                {
                    Defs = null,
                    StartTime = new DateTime(now.Year, now.Month, r.Next(1, 7), r.Next(10, 16), 0, 0),
                    EndTime = new DateTime(now.Year, now.Month, 1, r.Next(16, 23), 0, 0),
                    Value = r.Next(5, (int)period * baseValue * 2),
                },
            };
        }

        public SalesHeatMapData GetSalesHeatMapData(RequestType request, PeriodType period)
        {
            Random r = new Random();
            List<SalesHeatMapChartData> ChartData = new List<SalesHeatMapChartData>();
            List<SalesHeatMapPercentData> PercentData = new List<SalesHeatMapPercentData>();
            for (int i = 1; i <= 7; i++)
            {
                PercentData.Add(new SalesHeatMapPercentData()
                {
                    Day = i,
                    Value = r.Next(5, 20)
                });
                for (int j = 10; j <= 24; j++)
                {
                    ChartData.Add(new SalesHeatMapChartData()
                    {
                        Day = i,
                        Hour = j,
                        Value = r.Next(0, (int)period * 100 / (int)request)
                    });
                }
                for (int j = 1; j < 3; j++)
                {
                    ChartData.Add(new SalesHeatMapChartData()
                    {
                        Day = i,
                        Hour = j,
                        Value = r.Next(5, (int)period * 50 / (int)request)
                    });
                }
            }
            return new SalesHeatMapData()
            {
                ChartData = ChartData,
                PercentData = PercentData
            };
        }

        public List<TransactionData> GetTransactionSummaryData(TransactionSummaryViewModel queryParams)
        {
            List<TransactionData> data = new List<TransactionData>();
            DateTime now = DateTime.Today;
            Random r = new Random();
            for (DateTime day = queryParams.dateStart; day < queryParams.dateEnd; day = day.AddDays(1))
            {
                data.Add(new TransactionData() { Date = day, Amount = r.Next(queryParams.amountStart, queryParams.amountEnd), CardId = "VS-8056" });
            }
            return data;
        }

        public CompetitionSummaryData GetCmpSummaryData(PeriodType period)
        {
            Random r = new Random();
            int ratio = (int)period * 10;
            return new CompetitionSummaryData()
            {
                Sales = new SummaryData() { Value = r.Next((int)(2.1 * ratio), (int)(3.5 * ratio)), Percent = r.Next(4, 8) },
                Customers = new SummaryData() { Value = r.Next((int)(1.5 * ratio), (int)(1.9 * ratio)), Percent = r.Next(-10, 20) },
                AVGTicket = new SummaryData() { Value = r.Next((int)(0.6 * ratio), (int)(1.8 * ratio)), Percent = r.Next(23, 25) },
            };
        }

        public CompareData<SalesSummaryData> GetCmpSalesSummaryData(PeriodType period)
        {
            return new CompareData<SalesSummaryData>()
            {
                StoreValue = this.GetSalesSummaryData(period, 25),
                AvgValue = this.GetSalesSummaryData(period)
            };
        }

        public CompareData<CustomersSummaryData> GetCmpCustomersSummaryData(PeriodType period)
        {
            return new CompareData<CustomersSummaryData>()
            {
                StoreValue = this.GetCustomersSummaryData(period, 25),
                AvgValue = this.GetCustomersSummaryData(period)
            };
        }

        public CompareData<CustomersSegmentSummaryData> GetCmpActiveCustomersSummaryData(PeriodType period)
        {
            return new CompareData<CustomersSegmentSummaryData>()
            {
                StoreValue = this.GetCustomersSegmentSummaryData(period, 25),
                AvgValue = this.GetCustomersSegmentSummaryData(period)
            };
        }

        public List<ComparisonChartData> GetCmpChartData(PeriodType period, int baseValue)
        {
            List<DateTime> dates = this.GetDates(period);
            List<ComparisonChartData> data = new List<ComparisonChartData>();
            Random r = new Random();
            dates.ForEach(d => data.Add(new ComparisonChartData
            {
                Date = d,
                StoreValue = r.Next(5, baseValue) * r.Next(1, (int)period),
                AvgValue = r.Next(5, (int)(baseValue / 0.8)) * r.Next(1, (int)period),
            })
                         );
            return data;
        }

        public List<ComparisonChartData> GetCmpSalesChartData(PeriodType period)
        {
            return this.GetCmpChartData(period, 1000);
        }

        public List<ComparisonChartData> GetCmpCustomerBaseChartData(PeriodType period)
        {
            return this.GetCmpChartData(period, 100);
        }

        public List<ComparisonChartData> GetCmpActiveCustomersChartData(PeriodType period)
        {
            return this.GetCmpChartData(period, 150);
        }
    }
}