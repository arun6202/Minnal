---
name: Enterprise Obsidian
colors:
  surface: '#101419'
  surface-dim: '#101419'
  surface-bright: '#36393f'
  surface-container-lowest: '#0b0e14'
  surface-container-low: '#181c21'
  surface-container: '#1c2025'
  surface-container-high: '#272a30'
  surface-container-highest: '#32353b'
  on-surface: '#e0e2ea'
  on-surface-variant: '#c1c7d4'
  inverse-surface: '#e0e2ea'
  inverse-on-surface: '#2d3037'
  outline: '#8b919d'
  outline-variant: '#414752'
  surface-tint: '#a4c9ff'
  primary: '#a4c9ff'
  on-primary: '#00315d'
  primary-container: '#0672cb'
  on-primary-container: '#f3f6ff'
  inverse-primary: '#005fac'
  secondary: '#8ccdff'
  on-secondary: '#00344e'
  secondary-container: '#2899d8'
  on-secondary-container: '#002d44'
  tertiary: '#ffb68b'
  on-tertiary: '#522300'
  tertiary-container: '#b55502'
  on-tertiary-container: '#fff4ef'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#d4e3ff'
  primary-fixed-dim: '#a4c9ff'
  on-primary-fixed: '#001c39'
  on-primary-fixed-variant: '#004884'
  secondary-fixed: '#cae6ff'
  secondary-fixed-dim: '#8ccdff'
  on-secondary-fixed: '#001e2f'
  on-secondary-fixed-variant: '#004b6f'
  tertiary-fixed: '#ffdbc8'
  tertiary-fixed-dim: '#ffb68b'
  on-tertiary-fixed: '#321200'
  on-tertiary-fixed-variant: '#753400'
  background: '#101419'
  on-background: '#e0e2ea'
  surface-variant: '#32353b'
typography:
  display-xl:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: -0.01em
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.3'
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.5'
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.5'
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: '1'
    letterSpacing: 0.05em
  code-md:
    fontFamily: ui-monospace
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.7'
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  xxs: 4px
  xs: 8px
  sm: 12px
  md: 16px
  lg: 24px
  xl: 32px
  xxl: 48px
---

## Brand & Style

The design system bridges the gap between executive-level enterprise reliability and high-performance developer environments. It targets technical decision-makers and engineers who require high-density information without cognitive overload. The brand personality is authoritative, precise, and sophisticated.

The visual style is **Corporate / Modern** with a heavy influence of **Minimalism**. It prioritizes clarity and functional hierarchy over decorative elements. By utilizing a "dark-first" architecture, the system creates a focused "cockpit" experience that reduces eye strain during long-term use. Visual interest is generated through precise light-play on borders and the strategic use of high-chroma accent colors to denote intelligence and action.

## Colors

The palette is anchored by "Obsidian" (#050505) for the lowest Z-index layers and "Deep Charcoal" (#0F1115) for primary UI surfaces. This ensures a true high-contrast foundation that makes data pop.

**Primary Accent:** Dell Signature Blue is used exclusively for "Commit" actions, active navigation states, and critical paths. 
**Secondary Accent:** Teal is reserved for AI-augmented features, machine learning insights, and "Intelligence" markers to distinguish automated logic from user-initiated actions.
**Functional Neutrals:** We use a tiered gray scale. Pure white is reserved for headers and primary data. `text_secondary` (#94A3B8) is used for all descriptive text to maintain a calm, readable environment that avoids the harshness of pure white-on-black.

## Typography

This design system utilizes **Inter** for all UI elements to ensure maximum legibility across high-density data tables and complex dashboards. The scale is built on a modular rhythm, prioritizing vertical rhythm and clear hierarchy.

Headlines should use tighter tracking and heavier weights to feel "anchored." Body text uses a standard weight for readability, while labels use a slightly increased letter-spacing and uppercase styling to provide clear categorization without occupying significant vertical space. For code blocks and technical logs, a system-default monospaced font is used to provide a familiar environment for developers.

## Layout & Spacing

The layout philosophy follows a **Fluid Grid** model with high-density spacing. We use a 4px base unit to allow for the precision required in enterprise toolsets. 

The 12-column grid is the standard for page layouts, but internal components should use the 4px/8px rhythm for padding and margins. In complex data views, the "Compact" density (using `xs` and `sm` spacing) is preferred to maximize the information visible above the fold. Gutters are kept tight (16px) to maintain a cohesive, "single-app" feel.

## Elevation & Depth

In a dark-on-dark interface, depth is conveyed through **Tonal Layers** and **Low-contrast Outlines** rather than traditional shadows. 

1. **Level 0 (Background):** Obsidian (#050505) - The base canvas.
2. **Level 1 (Surface):** Deep Charcoal (#0F1115) - Used for cards, sidebars, and headers.
3. **Level 2 (Overlay):** A slightly lighter gray (#1E293B) - Used for tooltips, menus, and modals.

Each surface level is defined by a 1px border using `border_subtle`. Shadows are used sparingly, only on Level 2 elements, and they should be tight, dark, and high-blur to provide a subtle "lift" without creating a "glow" effect.

## Shapes

The shape language is defined by **Sharp / Precise** edges. We use a consistent 4px (0.25rem) radius across all buttons, input fields, and containers. This subtle rounding softens the clinical edge of a pure 0px system while maintaining a rigorous, technical appearance.

Do not use fully rounded "pill" shapes for buttons or tags. Every element must respect the 4px constraint to ensure that the grid feels architectural and stable.

## Components

### Buttons
*   **Primary:** Solid `primary_color_hex` with white text. No gradient. 4px radius.
*   **Secondary/Ghost:** `border_subtle` with `text_secondary`. On hover, the border brightens.
*   **AI/Intelligence:** Solid `secondary_color_hex` (Teal) or a subtle teal glow outline for suggested actions.

### Input Fields
Inputs should have an obsidian background and a 1px `border_subtle`. On focus, the border transitions to the primary Dell Blue. Error states use a high-contrast red (#FF5252) that remains legible against the dark background.

### Cards
Cards are flat. They use the `background_surface` color with a 1px `border_subtle`. Titles within cards should be `headline-md` or `body-sm` bold.

### Data Tables
Tables are the heart of the system. Use alternate row striping with a very subtle difference in background hex (e.g., #050505 vs #080808). Borders should be horizontal only to minimize visual noise.

### Code Blocks
Code blocks use a pure black (#000000) background to distinguish them from the UI surfaces. Syntax highlighting should use a high-contrast, "neon-on-dark" palette that respects the Teal and Blue accents.