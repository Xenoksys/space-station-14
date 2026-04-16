using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.SS220.Hookah.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Localization;

namespace Content.Server.SS220.Hookah;

public sealed class HookahAssemblySystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HookahFlaskComponent, InteractUsingEvent>(OnFlaskInteract);
        SubscribeLocalEvent<HookahPartialComponent, InteractUsingEvent>(OnPartialInteract);
        SubscribeLocalEvent<HookahPartialFullComponent, InteractUsingEvent>(OnPartialFullInteract);
    }

    private void OnFlaskInteract(Entity<HookahFlaskComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !HasComp<HookahShaftComponent>(args.Used))
            return;

        Assemble(ent, args.Used, "HookahPartial", ent.Comp.AssemblySound);
        _popup.PopupEntity(Loc.GetString("hookah-assembly-stage1"), args.User, args.User);
        args.Handled = true;
    }

    private void OnPartialInteract(Entity<HookahPartialComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !HasComp<HookahSaucerComponent>(args.Used))
            return;

        Assemble(ent, args.Used, "HookahPartialFull", ent.Comp.AssemblySound);
        _popup.PopupEntity(Loc.GetString("hookah-assembly-stage2"), args.User, args.User);
        args.Handled = true;
    }

    private void OnPartialFullInteract(Entity<HookahPartialFullComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !HasComp<HookahTubePartComponent>(args.Used))
            return;

        Assemble(ent, args.Used, "HookahBase", ent.Comp.AssemblySound);
        _popup.PopupEntity(Loc.GetString("hookah-assembly-complete"), args.User, args.User);
        args.Handled = true;
    }

    private void Assemble(EntityUid target, EntityUid used, string resultProto, SoundSpecifier sound)
    {
        var coords = Transform(target).Coordinates;
        var result = Spawn(resultProto, coords);
        _audio.PlayPvs(sound, result);
        QueueDel(target);
        QueueDel(used);
    }
}
