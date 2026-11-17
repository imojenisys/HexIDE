# macOS icon fixtures

Apple's own encoding of HexIDE's 16- and 32-pixel icon images, for testing the icon writer's ARGB elements (#4493).

| File | What it is |
|---|---|
| `stock-16.png`, `stock-32.png` | The 16- and 32-pixel PNGs stored in stock's `IDE/HexIDE/Icons/AppIcon/hexide.icns` (its `icp4` and `icp5`), unchanged. |
| `apple-iconutil-ic04-ic05.icns` | The `ic04` and `ic05` elements of the icon Apple's `iconutil -c icns` wrote from those images and the other eight sizes, with every other element removed. |

Made on macOS 15.5 (24F74), M4, on 2026-11-17. `iconutil` wrote these elements in this order: `ic12 ic07 ic13 ic08 ic04 ic14 ic09 ic05 ic10 ic11 info`. Its 16- and 32-pixel elements are `ic04` and `ic05`, and it wrote no `icp4` or `icp5`.

Each of `ic04` and `ic05` holds `ARGB`, then the alpha, red, green and blue channels in that order, `size × size` bytes each, run-length packed. A control byte below `0x80` copies the next `c + 1` bytes. One of `0x80` or above repeats the next byte `c − 0x80 + 3` times. Alpha is straight, not premultiplied. Decoded that way, both match the PNGs above: alpha exactly, and colour within 1 on every visible pixel.
