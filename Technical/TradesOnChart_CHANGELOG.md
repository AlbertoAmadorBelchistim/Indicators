# Trades On Chart - Release Notes

## New Features

### Trade Labels
Added customizable labels that display trade information directly on the chart at exit points.

**Label Display Modes:**
- **Hide** (default): No labels shown
- **Short**: Compact view showing direction, volume, and result
  - Example: `L 10 +125 (5t)`
- **Full**: Complete view with entry and exit prices
  - Example: `L 10 | 50000→50025 +125 (5t)`

**Label Features:**
- **Dual-color design**: Direction color (left) and result color (right)
- **Smart positioning**: Labels automatically avoid overlapping
- **Interactive**: Hover over labels to see detailed trade information
- **Aligned to candles**: Labels position themselves relative to candle edges for clarity

### Enhanced Tooltips
Tooltips now feature a modern two-section design:
- **Top section**: Shows direction, volume, instrument, entry/exit prices with timestamps
- **Bottom section**: Highlights the result (P&L and ticks) with color-coded background
- **Vertical color split**: Direction color for trade details, result color for outcome

### New Color Settings
Added separate color controls for profit and loss visualization:
- **Profit Color** (#49A069): Applied to winning trades
- **Loss Color** (#C64F4F): Applied to losing trades
- Colors affect both labels and tooltip result sections

### Updated Default Colors
All trade direction colors now use consistent theme:
- **Buy/Sell Color** (#007ACC): Unified blue tone for cleaner look
- Works harmoniously with standard candle colors

## Visual Improvements
- Rounded corners on labels and tooltips for modern appearance
- Sharp borders between color sections for clear visual separation
- Improved contrast for better readability
- Consistent spacing between multiple labels at all zoom levels

## How to Use
1. Open indicator settings
2. Find "Label Display" in Visualization section
3. Choose your preferred mode: Hide, Short, or Full
4. Customize colors to match your chart theme
5. Hover over markers or labels to see detailed trade information

## Notes
- Labels hidden by default to maintain clean charts
- All new features respect existing Show Lines and Show Tooltip settings
- Labels work seamlessly with existing marker system
