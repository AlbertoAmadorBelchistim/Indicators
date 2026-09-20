namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

using ATAS.DataFeedsCore;

using OFT.Rendering.Context;
using OFT.Rendering.Tools;

using Utils.Common.Logging;

/// <summary>
/// Account panel for funded (prop firm) accounts: balance and PnL of the selected account, with
/// the trailing drawdown, the daily loss limit and profit target, the session statistics of the
/// chart instrument and a suggested trading status.
/// </summary>
[Category(IndicatorCategories.Trading)]
[DisplayName("Prop Risk Monitor")]
[Description("Account panel for funded accounts: trailing drawdown, daily loss limit and profit target, session statistics and a suggested trading status.")]
public class PropRiskMonitor : Indicator
{
	#region Nested types

	public enum HorizontalAlignment
	{
		Left,
		Center,
		Right
	}

	public enum VerticalAlignment
	{
		Top,
		Middle,
		Bottom
	}

	// One row of the panel. RawForColoring colours the value by sign; ValueColor overrides it.
	private sealed record DisplayLine(string Label, string Value, decimal? RawForColoring, Color? ValueColor = null);

	#endregion

	#region Fields

	private const int Padding = 10;

	private Color _backgroundColor = Color.FromArgb(200, 20, 25, 35);
	private Color _textColor = Color.FromArgb(220, 220, 220);
	private Color _positiveColor = Color.FromArgb(0, 230, 118);
	private Color _negativeColor = Color.FromArgb(255, 82, 82);
	private Color _neutralColor = Color.FromArgb(150, 150, 150);
	private readonly RenderPen _borderPen = new(Color.Gray, 1);
	private RenderFont _font = new("Arial", 11);

	private Portfolio _currentPortfolio;

	#endregion

	#region Properties

	[Display(Name = "Background", GroupName = "Visualization", Order = 10)]
	public CrossColor BackgroundColor
	{
		get => _backgroundColor.Convert();
		set => _backgroundColor = value.Convert();
	}

	[Display(Name = "Text color", GroupName = "Visualization", Order = 20)]
	public CrossColor TextColor
	{
		get => _textColor.Convert();
		set => _textColor = value.Convert();
	}

	[Display(Name = "Positive color", GroupName = "Visualization", Order = 30)]
	public CrossColor PositiveColor
	{
		get => _positiveColor.Convert();
		set => _positiveColor = value.Convert();
	}

	[Display(Name = "Negative color", GroupName = "Visualization", Order = 40)]
	public CrossColor NegativeColor
	{
		get => _negativeColor.Convert();
		set => _negativeColor = value.Convert();
	}

	[Display(Name = "Neutral color", GroupName = "Visualization", Order = 50)]
	public CrossColor NeutralColor
	{
		get => _neutralColor.Convert();
		set => _neutralColor = value.Convert();
	}

	[Display(Name = "Font size", GroupName = "Visualization", Order = 60)]
	[Range(6, 30)]
	public float FontSize
	{
		get => _font.Size;
		set
		{
			if (Math.Abs(_font.Size - value) < 0.01f)
				return;

			_font = new RenderFont("Arial", value);
		}
	}

	[Display(Name = "Account ID", GroupName = "Account rows", Order = 10)]
	public bool ShowAccountId { get; set; } = true;

	[Display(Name = "Currency", GroupName = "Account rows", Order = 20)]
	public bool ShowCurrency { get; set; }

	[Display(Name = "Balance", GroupName = "Account rows", Order = 30)]
	public bool ShowBalance { get; set; } = true;

	[Display(Name = "Available balance", GroupName = "Account rows", Order = 40)]
	public bool ShowAvailableBalance { get; set; }

	[Display(Name = "Blocked margin", GroupName = "Account rows", Order = 50)]
	public bool ShowMargin { get; set; }

	[Display(Name = "Leverage", GroupName = "Account rows", Order = 60)]
	public bool ShowLeverage { get; set; }

	[Display(Name = "Open PnL", GroupName = "Account rows", Order = 70)]
	public bool ShowOpenPnL { get; set; } = true;

	[Display(Name = "Closed PnL", GroupName = "Account rows", Order = 80)]
	public bool ShowClosedPnL { get; set; } = true;

	[Display(Name = "Total PnL", GroupName = "Account rows", Order = 90)]
	public bool ShowTotalPnL { get; set; }

	[Display(Name = "Horizontal position", GroupName = "Layout", Order = 10)]
	public HorizontalAlignment HorizontalPosition { get; set; } = HorizontalAlignment.Left;

	[Display(Name = "Vertical position", GroupName = "Layout", Order = 20)]
	public VerticalAlignment VerticalPosition { get; set; } = VerticalAlignment.Bottom;

	[Display(Name = "Offset X", GroupName = "Layout", Order = 30)]
	[Range(0, 1000)]
	public int OffsetX { get; set; } = 20;

	[Display(Name = "Offset Y", GroupName = "Layout", Order = 40)]
	[Range(0, 1000)]
	public int OffsetY { get; set; } = 20;

	[Display(Name = "Column spacing", GroupName = "Layout", Order = 50)]
	[Range(5, 50)]
	public int ColumnSpacing { get; set; } = 15;

	#endregion

	#region ctor

	public PropRiskMonitor()
		: base(true)
	{
		DenyToChangePanel = true;
		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);
		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
	}

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		this.LogInfo($"PropRiskMonitor: initialized ({typeof(PropRiskMonitor).Assembly.GetName().Version}).");

		if (TradingManager != null)
		{
			TradingManager.PortfolioSelected += OnPortfolioSelected;
			_currentPortfolio = TradingManager.Portfolio;
		}
	}

	protected override void OnDispose()
	{
		if (TradingManager != null)
			TradingManager.PortfolioSelected -= OnPortfolioSelected;
	}

	protected override void OnCalculate(int bar, decimal value)
	{
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (ChartInfo == null || Container == null)
			return;

		var portfolio = _currentPortfolio ?? TradingManager?.Portfolio;

		if (portfolio == null)
			return;

		var lines = BuildLines(portfolio);

		if (lines.Count == 0)
			return;

		var lineHeight = context.MeasureString("A", _font).Height;
		var maxLabelWidth = 0;
		var maxValueWidth = 0;

		foreach (var line in lines)
		{
			maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(line.Label, _font).Width);
			maxValueWidth = Math.Max(maxValueWidth, context.MeasureString(line.Value, _font).Width);
		}

		var rectWidth = maxLabelWidth + ColumnSpacing + maxValueWidth + Padding * 2;
		var rectHeight = lines.Count * lineHeight + Padding * 2;
		var rectangle = new Rectangle(CalculateXPosition(rectWidth), CalculateYPosition(rectHeight), rectWidth, rectHeight);

		context.FillRectangle(_backgroundColor, rectangle);
		context.DrawRectangle(_borderPen, rectangle);

		var labelX = rectangle.X + Padding;
		var valueX = labelX + maxLabelWidth + ColumnSpacing;
		var y = rectangle.Y + Padding;

		foreach (var line in lines)
		{
			context.DrawString(line.Label, _font, _textColor, labelX, y);
			context.DrawString(line.Value, _font, line.ValueColor ?? ColorFor(line.RawForColoring), valueX, y);
			y += lineHeight;
		}
	}

	#endregion

	#region Private Methods

	private void OnPortfolioSelected(Portfolio portfolio)
	{
		_currentPortfolio = portfolio;
		RedrawChart();
	}

	private List<DisplayLine> BuildLines(Portfolio p)
	{
		var lines = new List<DisplayLine>();

		if (ShowAccountId)
			lines.Add(new("Account", p.AccountID, null));

		if (ShowCurrency && p.Currency.HasValue)
			lines.Add(new("Currency", p.Currency.Value.ToString(), null));

		if (ShowBalance)
			lines.Add(new("Balance", FormatCurrency(p.Balance), null));

		if (ShowAvailableBalance && p.BalanceAvailable.HasValue)
			lines.Add(new("Available", FormatCurrency(p.BalanceAvailable.Value), null));

		if (ShowMargin)
			lines.Add(new("Blocked margin", FormatCurrency(p.BlockedMargin), null));

		if (ShowLeverage && p.Leverage != 1)
			lines.Add(new("Leverage", $"{p.Leverage:F2}x", null));

		if (ShowOpenPnL)
			lines.Add(new("Open PnL", FormatCurrency(p.OpenPnL), p.OpenPnL));

		if (ShowClosedPnL)
			lines.Add(new("Closed PnL", FormatCurrency(p.ClosedPnL), p.ClosedPnL));

		// Session total (open + closed of the current session), not Portfolio.TotalPnL, which
		// includes the closed PnL of previous sessions.
		if (ShowTotalPnL)
			lines.Add(new("Total PnL", FormatCurrency(p.ClosedPnL + p.OpenPnL), p.ClosedPnL + p.OpenPnL));

		return lines;
	}

	private Color ColorFor(decimal? raw)
	{
		return raw is null ? _textColor
			: raw > 0m ? _positiveColor
			: raw < 0m ? _negativeColor
			: _neutralColor;
	}

	private static string FormatCurrency(decimal value)
	{
		return value.ToString("N2");
	}

	private int CalculateXPosition(int width)
	{
		return HorizontalPosition switch
		{
			HorizontalAlignment.Center => (Container.Region.Width - width) / 2,
			HorizontalAlignment.Right => Container.Region.Width - width - OffsetX,
			_ => OffsetX
		};
	}

	private int CalculateYPosition(int height)
	{
		return VerticalPosition switch
		{
			VerticalAlignment.Middle => (Container.Region.Height - height) / 2,
			VerticalAlignment.Bottom => Container.Region.Height - height - OffsetY,
			_ => OffsetY
		};
	}

	#endregion
}
