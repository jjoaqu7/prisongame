"""Download unmodified official Steam gallery references and retain attribution."""
import concurrent.futures
import json
from pathlib import Path
import urllib.request

ROOT = Path(__file__).resolve().parents[1] / 'docs' / 'art-references' / 'character-styles'
GAMES = [('tabs', 508440), ('peak', 3527290), ('human-fall-flat', 477160), ('gang-beasts', 285900)]

def fetch(game):
    slug, appid = game
    endpoint = f'https://store.steampowered.com/api/appdetails?appids={appid}&l=english'
    with urllib.request.urlopen(endpoint, timeout=30) as response:
        payload = json.load(response)
        if str(appid) not in payload:
            raise RuntimeError(f'Steam returned mismatched app IDs for {appid}; refusing to misattribute images')
        result = payload[str(appid)]
    if not result.get('success'):
        raise RuntimeError(f'Steam returned no data for {appid}')
    data = result['data']
    records = []
    shots = data['screenshots'][:4] if slug in ('peak', 'tabs') else data['screenshots'][:2]
    for shot in shots:
        dest = ROOT / f'{slug}-{shot["id"]}.jpg'
        if not dest.exists():
            with urllib.request.urlopen(shot['path_full'], timeout=30) as response:
                dest.write_bytes(response.read())
        records.append({'file': dest.name, 'image_url': shot['path_full']})
    return {'game': data['name'], 'developer': data.get('developers'), 'publisher': data.get('publishers'),
            'store_url': f'https://store.steampowered.com/app/{appid}/', 'screenshots': records}

if __name__ == '__main__':
    ROOT.mkdir(parents=True, exist_ok=True)
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        records = list(pool.map(fetch, GAMES))
    (ROOT / 'sources.json').write_text(json.dumps(records, indent=2), encoding='utf-8')
    print(json.dumps(records, indent=2))
