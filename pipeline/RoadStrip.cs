using System.Globalization;
using System.Numerics;
using System.Text.Json;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

namespace DrivingTime.Pipeline;

public sealed record RoadStripConfiguration(int SchemaVersion, string GraphPath, string StagePath, string SegmentId,
    double StartMetres, double LengthMetres, Origin Origin, Bounds Region, double FlatLocalHeight,
    Dictionary<string, double> WidthFallbacks, double DefaultWidthMetres, double MinimumEdgeMetres, double MaxJoinRatio);
public sealed record StripMesh(Vector3[] Vertices, int[] Indices, double WidthMetres, string WidthBasis,
    GraphPoint[] Centerline, string[] Assumptions);

public static class RoadStrip
{
    public static StripMesh Generate(GraphSegment segment, RoadStripConfiguration c)
    {
        if (c.SchemaVersion != 1 || c.Origin is null || c.Region is null || c.WidthFallbacks is null)
            throw new ArgumentException("Strip requires schemaVersion=1, origin, region and widthFallbacks.");
        CoordinateFrame.Finite(c.StartMetres,c.LengthMetres,c.FlatLocalHeight,c.DefaultWidthMetres,c.MinimumEdgeMetres,c.MaxJoinRatio,
            c.Origin.East,c.Origin.North,c.Origin.Height,c.Region.MinEast,c.Region.MinNorth,c.Region.MaxEast,c.Region.MaxNorth);
        if (c.StartMetres <= 0 || c.LengthMetres <= 0 || c.LengthMetres > 200 || c.MinimumEdgeMetres < 0.001 || c.MinimumEdgeMetres > 1 ||
            c.MaxJoinRatio < 1 || c.MaxJoinRatio > 4 || c.DefaultWidthMetres <= 0 || c.DefaultWidthMetres > 20 ||
            c.WidthFallbacks.Values.Any(w => !double.IsFinite(w) || w <= 0 || w > 20) ||
            c.Region.MinEast >= c.Region.MaxEast || c.Region.MinNorth >= c.Region.MaxNorth)
            throw new ArgumentException("Require interior start >0, length (0,200], edge tolerance [0.001,1], join ratio [1,4], widths (0,20] and positive region.");
        if (segment.Tags.GetValueOrDefault("bridge", "no") != "no" || segment.Tags.GetValueOrDefault("tunnel", "no") != "no" ||
            segment.Tags.GetValueOrDefault("layer", "0") != "0" || segment.Tags.ContainsKey("junction") ||
            segment.Tags.GetValueOrDefault("highway") is "construction" or "proposed")
            throw new ArgumentException("Choose a ground-level segment without junction/bridge/tunnel/construction tags.");
        var notes = new List<string> { "Flat local Y is configured, synthetic; no measured road elevation or vertical datum conversion.",
            "Asphalt appearance is provisional; no lane markings, shoulders or junction surfaces.",
            "Interior of one P11 segment only; name-filtered graph cannot establish absence of unobserved junctions/access restrictions." };
        var width = c.DefaultWidthMetres; var basis = "configured default";
        if (c.WidthFallbacks.TryGetValue(segment.Tags.GetValueOrDefault("highway", ""), out var fallback))
        { width = fallback; basis = "configured highway fallback: " + segment.Tags.GetValueOrDefault("highway"); }
        if (segment.Tags.TryGetValue("width", out var tag))
        {
            if (double.TryParse(tag, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed) && parsed > 0 && parsed <= 20)
            { width = parsed; basis = "OSM width tag in metres (not surveyed)"; }
            else notes.Add("Unsupported width tag '" + tag + "'; used fallback. Only positive decimal metres <=20 accepted.");
        }
        notes.Add($"Width {width.ToString("R",CultureInfo.InvariantCulture)} m from {basis}; lanes/parking do not imply carriageway width.");
        var clean = new List<GraphPoint>(); var removed = 0;
        foreach (var p in segment.Geometry)
        {
            CoordinateFrame.Finite(p.East,p.North,p.SourceIndex);
            if (clean.Count == 0 || Distance(clean[^1],p) >= c.MinimumEdgeMetres) clean.Add(p); else removed++;
        }
        notes.Add($"Removed {removed} duplicate/near-zero consecutive points before distance cutting.");
        var line = new List<GraphPoint>(); var distance = 0d; var end = c.StartMetres+c.LengthMetres;
        for (var i=1;i<clean.Count;i++)
        {
            var a=clean[i-1]; var b=clean[i]; var len=Distance(a,b);
            var lo=Math.Max(c.StartMetres,distance); var hi=Math.Min(end,distance+len);
            GraphPoint At(double d) { var t=(d-distance)/len; return t==0 ? a : t==1 ? b : new(a.East+t*(b.East-a.East),a.North+t*(b.North-a.North),null,a.SourceIndex+t*(b.SourceIndex-a.SourceIndex)); }
            if (hi>lo) { if(line.Count==0) line.Add(At(lo)); line.Add(At(hi)); }
            distance+=len;
        }
        if (end >= distance-c.MinimumEdgeMetres || line.Count<2)
            throw new ArgumentException("Selected cut must end strictly inside the segment; shorten length/start or choose a longer segment.");
        // Remove tiny remnants introduced by cutting while preserving both configured endpoints.
        for(var i=line.Count-2;i>0;i--) if(Distance(line[i],line[i+1])<c.MinimumEdgeMetres || Distance(line[i-1],line[i])<c.MinimumEdgeMetres) line.RemoveAt(i);
        var vertices=new List<Vector3>(); var indices=new List<int>(); var capped=0; var half=width/2;
        for(var i=0;i<line.Count;i++)
        {
            (double X,double Z) Direction(int a,int b) { var len=Distance(line[a],line[b]); return ((line[b].East-line[a].East)/len,-(line[b].North-line[a].North)/len); }
            var prev=Direction(Math.Max(0,i-1), i==0 ? 1 : i);
            var next=Direction(i==line.Count-1 ? i-1 : i, Math.Min(line.Count-1,i+1));
            var nx=-prev.Z-next.Z; var nz=prev.X+next.X; var norm=double.Hypot(nx,nz);
            if(norm<0.02) throw new ArgumentException("Near reversal cannot form a simple strip; select a section without a hairpin.");
            nx/=norm; nz/=norm; var dot=nx*(-next.Z)+nz*next.X;
            var wanted=half/dot; var limit=half*c.MaxJoinRatio;
            // Limit the tangential excursion too, preventing join inversion on short adjacent edges.
            var tangent=Math.Abs(nx*next.X+nz*next.Z);
            if(i>0 && i<line.Count-1 && tangent>1e-9)
                limit=Math.Min(limit,0.45*Math.Min(Distance(line[i-1],line[i]),Distance(line[i],line[i+1]))/tangent);
            var extent=Math.Min(wanted,limit); if(extent<wanted-1e-8)capped++;
            foreach(var sign in new[]{1,-1})
            {
                var east=line[i].East+sign*nx*extent; var north=line[i].North-sign*nz*extent;
                if(east<c.Region.MinEast || east>c.Region.MaxEast || north<c.Region.MinNorth || north>c.Region.MaxNorth)
                    throw new ArgumentException("Road surface exceeds configured region; choose a contained section/width.");
                var v=new Vector3((float)(east-c.Origin.East),(float)c.FlatLocalHeight,(float)-(north-c.Origin.North));
                if(!float.IsFinite(v.X)||!float.IsFinite(v.Y)||!float.IsFinite(v.Z))throw new ArgumentException("Localized float mesh exceeds representable coordinates.");
                vertices.Add(v);
            }
        }
        // A capped join can make a quad concave: choose its interior diagonal.
        for(var i=0;i<line.Count-1;i++)
        {
            var a=2*i; var b=a+1; var d=a+2; var e=a+3;
            if(Area(a,d,b)>1e-8f && Area(b,d,e)>1e-8f) { Triangle(a,d,b); Triangle(b,d,e); }
            else { Triangle(a,d,e); Triangle(a,e,b); }
        }
        float Area(int a,int b,int d)=>Vector3.Cross(vertices[b]-vertices[a],vertices[d]-vertices[a]).Y;
        void Triangle(int a,int b,int d)
        {
            var area=Area(a,b,d);
            if(!float.IsFinite(area)||area<=1e-8f)throw new ArgumentException("Degenerate/inverted float triangle; change section or origin (hairpins unsupported).");
            indices.AddRange([a,b,d]);
        }
        notes.Add($"Capped {capped} joins at {c.MaxJoinRatio.ToString("R",CultureInfo.InvariantCulture)} half-widths and 45% of adjacent edge length; capped bends deliberately narrow instead of spiking.");
        return new(vertices.ToArray(),indices.ToArray(),width,basis,line.ToArray(),notes.ToArray());
    }
    private static double Distance(GraphPoint a,GraphPoint b)=>double.Hypot(b.East-a.East,b.North-a.North);

    public static byte[] Glb(StripMesh strip, bool collision)
    {
        var name=collision ? "RoadCollision" : "RoadAsphalt";
        var mesh=new MeshBuilder<VertexPositionNormal>(name);
        var material=new MaterialBuilder(name+"Material").WithMetallicRoughnessShader()
            .WithChannelParam(KnownChannel.BaseColor,KnownProperty.RGBA,collision ? new Vector4(0.1f,0.6f,0.9f,1) : new Vector4(0.055f,0.055f,0.055f,1))
            .WithChannelParam(KnownChannel.MetallicRoughness,KnownProperty.MetallicFactor,0f)
            .WithChannelParam(KnownChannel.MetallicRoughness,KnownProperty.RoughnessFactor,0.95f);
        var primitive=mesh.UsePrimitive(material);
        for(var i=0;i<strip.Indices.Length;i+=3)
            primitive.AddTriangle(new VertexPositionNormal(strip.Vertices[strip.Indices[i]],Vector3.UnitY),new VertexPositionNormal(strip.Vertices[strip.Indices[i+1]],Vector3.UnitY),new VertexPositionNormal(strip.Vertices[strip.Indices[i+2]],Vector3.UnitY));
        var scene=new SceneBuilder();
        // Godot's colonly suffix generates a StaticBody3D/concave collision, no visible mesh.
        scene.AddRigidMesh(mesh,new NodeBuilder(collision ? "RoadCollision-colonly" : name));
        return scene.ToGltf2().WriteGLB().ToArray();
    }
}

public static class RoadStripCommand
{
    public static int Run(string[] args)
    {
        try
        {
            if(args is not ["--config",_])throw new ArgumentException("road-strip --config <file.json> (offline only)");
            var path=Path.GetFullPath(args[1]); var configBytes=File.ReadAllBytes(path);
            var c=JsonSerializer.Deserialize<RoadStripConfiguration>(configBytes,RoadSample.Json) ?? throw new ArgumentException("Null strip config.");
            string Resolve(string p)=>string.IsNullOrWhiteSpace(p) ? throw new ArgumentException("Graph/stage paths must be nonempty.") : Path.GetFullPath(p,Path.GetDirectoryName(path)!);
            var input=Resolve(c.GraphPath); var stage=Resolve(c.StagePath);
            var outputs=new[]{"road.glb","collision.glb","manifest.json"}.Select(p=>Path.Combine(stage,p)).ToArray();
            if(input==path || outputs.Contains(input) || outputs.Contains(path))throw new ArgumentException("Input/config/output paths must differ.");
            var bytes=RoadSampleCommand.ReadBounded(File.OpenRead(input)).GetAwaiter().GetResult();
            var graph=JsonSerializer.Deserialize<IntermediateRoadGraph>(bytes,RoadSample.Json) ?? throw new ArgumentException("Null graph.");
            if(graph.SchemaVersion!=1 || graph.HorizontalCrs!="EPSG:3067" || graph.Units!="metre")throw new ArgumentException("Requires P11 graph v1 EPSG:3067 metres.");
            var segment=graph.Segments.SingleOrDefault(s=>s.Id==c.SegmentId) ?? throw new ArgumentException("segmentId absent from graph; regenerate P11/check selection.");
            var strip=RoadStrip.Generate(segment,c); var road=RoadStrip.Glb(strip,false); var collision=RoadStrip.Glb(strip,true);
            var min=strip.Vertices.Aggregate(Vector3.Min); var max=strip.Vertices.Aggregate(Vector3.Max);
            var manifest=new {schemaVersion=1, kind="flat-road-strip",units="metres",horizontalCrs=graph.HorizontalCrs,
                axes="X=east, Y=up, Z=south; localized; identity transforms", c.Origin,c.Region,c.SegmentId,segment.OsmWayId,
                c.StartMetres,c.LengthMetres,c.FlatLocalHeight,verticalDatum="synthetic flat height; not N2000 road elevation",
                strip.WidthMetres,strip.WidthBasis,c.MinimumEdgeMetres,c.MaxJoinRatio,
                min=new EnginePoint(min.X,min.Y,min.Z),max=new EnginePoint(max.X,max.Y,max.Z),triangles=strip.Indices.Length/3,
                assets=new[]{new {path="road.glb",sha256=RoadSample.Hash(road),role="visual"},new {path="collision.glb",sha256=RoadSample.Hash(collision),role="static concave collision; Godot colonly"}},
                centerline=strip.Centerline,assumptions=strip.Assumptions,tags=segment.Tags.OrderBy(p=>p.Key,StringComparer.Ordinal).ToDictionary(p=>p.Key,p=>p.Value),
                graphSha256=RoadSample.Hash(bytes),configurationSha256=RoadSample.Hash(configBytes),graph.DatumPolicy,graph.Provenance,
                limitations="No terrain, sidewalks, curbs, lane markings, junctions or physical access validation; hairpins rejected. Single open collision surface, for static use only."};
            var json=JsonSerializer.Serialize(manifest,RoadSample.Json)+"\n";
            Directory.CreateDirectory(stage); File.WriteAllBytes(outputs[0],road); File.WriteAllBytes(outputs[1],collision);File.WriteAllText(outputs[2],json);
            Console.WriteLine($"Strip: {c.SegmentId}; {c.LengthMetres} m; width {strip.WidthMetres} m ({strip.WidthBasis}); {strip.Indices.Length/3} triangles. Staged {stage}");
            foreach(var note in strip.Assumptions)Console.WriteLine(note);
            return 0;
        }
        catch(Exception e) when(e is ArgumentException or IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or KeyNotFoundException or NullReferenceException)
        { Console.Error.WriteLine("Road strip error: "+e.Message);Console.Error.WriteLine("Check P11 graph and strip config; no retrieval attempted.");return 2; }
    }
}
