// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.SS220.Shitspawn.AshDrake;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AshDrakeSwoopComponent : Component
{
    [DataField] public string SwoopPolymorphId = "AshDrakeSwoopPolymorph";
    [DataField] public EntProtoId LavaProto = "FloorLavaEntity";
    [DataField] public int LavaRadius = 3;
    [DataField] public float LavaChance = 0.5f;

    [DataField] public EntProtoId ActionId = "ActionAshDrakeSwoop";
    [DataField, AutoNetworkedField] public EntityUid? ActionEntity;
}

public sealed partial class AshDrakeSwoopActionEvent : InstantActionEvent { }
