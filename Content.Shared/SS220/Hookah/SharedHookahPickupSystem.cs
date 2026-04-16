using Content.Shared.SS220.Hookah.Components;
using Content.Shared.Item;

namespace Content.Shared.SS220.Hookah;

public sealed class SharedHookahPickupSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HookahPickupComponent, GettingPickedUpAttemptEvent>(OnPickupAttempt);
    }

    private void OnPickupAttempt(Entity<HookahPickupComponent> ent, ref GettingPickedUpAttemptEvent args)
    {
        if (!ent.Comp.PickupAuthorized)
            args.Cancel();
    }
}
