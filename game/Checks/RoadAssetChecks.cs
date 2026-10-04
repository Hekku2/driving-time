using Godot;
using System;
using System.Linq;
using System.Text.Json;

// Import validation only; a drivable Helsinki scene belongs to P13.
public partial class RoadAssetChecks : Node3D
{
    public override async void _Ready()
    {
        try
        {
            using var f=Godot.FileAccess.Open("res://Generated/P12/manifest.json",Godot.FileAccess.ModeFlags.Read);
            using var doc=JsonDocument.Parse(f.GetAsText());var root=doc.RootElement;
            var road=GD.Load<PackedScene>("res://Generated/P12/road.glb").Instantiate<Node3D>();AddChild(road);
            var collision=GD.Load<PackedScene>("res://Generated/P12/collision.glb").Instantiate<Node3D>();AddChild(collision);
            var meshes=road.FindChildren("*","MeshInstance3D",true,false).Cast<MeshInstance3D>().ToArray();
            Require(meshes.Length==1,"one visual mesh");Require(collision.FindChildren("*","MeshInstance3D",true,false).Count==0,"collision has no visual mesh");
            var shape=collision.FindChildren("*","CollisionShape3D",true,false).Cast<CollisionShape3D>().Single();
            Require(shape.GetParent() is StaticBody3D,"static collision body");Require(shape.Shape is ConcavePolygonShape3D,"concave triangle collision");
            Require(meshes[0].GlobalTransform==Transform3D.Identity && shape.GlobalTransform==Transform3D.Identity,"identity transforms");
            var arrays=meshes[0].Mesh.SurfaceGetArrays(0);var vertices=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();var indices=arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
            var faces=((ConcavePolygonShape3D)shape.Shape).GetFaces();var triangles=root.GetProperty("triangles").GetInt32();
            Require(indices.Length==triangles*3 && faces.Length==triangles*3,"triangle counts");
            Vector3 Point(JsonElement p)=>new((float)p.GetProperty("x").GetDouble(),(float)p.GetProperty("y").GetDouble(),(float)p.GetProperty("z").GetDouble());
            void Bounds(Vector3[] points)
            {
                var min=points.Aggregate((a,b)=>new Vector3(Math.Min(a.X,b.X),Math.Min(a.Y,b.Y),Math.Min(a.Z,b.Z)));
                var max=points.Aggregate((a,b)=>new Vector3(Math.Max(a.X,b.X),Math.Max(a.Y,b.Y),Math.Max(a.Z,b.Z)));
                Require(min.IsEqualApprox(Point(root.GetProperty("min"))) && max.IsEqualApprox(Point(root.GetProperty("max"))),"manifest metre bounds");
            }
            Bounds(vertices);Bounds(faces);
            for(var i=0;i<indices.Length;i+=3)
                Require((vertices[indices[i+1]]-vertices[indices[i]]).Cross(vertices[indices[i+2]]-vertices[indices[i]]).Y < -1e-8,"Godot clockwise nondegenerate triangles");
            await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
            await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
            var o=root.GetProperty("origin");
            var line=root.GetProperty("centerline").EnumerateArray().ToArray();
            for(var i=1;i<line.Length;i++)
            {
                // Probe interiors: exact open endpoints can round outside the surface in float physics.
                var east=(line[i-1].GetProperty("east").GetDouble()+line[i].GetProperty("east").GetDouble())/2;
                var north=(line[i-1].GetProperty("north").GetDouble()+line[i].GetProperty("north").GetDouble())/2;
                var center=new Vector3((float)(east-o.GetProperty("east").GetDouble()),(float)root.GetProperty("flatLocalHeight").GetDouble(),(float)-(north-o.GetProperty("north").GetDouble()));
                var query=PhysicsRayQueryParameters3D.Create(center+Vector3.Up,center-Vector3.Up);
                Require(GetWorld3D().DirectSpaceState.IntersectRay(query).Count>0,$"centerline interior collision ray {i}");
            }
            GD.Print($"P12 CHECKS PASS: {triangles} imported visual/collision triangles; identity, bounds, winding and centerline rays.");GetTree().Quit();
        }
        catch(Exception e){GD.PrintErr("P12 CHECKS FAILED: "+e);GetTree().Quit(1);}
    }
    private static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
}
