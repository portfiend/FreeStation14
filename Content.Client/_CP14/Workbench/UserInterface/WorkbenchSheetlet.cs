using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._CP14.Workbench.UserInterface;

[CommonSheetlet]
public sealed class WorkbenchSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        return [
            E<Button>()
                .Identifier("WorkbenchRecipeButton")
                .Class(WorkbenchRecipeButton.UncraftableClass)
                .Modulate(sheet.PrimaryPalette.DisabledElement),

            E<PanelContainer>()
                .Identifier("WorkbenchRequirementList")
                .Panel(new StyleBoxFlat(sheet.PrimaryPalette.Background)),
        ];
    }
}
