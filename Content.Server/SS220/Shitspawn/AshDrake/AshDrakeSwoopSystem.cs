// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Content.Shared.SS220.Shitspawn.AshDrake;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server.SS220.Shitspawn.AshDrake;

public sealed class AshDrakeSwoopSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AshDrakeSwoopComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AshDrakeSwoopComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<AshDrakeSwoopComponent, AshDrakeSwoopActionEvent>(OnSwoopAction);
        SubscribeLocalEvent<AshDrakeSwoopComponent, PolymorphedEvent>(OnPolymorphed);
    }

    private void OnMapInit(EntityUid uid, AshDrakeSwoopComponent comp, MapInitEvent args)
        => _actions.AddAction(uid, ref comp.ActionEntity, comp.ActionId);

    private void OnShutdown(EntityUid uid, AshDrakeSwoopComponent comp, ComponentShutdown args)
        => _actions.RemoveAction(uid, comp.ActionEntity);

    private void OnSwoopAction(EntityUid uid, AshDrakeSwoopComponent comp, AshDrakeSwoopActionEvent args)
    {
        if (args.Handled) return;
        args.Handled = true;
        _polymorph.PolymorphEntity(uid, new ProtoId<PolymorphPrototype>(comp.SwoopPolymorphId));
    }

    private void OnPolymorphed(EntityUid uid, AshDrakeSwoopComponent comp, PolymorphedEvent args)
    {
        if (!args.IsRevert) return;

        var xform = Transform(args.NewEntity);
        if (xform.GridUid == null) return;
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid)) return;

        var coords = _transform.GetMapCoordinates(args.NewEntity);
        var tilePos = grid.TileIndicesFor(coords);

        for (var x = -comp.LavaRadius; x <= comp.LavaRadius; x++)
        {
            for (var y = -comp.LavaRadius; y <= comp.LavaRadius; y++)
            {
                if (!_random.Prob(comp.LavaChance)) continue;
                var offset = new Vector2i(tilePos.X + x, tilePos.Y + y);
                Spawn(comp.LavaProto, grid.GridTileToLocal(offset));
            }
        }
    }
}
