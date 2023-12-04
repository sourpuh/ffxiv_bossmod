using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{

    public interface IArena
    {
        public BossModuleConfig Config { get; init; }
        public ArenaBounds Bounds { get; set; }
        public float ScreenHalfSize => 150 * Config.ArenaScale;
        public float ScreenMarginSize => 20 * Config.ArenaScale;

        public Vector2 ScreenCenter { get; }


        public void Begin(float cameraAzimuthRadians);


        public Vector2 WorldPositionToScreenPosition(WPos p);
        public Vector2 RotatedCoords(Vector2 coords);

        public void AddLine(WPos a, WPos b, uint color, float thickness = 1);

        public void AddTriangle(WPos p1, WPos p2, WPos p3, uint color, float thickness = 1);

        public void AddTriangleFilled(WPos p1, WPos p2, WPos p3, uint color);

        public void AddQuad(WPos p1, WPos p2, WPos p3, WPos p4, uint color, float thickness = 1);

        public void AddQuadFilled(WPos p1, WPos p2, WPos p3, WPos p4, uint color);

        public void AddCircle(WPos center, float radius, uint color, float thickness = 1);

        public void AddCircleFilled(WPos center, float radius, uint color);

        public void AddCone(WPos center, float radius, Angle centerDirection, Angle halfAngle, uint color, float thickness = 1);

        public void AddDonutCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color, float thickness = 1);

        public void AddPolygon(IEnumerable<WPos> vertices, uint color, float thickness = 1);

        public void PathLineTo(WPos p);

        public void PathArcTo(WPos center, float radius, float amin, float amax);

        public void PathStroke(bool closed, uint color, float thickness = 1);

        public void PathFillConvex(uint color);

        public void Zone(List<(WPos, WPos, WPos)> triangulation, uint color);

        public void ZoneCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color) => Zone(Bounds.ClipAndTriangulateCone(center, innerRadius, outerRadius, centerDirection, halfAngle), color);
        public void ZoneCircle(WPos center, float radius, uint color) => Zone(Bounds.ClipAndTriangulateCircle(center, radius), color);
        public void ZoneDonut(WPos center, float innerRadius, float outerRadius, uint color) => Zone(Bounds.ClipAndTriangulateDonut(center, innerRadius, outerRadius), color);
        public void ZoneTri(WPos a, WPos b, WPos c, uint color) => Zone(Bounds.ClipAndTriangulateTri(a, b, c), color);
        public void ZoneIsoscelesTri(WPos apex, WDir height, WDir halfBase, uint color) => Zone(Bounds.ClipAndTriangulateIsoscelesTri(apex, height, halfBase), color);
        public void ZoneIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height, uint color) => Zone(Bounds.ClipAndTriangulateIsoscelesTri(apex, direction, halfAngle, height), color);
        public void ZoneRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color) => Zone(Bounds.ClipAndTriangulateRect(origin, direction, lenFront, lenBack, halfWidth), color);
        public void ZoneRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth, uint color) => Zone(Bounds.ClipAndTriangulateRect(origin, direction, lenFront, lenBack, halfWidth), color);
        public void ZoneRect(WPos start, WPos end, float halfWidth, uint color) => Zone(Bounds.ClipAndTriangulateRect(start, end, halfWidth), color);

        public void TextScreen(Vector2 center, string text, uint color, float fontSize = 17);

        public void TextWorld(WPos center, string text, uint color, float fontSize = 17);

        public void Border(uint color);

        public void CardinalNames();

        public void Actor(WPos position, Angle rotation, uint color);

        public void Actor(Actor? actor, uint color, bool allowDeadAndUntargetable = false);

        public void Actors(IEnumerable<Actor> actors, uint color, bool allowDeadAndUntargetable = false);

        public void End();
    }
}
