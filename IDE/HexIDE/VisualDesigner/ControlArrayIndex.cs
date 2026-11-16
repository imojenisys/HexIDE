using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HexIDE.Localization;
using HexIDE.Runtime.Components;

namespace HexIDE.VisualDesigner;

// Measured file-load rules: #2701 / PR #4074. These are HexIDE's design-time refusals, not VB6 IDE prompts.
public static class ControlArrayIndex
{
    public sealed record Refusal(string Key, string English, string? Control = null)
    {
        public string Localized(ILocalizationService localization) =>
            string.Format(localization.GetString(Key), Control)
            + " (docs/vb6-fidelity-oracle/control-arrays.md; #2701 / PR #4074)";
    }

    public static Refusal? RefusalFor(ComponentInstance target, string value, IEnumerable<ComponentInstance> controls)
    {
        const string source = " See docs/vb6-fidelity-oracle/control-arrays.md (measurements in #2701 / PR #4074). Nothing was changed.";
        var siblings = controls.Where(c => !ReferenceEquals(c, target) && string.Equals(
            c.GetPropertyOrDefault(VBProperties.NameProperty), target.GetPropertyOrDefault(VBProperties.NameProperty),
            StringComparison.OrdinalIgnoreCase)).ToList();
        if (value.Length == 0)
            return siblings.Count == 0 ? null : new("Str.ControlArray.ClearRefused",
                $"Cannot clear Index while '{PropertyText.AddressedName(siblings[0])}' shares the name. "
                + "Reasoned from the measured indexed-first .frm: VB6 logs a missing Index and loads the unindexed control outside the array, where Index raises 343; reverse order and IDE prompts are unmeasured."
                + source, PropertyText.AddressedName(siblings[0]));
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) || index < 0 || index > 32767)
            return new("Str.ControlArray.InvalidIndex",
                "Invalid property value. Index takes an integer from 0 to 32767, or empty. VB6 will not load a .frm with an out-of-range Index." + source);
        if (siblings.FirstOrDefault(c => PropertyText.ArrayIndexOf(c) == index || PropertyText.ArrayIndexOf(c) is null) is { } other)
            return new("Str.ControlArray.DuplicateIndex",
                $"Invalid property value. '{PropertyText.AddressedName(other)}' already holds this Index or has no Index. "
                + "VB6 will not load duplicate indexes; mixing indexed and unindexed controls of the same name loads them separately with a missing-Index log." + source,
                PropertyText.AddressedName(other));
        return null;
    }
}
