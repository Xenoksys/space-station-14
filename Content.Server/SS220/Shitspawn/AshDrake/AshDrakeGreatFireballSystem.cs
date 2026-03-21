// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.Actions;
using Content.Shared.Item;
using Content.Shared.SS220.Shitspawn.AshDrake;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Content.Shared.Physics;
using Robust.Shared.Physics.Components;
using System.Numerics;

namespace Content.Server.SS220.Shitspawn.AshDrake;

public sealed class AshDrakeGreatFireballSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private const float LavaTrailInterval = 0.2f;
    private const float CollisionCheckInterval = 0.05f;

    private readonly List<(EntityUid Uid, Vector2 Velocity, EntityUid Shooter, float TimeSinceLastLava, float TimeSinceLastCheck)> _active = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AshDrakeGreatFireballComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AshDrakeGreatFireballComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<AshDrakeGreatFireballComponent, AshDrakeGreatFireballActionEvent>(OnFireballAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_active.Count == 0)
            return;

        for (var i = _active.Count - 1; i >= 0; i--)
        {
            var (uid, vel, shooter, lavaTimer, checkTimer) = _active[i];

            if (!Exists(uid))
            {
                _active.RemoveAt(i);
                continue;
            }

            var xform = Transform(uid);
            var cur = _transform.GetWorldPosition(xform);
            var next = cur + vel * frameTime;

            checkTimer += frameTime;
            if (checkTimer >= CollisionCheckInterval)
            {
                checkTimer = 0f;

                if (HitsWall(xform, next))
                {
                    _active.RemoveAt(i);
                    Explode(uid);
                    continue;
                }

                if (HitsEntity(next, xform.MapID, uid, shooter))
                {
                    _active.RemoveAt(i);
                    Explode(uid);
                    continue;
                }
            }

            _transform.SetWorldPosition(xform, next);

            lavaTimer += frameTime;
            if (lavaTimer >= LavaTrailInterval)
            {
                lavaTimer = 0f;
                SpawnLavaAt(xform, cur);
            }

            _active[i] = (uid, vel, shooter, lavaTimer, checkTimer);
        }
    }

    private void OnMapInit(EntityUid uid, AshDrakeGreatFireballComponent comp, MapInitEvent args)
        => _actions.AddAction(uid, ref comp.ActionEntity, comp.ActionId);

    private void OnShutdown(EntityUid uid, AshDrakeGreatFireballComponent comp, ComponentShutdown args)
        => _actions.RemoveAction(uid, comp.ActionEntity);

    private void OnFireballAction(EntityUid uid, AshDrakeGreatFireballComponent comp, AshDrakeGreatFireballActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        _audio.PlayPvs(comp.Sound, uid);

        var drakeXform = Transform(uid);
        var drakePos = _transform.GetWorldPosition(drakeXform);
        var targetCoords = _transform.ToMapCoordinates(args.Target);
        var targetPos = new Vector2(targetCoords.X, targetCoords.Y);

        var dir = targetPos - drakePos;
        if (dir.LengthSquared() < 0.01f)
            return;

        var velocity = Vector2.Normalize(dir) * comp.FireballSpeed;
        var spawnPos = new MapCoordinates(drakePos + Vector2.Normalize(dir) * 1.5f, drakeXform.MapID);
        var fireball = Spawn(comp.FireballProto, spawnPos);

        _transform.SetWorldRotation(fireball, new Angle(velocity));

        _active.Add((fireball, velocity, uid, LavaTrailInterval, 0f));
    }

    private void Explode(EntityUid uid)
    {
        var coords = Transform(uid).Coordinates;
        Spawn("AshDrakeGreatFireballExplosion", coords);
        Del(uid);
    }

    private void SpawnLavaAt(TransformComponent xform, Vector2 pos)
    {
        if (xform.GridUid == null)
            return;

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var center = grid.TileIndicesFor(new MapCoordinates(pos, xform.MapID));

        for (var x = -1; x <= 1; x++)
        for (var y = -1; y <= 1; y++)
        {
            var offset = new Vector2i(center.X + x, center.Y + y);
            Spawn("AshDrakeFlyLava", grid.GridTileToLocal(offset));
        }
    }

    private bool HitsWall(TransformComponent xform, Vector2 pos)
    {
        if (xform.GridUid != null && TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            var tile = grid.GetTileRef(grid.TileIndicesFor(new MapCoordinates(pos, xform.MapID)));
            if (tile.Tile.IsEmpty)
                return true;
        }

        var nearby = _lookup.GetEntitiesInRange(new MapCoordinates(pos, xform.MapID), 0.4f);
        foreach (var ent in nearby)
        {
            if (!TryComp<PhysicsComponent>(ent, out var body)) continue;
            if (!body.Hard || body.BodyType != BodyType.Static) continue;
            if ((body.CollisionLayer & (int) CollisionGroup.Impassable) != 0)
                return true;
        }

        return false;
    }

    private bool HitsEntity(Vector2 pos, MapId mapId, EntityUid fireball, EntityUid shooter)
    {
        var nearby = _lookup.GetEntitiesInRange(new MapCoordinates(pos, mapId), 0.5f);

        foreach (var ent in nearby)
        {
            if (ent == shooter || ent == fireball) continue;
            if (HasComp<ItemComponent>(ent)) continue;
            if (TryComp<PhysicsComponent>(ent, out var body) &&
                body.Hard && body.BodyType != BodyType.Static)
                return true;
        }

        return false;
    }
}
