"""Build joined static M5 study surfaces. Requires numpy, scipy and scikit-image.

Metres, Y up, face toward -Z. Source OBJs have outward normals; Unity consumes
them through build-art03-m5-refined.cs. These are smooth study meshes, not
deformation-ready topology. No raster-image processing is performed here.
"""
from pathlib import Path
import json
import numpy as np
from scipy.interpolate import PchipInterpolator
from scipy.ndimage import gaussian_filter, map_coordinates
from skimage.measure import marching_cubes

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "art-source/characters/m5/refined"
OUT.mkdir(parents=True, exist_ok=True)


def smooth_union(a, b, width):
    h = np.maximum(width - np.abs(a - b), 0) / width
    return np.minimum(a, b) - h * h * width * 0.25


def profile(x, y, z, rows, cx=0.0):
    rows = np.asarray(rows)
    cy = np.clip(y, rows[0, 0], rows[-1, 0])
    rx = PchipInterpolator(rows[:, 0], rows[:, 1])(cy)
    rz = PchipInterpolator(rows[:, 0], rows[:, 2])(cy)
    radial = (np.sqrt(((x-cx)/rx)**2 + (z/rz)**2) - 1) * np.minimum(rx, rz)
    axial = np.maximum(rows[0, 0]-y, y-rows[-1, 0])
    return np.maximum(radial, axial)


def ellipsoid(x, y, z, center, radii):
    q = [x-center[0], y-center[1], z-center[2]]
    k0 = np.sqrt(sum((p/r)**2 for p, r in zip(q, radii)))
    k1 = np.sqrt(sum((p/r**2)**2 for p, r in zip(q, radii)))
    return k0*(k0-1)/np.maximum(k1, 1e-8)


def shirt(x, y, z):
    body = profile(x, y, z, [
        (.94,.103,.067),(.96,.107,.070),(1.12,.105,.068),
        (1.28,.117,.075),(1.345,.132,.075),(1.375,.099,.062),
        (1.397,.038,.036)])
    for sign in [-1, 1]:
        center = sign*(.14 + (1.29-y)*.10)
        sleeve = profile(x, y, z, [
            (1.20,.044,.045),(1.24,.046,.047),
            (1.31,.047,.049),(1.35,.033,.040),(1.365,.014,.02)], center)
        body = smooth_union(body, sleeve, .042)
    return body


def trousers(x, y, z):
    pelvis = profile(x, y, z, [
        (.858,.090,.049),(.885,.104,.062),(.945,.104,.065),(.96,.101,.063)])
    for sign in [-1, 1]:
        leg = profile(x,y,z,[
            (.092,.038,.041),(.12,.040,.043),(.43,.035,.040),
            (.63,.038,.044),(.81,.046,.052),(.94,.051,.058)], sign*.063)
        pelvis = smooth_union(pelvis, leg, .023)
    return pelvis


HEAD_ROWS = [
    (1.46,.018,.025),(1.48,.039,.041),(1.53,.065,.061),
    (1.61,.084,.079),(1.72,.103,.091),(1.83,.116,.097),
    (1.90,.106,.089),(1.94,.073,.063),(1.956,.001,.001)]


def head_neck(x,y,z):
    head = profile(x,y,z,HEAD_ROWS)
    neck = profile(x,y,z,[
        (1.369,.043,.036),(1.39,.037,.032),(1.43,.029,.028),
        (1.46,.031,.030),(1.492,.04,.036)])
    return smooth_union(head, neck, .018)


def arm_hand(x,y,z,sign):
    center=sign*(.147 + (1.20-y)*.075)
    arm=profile(x,y,z,[
        (.805,.020,.022),(.84,.021,.022),(.96,.020,.022),
        (1.08,.023,.025),(1.24,.024,.026)],center)
    hand=ellipsoid(x,y,z,(sign*.176,.80,-.005),(.027,.051,.023))
    thumb=ellipsoid(x,y,z,(sign*.155,.808,-.022),(.012,.027,.013))
    return smooth_union(smooth_union(arm,hand,.016),thumb,.014)


def save(name, field, low, high, spacing=.011):
    axes=[np.arange(a,b+spacing,spacing,dtype=np.float32) for a,b in zip(low,high)]
    xyz=np.meshgrid(*axes,indexing="ij")
    volume=gaussian_filter(field(*xyz).astype(np.float32),sigma=.55)
    verts,faces,_,_=marching_cubes(volume,0,spacing=(spacing,)*3,allow_degenerate=False)
    gradient=np.gradient(volume,spacing)
    coords=(verts/spacing).T
    normals=np.column_stack([map_coordinates(g,coords,order=1) for g in gradient])
    normals/=np.maximum(np.linalg.norm(normals,axis=1,keepdims=True),1e-9)
    verts+=np.array(low)
    a,b,c=verts[faces[:,0]],verts[faces[:,1]],verts[faces[:,2]]
    if np.sum(np.einsum("ij,ij->i",a,np.cross(b,c)))<0:
        faces=faces[:,::-1]
    # Require a closed surface, finite geometry and outward faces before export.
    edges=np.sort(np.concatenate([faces[:,[0,1]],faces[:,[1,2]],faces[:,[2,0]]]),axis=1)
    _,counts=np.unique(edges,axis=0,return_counts=True)
    if not np.all(counts==2) or not np.isfinite(verts).all():
        raise RuntimeError(f"{name}: open or invalid mesh")
    with (OUT/f"{name}.obj").open("w",encoding="ascii",newline="\n") as out:
        out.write(f"# M5 joined static surface: {name}; Y up; metres; front -Z\n")
        for v in verts: out.write("v "+" ".join(f"{c:.7f}" for c in v)+"\n")
        for n in normals: out.write("vn "+" ".join(f"{c:.7f}" for c in n)+"\n")
        for f in faces+1: out.write("f "+" ".join(f"{i}//{i}" for i in f)+"\n")
    return dict(mesh=name,vertices=len(verts),triangles=len(faces),closed=True,
                bounds_min=verts.min(0).tolist(),bounds_max=verts.max(0).tolist())


results=[
    save("JoinedShirt",shirt,(-.23,.91,-.11),(.23,1.43,.11)),
    save("JoinedTrousers",trousers,(-.14,.065,-.10),(.14,.99,.10)),
    save("JoinedHeadNeck",head_neck,(-.14,1.34,-.12),(.14,1.98,.12),.007),
]
for sign,name in [(-1,"Left"),(1,"Right")]:
    results.append(save(name+"JoinedArmHand",lambda x,y,z: arm_hand(x,y,z,sign),
                        (-.23,.73,-.07),(.23,1.27,.07),.006))
def hair_cap():
    sides, rows = 64, 20
    head=np.asarray(HEAD_ROWS)
    rx=PchipInterpolator(head[:,0],head[:,1])
    rz=PchipInterpolator(head[:,0],head[:,2])
    vertices=[]
    for row in range(rows+1):
        for j in range(sides):
            a=j*np.pi*2/sides
            bottom=1.841-.058*np.sin(a)+.013*np.cos(a)
            y=bottom+(1.955-bottom)*row/rows
            vertices.append([np.cos(a)*(float(rx(y))+.002),
                             y+.001, np.sin(a)*(float(rz(y))+.002)])
    faces=[]
    for row in range(rows):
        for j in range(sides):
            a=row*sides+j; b=row*sides+(j+1)%sides
            faces.extend([[a,a+sides,b],[b,a+sides,b+sides]])
    cap=len(vertices);vertices.append([0,1.959,0])
    for j in range(sides):
        faces.append([cap,rows*sides+(j+1)%sides,rows*sides+j])
    v=np.array(vertices);f=np.array(faces)
    normals=np.zeros_like(v)
    face_normals=np.cross(v[f[:,1]]-v[f[:,0]],v[f[:,2]]-v[f[:,0]])
    for corner in range(3):np.add.at(normals,f[:,corner],face_normals)
    normals/=np.maximum(np.linalg.norm(normals,axis=1,keepdims=True),1e-9)
    with (OUT/"SmoothHairCap.obj").open("w",encoding="ascii",newline="\n") as out:
        out.write("# M5 hair shell, intentionally open at scalp boundary\n")
        for p in v:out.write("v "+" ".join(f"{c:.7f}" for c in p)+"\n")
        for n in normals:out.write("vn "+" ".join(f"{c:.7f}" for c in n)+"\n")
        for face in f+1:out.write("f "+" ".join(f"{i}//{i}" for i in face)+"\n")
    return dict(mesh="SmoothHairCap",vertices=len(v),triangles=len(f),
                closed=False,note="Scalp shell with intentional open lower boundary")

results.append(hair_cap())
(OUT/"geometry-checks.json").write_text(json.dumps(results,indent=2)+"\n",encoding="utf-8")
print(json.dumps(results))
