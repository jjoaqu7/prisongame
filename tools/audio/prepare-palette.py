"""Prepare the licensed prototype palette. Run from game root; dependencies in ignored Temp/audio-python."""
from pathlib import Path
import sys,io,zipfile,json,hashlib,re
sys.path.insert(0,str(Path('PrisonGame/Temp/audio-python').resolve()))
import soundfile as sf
import numpy as np
from scipy.signal import butter,sosfiltfilt,resample_poly
from math import gcd
ROOT=Path('audio-source'); OUT=Path('PrisonGame/Assets/Prototype/Audio01');OUT.mkdir(parents=True,exist_ok=True)
SR=44100
sources={
 'impact':{'creator':'Kenney','title':'Impact Sounds 1.0','page':'https://kenney.nl/assets/impact-sounds','file':'impact.zip'},
 'interface':{'creator':'Kenney','title':'Interface Sounds 1.0','page':'https://kenney.nl/assets/interface-sounds','file':'interface.zip'},
 'paper':{'creator':'alec_mackay','title':'paper shuffle.wav','page':'https://freesound.org/people/alec_mackay/sounds/463682/','file':'paper.mp3'},
 'room':{'creator':'leonelmail','title':'Room Tone In An Apartment Living Room 2','page':'https://freesound.org/people/leonelmail/sounds/329533/','file':'room.mp3'}}
for k,s in sources.items():
 s['license']='CC0 1.0';s['license_url']='https://creativecommons.org/publicdomain/zero/1.0/';s['sha256']=hashlib.sha256((ROOT/s['file']).read_bytes()).hexdigest()
 html=(ROOT/(k+'-source.html')).read_text(encoding='utf-8')
 pattern=r'https://cdn.freesound.org/[^"\s]+-hq.mp3' if k in ['room','paper'] else r'https://kenney.nl/[^"\s]+\.zip'
 s['download_url']=re.search(pattern,html)[0];s['format_note']='Public high-quality MP3 preview, not original WAV' if k in ['room','paper'] else 'Original creator ZIP archive'
previous=json.loads((ROOT/'manifest.json').read_text()) if (ROOT/'manifest.json').exists() else {}
manifest={'sources':sources,'outputs':[],'retrieved_utc':previous.get('retrieved_utc','See source-page snapshots')}
def read(k,member=None):
 data=io.BytesIO(zipfile.ZipFile(ROOT/sources[k]['file']).read(member)) if member else ROOT/sources[k]['file']
 x,sr=sf.read(data,always_2d=True);x=x.mean(axis=1)
 if sr!=SR:x=resample_poly(x,SR//gcd(sr,SR),sr//gcd(sr,SR))
 return x
def shape(x,peak=.45,cutoff=4500):
 x=x-np.mean(x);x=sosfiltfilt(butter(2,65,fs=SR,btype='highpass',output='sos'),x)
 x=sosfiltfilt(butter(2,cutoff,fs=SR,output='sos'),x)
 x=np.tanh(x*1.3);m=np.max(np.abs(x));x*=peak/max(m,1e-9)
 n=min(int(SR*.008),len(x)//4);x[:n]*=np.linspace(0,1,n);x[-n:]*=np.linspace(1,0,n)
 return x
def export(name,x,origin,edit):
 assert np.all(np.isfinite(x)) and np.max(np.abs(x))<.98
 path=OUT/(name+'.wav');sf.write(path,x,SR,subtype='PCM_16')
 manifest['outputs'].append({'file':str(path).replace('\\','/'),'sources':origin,'processing':edit,'seconds':round(len(x)/SR,3),'peak':round(float(np.max(np.abs(x))),4),'rms':round(float(np.sqrt(np.mean(x*x))),5),'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
 return x
palette=[]
for i in range(4):
 member=f'Audio/footstep_concrete_{i:03}.ogg';x=shape(read('impact',member),.42,3000)
 palette.append(export(f'footstep_{i+1}',x,[{'source':'impact','member':member}], 'Mono; remove DC/rumble; low-pass 3 kHz; soft saturation; peak 0.42; 8 ms edge fades'))
paper=read('paper');window=int(SR*.6); energy=np.convolve(paper[::441]**2,np.ones(60),mode='valid');start=int(np.argmax(energy))*441
paper=paper[start:start+window]
for name,peak,rate in [('pickup',.36,1),('wrap',.5,.85),('restock',.42,.95)]:
 x=np.interp(np.arange(0,len(paper),rate),np.arange(len(paper)),paper);x=shape(x,peak,4000)
 palette.append(export(name,x,[{'source':'paper','segment_seconds':[round(start/SR,3),round((start+window)/SR,3)]}],f'Mono paper excerpt; speed {rate}; HP 65 Hz / LP 4 kHz; soft saturation; fades; peak {peak}'))
for name,source,member,peak,cutoff in [
 ('place','impact','Audio/impactSoft_medium_000.ogg',.45,3200),
 ('crackers','impact','Audio/impactWood_light_001.ogg',.4,4000),
 ('fruit','impact','Audio/impactSoft_medium_001.ogg',.4,3500),
 ('door','impact','Audio/impactMetal_medium_000.ogg',.5,2400),
 ('shelf','impact','Audio/impactWood_medium_000.ogg',.5,3500),
 ('sale','interface','Audio/pluck_001.ogg',.38,3200)]:
 x=read(source,member)
 rate=.85 if name in ['door','sale'] else 1
 x=np.interp(np.arange(0,len(x),rate),np.arange(len(x)),x);x=shape(x,peak,cutoff)
 palette.append(export(name,x,[{'source':source,'member':member}],f'Mono; speed {rate}; HP 65 Hz / LP {cutoff} Hz; soft saturation; edge fades; peak {peak}'))
x=read('room')[2*SR:16*SR];x-=np.mean(x);x=sosfiltfilt(butter(2,[80,1600],fs=SR,btype='bandpass',output='sos'),x)
f=int(.75*SR);w=np.linspace(0,1,f);y=x[f:].copy();y[-f:]=x[-f:]*(1-w)+x[:f]*w;y*=min(.07/max(np.sqrt(np.mean(y*y)),1e-9),.35/max(np.abs(y)))
export('room_tone',y,[{'source':'room','segment_seconds':[2,16]}], 'Mono room recording; 80-1600 Hz bandpass; 750 ms loop overlap; RMS capped 0.07 and peak 0.35')
(ROOT/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
# A quiet preview of the prepared palette, separated by silence. Runtime mixing is quieter and spatial.
preview=np.concatenate([np.zeros(SR//2)]+[np.concatenate([x*.55,np.zeros(int(.4*SR))]) for x in palette]+[y[:3*SR]*.14])
sf.write(ROOT/'palette-preview.wav',preview,SR,subtype='PCM_16')
print(json.dumps([{'file':Path(o['file']).name,'seconds':o['seconds'],'peak':o['peak'],'rms':o['rms']} for o in manifest['outputs']],indent=2))
