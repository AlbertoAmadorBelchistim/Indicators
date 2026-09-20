namespace ATAS.Indicators.Technical.PropRisk;

/// <summary>How the account equity is read from the portfolio.</summary>
public enum EquitySource
{
	/// <summary>Balance plus open PnL, for connections whose balance leaves out the open trades.</summary>
	BalancePlusOpenPnl,

	/// <summary>The balance alone, for connections whose balance already includes them.</summary>
	Balance
}
