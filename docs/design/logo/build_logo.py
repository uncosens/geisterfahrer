#!/usr/bin/env python3
"""Builds the Geisterfahrer logo files (SVG + preview page).

Needs only Python 3 and two OFL fonts from Assets/ThirdParty/Fonts (get them with tools/fetch-assets.sh):
Inter-Black.ttf and BarlowSemiCondensed-SemiBold.ttf. Text is turned into outlines by a small TTF reader below.

  python build_logo.py            # writes logo-preview.html and the files of the picked candidate
"""
import math
import os
import struct
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))
FONT_DIR = os.path.join(ROOT, 'Assets', 'ThirdParty', 'Fonts')

PICK = 'a'

RED, DARK, INK, WARM, WHITE, BLUE = '#C8102E', '#2B2D31', '#17181B', '#FFF1D0', '#F2F2F0', '#0E518D'
LINE = '#383A3F'  # faint road lines on the asphalt background


def n(v):
    s = f'{v:.2f}'.rstrip('0').rstrip('.')
    return '0' if s in ('-0', '') else s


# ---------------------------------------------------------------- text to outlines
class Font:
    def __init__(self, path):
        d = self.d = open(path, 'rb').read()
        cnt = struct.unpack('>H', d[4:6])[0]
        self.t = {}
        for i in range(cnt):
            tag, _, off, ln = struct.unpack('>4sIII', d[12 + 16 * i:28 + 16 * i])
            self.t[tag.decode()] = (off, ln)
        o = self.t['head'][0]
        self.upm = struct.unpack('>H', d[o + 18:o + 20])[0]
        locfmt = struct.unpack('>h', d[o + 50:o + 52])[0]
        ng = struct.unpack('>H', d[self.t['maxp'][0] + 4:self.t['maxp'][0] + 6])[0]
        self.nh = struct.unpack('>H', d[self.t['hhea'][0] + 34:self.t['hhea'][0] + 36])[0]
        o = self.t['loca'][0]
        if locfmt == 0:
            self.loca = [2 * x for x in struct.unpack('>%dH' % (ng + 1), d[o:o + 2 * (ng + 1)])]
        else:
            self.loca = list(struct.unpack('>%dI' % (ng + 1), d[o:o + 4 * (ng + 1)]))
        self.gid = self._cmap()

    def _cmap(self):
        d, o = self.d, self.t['cmap'][0]
        cnt = struct.unpack('>H', d[o + 2:o + 4])[0]
        sub = None
        for i in range(cnt):
            pid, eid, off = struct.unpack('>HHI', d[o + 4 + 8 * i:o + 12 + 8 * i])
            fmt = struct.unpack('>H', d[o + off:o + off + 2])[0]
            if fmt == 12 and (pid, eid) in ((3, 10), (0, 4), (0, 6)):
                sub = (o + off, 12)
                break
            if fmt == 4 and (pid, eid) in ((3, 1), (0, 3)) and sub is None:
                sub = (o + off, 4)
        base, fmt = sub
        if fmt == 12:
            ngr = struct.unpack('>I', d[base + 12:base + 16])[0]
            groups = [struct.unpack('>III', d[base + 16 + 12 * i:base + 28 + 12 * i]) for i in range(ngr)]

            def f12(cp):
                for s, e, g in groups:
                    if s <= cp <= e:
                        return g + cp - s
                return 0
            return f12
        segx2 = struct.unpack('>H', d[base + 6:base + 8])[0]
        sc = segx2 // 2
        ends = struct.unpack('>%dH' % sc, d[base + 14:base + 14 + segx2])
        starts = struct.unpack('>%dH' % sc, d[base + 16 + segx2:base + 16 + 2 * segx2])
        deltas = struct.unpack('>%dh' % sc, d[base + 16 + 2 * segx2:base + 16 + 3 * segx2])
        rbase = base + 16 + 3 * segx2
        ros = struct.unpack('>%dH' % sc, d[rbase:rbase + segx2])

        def f4(cp):
            for i in range(sc):
                if starts[i] <= cp <= ends[i]:
                    if ros[i] == 0:
                        return (cp + deltas[i]) & 0xFFFF
                    p = rbase + 2 * i + ros[i] + 2 * (cp - starts[i])
                    g = struct.unpack('>H', d[p:p + 2])[0]
                    return (g + deltas[i]) & 0xFFFF if g else 0
            return 0
        return f4

    def advance(self, gid):
        o = self.t['hmtx'][0]
        i = min(gid, self.nh - 1)
        return struct.unpack('>H', self.d[o + 4 * i:o + 4 * i + 2])[0]

    def contours(self, gid):
        d = self.d
        a, b = self.loca[gid], self.loca[gid + 1]
        if a == b:
            return []
        g = self.t['glyf'][0] + a
        nc = struct.unpack('>h', d[g:g + 2])[0]
        if nc < 0:  # composite: offsets only
            p, out = g + 10, []
            while True:
                flags, gi = struct.unpack('>HH', d[p:p + 4])
                p += 4
                if flags & 1:
                    a1, a2 = struct.unpack('>hh', d[p:p + 4])
                    p += 4
                else:
                    a1, a2 = struct.unpack('>bb', d[p:p + 2])
                    p += 2
                p += 2 if flags & 8 else 4 if flags & 0x40 else 8 if flags & 0x80 else 0
                dx, dy = (a1, a2) if flags & 2 else (0, 0)
                out += [[(x + dx, y + dy, on) for x, y, on in c] for c in self.contours(gi)]
                if not flags & 0x20:
                    return out
        ends = struct.unpack('>%dH' % nc, d[g + 10:g + 10 + 2 * nc])
        p = g + 10 + 2 * nc
        p += 2 + struct.unpack('>H', d[p:p + 2])[0]
        npts = ends[-1] + 1
        flags = []
        while len(flags) < npts:
            f = d[p]
            p += 1
            flags.append(f)
            if f & 8:
                flags += [f] * d[p]
                p += 1
        coords = []
        for short, same in ((2, 16), (4, 32)):
            v, vals = 0, []
            for f in flags:
                if f & short:
                    dv = d[p]
                    p += 1
                    v += dv if f & same else -dv
                elif not f & same:
                    v += struct.unpack('>h', d[p:p + 2])[0]
                    p += 2
                vals.append(v)
            coords.append(vals)
        pts = [(coords[0][i], coords[1][i], bool(flags[i] & 1)) for i in range(npts)]
        out, s = [], 0
        for e in ends:
            out.append(pts[s:e + 1])
            s = e + 1
        return out


def contour_d(c, k, ox, oy, pts):
    """One TrueType contour as SVG path data. y is flipped; k = size / unitsPerEm."""
    def T(p):
        q = (p[0] * k + ox, oy - p[1] * k)
        pts.append(q)
        return q
    i0 = next((i for i, p in enumerate(c) if p[2]), None)
    if i0 is None:
        mid = ((c[0][0] + c[-1][0]) / 2, (c[0][1] + c[-1][1]) / 2, True)
        c = [mid] + c
    else:
        c = c[i0:] + c[:i0]
    q = c[1:] + [c[0]]
    x, y = T(c[0])
    d = [f'M{n(x)} {n(y)}']
    i = 0
    while i < len(q):
        p = q[i]
        if p[2]:
            x, y = T(p)
            d.append(f'L{n(x)} {n(y)}')
            i += 1
            continue
        nx = q[i + 1]
        end = nx if nx[2] else ((p[0] + nx[0]) / 2, (p[1] + nx[1]) / 2, True)
        cx, cy = T(p)
        ex, ey = T(end)
        d.append(f'Q{n(cx)} {n(cy)} {n(ex)} {n(ey)}')
        i += 2 if nx[2] else 1
    return ''.join(d) + 'Z'


def text_path(font, text, size, tracking=0.0, kern=None):
    """Returns (path data, width, height) with the text's bounding box starting at (0, 0)."""
    k = size / font.upm
    kern = kern or {}
    for second in (False, True):  # pass 1 measures, pass 2 shifts the box to (0, 0)
        ox, oy = (-minx, -miny) if second else (0, 0)
        pts, parts, pen, prev = [], [], 0.0, None
        for ch in text:
            if prev:
                pen += kern.get((prev, ch), 0) * size
            gid = font.gid(ord(ch))
            for c in font.contours(gid):
                parts.append(contour_d(c, k, pen + ox, oy, pts))
            pen += font.advance(gid) * k + tracking * size
            prev = ch
        minx, maxx = min(p[0] for p in pts), max(p[0] for p in pts)
        miny, maxy = min(p[1] for p in pts), max(p[1] for p in pts)
    return ''.join(parts), maxx - minx, maxy - miny


# ---------------------------------------------------------------- icon artwork (local coordinates, y down)
A_BODY = ('M-176,56 C-176,16 -166,-8 -142,-20 C-130,-25 -114,-20 -96,-12 L96,-12 C114,-20 130,-25 142,-20 '
          'C166,-8 176,16 176,56 C176,72 164,82 148,82 L-148,82 C-164,82 -176,72 -176,56Z')
A_CABIN = 'M-112,-8 C-98,-54 -74,-88 -38,-94 L38,-94 C74,-88 98,-54 112,-8Z'
A_GLASS = 'M-88,-14 C-80,-48 -62,-76 -36,-80 L36,-80 C62,-76 80,-48 88,-14Z'


def art_a(red=RED):
    """Front view of a red 911-like car coming straight at you: round lamps on the fender humps, low wide stance."""
    wheel = ''.join(f'<rect x="{x}" y="70" width="54" height="42" rx="14" fill="#101114"/>' for x in (-150, 96))
    lamps = ''.join(f'<circle cx="{x}" cy="8" r="24" fill="{INK}"/><circle cx="{x}" cy="8" r="19.5" fill="{WARM}"/>'
                    for x in (-122, 122))
    return (f'{wheel}<path d="{A_BODY}" fill="{red}"/><path d="{A_CABIN}" fill="{red}"/>'
            f'<path d="{A_GLASS}" fill="{INK}"/>{lamps}'
            f'<rect x="-84" y="44" width="168" height="20" rx="10" fill="{INK}"/>')


def mono_a(color, uid):
    wheel = ''.join(f'<rect x="{x}" y="70" width="54" height="42" rx="14"/>' for x in (-150, 96))
    holes = (f'<path d="{A_GLASS}"/><circle cx="-122" cy="8" r="19.5"/><circle cx="122" cy="8" r="19.5"/>'
             '<rect x="-84" y="44" width="168" height="20" rx="10"/>')
    return (f'<mask id="m{uid}" maskUnits="userSpaceOnUse" x="-260" y="-260" width="520" height="520">'
            f'<rect x="-260" y="-260" width="520" height="520" fill="#fff"/><g fill="#000">{holes}</g></mask>'
            f'<g fill="{color}" mask="url(#m{uid})">{wheel}<path d="{A_BODY}"/><path d="{A_CABIN}"/></g>')


def _persp(y, vy=-70, by=125):
    return (y - vy) / (by - vy)


def art_b(red=RED):
    """A road running to the horizon; the oncoming headlights are the only light."""
    vy, by, ex = -70, 125, 118
    edge = ''.join(
        f'<path d="M{n(s * -1.5)} {vy} L{n(s * 1.5)} {vy} L{n(s * (ex - 11))} {by} L{n(s * (ex + 11))} {by}Z" fill="{WHITE}"/>'
        for s in (-1, 1))
    dashes = ''
    for y1, y2 in ((-38, -18), (4, 34), (76, 125)):
        h1, h2 = 1.5 + 11 * _persp(y1), 1.5 + 11 * _persp(y2)
        dashes += f'<path d="M{n(-h1)} {y1} L{n(h1)} {y1} L{n(h2)} {y2} L{n(-h2)} {y2}Z" fill="{red}"/>'
    lights = ''.join(f'<circle cx="{x}" cy="-76" r="20" fill="{WARM}" opacity=".22"/><circle cx="{x}" cy="-76" r="8.5" fill="{WARM}"/>'
                     for x in (-12, 12))
    return edge + dashes + lights


def mono_b(color, uid):
    vy, by, ex = -70, 125, 118
    shapes = ''.join(
        f'<path d="M{n(s * -1.5)} {vy} L{n(s * 1.5)} {vy} L{n(s * (ex - 11))} {by} L{n(s * (ex + 11))} {by}Z"/>' for s in (-1, 1))
    for y1, y2 in ((-38, -18), (4, 34), (76, 125)):
        h1, h2 = 1.5 + 11 * _persp(y1), 1.5 + 11 * _persp(y2)
        shapes += f'<path d="M{n(-h1)} {y1} L{n(h1)} {y1} L{n(h2)} {y2} L{n(-h2)} {y2}Z"/>'
    shapes += ''.join(f'<circle cx="{x}" cy="-76" r="9"/>' for x in (-12, 12))
    return f'<g fill="{color}">{shapes}</g>'


def _ring():
    ro, ri = 190, 128
    P = lambda a, r: (r * math.cos(math.radians(a)), r * math.sin(math.radians(a)))
    (x1, y1), (x2, y2) = P(-6, ro), P(-62, ro)
    (x3, y3), (x4, y4) = P(-62, ri), P(-6, ri)
    return (f'M{n(x1)} {n(y1)} A{ro} {ro} 0 1 1 {n(x2)} {n(y2)} L{n(x3)} {n(y3)} '
            f'A{ri} {ri} 0 1 0 {n(x4)} {n(y4)}Z')


def _bar():
    """The G's bar: 62 thick like the ring, covers the ring end, and its right end follows the outer circle."""
    ro, top, bot = 190, -20, 42
    xt, xb = math.sqrt(ro * ro - top * top), math.sqrt(ro * ro - bot * bot)
    return f'M2 {top} L{n(xt)} {top} A{ro} {ro} 0 0 1 {n(xb)} {bot} L2 {bot}Z'


def art_c(red=RED):
    """A 'G' made from the no-entry sign: red ring, warm white bar (the colour of the headlights)."""
    return f'<path d="{_ring()}" fill="{red}"/><path d="{_bar()}" fill="{WARM}"/>'


def mono_c(color, uid):
    return f'<g fill="{color}"><path d="{_ring()}"/><path d="{_bar()}"/></g>'


def bg_lines(size):
    """Faint perspective lines on the asphalt (candidate a)."""
    vx, vy, w = size / 2, size * 0.30, size * 0.014
    return (f'<g stroke="{LINE}" stroke-width="{n(w)}" stroke-linecap="round" fill="none">'
            f'<path d="M{n(vx)} {n(vy)} L{n(-size * 0.1)} {n(size * 1.05)}"/><path d="M{n(vx)} {n(vy)} L{n(size * 1.1)} {n(size * 1.05)}"/>'
            f'<path d="M{n(vx)} {n(vy)} L{n(size * 0.5)} {n(size * 1.05)}" stroke-dasharray="{n(size * 0.05)} {n(size * 0.06)}"/></g>')


def no_bg(size):
    return ''


CANDS = {
    'a': dict(name='a', title='Frontal', art=art_a, mono=mono_a, R=196, dy=9, bg=bg_lines,
              font='Inter-Black.ttf', tracking=0.05, kern={('F', 'A'): -0.04, ('S', 'T'): -0.01},
              blurb='Red 911-like car coming straight at you, headlights on: the wrong-way driver. Road lines fade behind it.'),
    'b': dict(name='b', title='Fluchtpunkt', art=art_b, mono=mono_b, R=185, dy=15, bg=no_bg,
              font='BarlowSemiCondensed-SemiBold.ttf', tracking=0.22, kern={},
              blurb='A road running to the horizon with a red centre line. Two headlights come out of the vanishing point.'),
    'c': dict(name='c', title='Einfahrt', art=art_c, mono=mono_c, R=190, dy=0, bg=no_bg,
              font='Inter-Black.ttf', tracking=0.02, kern={('F', 'A'): -0.04, ('S', 'T'): -0.01},
              blurb='A "G" built from the no-entry sign: red ring and a headlight-white bar. Rejected: it looks too much like a well-known "G" mark.'),
}

_uid = [0]


def uid():
    _uid[0] += 1
    return f'{_uid[0]}'


XMLNS = 'xmlns="http://www.w3.org/2000/svg"'


def place(frag, cx, cy, s):
    return f'<g transform="translate({n(cx)} {n(cy)}) scale({n(s)})">{frag}</g>'


def icon_svg(c, px=None, rx=0.0, bg=True, art=None):
    """Full icon on a 512 canvas: asphalt, optional faint lines, artwork inside the central ~80 %."""
    u = uid()
    s = 0.40 * 512 / c['R']
    size = f' width="{px}" height="{px}"' if px else ''
    clip = f'<clipPath id="c{u}"><rect width="512" height="512" rx="{n(rx * 512)}"/></clipPath>' if rx else ''
    ca = f' clip-path="url(#c{u})"' if rx else ''
    body = (f'<rect width="512" height="512" fill="{DARK}"/>' + c['bg'](512) if bg else '') + place(
        (art or c['art'])(), 256, 256 - c['dy'] * s, s)
    return f'<svg {XMLNS} viewBox="0 0 512 512"{size}><defs>{clip}</defs><g{ca}>{body}</g></svg>'


def fg_scale(c):
    return 31.5 / c['R']  # keeps the artwork inside the 66 dp safe circle (radius 33) of the 108 dp canvas


def foreground_svg(c, px=None):
    s = fg_scale(c)
    size = f' width="{px}" height="{px}"' if px else ''
    return f'<svg {XMLNS} viewBox="0 0 108 108"{size}>{place(c["art"](), 54, 54 - c["dy"] * s, s)}</svg>'


def background_svg(c, px=None):
    size = f' width="{px}" height="{px}"' if px else ''
    return f'<svg {XMLNS} viewBox="0 0 108 108"{size}><rect width="108" height="108" fill="{DARK}"/>{c["bg"](108)}</svg>'


def monochrome_svg(c, px=None, color='#000000'):
    s = fg_scale(c)
    size = f' width="{px}" height="{px}"' if px else ''
    return f'<svg {XMLNS} viewBox="0 0 108 108"{size}>{place(c["mono"](color, uid()), 54, 54 - c["dy"] * s, s)}</svg>'


def adaptive_preview(c, px, shape='circle'):
    u = uid()
    s = fg_scale(c)
    clip = (f'<circle cx="54" cy="54" r="54"/>' if shape == 'circle'
            else '<rect width="108" height="108" rx="30"/>')
    return (f'<svg {XMLNS} viewBox="0 0 108 108" width="{px}" height="{px}"><defs><clipPath id="a{u}">{clip}</clipPath></defs>'
            f'<g clip-path="url(#a{u})"><rect width="108" height="108" fill="{DARK}"/>{c["bg"](108)}'
            f'{place(c["art"](), 54, 54 - c["dy"] * s, s)}</g></svg>')


_fonts = {}


def wordmark(c, color, height=None):
    """Returns (svg, w, h) of the wordmark with text as outlines."""
    f = _fonts.setdefault(c['font'], Font(os.path.join(FONT_DIR, c['font'])))
    size = 100
    d, w, h = text_path(f, 'GEISTERFAHRER', size, c['tracking'], c['kern'])
    k = (height / h) if height else 1
    svg = (f'<svg {XMLNS} viewBox="0 0 {n(w)} {n(h)}" width="{n(w * k)}" height="{n(h * k)}">'
           f'<path d="{d}" fill="{color}"/></svg>')
    return svg, w * k, h * k, d, w, h


def horizontal_svg(c, text_color):
    """Rounded icon + wordmark, 200 units high."""
    H, th = 200, 52
    _, tw, thh, d, w, h = wordmark(c, text_color, th)
    k = th / h
    gap = 46
    width = H + gap + tw
    ic = icon_svg(c, rx=0.225).replace(f'{XMLNS} viewBox="0 0 512 512"', 'viewBox="0 0 512 512" x="0" y="0" width="200" height="200"')
    return (f'<svg {XMLNS} viewBox="0 0 {n(width)} {H}" width="{n(width)}" height="{H}">{ic}'
            f'<g transform="translate({n(H + gap)} {n((H - th) / 2)}) scale({n(k)})"><path d="{d}" fill="{text_color}"/></g></svg>')


# ---------------------------------------------------------------- preview page
CSS = """
:root{--bg:#111214;--card:#1a1b1e;--line:#2a2c30;--text:#e9e9ea;--muted:#9a9ca1}
*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--text);font:14px/1.45 system-ui,-apple-system,"Segoe UI",sans-serif}
main{max-width:1500px;margin:0 auto;padding:24px 16px 40px}
h1{margin:0 0 4px;font-size:20px;font-weight:650}.sub{color:var(--muted);margin:0 0 20px}
.grid{display:grid;gap:16px;grid-template-columns:repeat(auto-fit,minmax(min(100%,440px),1fr));align-items:start}
.card{background:var(--card);border:1px solid var(--line);border-radius:16px;padding:16px}
h2{margin:0 0 2px;font-size:16px;display:flex;gap:8px;align-items:center}
.pick{font-size:11px;font-weight:600;color:#fff;background:#3D7BFF;border-radius:6px;padding:2px 7px}
.why{color:var(--muted);margin:0 0 12px}
.cap{color:var(--muted);font-size:11px;margin:10px 0 4px}
.tile{border-radius:12px;padding:14px;display:flex;gap:16px;align-items:flex-end;flex-wrap:wrap}
.light{background:#F4F4F2}.dark{background:#0C0D0F}
.tile svg{display:block}
"""


def card(c):
    ic = lambda px: icon_svg(c, px, rx=0.225)
    wm_d = wordmark(c, DARK, 30)[0]
    wm_l = wordmark(c, WHITE, 30)[0]
    pick = '<span class="pick">Designer pick</span>' if c['name'] == PICK else ''
    return f"""<section class="card"><h2>{c['name']} — {c['title']}{pick}</h2><p class="why">{c['blurb']}</p>
<div class="cap">Icon 48 / 96 / 192 px, light and dark</div>
<div class="tile light">{ic(48)}{ic(96)}{ic(192)}</div><div class="tile dark" style="margin-top:8px">{ic(48)}{ic(96)}{ic(192)}</div>
<div class="cap">Wordmark (font: {c['font'].split('-')[0]}) and horizontal logo</div>
<div class="tile light">{wm_d}</div><div class="tile dark" style="margin-top:8px">{wm_l}</div>
<div class="tile light" style="margin-top:8px">{horizontal_svg(c, DARK).replace('width="', 'data-w="', 1).replace('height="200"', 'height="64" width="auto"', 1)}</div>
<div class="tile dark" style="margin-top:8px">{horizontal_svg(c, WHITE).replace('width="', 'data-w="', 1).replace('height="200"', 'height="64" width="auto"', 1)}</div>
<div class="cap">Android: adaptive circle / rounded mask at 96 px, small 48 px, and monochrome</div>
<div class="tile dark">{adaptive_preview(c, 96)}{adaptive_preview(c, 96, 'rounded')}{adaptive_preview(c, 48)}
<div style="background:#fff;border-radius:50%;padding:0">{monochrome_svg(c, 96)}</div></div></section>"""


def preview():
    cards = ''.join(card(c) for c in CANDS.values())
    html = (f'<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">'
            f'<title>Geisterfahrer Logo</title><style>{CSS}</style></head><body><main>'
            '<h1>Geisterfahrer — logo candidates a / b / c</h1>'
            '<p class="sub">Colours: car red #C8102E, asphalt #2B2D31, warm white #FFF1D0 (headlights), light grey #F2F2F0. Fonts are OFL (Inter Black, Barlow Semi Condensed SemiBold); the text is outlined, so no font is loaded.</p>'
            f'<div class="grid">{cards}</div></main></body></html>')
    open(os.path.join(HERE, 'logo-preview.html'), 'w', encoding='utf-8').write(html)


def finals(c):
    w = lambda name, s: open(os.path.join(HERE, name), 'w', encoding='utf-8').write(s + '\n')
    w('icon.svg', icon_svg(c, 1024))
    w('icon-foreground.svg', foreground_svg(c, 432))
    w('icon-background.svg', background_svg(c, 432))
    w('icon-monochrome.svg', monochrome_svg(c, 432))
    for name, col in (('wordmark', WHITE), ('wordmark-dark', DARK)):
        s, ww, hh, d, w0, h0 = wordmark(c, col)
        # 1200 px wide, same as the PNG
        k = 1200 / w0
        w(name + '.svg', f'<svg {XMLNS} viewBox="0 0 {n(w0)} {n(h0)}" width="1200" height="{n(h0 * k)}"><path d="{d}" fill="{col}"/></svg>')
    w('logo-horizontal.svg', horizontal_svg(c, DARK))
    w('logo-horizontal-dark.svg', horizontal_svg(c, WHITE))


if __name__ == '__main__':
    pick = sys.argv[1] if len(sys.argv) > 1 else PICK
    preview()
    finals(CANDS[pick])
    print('ok, pick =', pick)
