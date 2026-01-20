using ATAS.Indicators;
using ATAS.Indicators.Technical.Properties;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text;

namespace ATAS.Indicators.Technical
{
    [Category("Custom")]
    [DisplayName("GammaLevels")]
    public sealed class GammaLevels : Indicator
    {
        #region Nested types: model

        public enum LabelSide
        {
            [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Left))]
            Left = 0,

            [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Right))]
            Right = 1
        }

        #endregion

        #region Nested types: sources

        #endregion

        #region Fields

        // -----------------------------
        // Rendering (shell only)
        // -----------------------------
        private RenderFont _font;

        // NOTE: Keep halo behavior aligned with legacy LevelsLolo (red halo style).
        // The actual drawing will be implemented in later commits.

        // -----------------------------
        // Dirty flags
        // -----------------------------
        private bool _dataDirty = true;
        private bool _visualDirty = true;

        // -----------------------------
        // UI: Source - LoloText
        // -----------------------------
        private bool _enableLoloText = true;
        private string _loloTextRaw = string.Empty;

        // -----------------------------
        // UI: Visibility
        // -----------------------------
        private bool _onlyVisiblePriceRange = true;
        private bool _lastBarOnly = false;

        // -----------------------------
        // UI: Labels
        // -----------------------------
        private LabelSide _labelAlignment = LabelSide.Right;
        private int _offsetX = 6;
        private int _offsetY = 0;

        // -----------------------------
        // UI: Lines / Tiers (shell only - used later)
        // -----------------------------
        private int _thickMaxRank = 3;
        private int _mediumMaxRank = 10;

        private int _thickLineWidth = 3;
        private int _mediumLineWidth = 2;
        private int _thinLineWidth = 1;

        private int _thickLineTransparency = 0;
        private int _mediumLineTransparency = 40;
        private int _thinLineTransparency = 70;

        // -----------------------------
        // UI: 0DTE Halo (shell only)
        // -----------------------------
        private bool _show0DteHalo = true;
        private int _haloWidth = 6;
        private int _haloTransparency = 60;
        private bool _dash0Dte = true;

        #endregion

        #region Properties: Sources (LoloText)
        // -----------------------------
        // UI: Source - LoloText
        // -----------------------------

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Enabled),
            GroupName = nameof(Resources.LoloText),
            Description = nameof(Resources.LoloTextDesc),
            Order = 10)]
        public bool EnableLoloText
        {
            get => _enableLoloText;
            set
            {
                if (_enableLoloText == value)
                    return;

                _enableLoloText = value;
                _dataDirty = true;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Text),
            GroupName = nameof(Resources.LoloText),
            Description = nameof(Resources.LoloTextDesc),
            Order = 20)]
        public string LoloTextRaw
        {
            get => _loloTextRaw;
            set
            {
                value ??= string.Empty;

                if (string.Equals(_loloTextRaw, value, StringComparison.Ordinal))
                    return;

                _loloTextRaw = value;
                _dataDirty = true;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Clear),
            GroupName = nameof(Resources.LoloText),
            Description = nameof(Resources.ClearDesc),
            Order = 30)]
        public bool ClearLoloTextNow
        {
            get => false;
            set
            {
                if (!value)
                    return;

                if (_loloTextRaw.Length == 0)
                    return;

                _loloTextRaw = string.Empty;
                _dataDirty = true;
                RecalculateValues();
            }
        }
        #endregion
        // -----------------------------
        // UI: Visibility
        // -----------------------------
        #region Properties: Visibility
        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.OnlyVisiblePriceRange),
            GroupName = nameof(Resources.Visibility),
            Description = nameof(Resources.OnlyVisiblePriceRangeDesc),
            Order = 100)]
        public bool OnlyVisiblePriceRange
        {
            get => _onlyVisiblePriceRange;
            set
            {
                if (_onlyVisiblePriceRange == value)
                    return;

                _onlyVisiblePriceRange = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.LastBarOnly),
            GroupName = nameof(Resources.Visibility),
            Description = nameof(Resources.LastBarOnlyDesc),
            Order = 110)]
        public bool LastBarOnly
        {
            get => _lastBarOnly;
            set
            {
                if (_lastBarOnly == value)
                    return;

                _lastBarOnly = value;
                _visualDirty = true;
                RedrawChart();
            }
        }
        #endregion
        #region Properties: Labels
        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.LabelAlignment),
            GroupName = nameof(Resources.Labels),
            Description = nameof(Resources.LabelAlignmentDesc),
            Order = 200)]
        public LabelSide LabelAlignment
        {
            get => _labelAlignment;
            set
            {
                if (_labelAlignment == value)
                    return;

                _labelAlignment = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.OffsetX),
            GroupName = nameof(Resources.Labels),
            Order = 210)]
        public int OffsetX
        {
            get => _offsetX;
            set
            {
                if (_offsetX == value)
                    return;

                _offsetX = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.OffsetY),
            GroupName = nameof(Resources.Labels),
            Order = 220)]
        public int OffsetY
        {
            get => _offsetY;
            set
            {
                if (_offsetY == value)
                    return;

                _offsetY = value;
                _visualDirty = true;
                RedrawChart();
            }
        }
        #endregion
        // -----------------------------
        // UI: Lines / Tiers (shell only - used later)
        // -----------------------------
        #region Properties: Tiers
        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThickMaxRank),
            GroupName = nameof(Resources.Tiers),
            Description = nameof(Resources.ThickMaxRankDesc),
            Order = 300)]
        [Range(0, 999)]
        public int ThickMaxRank
        {
            get => _thickMaxRank;
            set
            {
                if (_thickMaxRank == value)
                    return;

                _thickMaxRank = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.MediumMaxRank),
            GroupName = nameof(Resources.Tiers),
            Description = nameof(Resources.MediumMaxRankDesc),
            Order = 310)]
        [Range(0, 999)]
        public int MediumMaxRank
        {
            get => _mediumMaxRank;
            set
            {
                if (_mediumMaxRank == value)
                    return;

                _mediumMaxRank = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThickLineWidth),
            GroupName = nameof(Resources.Tiers),
            Order = 320)]
        [Range(1, 10)]
        public int ThickLineWidth
        {
            get => _thickLineWidth;
            set
            {
                if (_thickLineWidth == value)
                    return;

                _thickLineWidth = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.MediumLineWidth),
            GroupName = nameof(Resources.Tiers),
            Order = 330)]
        [Range(1, 10)]
        public int MediumLineWidth
        {
            get => _mediumLineWidth;
            set
            {
                if (_mediumLineWidth == value)
                    return;

                _mediumLineWidth = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThinLineWidth),
            GroupName = nameof(Resources.Tiers),
            Order = 340)]
        [Range(1, 10)]
        public int ThinLineWidth
        {
            get => _thinLineWidth;
            set
            {
                if (_thinLineWidth == value)
                    return;

                _thinLineWidth = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThickLineTransparency),
            GroupName = nameof(Resources.Tiers),
            Order = 350)]
        [Range(0, 100)]
        public int ThickLineTransparency
        {
            get => _thickLineTransparency;
            set
            {
                if (_thickLineTransparency == value)
                    return;

                _thickLineTransparency = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.MediumLineTransparency),
            GroupName = nameof(Resources.Tiers),
            Order = 360)]
        [Range(0, 100)]
        public int MediumLineTransparency
        {
            get => _mediumLineTransparency;
            set
            {
                if (_mediumLineTransparency == value)
                    return;

                _mediumLineTransparency = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.ThinLineTransparency),
            GroupName = nameof(Resources.Tiers),
            Order = 370)]
        [Range(0, 100)]
        public int ThinLineTransparency
        {
            get => _thinLineTransparency;
            set
            {
                if (_thinLineTransparency == value)
                    return;

                _thinLineTransparency = value;
                _visualDirty = true;
                RedrawChart();
            }
        }
        #endregion
        // -----------------------------
        // UI: 0DTE Halo (shell only)
        // -----------------------------
        #region Properties: 0DTE halo
        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Show0DteHalo),
            GroupName = nameof(Resources.Halo),
            Description = nameof(Resources.Show0DteHaloDesc),
            Order = 400)]
        public bool Show0DteHalo
        {
            get => _show0DteHalo;
            set
            {
                if (_show0DteHalo == value)
                    return;

                _show0DteHalo = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.HaloWidth),
            GroupName = nameof(Resources.Halo),
            Description = nameof(Resources.HaloWidthDesc),
            Order = 410)]
        [Range(1, 20)]
        public int HaloWidth
        {
            get => _haloWidth;
            set
            {
                if (_haloWidth == value)
                    return;

                _haloWidth = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.HaloTransparency),
            GroupName = nameof(Resources.Halo),
            Description = nameof(Resources.HaloTransparencyDesc),
            Order = 420)]
        [Range(0, 100)]
        public int HaloTransparency
        {
            get => _haloTransparency;
            set
            {
                if (_haloTransparency == value)
                    return;

                _haloTransparency = value;
                _visualDirty = true;
                RedrawChart();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = nameof(Resources.Dash0Dte),
            GroupName = nameof(Resources.Halo),
            Description = nameof(Resources.Dash0DteDesc),
            Order = 430)]
        public bool Dash0Dte
        {
            get => _dash0Dte;
            set
            {
                if (_dash0Dte == value)
                    return;

                _dash0Dte = value;
                _visualDirty = true;
                RedrawChart();
            }
        }
        #endregion
        // -----------------------------
        // Lifecycle
        // -----------------------------
        #region Ctor
        public GammaLevels()
        {
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            // Keep a simple default font; will be finalized when text rendering is implemented.
            _font = new RenderFont("Arial", 12);
        }
        #endregion

        #region Overrides
        protected override void OnCalculate(int bar, decimal value)
        {
            // Shell: no calculations yet.
            // Future commits will parse sources and rebuild render packets when _dataDirty/_visualDirty.
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            // Shell: do nothing yet, but keep safe early-exits aligned with legacy indicator patterns.
            if (ChartInfo is not { PriceChartContainer.BarsWidth: > 2 })
                return;

            if (LastVisibleBarNumber > CurrentBar - 1)
                return;

            // Future commits will draw levels here.
        }
        #endregion

        #region Private methods
        private void RedrawChart()
        {
            // RecalculateValues triggers a render refresh in ATAS indicators.
            // Use it for visual-only changes too to keep behavior consistent.
            RecalculateValues();
        }
        #endregion
    }
}
