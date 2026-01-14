namespace Ara3D.Geometry
{
    public enum StandardPlane
    {
        Xy,
        Xz,
        Yz,
    }

    public partial struct Vector3
    {
        public Vector3 NormalizedCross(Vector3 other)
            => Vector3.Cross(this, other).Normalize;
    }

    // TODO: many of these functions should live in other places, particular in the math 3D
    public static class GeometryUtil
    {
        public const float AngleTolerance = MathF.PI / 1000;

        public const float NumberTolerance = float.Epsilon * 10;

        public static IReadOnlyList<Vector3> Normalize(this IReadOnlyList<Vector3> vectors)
            => vectors.Map(v => v.Normalize);

        public static IReadOnlyList<Vector3> Rotate(this IReadOnlyList<Vector3> self, Vector3 axis, float angle)
            => self.Transform(Matrix4x4.CreateFromAxisAngle(axis, angle));

        public static IReadOnlyList<Vector3> Transform(this IReadOnlyList<Vector3> self, Matrix4x4 matrix)
            => self.Map(x => x.Transform(matrix));

        public static Integer3 Sort(this Integer3 v) =>
            v.A < v.B
                ? (v.B < v.C)
                    ? (v.A, v.B, v.C)
                    : (v.A < v.C)
                        ? (v.A, v.C, v.B)
                        : (v.C, v.A, v.B)
                : (v.A < v.C)
                    ? (v.B, v.A, v.C)
                    : (v.B < v.C)
                        ? (v.B, v.C, v.A)
                        : (v.C, v.B, v.A);

        // Fins the intersection between two lines.
        // Returns true if they intersect
        // References:
        // https://www.codeproject.com/Tips/862988/Find-the-Intersection-Point-of-Two-Line-Segments
        // https://gist.github.com/unitycoder/10241239e080720376830f84511ccd3c
        // https://en.m.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line
        // https://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines
        public static bool Intersection(this Line2D line1, Line2D line2, out Vector2 point, float epsilon = 0.000001f)
        {

            var x1 = line1.A.X;
            var y1 = line1.A.Y;
            var x2 = line1.B.X;
            var y2 = line1.B.Y;
            var x3 = line2.A.X;
            var y3 = line2.A.Y;
            var x4 = line2.B.X;
            var y4 = line2.B.Y;

            var denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            if (denominator.Abs < epsilon)
            {
                point = Vector2.Zero;
                return false;
            }

            var num1 = (x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4);
            var num2 = (x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3);
            var t1 = num1 / denominator;
            var t2 = -num2 / denominator;
            var p1 = line1.Lerp(t1);
            var p2 = line2.Lerp(t2);
            point = p1.Average(p2);

            return true;
        }

        // Returns the distance between two lines
        // t and u are the distances if the intersection points along the two lines 
        public static float LineLineDistance(Line2D line1, Line2D line2, out float t, out float u, float epsilon = 0.0000001f)
        {
            var x1 = line1.A.X;
            var y1 = line1.A.Y;
            var x2 = line1.B.X;
            var y2 = line1.B.Y;
            var x3 = line2.A.X;
            var y3 = line2.A.Y;
            var x4 = line2.B.X;
            var y4 = line2.B.Y;

            var denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            if (denominator.Abs >= epsilon)
            {
                // Lines are not parallel, they should intersect nicely
                var num1 = (x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4);
                var num2 = (x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3);

                t = num1 / denominator;
                u = -num2 / denominator;

                var e = 0.0;
                if (t >= -e && t <= 1.0 + e && u >= -e && u <= 1.0 + e)
                {
                    t = float.Clamp(t, 0.0f, 1.0f);
                    u = float.Clamp(t, 0.0f, 1.0f);
                    return 0;
                }
            }

            // Parallel or non intersecting lines - default to point to line checks

            u = 0.0f;
            var minDistance = Distance(line1, line2.A, out t);
            var distance = Distance(line1, line2.B, out var amount);
            if (distance < minDistance)
            {
                minDistance = distance;
                t = amount;
                u = 1.0f;
            }

            distance = Distance(line2, line1.A, out amount);
            if (distance < minDistance)
            {
                minDistance = distance;
                u = amount;
                t = 0.0f;
            }

            distance = Distance(line2, line1.B, out amount);
            if (distance < minDistance)
            {
                minDistance = distance;
                u = amount;
                t = 1.0f;
            }

            return minDistance;
        }

        // Returns the distance between a line and a point.
        // t is the distance along the line of the closest point
        public static float Distance(this Line2D line, Point2D p, out float t)
        {
            var (a, b) = line;

            // Return minimum distance between line segment vw and point p
            var l2 = (a - b).LengthSquared; // i.e. |w-v|^2 -  avoid a sqrt
            if (l2 == 0.0f) // v == w case
            {
                t = 0.5f;
                return (p - a).Length;
            }

            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line. 
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
            // We clamp t from [0,1] to handle points outside the segment vw.
            t = ((p - a).Dot(b - a) / l2).Clamp(0.0f, 1.0f);
            var closestPoint = a + t * (b - a); // Projection falls on the segment
            return (p - closestPoint).Length;
        }

        public static Number Unlerp(this Number value, Number min, Number max)
        {
            var d = max - min;
            if (d.AlmostZero)
                return 0.5f; // If min and max are equal, return 0.5 to avoid division by zero.
            return (value - min) / (max - min);
        }

        public static Vector3 InverseLerp(this Vector3 v, Vector3 min, Vector3 max)
            => (v.X.Unlerp(min.X, max.X),
                v.Y.Unlerp(min.Y, max.Y),
                v.Z.Unlerp(min.Z, max.Z));

        public static Vector3 InverseLerp(this Point3D point, Bounds3D bounds)
            => InverseLerp(point, bounds.Min, bounds.Max);

        public static Vector3 WithComponent(this Vector3 self, Integer component, Number value)
        {
            if (component == 0) return self.WithX(value);
            if (component == 1) return self.WithY(value);
            if (component == 2) return self.WithZ(value);
            throw new IndexOutOfRangeException();
        }

        public static Vector3 AxisVector(this int i)
            => AxisVector((Integer)i);

        public static Vector3 AxisVector(this Integer i)
            => Vector3.Zero.WithComponent(i, 1);

        public static IReadOnlyList<Point3D> GetPoints(this IReadOnlyList<Triangle3D> self)
        {
            var points = new List<Point3D>(self.Count * 3);
            foreach (var triangle in self)
                points.AddRange(triangle.Points);
            return points;
        }

        public static IReadOnlyList<Integer3> GetFaces(this IReadOnlyList<Triangle3D> self)
        {
            var faces = new List<Integer3>(self.Count);
            for (var i = 0; i < self.Count; i++)
                faces.Add((i * 3, i * 3 + 1, i * 3 + 2));
            return faces;
        }

        public static TriangleMesh3D ToTriangleMesh3D(this IReadOnlyList<Triangle3D> self)
            => new(self.GetPoints(), self.GetFaces());

        public static IReadOnlyList<Integer> CornerIndices(this LineMesh3D self)
            => self.FaceIndices.FlatMap(x => (x.A, x.B));

        public static IReadOnlyList<Integer> CornerIndices(this TriangleMesh3D self)
            => self.FaceIndices.FlatMap(x => (x.A, x.B, x.C));

        public static IReadOnlyList<Integer> CornerIndices(this QuadMesh3D self)
            => self.FaceIndices.FlatMap(x => (x.A, x.B, x.C, x.D));

        public static Quad3D Inset(this Quad3D q, float amount)
            => (q.A.Lerp(q.Center, amount), q.B.Lerp(q.Center, amount),
                q.C.Lerp(q.Center, amount), q.D.Lerp(q.Center, amount));

        public static Vector3 Bilinear(this Quad3D q, Vector2 uv)
        {
            var ab = q.A.Lerp(q.B, uv.X);
            var dc = q.D.Lerp(q.C, uv.X);
            return ab.Lerp(dc, uv.Y);
        }

        public static Quad3D RemapQuad(this Quad3D q, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
            => (Bilinear(q, a), Bilinear(q, b), Bilinear(q, c), Bilinear(q, d));

        public static Quad3D InsetQuad(this Quad3D q, float x0, float x1, float y0, float y1)
            => RemapQuad(q, (x0, y0), (1f - x1, y0), (1f - x1, 1f - y1), (x0, 1f - y1));

        public static Quad3D InsetQuadAbs(this Quad3D q, float inset)
            => InsetQuadAbs(q, inset, inset, inset, inset);

        public static float AbsToRel(this Vector3 a, Vector3 b, float f)
            => f / (b - a).Length;

        public static float AbsToRel(this Point3D a, Point3D b, float f)
            => f / (b - a).Length;

        public static Quad3D InsetQuadAbs(this Quad3D q, float x0Abs, float x1Abs, float y0Abs, float y1Abs)
        {
            var ab = AbsToRel(q.A, q.B, x0Abs);
            var ba = AbsToRel(q.B, q.A, x1Abs);

            var cd = AbsToRel(q.C, q.D, x0Abs);
            var dc = AbsToRel(q.D, q.C, x1Abs);

            var ad = AbsToRel(q.A, q.D, y0Abs);
            var da = AbsToRel(q.D, q.A, y1Abs);

            var bc = AbsToRel(q.B, q.C, y0Abs);
            var cb = AbsToRel(q.C, q.B, y1Abs);

            var x0 = (ab + dc) / 2f;
            var x1 = (ba + cd) / 2f;
            var y0 = (ad + bc) / 2f;
            var y1 = (da + cb) / 2f;

            return InsetQuad(q, x0, x1, y0, y1);
        }

        public static Quad3D GetQuad(this IReadOnlyList<Point3D> points, Integer4 face)
            => (points[face.A], points[face.B], points[face.C], points[face.D]);

        public static Triangle3D GetTriangle(this IReadOnlyList<Point3D> points, Integer3 face)
            => (points[face.A], points[face.B], points[face.C]);

        public static Quad3D PushQuad(this Quad3D q, float distance)
            => q.Translate(q.Normal * distance);

        public static Integer4 InsertFace(this QuadMesh3DBuilder self, Integer4 f, Quad3D q)
        {
            var n = self.Points.Count;
            self.Points.AddRange([q.A, q.B, q.C, q.D]);
            var f0 = new Integer4(f.A, f.B, n + 1, n);
            var f1 = new Integer4(f.B, f.C, n + 2, n + 1);
            var f2 = new Integer4(f.C, f.D, n + 3, n + 2);
            var f3 = new Integer4(f.D, f.A, n, n + 3);
            var f4 = new Integer4(n, n + 1, n + 2, n + 3);
            self.Faces.AddRange([f0, f1, f2, f3, f4]);
            return f4;
        }

        public static Integer4 ExtrudeFace(this QuadMesh3DBuilder self, Integer4 f, float amount)
            => self.InsertFace(f, PushQuad(self.Points.GetQuad(f), amount));

        public static void DeleteLastFace(this QuadMesh3DBuilder self)
            => self.Faces.RemoveAt(self.Faces.Count - 1);

        public static Quad3D GetLastQuad(this QuadMesh3DBuilder self)
            => self.Points.GetQuad(self.Faces[^1]);

        public static QuadGrid3D Subdivide(this Quad3D q, int xSegments, int ySegments)
        {
            var leftSidePoints = q.A.Sample(q.D, ySegments);
            var rightSidePoints = q.B.Sample(q.C, ySegments);
            var rows = ySegments.MapRange(i => leftSidePoints[i].Sample(rightSidePoints[i], xSegments));
            return rows.RowsToArray().ToQuadGrid3D(false, false);
        }

        public static QuadMesh3D ToQuadMesh3D(this IReadOnlyList<Quad3D> quads)
            => new(quads.SelectMany(q => q.Points).ToList(),
                quads.Count.MapRange(i => new Integer4(i * 4, i * 4 + 1, i * 4 + 2, i * 4 + 3)));
    }
}
