namespace ATAS.Indicators.Technical.PropRisk;

using System;
using System.Collections.Generic;

/// <summary>State of the chart instrument's position at one moment.</summary>
public readonly record struct PositionSnapshot(decimal Volume, decimal AveragePrice, decimal UnrealizedPnl, decimal RealizedPnl);

/// <summary>A trade of the chart instrument that is open or was closed.</summary>
public sealed class TradeRecord
{
	/// <summary>1 long, -1 short.</summary>
	public int Side { get; set; }

	public DateTime OpenTimeUtc { get; set; }

	public DateTime? CloseTimeUtc { get; set; }

	/// <summary>Position realized PnL when the trade opened; the trade's PnL is measured from it.</summary>
	public decimal RealizedAtOpen { get; set; }

	/// <summary>Realized PnL of the trade (set when it closes, corrected by late updates while flat).</summary>
	public decimal Pnl { get; set; }

	/// <summary>Highest and lowest open PnL of the trade (partial closes included): MFE and MAE.</summary>
	public decimal MaxOpenPnl { get; set; }

	public decimal MinOpenPnl { get; set; }

	public int MaxQuantity { get; set; }

	public TradeRecord Clone() => (TradeRecord)MemberwiseClone();
}

/// <summary>Persisted trades of the chart instrument in the current trading day.</summary>
public sealed class LedgerState
{
	public DateTime Day { get; set; }

	public TradeRecord Current { get; set; }

	public TradeRecord Last { get; set; }

	/// <summary>PnL of each trade closed today, in order.</summary>
	public List<decimal> ClosedToday { get; set; } = new();

	/// <summary>Position realized PnL at the last update, to apply late realized updates while flat.</summary>
	public decimal LastRealized { get; set; }

	public LedgerState Clone()
	{
		var clone = (LedgerState)MemberwiseClone();
		clone.Current = Current?.Clone();
		clone.Last = Last?.Clone();
		clone.ClosedToday = new List<decimal>(ClosedToday);
		return clone;
	}
}

public readonly record struct LedgerStats(int Trades, int Wins, int Losses, int Streak, decimal RealizedPnl);

/// <summary>
/// Builds the trades of the chart instrument from its position: a trade opens when the position
/// leaves flat, closes when it returns to flat, and a reversal (long to short or back without
/// going flat) closes one trade and opens another. Adding to or reducing a position stays in the
/// same trade. The PnL of a trade is the position's realized PnL from open to close, so partial
/// closes count; its open PnL (unrealized plus realized since the open) gives MFE and MAE.
/// </summary>
public static class TradeLedger
{
	/// <summary>Applies a position update. Returns the trade that closed with it, if any.</summary>
	public static TradeRecord Update(LedgerState state, PositionSnapshot position, DateTime tradingDay, DateTime utcNow)
	{
		if (tradingDay > state.Day)
		{
			state.Day = tradingDay;
			state.ClosedToday.Clear();
		}

		var side = Math.Sign(position.Volume);
		TradeRecord closed = null;

		if (state.Current != null && side != state.Current.Side)
		{
			closed = state.Current;
			closed.CloseTimeUtc = utcNow;
			closed.Pnl = position.RealizedPnl - closed.RealizedAtOpen;
			state.Current = null;
			state.Last = closed;
			state.ClosedToday.Add(closed.Pnl);
		}
		else if (state.Current == null && side == 0 && state.Last != null && state.ClosedToday.Count > 0
			&& position.RealizedPnl != state.LastRealized)
		{
			// The realized PnL of the close can arrive in a later update than the flat volume.
			var delta = position.RealizedPnl - state.LastRealized;
			state.Last.Pnl += delta;
			state.ClosedToday[^1] += delta;
		}

		if (state.Current == null && side != 0)
		{
			state.Current = new TradeRecord
			{
				Side = side,
				OpenTimeUtc = utcNow,
				RealizedAtOpen = position.RealizedPnl,
				MaxOpenPnl = position.UnrealizedPnl,
				MinOpenPnl = position.UnrealizedPnl
			};
		}

		if (state.Current != null)
		{
			var openPnl = position.UnrealizedPnl + position.RealizedPnl - state.Current.RealizedAtOpen;
			state.Current.MaxOpenPnl = Math.Max(state.Current.MaxOpenPnl, openPnl);
			state.Current.MinOpenPnl = Math.Min(state.Current.MinOpenPnl, openPnl);
			state.Current.MaxQuantity = Math.Max(state.Current.MaxQuantity, (int)Math.Abs(position.Volume));
		}

		state.LastRealized = position.RealizedPnl;
		return closed;
	}

	/// <summary>Trades closed today: count, wins, losses, current streak (+ wins, - losses) and PnL.</summary>
	public static LedgerStats Stats(LedgerState state, DateTime tradingDay)
	{
		if (tradingDay > state.Day)
			return default;

		int wins = 0, losses = 0, streak = 0;
		decimal pnl = 0m;

		foreach (var trade in state.ClosedToday)
		{
			pnl += trade;

			if (trade > 0m)
			{
				wins++;
				streak = streak > 0 ? streak + 1 : 1;
			}
			else if (trade < 0m)
			{
				losses++;
				streak = streak < 0 ? streak - 1 : -1;
			}
		}

		return new LedgerStats(state.ClosedToday.Count, wins, losses, streak, pnl);
	}
}
