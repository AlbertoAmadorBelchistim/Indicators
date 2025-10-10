// File: AccountBalanceLine.cs
namespace MyIndicators
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using ATAS.Indicators;                 // ValueDataSeries, IndicatorDataProvider
    using ATAS.Indicators.Technical;       // Indicator
    using OFT.Rendering.Settings;          // VisualMode
    using ATAS.DataFeedsCore;              // Portfolio (for the override signature)

    [Category("Custom")]
    [DisplayName("Account Balance (Realtime)")]
    public class AccountBalanceLine : Indicator
    {
        private readonly ValueDataSeries _balance;

        public AccountBalanceLine()
        {
            // ▶ Panel propio, bloqueado (doc: IndicatorExamples → NewPanel + DenyToChangePanel)
            Panel = IndicatorDataProvider.NewPanel;
            DenyToChangePanel = true;

            _balance = new ValueDataSeries("Balance")
            {
                VisualType = VisualMode.Line,
                // Ajusta si quieres más/menos decimales
                Digits = 2
            };

            DataSeries[0] = _balance;
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            // Lectura directa del balance del portfolio seleccionado
            _balance[bar] = GetCurrentBalance();
        }

        // Event-driven: si el portfolio cambia (p. ej. PnL moviéndose), refrescamos la serie
        protected override void OnPortfolioChanged(Portfolio portfolio)
        {
            if (CurrentBar > 0)
            {
                _balance[CurrentBar - 1] = portfolio?.Balance ?? 0m;
                RedrawChart();
            }
        }

        private decimal GetCurrentBalance()
        {
            // Doc: “Working with trading events” → TradingManager.Portfolio
            return TradingManager?.Portfolio?.Balance ?? 0m;
        }
    }
}

