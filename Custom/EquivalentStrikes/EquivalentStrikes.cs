namespace ATAS.Indicators.Technical
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;
	using System.Drawing;
	using System.Globalization;

	using ATAS.Indicators.Drawing;
	using ATAS.Indicators.Technical.EquivalentStrikesCore;

	using OFT.Rendering.Context;
	using OFT.Rendering.Settings;
	using OFT.Rendering.Tools;

	using Utils.Common.Logging;

	using Res = ATAS.Indicators.Technical.EquivalentStrikesResources;

	[Category("Custom")]
	[DisplayName("Equivalent Strikes")]
	[Display(ResourceType = typeof(Res), Description = nameof(Res.EquivalentStrikesDescription))]
	public sealed class EquivalentStrikes : Indicator
	{
		#region Fields

		private readonly StrikeGrid _grid = new();
		private readonly List<StrikeLine> _lines = new();

		private readonly RenderStringFormat _leftFormat = new()
			{ LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };

		private readonly RenderStringFormat _rightFormat = new()
			{ LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far };

		private RenderFont _font = new("Arial", 10);
		private StrikePreset _preset = StrikePreset.QqqOnNq;
		private string _underlyingName = "QQQ";
		private decimal _lastPrice;

		#endregion

		#region Pair

		[Display(ResourceType = typeof(Res), Name = nameof(Res.Pair), GroupName = nameof(Res.Underlying), Description = nameof(Res.PairDescription), Order = 100)]
		public StrikePreset Preset
		{
			get => _preset;
			set
			{
				_preset = value;

				if (StrikePresets.TryGet(value, out var values))
				{
					_grid.Factor = values.Factor;
					_grid.Spacing = values.Spacing;
					_grid.MajorSpacing = values.MajorSpacing;
					_underlyingName = values.Label;

					RaisePropertyChanged(nameof(Factor));
					RaisePropertyChanged(nameof(Spacing));
					RaisePropertyChanged(nameof(MajorSpacing));
					RaisePropertyChanged(nameof(UnderlyingName));
				}

				RedrawChart();
			}
		}

		[Display(ResourceType = typeof(Res), Name = nameof(Res.Name), GroupName = nameof(Res.Underlying), Description = nameof(Res.NameDescription), Order = 110)]
		public string UnderlyingName
		{
			get => _underlyingName;
			set
			{
				_underlyingName = value ?? string.Empty;
				RedrawChart();
			}
		}

		[Display(ResourceType = typeof(Res), Name = nameof(Res.Factor), GroupName = nameof(Res.Underlying), Description = nameof(Res.FactorDescription), Order = 120)]
		[Range(0.000001, 1000000)]
		public decimal Factor
		{
			get => _grid.Factor;
			set
			{
				if (value <= 0m)
					return;

				_grid.Factor = value;
				_preset = StrikePreset.Custom;
				RaisePropertyChanged(nameof(Preset));
				RedrawChart();
			}
		}

		[Display(ResourceType = typeof(Res), Name = nameof(Res.Basis), GroupName = nameof(Res.Underlying), Description = nameof(Res.BasisDescription), Order = 130)]
		public decimal Basis
		{
			get => _grid.Basis;
			set
			{
				_grid.Basis = value;
				RedrawChart();
			}
		}

		#endregion

		#region Anchor

		[Display(ResourceType = typeof(Res), Name = nameof(Res.UnderlyingPrice), GroupName = nameof(Res.Anchor), Description = nameof(Res.UnderlyingPriceDescription), Order = 200)]
		public decimal AnchorUnderlyingPrice { get; set; }

		[Display(ResourceType = typeof(Res), Name = nameof(Res.IndexPrice), GroupName = nameof(Res.Anchor), Description = nameof(Res.IndexPriceDescription), Order = 210)]
		public decimal AnchorIndexPrice { get; set; }

		[Display(ResourceType = typeof(Res), Name = nameof(Res.Solve), GroupName = nameof(Res.Anchor), Description = nameof(Res.SolveDescription), Order = 220)]
		public AnchorTarget AnchorSolves { get; set; } = AnchorTarget.Basis;

		[Display(ResourceType = typeof(Res), Name = nameof(Res.ApplyAnchor), GroupName = nameof(Res.Anchor), Description = nameof(Res.ApplyAnchorDescription), Order = 230)]
		public bool ApplyAnchor
		{
			get => false;
			set
			{
				if (!value)
					return;

				Anchor();
			}
		}

		#endregion

		#region Grid

		[Display(ResourceType = typeof(Res), Name = nameof(Res.StrikeSpacing), GroupName = nameof(Res.Grid), Description = nameof(Res.StrikeSpacingDescription), Order = 300)]
		[Range(0.000001, 100000)]
		public decimal Spacing
		{
			get => _grid.Spacing;
			set
			{
				if (value <= 0m)
					return;

				_grid.Spacing = value;
				RedrawChart();
			}
		}

		[Display(ResourceType = typeof(Res), Name = nameof(Res.MajorEvery), GroupName = nameof(Res.Grid), Description = nameof(Res.MajorEveryDescription), Order = 310)]
		[Range(0, 100000)]
		public decimal MajorSpacing
		{
			get => _grid.MajorSpacing;
			set
			{
				if (value < 0m)
					return;

				_grid.MajorSpacing = value;
				RedrawChart();
			}
		}

		[Display(ResourceType = typeof(Res), Name = nameof(Res.MaximumLines), GroupName = nameof(Res.Grid), Description = nameof(Res.MaximumLinesDescription), Order = 320)]
		[Range(1, 500)]
		public int MaxLines
		{
			get => _grid.MaxLines;
			set
			{
				_grid.MaxLines = value;
				RedrawChart();
			}
		}

		[Display(ResourceType = typeof(Res), Name = nameof(Res.MinorLines), GroupName = nameof(Res.Grid), Description = nameof(Res.MinorLinesDescription), Order = 330)]
		public bool ShowMinorLines { get; set; } = true;

		#endregion

		#region Visualization

		[Display(ResourceType = typeof(Res), Name = nameof(Res.MinorLine), GroupName = nameof(Res.Visualization), Order = 400)]
		public PenSettings MinorPen { get; set; } = new()
			{ Color = DefaultColors.Gray.Convert(), Width = 1 };

		[Display(ResourceType = typeof(Res), Name = nameof(Res.MajorLine), GroupName = nameof(Res.Visualization), Order = 410)]
		public PenSettings MajorPen { get; set; } = new()
			{ Color = DefaultColors.Blue.Convert(), Width = 2 };

		[Display(ResourceType = typeof(Res), Name = nameof(Res.Labels), GroupName = nameof(Res.Visualization), Description = nameof(Res.LabelsDescription), Order = 420)]
		public bool ShowLabels { get; set; } = true;

		[Display(ResourceType = typeof(Res), Name = nameof(Res.MajorLabelsOnly), GroupName = nameof(Res.Visualization), Order = 430)]
		public bool MajorLabelsOnly { get; set; }

		[Display(ResourceType = typeof(Res), Name = nameof(Res.LabelsOnLeft), GroupName = nameof(Res.Visualization), Description = nameof(Res.LabelsOnLeftDescription), Order = 440)]
		public bool LabelsOnLeft { get; set; }

		[Display(ResourceType = typeof(Res), Name = nameof(Res.PriceInLabel), GroupName = nameof(Res.Visualization), Description = nameof(Res.PriceInLabelDescription), Order = 450)]
		public bool ShowPriceInLabel { get; set; }

		[Display(ResourceType = typeof(Res), Name = nameof(Res.FontSize), GroupName = nameof(Res.Visualization), Order = 460)]
		[Range(5, 30)]
		public float FontSize
		{
			get => _font.Size;
			set
			{
				_font = new RenderFont("Arial", value);
				RedrawChart();
			}
		}

		[Display(ResourceType = typeof(Res), Name = nameof(Res.Equivalence), GroupName = nameof(Res.Visualization), Description = nameof(Res.EquivalenceDescription), Order = 470)]
		public bool ShowEquivalence { get; set; } = true;

		[Display(ResourceType = typeof(Res), Name = nameof(Res.EquivalenceText), GroupName = nameof(Res.Visualization), Order = 480)]
		public CrossColor EquivalenceColor { get; set; } = DefaultColors.Blue.Convert();

		#endregion

		#region ctor

		public EquivalentStrikes()
			: base(useCandles: true)
		{
			// Overlay on the main price panel; everything is drawn in OnRender.
			DenyToChangePanel = true;
			DrawAbovePrice = false;
			EnableCustomDrawing = true;
			SubscribeToDrawingEvents(DrawingLayouts.Final);

			DataSeries[0].IsHidden = true;
			((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

			_grid.Factor = 41m;
			_grid.Spacing = 1m;
			_grid.MajorSpacing = 5m;
			_grid.MaxLines = 80;
		}

		#endregion

		#region Protected methods

		protected override void OnInitialize()
		{
			this.LogInfo($"EquivalentStrikes: initialized ({typeof(EquivalentStrikes).Assembly.GetName().Version}).");
		}

		protected override void OnCalculate(int bar, decimal value)
		{
			// The grid does not depend on the bars; the last price is kept for the anchor and the readout.
			if (bar == CurrentBar - 1)
				_lastPrice = GetCandle(bar).Close;
		}

		protected override void OnRender(RenderContext context, DrawingLayouts layout)
		{
			if (ChartInfo == null || InstrumentInfo == null || ChartInfo.Region.Height <= 0)
				return;

			var step = _grid.Build(ChartInfo.PriceChartContainer.Low, ChartInfo.PriceChartContainer.High, _lines);

			if (step > 0m)
				DrawLines(context);

			if (ShowEquivalence)
				DrawEquivalence(context);
		}

		#endregion

		#region Private methods

		private void DrawLines(RenderContext context)
		{
			var height = ChartInfo.Region.Height;
			var width = ChartInfo.Region.Width;
			var textHeight = context.MeasureString("0", _font).Height;
			var lastLabelY = int.MinValue;

			foreach (var line in _lines)
			{
				if (!line.IsMajor && !ShowMinorLines)
					continue;

				var y = ChartInfo.GetYByPrice(line.Price, false);

				if (y < 0 || y > height)
					continue;

				var pen = line.IsMajor ? MajorPen : MinorPen;
				context.DrawLine(pen.RenderObject, 0, y, width, y);

				if (!ShowLabels || (MajorLabelsOnly && !line.IsMajor))
					continue;

				// Labels closer together than their own height would overlap into a smear.
				if (Math.Abs(y - lastLabelY) < textHeight)
					continue;

				DrawLabel(context, line, y, textHeight, width, pen.RenderObject.Color);
				lastLabelY = y;
			}
		}

		private void DrawLabel(RenderContext context, StrikeLine line, int y, int textHeight, int width, Color color)
		{
			var text = _underlyingName.Length == 0
				? Format(line.Strike)
				: _underlyingName + " " + Format(line.Strike);

			if (ShowPriceInLabel)
				text += " = " + line.Price.ToString("0.####", CultureInfo.InvariantCulture);

			var textWidth = context.MeasureString(text, _font).Width;

			// Just above the line, so the line itself stays readable under the text.
			var rect = LabelsOnLeft
				? new Rectangle(2, y - textHeight, textWidth + 2, textHeight)
				: new Rectangle(width - textWidth - 4, y - textHeight, textWidth + 2, textHeight);

			context.DrawString(text, _font, color, rect, LabelsOnLeft ? _leftFormat : _rightFormat);
		}

		private void DrawEquivalence(RenderContext context)
		{
			if (!_grid.IsValid || _lastPrice <= 0m)
				return;

			var text = string.Format(
				CultureInfo.InvariantCulture,
				"{0} {1} = {2} {3}",
				InstrumentInfo.Instrument,
				_lastPrice.ToString("0.####", CultureInfo.InvariantCulture),
				_underlyingName,
				_grid.ToStrike(_lastPrice).ToString("0.00", CultureInfo.InvariantCulture));

			var size = context.MeasureString(text, _font);
			context.DrawString(text, _font, EquivalenceColor.Convert(), new Rectangle(4, 2, size.Width + 2, size.Height), _leftFormat);
		}

		private void Anchor()
		{
			var result = StrikeAnchor.Solve(
				_lastPrice,
				AnchorUnderlyingPrice,
				AnchorIndexPrice,
				_grid.Factor,
				_grid.Basis,
				AnchorSolves);

			if (!result.Ok)
				return;

			_grid.Factor = result.Factor;
			_grid.Basis = result.Basis;
			_preset = StrikePreset.Custom;

			RaisePropertyChanged(nameof(Factor));
			RaisePropertyChanged(nameof(Basis));
			RaisePropertyChanged(nameof(Preset));
			RedrawChart();
		}

		private static string Format(decimal strike)
		{
			return strike.ToString(strike == Math.Truncate(strike) ? "0" : "0.####", CultureInfo.InvariantCulture);
		}

		#endregion
	}
}
