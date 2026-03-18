using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.Shitspawn.SlotMachine;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlotMachineComponent : Component
{
    public const int MinBet = 100;
    public const ProtoId<StackPrototype> CreditStackId = "Credit";
    public const ProtoId<EntityPrototype> CashPrototypeId = "SpaceCash";

    [DataField, AutoNetworkedField]
    public List<string> Reels = new() { "seven", "seven", "seven" };

    [DataField]
    public List<List<string>> ReelPools = new();

    [DataField, AutoNetworkedField]
    public List<SlotMachineRule> Rules = new();

    [DataField, AutoNetworkedField]
    public int StoredCredits;

    [DataField, AutoNetworkedField]
    public int LastBet;

    [DataField, AutoNetworkedField]
    public int LastPayout;

    [AutoNetworkedField]
    public SlotMachineResult LastResult = SlotMachineResult.None;

    [DataField]
    public SoundSpecifier InsertSound = new SoundPathSpecifier("/Audio/Machines/id_insert.ogg");

    [DataField]
    public SoundSpecifier SpinSound = new SoundPathSpecifier("/Audio/SS220/Shitspawn/SlotMachine/slot_spin.ogg");

    [DataField]
    public SoundSpecifier WinSound = new SoundPathSpecifier("/Audio/SS220/Shitspawn/SlotMachine/slot_win.ogg");

    public bool HasPendingResult;
    public List<string> PendingReels = new() { "seven", "seven", "seven" };
    public SlotMachineResult PendingResult = SlotMachineResult.None;
    public int PendingPayout;
    public TimeSpan SpinEndTime;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class SlotMachineRule
{
    [DataField]
    public List<string>? Symbols;

    [DataField]
    public int? Index;

    [DataField]
    public int Multiplier;

    [DataField]
    public SlotMachineResult Result = SlotMachineResult.Triple;
}

[Serializable, NetSerializable]
public enum SlotMachineResult
{
    None,
    Lose,
    ApplePair,
    CherryPair,
    Triple,
    Triple7,
    Jackpot
}
