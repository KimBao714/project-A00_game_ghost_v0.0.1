from pathlib import Path
import struct
import zlib


ROOT = Path(__file__).resolve().parents[1] / "Assets" / "GhostRiver" / "Resources" / "GhostRiverArt"
INK = (29, 42, 48, 255)
DEEP = (42, 63, 68, 255)
TEAL = (61, 110, 112, 255)
MINT = (142, 194, 164, 255)
CREAM = (245, 224, 169, 255)
GOLD = (238, 178, 78, 255)
RUST = (191, 83, 61, 255)
SOIL = (91, 67, 52, 255)
LEAF = (51, 91, 67, 255)


class Image:
    def __init__(self, width, height, fill=(0, 0, 0, 0)):
        self.width = width
        self.height = height
        self.pixels = [fill] * (width * height)

    def dot(self, x, y, color):
        if 0 <= x < self.width and 0 <= y < self.height:
            self.pixels[y * self.width + x] = color

    def rect(self, x, y, width, height, color):
        for py in range(y, y + height):
            for px in range(x, x + width):
                self.dot(px, py, color)

    def line(self, x0, y0, x1, y1, color):
        dx = abs(x1 - x0)
        sx = 1 if x0 < x1 else -1
        dy = -abs(y1 - y0)
        sy = 1 if y0 < y1 else -1
        error = dx + dy
        while True:
            self.dot(x0, y0, color)
            if x0 == x1 and y0 == y1:
                break
            twice = 2 * error
            if twice >= dy:
                error += dy
                x0 += sx
            if twice <= dx:
                error += dx
                y0 += sy

    def polygon(self, points, color, outline=None):
        for y in range(min(p[1] for p in points), max(p[1] for p in points) + 1):
            intersections = []
            for i, (x0, y0) in enumerate(points):
                x1, y1 = points[(i + 1) % len(points)]
                if y0 == y1:
                    continue
                low, high = (y0, y1) if y0 < y1 else (y1, y0)
                if low <= y < high:
                    intersections.append(x0 + (y - y0) * (x1 - x0) / (y1 - y0))
            intersections.sort()
            for i in range(0, len(intersections), 2):
                if i + 1 < len(intersections):
                    for x in range(round(intersections[i]), round(intersections[i + 1]) + 1):
                        self.dot(x, y, color)
        if outline:
            for i, (x0, y0) in enumerate(points):
                x1, y1 = points[(i + 1) % len(points)]
                self.line(x0, y0, x1, y1, outline)

    def ellipse(self, x, y, width, height, color, outline=None):
        cx = x + (width - 1) / 2
        cy = y + (height - 1) / 2
        rx = max(0.5, width / 2)
        ry = max(0.5, height / 2)
        for py in range(y, y + height):
            for px in range(x, x + width):
                if ((px - cx) / rx) ** 2 + ((py - cy) / ry) ** 2 <= 1:
                    self.dot(px, py, color)
        if outline:
            for px in range(x, x + width):
                for py in (y, y + height - 1):
                    if 0 <= px < self.width and 0 <= py < self.height:
                        if self.pixels[py * self.width + px] == color:
                            self.dot(px, py, outline)
            for py in range(y, y + height):
                for px in (x, x + width - 1):
                    if 0 <= px < self.width and 0 <= py < self.height:
                        if self.pixels[py * self.width + px] == color:
                            self.dot(px, py, outline)

    def save(self, name):
        raw = bytearray()
        for y in range(self.height):
            raw.append(0)
            for color in self.pixels[y * self.width:(y + 1) * self.width]:
                raw.extend(color)

        def chunk(kind, data):
            return (struct.pack(">I", len(data)) + kind + data +
                    struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF))

        png = (b"\x89PNG\r\n\x1a\n" +
               chunk(b"IHDR", struct.pack(">2I5B", self.width, self.height, 8, 6, 0, 0, 0)) +
               chunk(b"IDAT", zlib.compress(bytes(raw), 9)) +
               chunk(b"IEND", b""))
        ROOT.mkdir(parents=True, exist_ok=True)
        (ROOT / (name + ".png")).write_bytes(png)


def ghost():
    im = Image(32, 32)
    im.ellipse(3, 3, 26, 26, INK)
    im.ellipse(5, 4, 22, 23, (18, 22, 29, 255))
    im.ellipse(8, 6, 7, 3, (74, 83, 99, 255))
    im.ellipse(7, 8, 18, 17, (9, 12, 18, 255))
    im.ellipse(10, 10, 5, 7, (248, 249, 242, 255))
    im.ellipse(18, 10, 5, 7, (248, 249, 242, 255))
    im.dot(13, 13, (12, 17, 24, 255))
    im.dot(21, 13, (12, 17, 24, 255))
    im.dot(11, 11, (255, 255, 255, 255))
    im.dot(19, 11, (255, 255, 255, 255))
    return im


def ghost_walk(step):
    im = ghost()
    im.rect(2 + step * 2, 15, 3, 2, (81, 204, 217, 255))
    im.rect(25 - step * 2, 19, 3, 2, (81, 204, 217, 255))
    return im


def ghost_jump():
    im = ghost()
    im.ellipse(5, 5, 22, 22, (18, 22, 29, 255))
    im.ellipse(11, 9, 5, 7, (248, 249, 242, 255))
    im.ellipse(19, 9, 5, 7, (248, 249, 242, 255))
    return im


def ghost_dash():
    im = ghost()
    im.rect(0, 9, 5, 2, (81, 204, 217, 210))
    im.rect(0, 15, 7, 2, (81, 204, 217, 175))
    im.rect(1, 22, 5, 2, (81, 204, 217, 140))
    return im


def boss():
    im = Image(64, 64)
    im.ellipse(5, 5, 54, 54, INK)
    im.ellipse(9, 7, 46, 49, (49, 39, 54, 255))
    im.ellipse(14, 11, 36, 39, (23, 22, 33, 255))
    im.polygon([(8, 20), (18, 4), (25, 18)], (125, 53, 70, 255), INK)
    im.polygon([(56, 20), (46, 4), (39, 18)], (125, 53, 70, 255), INK)
    im.ellipse(17, 23, 11, 12, (255, 220, 135, 255))
    im.ellipse(36, 23, 11, 12, (255, 220, 135, 255))
    im.ellipse(21, 26, 4, 7, INK)
    im.ellipse(40, 26, 4, 7, INK)
    im.line(21, 44, 43, 44, (204, 87, 100, 255))
    im.rect(24, 46, 16, 3, (204, 87, 100, 255))
    im.ellipse(8, 49, 10, 9, (88, 52, 69, 255))
    im.ellipse(46, 49, 10, 9, (88, 52, 69, 255))
    return im


def boss_bolt():
    im = Image(16, 16)
    im.polygon([(8, 0), (12, 4), (15, 8), (11, 12), (8, 15),
                (4, 12), (0, 8), (4, 4)], (151, 57, 91, 255), INK)
    im.polygon([(8, 3), (11, 6), (12, 8), (9, 11), (8, 13),
                (6, 10), (3, 8), (6, 5)], (229, 104, 119, 255))
    im.rect(7, 5, 3, 6, (255, 222, 164, 255))
    im.rect(5, 7, 6, 2, (255, 222, 164, 255))
    return im


def ruin_pillar():
    im = Image(48, 96)
    im.rect(5, 9, 38, 7, (35, 44, 53, 255))
    im.rect(9, 16, 30, 65, (69, 82, 88, 255))
    im.rect(13, 19, 22, 60, (88, 102, 102, 255))
    im.rect(4, 79, 40, 7, (43, 52, 61, 255))
    im.rect(8, 82, 32, 3, (114, 130, 123, 255))
    im.rect(17, 25, 2, 45, (49, 64, 70, 255))
    im.rect(29, 34, 3, 32, (49, 64, 70, 255))
    im.rect(12, 72, 4, 3, (113, 149, 127, 255))
    im.rect(33, 22, 4, 3, (113, 149, 127, 255))
    return im


def crystal():
    im = Image(32, 48)
    im.polygon([(16, 1), (26, 15), (22, 43), (16, 47), (9, 42), (5, 15)],
               (45, 122, 145, 255), INK)
    im.polygon([(16, 4), (23, 16), (20, 40), (16, 43)], (92, 207, 218, 255))
    im.polygon([(16, 4), (11, 17), (13, 39), (16, 43)], (54, 143, 162, 255))
    im.line(16, 6, 16, 37, (182, 245, 230, 255))
    im.rect(2, 44, 28, 3, (44, 65, 75, 255))
    return im


def mud_spirit():
    im = Image(32, 24)
    im.polygon([(3, 17), (5, 11), (10, 7), (15, 8), (20, 5), (26, 9),
                (29, 15), (27, 21), (23, 22), (19, 20), (15, 23), (10, 21), (6, 22)], INK)
    im.polygon([(5, 16), (7, 12), (11, 9), (16, 10), (20, 7), (24, 10),
                (27, 15), (25, 19), (22, 20), (18, 18), (15, 21), (11, 19), (7, 20)], (94, 139, 94, 255))
    im.rect(10, 13, 3, 3, CREAM)
    im.rect(20, 13, 3, 3, CREAM)
    im.dot(11, 14, INK)
    im.dot(21, 14, INK)
    im.rect(15, 17, 4, 2, (47, 76, 63, 255))
    im.rect(7, 18, 3, 2, (135, 174, 111, 255))
    return im


def crab():
    im = Image(32, 24)
    im.polygon([(7, 10), (11, 5), (21, 5), (26, 10), (25, 17), (21, 20),
                (11, 20), (7, 17)], INK)
    im.polygon([(9, 10), (12, 7), (20, 7), (24, 10), (23, 16), (20, 18),
                (12, 18), (9, 16)], (190, 94, 67, 255))
    im.rect(12, 9, 8, 2, (229, 140, 91, 255))
    im.rect(12, 12, 3, 3, CREAM)
    im.rect(19, 12, 3, 3, CREAM)
    im.dot(13, 13, INK)
    im.dot(20, 13, INK)
    im.line(7, 13, 3, 9, INK)
    im.line(3, 9, 2, 6, INK)
    im.line(25, 13, 29, 9, INK)
    im.line(29, 9, 30, 6, INK)
    for x in (9, 14, 19, 24):
        im.line(x, 18, x - 2, 22, INK)
    return im


def firefly():
    im = Image(24, 32)
    im.ellipse(6, 2, 12, 10, (112, 158, 143, 190))
    im.ellipse(1, 7, 9, 10, (142, 191, 176, 190))
    im.ellipse(14, 7, 9, 10, (142, 191, 176, 190))
    im.polygon([(7, 13), (11, 10), (16, 13), (18, 20), (14, 26), (10, 26), (6, 21)], INK)
    im.polygon([(9, 14), (12, 12), (15, 15), (16, 20), (13, 23), (10, 22), (8, 19)], GOLD)
    im.rect(9, 15, 2, 3, CREAM)
    im.rect(14, 15, 2, 3, CREAM)
    im.line(8, 22, 5, 27, (73, 99, 73, 255))
    im.line(16, 22, 19, 27, (73, 99, 73, 255))
    return im


def soil():
    im = Image(32, 32, (57, 65, 69, 255))
    im.rect(0, 0, 32, 3, (106, 137, 127, 255))
    im.rect(0, 3, 32, 2, (72, 99, 98, 255))
    for x, y, w, h, c in [
        (3, 9, 5, 2, (76, 86, 91, 255)), (18, 7, 7, 2, (81, 94, 98, 255)),
        (10, 16, 4, 2, (44, 52, 60, 255)), (24, 20, 5, 2, (79, 91, 96, 255)),
        (4, 25, 6, 2, (46, 55, 62, 255)), (14, 28, 5, 2, (88, 101, 100, 255)),
        (20, 13, 2, 2, (111, 133, 126, 255)), (8, 12, 2, 2, (102, 128, 120, 255)),
    ]:
        im.rect(x, y, w, h, c)
    return im


def grass_edge():
    im = Image(32, 12)
    im.rect(0, 3, 32, 9, (55, 64, 69, 255))
    im.rect(0, 2, 32, 3, (72, 106, 104, 255))
    im.rect(0, 0, 32, 3, (111, 145, 131, 255))
    im.rect(3, 1, 5, 1, (164, 196, 160, 255))
    im.rect(19, 1, 4, 1, (164, 196, 160, 255))
    im.rect(12, 5, 4, 2, (86, 110, 108, 255))
    im.rect(25, 8, 3, 2, (43, 52, 59, 255))
    return im


def platform():
    im = Image(48, 16)
    im.rect(0, 4, 48, 12, (58, 69, 76, 255))
    im.rect(0, 2, 48, 4, (91, 108, 111, 255))
    im.rect(0, 0, 48, 3, (135, 157, 145, 255))
    for x in (5, 17, 31, 42):
        im.rect(x, 8, 4, 2, (42, 53, 62, 255))
        im.rect(x + 1, 12, 2, 1, (98, 123, 119, 255))
    return im


def river():
    im = Image(64, 32, (30, 103, 117, 255))
    im.rect(0, 0, 64, 4, (50, 141, 146, 255))
    for x, y, w in [(3, 7, 11), (25, 10, 17), (49, 6, 12), (13, 19, 14), (40, 24, 19), (4, 28, 8)]:
        im.rect(x, y, w, 2, (86, 178, 168, 255))
        im.rect(x + 2, y + 2, max(2, w - 5), 1, (48, 138, 144, 255))
    return im


def sky():
    im = Image(426, 216)
    for y in range(216):
        if y < 112:
            t = y / 112
            color = (int(90 + 28 * t), int(168 + 31 * t), int(188 + 12 * t), 255)
        else:
            t = (y - 112) / 104
            color = (int(118 + 86 * t), int(199 - 17 * t), int(200 - 48 * t), 255)
        im.rect(0, y, 426, 1, color)
    # Soft block clouds and distant mangrove islands.
    for x, y, w in [(32, 51, 35), (104, 72, 48), (205, 43, 52), (307, 67, 44), (368, 35, 30)]:
        im.rect(x, y, w, 4, (181, 219, 204, 255))
        im.rect(x + 7, y - 3, max(5, w - 14), 3, (205, 230, 208, 255))
    for x, y, w, h in [(0, 174, 80, 20), (81, 182, 63, 15), (237, 177, 80, 22), (349, 180, 77, 19)]:
        im.rect(x, y, w, h, (61, 115, 101, 255))
        for tx in range(x + 4, x + w - 2, 12):
            im.rect(tx, y - 4, 4, 5, (51, 98, 88, 255))
    return im


def key():
    im = Image(16, 24)
    im.ellipse(2, 1, 12, 12, INK)
    im.ellipse(4, 3, 8, 8, GOLD)
    im.ellipse(6, 5, 4, 4, (72, 54, 42, 255))
    im.rect(7, 11, 4, 10, GOLD)
    im.rect(7, 17, 8, 3, GOLD)
    im.rect(12, 20, 3, 3, (183, 119, 58, 255))
    im.rect(8, 12, 2, 5, CREAM)
    return im


def coin():
    im = Image(16, 16)
    im.ellipse(1, 1, 14, 14, INK)
    im.ellipse(3, 3, 10, 10, GOLD)
    im.ellipse(5, 5, 6, 6, (255, 220, 123, 255))
    im.rect(7, 5, 2, 6, (174, 112, 50, 255))
    im.rect(5, 7, 6, 2, (174, 112, 50, 255))
    return im


def gate():
    im = Image(56, 72)
    im.polygon([(3, 70), (5, 18), (14, 7), (27, 1), (42, 7), (51, 18), (53, 70)], INK)
    im.polygon([(8, 68), (10, 20), (17, 11), (27, 6), (38, 11), (46, 20), (48, 68)],
               (83, 104, 107, 255))
    im.rect(12, 21, 5, 48, (46, 61, 70, 255))
    im.rect(39, 21, 5, 48, (46, 61, 70, 255))
    im.rect(15, 20, 26, 4, (123, 153, 141, 255))
    im.ellipse(22, 35, 12, 19, (49, 129, 148, 255))
    im.ellipse(25, 38, 6, 12, (101, 224, 225, 255))
    im.rect(26, 41, 3, 5, (229, 244, 215, 255))
    return im


def arena_arch():
    im = Image(96, 88)
    im.polygon([(2, 87), (5, 29), (18, 15), (34, 9), (48, 4), (62, 9), (78, 16),
                (91, 30), (94, 87), (77, 87), (74, 40), (63, 30), (48, 25), (33, 30),
                (22, 40), (19, 87)], (59, 74, 82, 255), INK)
    im.rect(7, 78, 16, 8, (109, 133, 126, 255))
    im.rect(73, 78, 16, 8, (109, 133, 126, 255))
    im.line(18, 30, 31, 18, (140, 165, 146, 255))
    im.line(78, 30, 65, 18, (140, 165, 146, 255))
    im.ellipse(43, 13, 10, 10, GOLD)
    return im


def sign():
    im = Image(80, 56)
    im.rect(16, 15, 4, 40, (105, 72, 48, 255))
    im.rect(60, 15, 4, 40, (105, 72, 48, 255))
    im.rect(7, 5, 66, 27, INK)
    im.rect(10, 8, 60, 21, (162, 90, 56, 255))
    im.rect(13, 11, 54, 15, (193, 119, 72, 255))
    im.rect(17, 15, 4, 4, GOLD)
    im.rect(59, 15, 4, 4, GOLD)
    im.rect(31, 12, 18, 3, CREAM)
    im.rect(25, 17, 30, 3, CREAM)
    im.rect(34, 22, 12, 2, GOLD)
    return im


def panel():
    im = Image(16, 16)
    # Transparent center lets the UI choose its own ink/teal fill.
    for i in range(16):
        im.dot(i, 0, INK)
        im.dot(i, 15, INK)
        im.dot(0, i, INK)
        im.dot(15, i, INK)
        im.dot(i, 1, GOLD)
        im.dot(i, 14, GOLD)
        im.dot(1, i, GOLD)
        im.dot(14, i, GOLD)
    for x, y in [(2, 2), (13, 2), (2, 13), (13, 13)]:
        im.dot(x, y, CREAM)
    return im


def heart():
    im = Image(16, 16)
    im.polygon([(2, 4), (4, 2), (7, 2), (8, 4), (10, 2), (13, 2), (15, 5),
                (14, 9), (8, 15), (2, 9)], INK)
    im.polygon([(3, 5), (5, 3), (7, 4), (8, 6), (10, 4), (13, 4), (14, 6),
                (13, 9), (8, 13), (3, 8)], RUST)
    im.rect(5, 4, 2, 2, (245, 145, 118, 255))
    return im


def potion():
    im = Image(16, 16)
    im.rect(6, 1, 4, 3, INK)
    im.rect(5, 4, 6, 2, (178, 141, 89, 255))
    im.polygon([(5, 6), (11, 6), (13, 10), (12, 14), (4, 14), (3, 10)], INK)
    im.polygon([(6, 7), (10, 7), (12, 10), (11, 13), (5, 13), (4, 10)], (191, 83, 61, 255))
    im.rect(6, 8, 2, 4, (243, 163, 126, 255))
    return im


def icon_map():
    im = Image(16, 16)
    im.rect(1, 3, 14, 11, INK)
    im.polygon([(2, 4), (6, 3), (6, 12), (2, 13)], (136, 176, 126, 255))
    im.polygon([(7, 3), (10, 4), (10, 13), (7, 12)], (220, 197, 134, 255))
    im.polygon([(11, 4), (14, 3), (14, 12), (11, 13)], (89, 148, 143, 255))
    im.rect(8, 7, 1, 3, RUST)
    return im


def icon_book():
    im = Image(16, 16)
    im.polygon([(1, 3), (7, 4), (8, 6), (9, 4), (15, 3), (15, 13),
                (9, 14), (8, 15), (7, 14), (1, 13)], INK)
    im.polygon([(2, 4), (7, 5), (7, 13), (2, 12)], (194, 128, 70, 255))
    im.polygon([(9, 5), (14, 4), (14, 12), (9, 13)], (223, 177, 103, 255))
    im.line(3, 7, 6, 8, CREAM)
    im.line(10, 8, 13, 7, CREAM)
    return im


def icon_stamina():
    im = Image(16, 16)
    im.polygon([(2, 8), (6, 2), (14, 1), (12, 9), (7, 14), (3, 12)], INK)
    im.polygon([(4, 8), (7, 4), (12, 3), (10, 8), (7, 12), (4, 11)], (90, 156, 112, 255))
    im.line(4, 11, 11, 4, (183, 217, 153, 255))
    return im


def icon_stress():
    im = Image(16, 16)
    im.polygon([(8, 1), (12, 6), (14, 10), (12, 14), (8, 15), (4, 14), (2, 10), (4, 6)], INK)
    im.polygon([(8, 4), (10, 7), (12, 10), (10, 13), (6, 13), (4, 10), (6, 7)], RUST)
    im.rect(7, 7, 2, 4, CREAM)
    return im


def make_assets():
    assets = {
        "orb": ghost, "orb_walk_a": lambda: ghost_walk(0),
        "orb_walk_b": lambda: ghost_walk(1), "orb_jump": ghost_jump,
        "orb_dash": ghost_dash, "mud_spirit": mud_spirit, "crab": crab, "firefly": firefly,
        "boss": boss, "boss_bolt": boss_bolt, "ruin_pillar": ruin_pillar,
        "crystal": crystal, "arena_arch": arena_arch,
        "soil_tile": soil,
        "grass_edge": grass_edge, "platform": platform, "river_tile": river,
        "sky_tile": sky, "key": key, "coin": coin, "gate": gate, "sign": sign,
        "ui_frame": panel, "heart": heart, "potion": potion,
        "map_icon": icon_map, "book_icon": icon_book,
        "stamina_icon": icon_stamina, "stress_icon": icon_stress,
    }
    for name, create in assets.items():
        create().save(name)
    print("Generated {} pixel-art textures in {}".format(len(assets), ROOT))


if __name__ == "__main__":
    make_assets()
