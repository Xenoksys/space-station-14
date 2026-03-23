// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using System.Numerics;

namespace Content.Server.SS220.Shitspawn.AshDrake;

public sealed class AshDrakeGreatFireballLavaTrailSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AshDrakeGreatFireballLavaTrailComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Velocity == Vector2.Zero)
                continue;

            var xform = Transform(uid);
            var curPos = _transform.GetWorldPosition(xform);
            var newPos = curPos + comp.Velocity * frameTime;

            if (HitsWall(xform, newPos))
            {
                ExplodeAt(uid, xform);
                continue;
            }

            _transform.SetWorldPosition(xform, newPos);

            comp.TimeSinceLastLava += frameTime;
            if (comp.TimeSinceLastLava >= AshDrakeGreatFireballLavaTrailComponent.LavaTrailInterval)
            {
                comp.TimeSinceLastLava = 0f;
                SpawnLavaAt(uid, xform);
            }
        }
    }

    private bool HitsWall(TransformComponent xform, Vector2 newPos)
    {
        if (xform.GridUid == null || !TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return false;

        var tileIndices = grid.TileIndicesFor(new MapCoordinates(newPos, xform.MapID));
        var tileRef = grid.GetTileRef(tileIndices);

        return tileRef.Tile.IsEmpty || _turf.IsTileBlocked(tileRef, CollisionGroup.Impassable);
    }

    private void ExplodeAt(EntityUid uid, TransformComponent xform)
    {
        if (Deleted(uid))
            return;

        Spawn("AshDrakeGreatFireballExplosion", xform.Coordinates);
        Del(uid);
    }

    private void SpawnLavaAt(EntityUid uid, TransformComponent xform)
    {
        if (xform.GridUid == null || !TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var pos = _transform.GetWorldPosition(xform);
        var center = grid.TileIndicesFor(new MapCoordinates(pos, xform.MapID));

        for (var x = -1; x <= 1; x++)
        for (var y = -1; y <= 1; y++)
        {
            var offset = new Vector2i(center.X + x, center.Y + y);
            Spawn("AshDrakeFlyLava", grid.GridTileToLocal(offset));
        }
    }
}
