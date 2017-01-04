module RayTracerFSharp.Tracer

open GlmNet
open Tracer

let nonZero (vector:vec3) = vector.x <> 0.0f || vector.y <> 0.0f || vector.z <> 0.0f

let zeroVector = new vec3(0.0f, 0.0f, 0.0f)

let Trace (rayorig:vec3) (raydir:vec3) (depth:int) (sceneObjects:ResizeArray<SceneObject>) =

    let lights = sceneObjects |> Seq.filter (fun o -> o.GetMaterial(zeroVector).emissive |> nonZero)

    // Return ray as a color - scaled 0->1
    raydir * 0.5f + new vec3(0.5f, 0.5f, 0.5f)

