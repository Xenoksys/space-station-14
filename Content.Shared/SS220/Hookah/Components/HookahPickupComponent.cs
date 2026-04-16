using Robust.Shared.GameStates;

namespace Content.Shared.SS220.Hookah.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HookahPickupComponent : Component
{
    [DataField]
    public float PickupDelay = 3f;

    [NonSerialized]
    public bool PickupAuthorized;
}
