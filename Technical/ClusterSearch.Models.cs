namespace ATAS.Indicators.Technical;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using OFT.Localization;

public partial class ClusterSearch
{
	#region Nested types

	private class CustomVolumeInfo : PriceVolumeInfo
	{
		#region Properties

		public decimal AvgTrade => Ticks is 0 ? 0 : Volume / Ticks;

		public decimal Delta => Ask - Bid;

		#endregion

		#region ctor
		
		public CustomVolumeInfo(decimal price)
		{
			Price = price;
		}

		#endregion
	}

	public enum CalcMode
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bid))]
		Bid,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Ask))]
		Ask,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Delta))]
		Delta,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Volume))]
		Volume,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Ticks))]
		Tick,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.PocLevel))]
		MaxVolume,

		[Browsable(false)]
		[Obsolete]
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Time))]
		Time
	}

	public enum CandleDirection
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bearlish))]
		Bearish,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bullish))]
		Bullish,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Any))]
		Any,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Neutral))]
		Neutral
	}

	public enum PriceLocation
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.AtHigh))]
		AtHigh,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.AtLow))]
		AtLow,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Any))]
		Any,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Body))]
		Body,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.UpperWick))]
		UpperWick,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.LowerWick))]
		LowerWick,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.AtHighOrLow))]
		AtHighOrLow,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.AnyWick))]
		AtUpperLowerWick
	}

	#endregion
}