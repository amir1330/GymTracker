## ADDED Requirements

### Requirement: Gruvbox color theme
The system SHALL use Gruvbox color palette for all UI elements.

#### Scenario: Light mode colors
- **WHEN** user selects light mode
- **THEN** system displays with Gruvbox light palette (bg=#fbf1c7, fg=#3c3836, accent colors)

#### Scenario: Dark mode colors
- **WHEN** user selects dark mode
- **THEN** system displays with Gruvbox dark palette (bg=#282828, fg=#ebdbb2, accent colors)

### Requirement: Theme toggle
The system SHALL allow users to switch between light and dark modes.

#### Scenario: Toggle to dark mode
- **WHEN** user clicks theme toggle
- **THEN** system switches to Gruvbox dark palette

#### Scenario: Toggle to light mode
- **WHEN** user clicks theme toggle
- **THEN** system switches to Gruvbox light palette

### Requirement: JetBrains Mono font
The system SHALL use JetBrains Mono as the primary font.

#### Scenario: Font rendering
- **WHEN** any page loads
- **THEN** all text displays in JetBrains Mono

### Requirement: TUI-inspired layout
The system SHALL use a minimal terminal-like aesthetic.

#### Scenario: Visual style
- **WHEN** user views any page
- **THEN** layout uses sharp borders, no shadows, monospace text, minimal decoration

### Requirement: Mobile-responsive layout
The system SHALL render all pages usable on phone-width screens (320px+).

#### Scenario: View on mobile
- **WHEN** user views any page on a screen width of 375px
- **THEN** layout adapts: nav wraps, forms stack vertically, tables scroll, no horizontal overflow
