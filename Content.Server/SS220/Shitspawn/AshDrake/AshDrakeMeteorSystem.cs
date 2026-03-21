// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.Actions;
using Content.Shared.SS220.Shitspawn.AshDrake;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System;
using System.Numerics;

namespace Content.Server.SS220.Shitspawn.AshDrake;

public sealed class AshDrakeMeteorSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private readonly List<(TimeSpan SpawnAt, MapCoordinates Target, float SpawnHeight, float Speed)> _pending = new();
    private readonly List<(EntityUid Uid, Vector2 Target, float Speed)> _falling = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AshDrakeMeteorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AshDrakeMeteorComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<AshDrakeMeteorComponent, AshDrakeMeteorActionEvent>(OnMeteorAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pending.Count == 0 && _falling.Count == 0)
            return;

        var now = _timing.CurTime;

        for (var i = _pending.Count - 1; i >= 0; i--)
        {
            var (spawnAt, target, spawnHeight, speed) = _pending[i];
            if (now < spawnAt) continue;
            _pending.RemoveAt(i);

            var startPos = new MapCoordinates(target.X, target.Y + spawnHeight, target.MapId);
            var visual = Spawn("AshDrakeFireMeteorFalling", startPos);
            _falling.Add((visual, new Vector2(target.X, target.Y), speed));
            Spawn("AshDrakeFireMeteorMarker", target);
        }

        for (var i = _falling.Count - 1; i >= 0; i--)
        {
            var (uid, target, speed) = _falling[i];
            if (!Exists(uid)) { _falling.RemoveAt(i); continue; }

            var xform = Transform(uid);
            var cur = _transform.GetWorldPosition(xform);
            var dir = target - cur;

            if (dir.Length() <= speed * frameTime)
            {
                _transform.SetWorldPosition(xform, target);
                _falling.RemoveAt(i);
                continue;
            }

            _transform.SetWorldPosition(xform, cur + Vector2.Normalize(dir) * speed * frameTime);
        }
    }

    private void OnMapInit(EntityUid uid, AshDrakeMeteorComponent comp, MapInitEvent args)
        => _actions.AddAction(uid, ref comp.ActionEntity, comp.ActionId);

    private void OnShutdown(EntityUid uid, AshDrakeMeteorComponent comp, ComponentShutdown args)
        => _actions.RemoveAction(uid, comp.ActionEntity);

    private void OnMeteorAction(EntityUid uid, AshDrakeMeteorComponent comp, AshDrakeMeteorActionEvent args)
    {
        if (args.Handled) return;
        args.Handled = true;

        _audio.PlayPvs(comp.Sound, uid);

        var mapCoords = _transform.GetMapCoordinates(uid);
        var now = _timing.CurTime;

        for (var i = 0; i < comp.Count; i++)
        {
            var pos = new MapCoordinates(
                mapCoords.X + _random.NextFloat(-comp.Radius, comp.Radius),
                mapCoords.Y + _random.NextFloat(-comp.Radius, comp.Radius),
                mapCoords.MapId);
            _pending.Add((now + TimeSpan.FromSeconds(i * comp.SpawnInterval), pos, comp.SpawnHeight, comp.Speed));
        }
    }
}
