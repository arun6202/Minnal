---
name: Obsidian Kinetic
colors:
  surface: '#1f0f0d'
  surface-dim: '#1f0f0d'
  surface-bright: '#493432'
  surface-container-lowest: '#190a08'
  surface-container-low: '#281715'
  surface-container: '#2d1b19'
  surface-container-high: '#382523'
  surface-container-highest: '#44302e'
  on-surface: '#fbdbd7'
  on-surface-variant: '#e6bdb8'
  inverse-surface: '#fbdbd7'
  inverse-on-surface: '#3f2c29'
  outline: '#ac8884'
  outline-variant: '#5c403c'
  surface-tint: '#ffb4ab'
  primary: '#ffb4ab'
  on-primary: '#690005'
  primary-container: '#dc2626'
  on-primary-container: '#fff6f5'
  inverse-primary: '#bf0715'
  secondary: '#6bd8cb'
  on-secondary: '#003732'
  secondary-container: '#29a195'
  on-secondary-container: '#00302b'
  tertiary: '#90cdff'
  on-tertiary: '#003450'
  tertiary-container: '#0078b2'
  on-tertiary-container: '#f3f8ff'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#ffdad6'
  primary-fixed-dim: '#ffb4ab'
  on-primary-fixed: '#410002'
  on-primary-fixed-variant: '#93000b'
  secondary-fixed: '#89f5e7'
  secondary-fixed-dim: '#6bd8cb'
  on-secondary-fixed: '#00201d'
  on-secondary-fixed-variant: '#005049'
  tertiary-fixed: '#cbe6ff'
  tertiary-fixed-dim: '#90cdff'
  on-tertiary-fixed: '#001e30'
  on-tertiary-fixed-variant: '#004b71'
  background: '#1f0f0d'
  on-background: '#fbdbd7'
  surface-variant: '#44302e'
typography:
  display:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: '1.1'
    letterSpacing: -0.04em
  h1:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  h2:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.3'
    letterSpacing: -0.01em
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.5'
  code-md:
    fontFamily: JetBrains Mono
    fontSize: 13px
    fontWeight: '400'
    lineHeight: '1.6'
  code-sm:
    fontFamily: JetBrains Mono
    fontSize: 11px
    fontWeight: '400'
    lineHeight: '1.4'
  label-caps:
    fontFamily: JetBrains Mono
    fontSize: 10px
    fontWeight: '700'
    lineHeight: '1'
    letterSpacing: 0.1em
spacing:
  unit: 4px
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 40px
  gutter: 1px
---

## Brand & Style

The design system is engineered for high-velocity API development and AI orchestration. It rejects the softness of modern consumer web design in favor of a "Never Settle" aesthetic defined by surgical precision and intense focus. The personality is uncompromising, professional, and rooted in a hacker-industrial aesthetic tailored for enterprise-grade performance.

Drawing heavily from **Minimalism** and **Brutalism**, the system utilizes a high-contrast palette to eliminate ambiguity. It prioritizes information density and structural clarity. Visual interest is not generated through decoration, but through the tension between deep obsidian voids and razor-sharp crimson accents. This design system communicates a sense of lethal efficiency—tools for engineers who demand absolute control over their environment.

## Colors

The color strategy is binary and high-impact. The foundation is built on "True Black" (#000000) and "Deep Obsidian" (#0A0A0A) to provide a canvas where data and code can emerge with maximum clarity. 

- **Primary (Blood Red):** Used exclusively for critical actions, destructive states, and primary focal points. It represents the "pulse" of the system.
- **Secondary (Electric Teal):** Reserved for AI-native features, machine learning processes, and successful terminal states. This creates a clear visual distinction between human-driven actions (Red) and machine-driven intelligence (Teal).
- **Neutral Grayscale:** A cold, zinc-based scale is used for borders and secondary text to maintain the industrial tone without introducing warmth.

## Typography

The typography system employs a dual-font strategy to separate interface logic from data content. 

**Inter** serves as the primary interface font. It is utilized for navigation, headers, and UI controls, providing high legibility at small sizes while maintaining a neutral, utilitarian character.

**JetBrains Mono** is the system's technical backbone. It is used for all code blocks, API endpoints, JSON payloads, and status labels. This monospace typeface reinforces the "hacker" aesthetic and ensures that technical data is perfectly aligned and easy to parse during intense debugging sessions.

## Layout & Spacing

The layout philosophy is based on a rigid, 4px grid system that favors high information density. The design system uses a **Fixed-Fluid hybrid grid**:
- **Fixed sidebars and utility panels:** These use 1px borders as separators rather than negative space.
- **Fluid main canvas:** The central workspace expands to maximize the visibility of code and terminal outputs.

Layouts are defined by structural containment. Every panel, card, and module is encased in a 1px solid border. Negative space is used sparingly; the system prefers to use borders to create "cells" of information, mimicking the structured nature of an IDE or terminal emulator.

## Elevation & Depth

In this design system, depth is communicated through **Tonal Layers** and **Bold Borders** rather than shadows. The concept of "upward" elevation is replaced by "z-index stacking" of solid colors.

- **Level 0 (Base):** True Black (#000000) for the main application background.
- **Level 1 (Surface):** Deep Obsidian (#0A0A0A) for primary panels and workspace areas.
- **Level 2 (Active/Overlay):** Matte Gray (#121212) for modals and menus.

**Glassmorphism** is used exclusively for transient overlays (e.g., command palettes or dropdowns) to maintain context of the underlying code. These overlays use a high-strength backdrop blur (20px) and a subtle 1px white-to-transparent border to denote their temporary state. No drop shadows are permitted; all depth is "flat" and mechanical.

## Shapes

The shape language is strictly **Sharp (0px radius)**. This design system rejects rounded corners to emphasize the "Never Settle" philosophy of precision engineering. Every button, input, card, and modal is a perfect rectangle. 

Geometric consistency is paramount. Icons should be composed of straight lines and 45/90-degree angles wherever possible. This visual rigidity ensures the UI feels like a single, cohesive machine rather than a collection of disparate web components.

## Components

Components are designed to look like industrial controls—built for durability and frequent use.

- **Buttons:** 
    - *Primary:* Solid Blood Red (#DC2626) background, white text, 0px radius. Hover state increases brightness slightly.
    - *Secondary:* Transparent background, 1px white border.
    - *AI Action:* Solid Electric Teal (#0D9488) background with a "Pulse" animation on the border.
- **Input Fields:** Minimalist design with a 1px border on all sides. On focus, the border changes to Blood Red. Placeholder text uses the JetBrains Mono font at 50% opacity.
- **Chips/Badges:** Monospace text in uppercase. Status indicators use a small 4x4px square of color (Teal for success, Red for error) rather than a rounded dot.
- **Cards/Panels:** Defined by a 1px border (#27272A) and a title bar with a distinct background color (#121212) to separate header metadata from content.
- **Code Editor:** Deep black background with syntax highlighting that utilizes the Primary Red and Secondary Teal colors to maintain brand alignment within the data itself.
- **Checkboxes:** Square, 0px radius, with a sharp "X" mark or solid fill when active.