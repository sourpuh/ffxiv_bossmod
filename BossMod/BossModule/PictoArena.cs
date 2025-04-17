using ImGuiNET;
using Pictomancy;

namespace BossMod;

public sealed class PictoArena(BossModuleConfig config, WPos center, ArenaBounds bounds)
{
    public readonly BossModuleConfig Config = config;
    private WPos _center = center;
    private readonly TriangulationCache _triCache = new();

    public WPos Center
    {
        get => _center;
        set
        {
            if (_center != value)
            {
                _center = value;
                _triCache.Invalidate();
            }
        }
    }

    public ArenaBounds Bounds
    {
        get;
        set
        {
            if (!ReferenceEquals(field, value))
            {
                field = value;
                _triCache.Invalidate();
            }
        }
    } = bounds;

    public float ScreenHalfSize => 150 * Config.ArenaScale;
    public float FloorHeight = 0;

    public bool InBounds(WPos position) => Bounds.Contains(position - Center);
    public WPos ClampToBounds(WPos position) => Center + Bounds.ClampToBounds(position - Center);
    public float IntersectRayBounds(WPos rayOrigin, WDir rayDir) => Bounds.IntersectRay(rayOrigin - Center, rayDir);

    public string DrawCacheStats() => _triCache.Stats();

    // prepare for drawing - set up internal state, clip rect etc.
    public void Begin(Angle cameraAzimuth)
    {
        if (Bounds.ScreenHalfSize != ScreenHalfSize)
        {
            Bounds.ScreenHalfSize = ScreenHalfSize;
            _triCache.Invalidate();
        }
        else
        {
            _triCache.NextFrame();
        }
        FloorHeight = Service.ClientState.LocalPlayer?.Position.Y ?? 0;
    }

    // unclipped primitive rendering that accept world-space positions; thin convenience wrappers around drawlist api
    public void AddLine(WPos a, WPos b, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        var drawlist = PictoService.GetDrawList();

        drawlist.PathLineTo(a.ToVec3(FloorHeight));
        drawlist.PathLineTo(b.ToVec3(FloorHeight));
        drawlist.PathStroke(color != 0 ? color : ArenaColor.Danger, thickness: thickness);
    }

    public void AddTriangle(WPos p1, WPos p2, WPos p3, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        var drawlist = PictoService.GetDrawList();

        drawlist.PathLineTo(p1.ToVec3(FloorHeight));
        drawlist.PathLineTo(p2.ToVec3(FloorHeight));
        drawlist.PathLineTo(p3.ToVec3(FloorHeight));
        drawlist.PathStroke(color != 0 ? color : ArenaColor.Danger, PctStrokeFlags.Closed, thickness: thickness);
    }

    public void AddTriangleFilled(WPos p1, WPos p2, WPos p3, uint color)
    {
        PictoService.GetDrawList().AddTriangleFilled(p1.ToVec3(FloorHeight), p2.ToVec3(FloorHeight), p3.ToVec3(FloorHeight), color != 0 ? color : ArenaColor.Danger);
    }

    public void AddQuad(WPos p1, WPos p2, WPos p3, WPos p4, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        PictoService.GetDrawList().AddQuad(p1.ToVec3(FloorHeight), p2.ToVec3(FloorHeight), p3.ToVec3(FloorHeight), p4.ToVec3(FloorHeight), color != 0 ? color : ArenaColor.Danger, thickness: thickness);
    }

    public void AddQuadFilled(WPos p1, WPos p2, WPos p3, WPos p4, uint color)
    {
        PictoService.GetDrawList().AddQuadFilled(p1.ToVec3(FloorHeight), p2.ToVec3(FloorHeight), p3.ToVec3(FloorHeight), p4.ToVec3(FloorHeight), color != 0 ? color : ArenaColor.Danger);
    }

    public void AddRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        var side = halfWidth * direction.OrthoR();
        var front = origin + lenFront * direction;
        var back = origin - lenBack * direction;
        AddQuad(front + side, front - side, back - side, back + side, color, thickness);
    }

    public void AddCircle(WPos center, float radius, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        PictoService.GetDrawList().AddCircle(center.ToVec3(FloorHeight), radius, color != 0 ? color : ArenaColor.Danger, thickness: thickness);
    }

    public void AddCircleFilled(WPos center, float radius, uint color)
    {
        PictoService.GetDrawList().AddCircleFilled(center.ToVec3(FloorHeight), radius, color != 0 ? color : ArenaColor.Danger);
    }

    public void AddCone(WPos center, float radius, Angle centerDirection, Angle halfAngle, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        float sDir = -centerDirection.Rad;
        PictoService.GetDrawList().AddFan(center.ToVec3(FloorHeight), 0, radius, sDir - halfAngle.Rad, sDir + halfAngle.Rad, color != 0 ? color : ArenaColor.Danger, thickness: thickness);
    }

    public void AddDonutCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        float sDir = -centerDirection.Rad;
        PictoService.GetDrawList().AddFan(center.ToVec3(FloorHeight), innerRadius, outerRadius, sDir - halfAngle.Rad, sDir + halfAngle.Rad, color != 0 ? color : ArenaColor.Danger, thickness: thickness);
    }

    public void AddPolygon(ReadOnlySpan<WPos> vertices, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        foreach (var p in vertices)
            PathLineTo(p);
        PathStroke(true, color != 0 ? color : ArenaColor.Danger, thickness);
    }

    public void AddPolygon(IEnumerable<WPos> vertices, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        foreach (var p in vertices)
            PathLineTo(p);
        PathStroke(true, color != 0 ? color : ArenaColor.Danger, thickness);
    }

    public void AddPolygonTransformed(WPos center, WDir rotation, ReadOnlySpan<WDir> vertices, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        foreach (var p in vertices)
            PathLineTo(center + p.Rotate(rotation));
        PathStroke(true, color != 0 ? color : ArenaColor.Danger, thickness);
    }

    public void AddComplexPolygon(WPos center, WDir rotation, RelSimplifiedComplexPolygon poly, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;

        foreach (var part in poly.Parts)
        {
            AddPolygonTransformed(center, rotation, part.Exterior, color, thickness);
            foreach (var h in part.Holes)
                AddPolygonTransformed(center, rotation, part.Interior(h), color, thickness);
        }
    }

    // path api: add new point to path; this adds new edge from last added point, or defines first vertex if path is empty
    public void PathLineTo(WPos p)
    {
        PictoService.GetDrawList().PathLineTo(p.ToVec3(FloorHeight));
    }

    // adds a bunch of points corresponding to arc - if path is non empty, this adds an edge from last point to first arc point
    public void PathArcTo(WPos center, float radius, float amin, float amax)
    {
        PictoService.GetDrawList().PathArcTo(center.ToVec3(FloorHeight), radius, amin, amax);
    }

    public void PathStroke(bool closed, uint color, float thickness = 1)
    {
        thickness *= Config.ThicknessScale;
        PictoService.GetDrawList().PathStroke(color != 0 ? color : ArenaColor.Danger, closed ? PctStrokeFlags.Closed : PctStrokeFlags.None, thickness);
    }

    public void PathFillConvex(uint color)
    {
        // No support :(
        // ImGui.GetWindowDrawList().PathFillConvex(color != 0 ? color : ArenaColor.Danger);
    }

    // draw clipped & triangulated zone
    public void Zone(List<RelTriangle> triangulation, uint color)
    {
        var drawlist = PictoService.GetDrawList();
        foreach (var tri in triangulation)
            drawlist.AddTriangleFilled((tri.A + Center).ToVec3(FloorHeight), (tri.B + Center).ToVec3(FloorHeight), (tri.C + Center).ToVec3(FloorHeight), color != 0 ? color : ArenaColor.AOE);
    }

    // draw zones - these are filled primitives clipped to arena border; note that triangulation is cached
    public void ZoneCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color)
        => Zone(_triCache[(1, center, innerRadius, outerRadius, centerDirection, halfAngle)] ??= Bounds.ClipAndTriangulateCone(center - Center, innerRadius, outerRadius, centerDirection, halfAngle), color);
    public void ZoneCircle(WPos center, float radius, uint color)
        => Zone(_triCache[(2, center, radius)] ??= Bounds.ClipAndTriangulateCircle(center - Center, radius), color);
    public void ZoneDonut(WPos center, float innerRadius, float outerRadius, uint color)
        => Zone(_triCache[(3, center, innerRadius, outerRadius)] ??= Bounds.ClipAndTriangulateDonut(center - Center, innerRadius, outerRadius), color);
    public void ZoneTri(WPos a, WPos b, WPos c, uint color)
        => Zone(_triCache[(4, a, b, c)] ??= Bounds.ClipAndTriangulateTri(a - Center, b - Center, c - Center), color);
    public void ZoneIsoscelesTri(WPos apex, WDir height, WDir halfBase, uint color)
        => Zone(_triCache[(5, apex, height, halfBase)] ??= Bounds.ClipAndTriangulateIsoscelesTri(apex - Center, height, halfBase), color);
    public void ZoneIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height, uint color)
        => Zone(_triCache[(6, apex, direction, halfAngle, height)] ??= Bounds.ClipAndTriangulateIsoscelesTri(apex - Center, direction, halfAngle, height), color);
    public void ZoneRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color)
        => Zone(_triCache[(7, origin, direction, lenFront, lenBack, halfWidth)] ??= Bounds.ClipAndTriangulateRect(origin - Center, direction, lenFront, lenBack, halfWidth), color);
    public void ZoneRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth, uint color)
        => Zone(_triCache[(8, origin, direction, lenFront, lenBack, halfWidth)] ??= Bounds.ClipAndTriangulateRect(origin - Center, direction, lenFront, lenBack, halfWidth), color);
    public void ZoneRect(WPos start, WPos end, float halfWidth, uint color)
        => Zone(_triCache[(9, start, end, halfWidth)] ??= Bounds.ClipAndTriangulateRect(start - Center, end - Center, halfWidth), color);
    public void ZoneComplex(WPos origin, Angle direction, RelSimplifiedComplexPolygon poly, uint color)
        => Zone(_triCache[(10, origin, direction, poly)] ?? Bounds.ClipAndTriangulate(poly.Transform(origin - Center, direction.ToDirection())), color);
    public void ZonePoly(object key, IEnumerable<WPos> contour, uint color)
        => Zone(_triCache[(11, key)] ??= Bounds.ClipAndTriangulate(contour.Select(p => p - Center)), color);
    public void ZoneRelPoly(object key, IEnumerable<WDir> relContour, uint color)
        => Zone(_triCache[(12, key)] ??= Bounds.ClipAndTriangulate(relContour), color);

    public void TextScreen(Vector2 center, string text, uint color, float fontSize = 17)
    {
        var size = ImGui.CalcTextSize(text) * Config.ArenaScale;
        ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), fontSize * Config.ArenaScale, center - size / 2, color, text);
    }

    public void TextWorld(WPos center, string text, uint color, float fontSize = 17)
    {
        PictoService.GetDrawList().AddText(center.ToVec3(FloorHeight), color, text, fontSize / 17);
    }

    // high level utilities
    // draw arena border
    public void Border(uint color)
    {
        var dl = PictoService.GetDrawList();
        foreach (var p in Bounds.ShapeSimplified.Parts)
        {
            foreach (var off in p.Exterior)
                dl.PathLineTo((Center + off).ToVec3(FloorHeight));
            dl.PathStroke(color, PctStrokeFlags.Closed, 2);

            foreach (var i in p.Holes)
            {
                foreach (var off in p.Interior(i))
                    dl.PathLineTo((Center + off).ToVec3(FloorHeight));
                dl.PathStroke(color, PctStrokeFlags.Closed, 2);
            }
        }
    }

    public void CardinalNames()
    {
        //var offS = new(0, offCenter);
        //var offE = new(offCenter, 0);
        //TextScreen(offS, "N", ArenaColor.Border, Config.CardinalsFontSize);
        //TextScreen(-offS, "S", ArenaColor.Border, Config.CardinalsFontSize);
        //TextScreen(offE, "E", ArenaColor.Border, Config.CardinalsFontSize);
        //TextScreen(-offE, "W", ArenaColor.Border, Config.CardinalsFontSize);
    }

    // draw actor representation
    public void ActorInsideBounds(WPos position, Angle rotation, uint color)
    {
        var dir = rotation.ToDirection();
        var normal = dir.OrthoR();
        if (Config.ShowOutlinesAndShadows)
            AddTriangle(position + 0.7f * dir, position - 0.35f * dir + 0.433f * normal, position - 0.35f * dir - 0.433f * normal, 0xFF000000, 2);
        AddTriangleFilled(position + 0.7f * dir, position - 0.35f * dir + 0.433f * normal, position - 0.35f * dir - 0.433f * normal, color);
    }

    public void ActorOutsideBounds(WPos position, Angle rotation, uint color)
    {
        var dir = rotation.ToDirection();
        var normal = dir.OrthoR();
        AddTriangle(position + 0.7f * dir, position - 0.35f * dir + 0.433f * normal, position - 0.35f * dir - 0.433f * normal, color);
    }

    public void ActorProjected(WPos from, WPos to, Angle rotation, uint color)
    {
        if (InBounds(to))
        {
            // projected position is inside bounds
            ActorInsideBounds(to, rotation, color);
            return;
        }

        var dir = to - from;
        var l = dir.Length();
        if (l == 0)
            return; // can't determine projection direction

        dir /= l;
        var t = IntersectRayBounds(from, dir);
        if (t < l)
            ActorOutsideBounds(from + t * dir, rotation, color);
    }

    public void Actor(WPos position, Angle rotation, uint color)
    {
        if (InBounds(position))
            ActorInsideBounds(position, rotation, color);
        else
            ActorOutsideBounds(ClampToBounds(position), rotation, color);
    }

    public void Actor(Actor? actor, uint color, bool allowDeadAndUntargetable = false)
    {
        if (actor != null && !actor.IsDestroyed && (allowDeadAndUntargetable || actor.IsTargetable && !actor.IsDead))
            Actor(actor.Position, actor.Rotation, color);
    }

    public void Actors(IEnumerable<Actor> actors, uint color, bool allowDeadAndUntargetable = false)
    {
        foreach (var a in actors)
            Actor(a, color, allowDeadAndUntargetable);
    }

    public void End()
    {
        //ImGui.GetWindowDrawList().PopClipRect();
    }
}
