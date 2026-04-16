using Content.Shared.Containers.ItemSlots;

namespace Content.Shared.SS220.Hookah.Components;

[RegisterComponent]
public sealed partial class SmokingFuelComponent : Component
{
    public const string TobaccoSlotId = "tobacco_slot";

    [DataField("tobacco_slot")]
    public ItemSlot TobaccoSlot = new();

    [DataField]
    public int TobaccoPuffs;

    [DataField]
    public string TobaccoId = "LeavesTobaccoDried";

    [DataField]
    public int PuffsPerPack = 20;

    public float CoalTime;
}
