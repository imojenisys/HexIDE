# Dark chrome palette evidence (#4234)

Measured under WSL on 2026-11-16, starting from `7f9b7416d`, with the real main-window
composition, SimpleTheme, DockSimpleTheme, ViewLocator and English localization loaded.
The window is 1200 × 800 at 100% scaling, with an empty project group. Skia's headless
`CaptureRenderedFrame` supplies the pixels; each pane reading is the most common colour
inside its view, excluding a four-pixel inset. These are headless results, not a live-IDE check.

| Pack | Pane | Before | After |
|---|---|---|---|
| Abyss | Project Explorer | `#282828` | `#000C18` |
| Abyss | Properties | `#282828` | `#000C18` |
| Abyss | Toolbox | `#282828` | `#000C18` |
| Dark | Project Explorer | `#282828` | `#252526` |
| Dark | Properties | `#282828` | `#252526` |
| Dark | Toolbox | `#282828` | `#252526` |

[Abyss before](before/Abyss.png) · [Abyss after](after/Abyss.png) ·
[Dark before](before/Dark.png) · [Dark after](after/Dark.png).
The adjacent `.txt` files contain pane pixels and resolved resources, explicitly using
the window's actual theme variant. Transparent Simple accents previously composited
cyan onto grey; the new accents are opaque members of each pack's own accent family.

## Dark overrides

All twenty mapped Simple/Dock keys are set. The baseline image shows grey inherited
pane backgrounds; the resolved brushes also reveal mid-grey raised controls, cyan
accents, transparent tab surfaces, bright blue active/selected tabs and differing text.
The overrides use existing Dark colours, including darker accent family members to
keep text legible. `ThemeControlMidHighBrush` uses Dark's existing `ScrollBar` colour
so borders and thumbs also meet 3:1 on raised and accented backgrounds.

| Key | New Dark value |
|---|---|
| ThemeBackgroundBrush | `#252526` |
| ThemeForegroundBrush | `#D4D4D4` |
| ThemeForegroundLowBrush | `#D4D4D4` |
| ThemeControlMidBrush | `#3E3E42` |
| ThemeControlMidHighBrush | `#C8C8C8` |
| ThemeControlHighlightMidBrush | `#4A4A4E` |
| ThemeAccentBrush2 | `#003F6B` |
| ThemeAccentBrush3 | `#094771` |
| ThemeAccentBrush4 | `#005F9E` |
| DockThemeBackgroundBrush | `#252526` |
| DockThemeControlBackgroundBrush | `#252526` |
| DockDocumentTabStripBackgroundBrush | `#252526` |
| DockSurfacePanelBrush | `#252526` |
| DockSurfaceHeaderBrush | `#3E3E42` |
| DockSurfaceSidebarBrush | `#2D2D30` |
| DockTabBackgroundBrush | `#2D2D30` |
| DockTabActiveBackgroundBrush | `#4A4A4E` |
| DockTabForegroundBrush | `#D4D4D4` |
| DockTabSelectedForegroundBrush | `#D4D4D4` |
| DockTabActiveForegroundBrush | `#D4D4D4` |

See [validation](validation.md) for full-suite totals and skipped cases.

## Regression proof

Against the original product resources, `ShippedThemePackTests` ran 97 cases: four failed
(the two dark-pack guards and two chrome-palette cases), 93 passed.
`MainWindowPanesUseThePacksBackground` failed for both packs on all three measured pane colours.
`DrawnChromeMeetsContrastThresholds` ran 14 cases: the four added Abyss/Dark cases
failed, and the ten existing high-contrast cases passed. That chrome run built only the test project
(`BuildProjectReferences=false`), using the existing assembly with the parent-revision packs
before rebuilding the edited JSON resources. The baseline effective
placeholder contrast was 1.95:1 and thumb/track contrast 1.45:1.
Thus ten cases demonstrated the regression before the fix; the initial probe setup
failures and diagnostic-runner failure do not count toward that total.

## Reproduce

From the repository root, after building the test project:

```sh
export PATH="$(printf '%s' "$PATH" | tr ':' '\n' | grep -v '^/mnt/' | paste -sd:)"
export UseSharedCompilation=false MSBUILDDISABLENODEREUSE=1
mkdir -p artifacts/4234/runtime
chmod 700 artifacts/4234/runtime
export XDG_RUNTIME_DIR="$PWD/artifacts/4234/runtime"
export HEXIDE_THEME_EVIDENCE="$PWD/artifacts/4234/render"
pwsh scripts/test.ps1 IDE/HexIDE.Integration.Tests/ --no-build -- --filter-class '*DarkThemeRenderTests' --hangdump --hangdump-timeout 3m
```

The runtime directory keeps .NET's named mutex files inside the writable sandbox.
`HEXIDE_THEME_EVIDENCE` is optional; without it the tests only assert their readings.
Placeholders use full opacity in both packs so their effective text also meets 4.5:1.

For a baseline run, keep the tests but restore the two pack JSON files to the parent
revision, rebuild, and expect both render cases, four new unit cases and four added placeholder/thumb cases to fail.

## Supervisor live check

Switch between Abyss, Dark and Classic with Project Explorer, Properties and Toolbox
open. Check the pane backgrounds, raised controls, borders and scrollbar thumbs; open
Options and a save prompt, and inspect Translation Editor rows, including selected
rows. Verify text remains legible and that theme switches update already-open surfaces.

## Possible drift for review

Reasoned from source, not rendered: the supervisor comment asks for every mapped Simple/Dock
brush in Abyss. `ThemeService.BuildDictionary` puts those brushes in an unscoped application
resource dictionary. `VBTextBox.StyleKeyOverride` uses Simple's `TextBox` style, while
`FormEditView` pins its canvas to a Light `ThemeVariantScope`. That scope selects a variant;
it does not isolate application-level brush overrides. Embedded control chrome may therefore
pick up the new navy brushes even though the form background remains bound to `Form.BackColor`.
This may head away from the theme-packs specification's requirement that the canvas and its
controls retain the form's own colours. The requested palette edit is reversible and is kept.
The supervisor should compare a designer/runtime TextBox with scrollbars under Classic and
Abyss, checking borders, selection and scrollbar colours as well as the saved BackColor/ForeColor.
