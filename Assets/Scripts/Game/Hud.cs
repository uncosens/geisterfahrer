using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Geisterfahrer.Game
{
    // uGUI built in code, laid out per docs/design/ui-spec.md (reference 1920 x 1080, match 0.5).
    public class Hud
    {
        public enum Screen { Start, Hud, Pause, Over }

        public Action Play, Pause, Resume, Home, Retry;

        static readonly Color Primary = Hex(0x3D7BFF), Good = Hex(0x2FD27A), Amber = Hex(0xFFB020);
        static readonly Color DimText = new Color(1, 1, 1, 0.75f), IconDim = new Color(1, 1, 1, 0.7f);
        static Sprite circle, ring, rounded, roundedOutline, edgeH, edgeV;

        readonly TMP_FontAsset bold, semi, black;
        readonly Material boldShadow, semiShadow;
        readonly Image dim;
        readonly Image[] edges = new Image[4]; // left, right, top, bottom
        readonly CanvasGroup hud, start, pause, over, touch;
        readonly TMP_Text speedValue, distance, startBestSpeed, startBestDist;
        readonly Image white;
        readonly RectTransform safe, ring_, dot, needle, gauge;
        readonly TMP_Text[] runValue = new TMP_Text[2], bestValue = new TMP_Text[2];
        readonly Image[] star = new Image[2];
        public readonly RectTransform PauseButton;
        float dimTarget, panelShownAt, ringAlpha;
        readonly float[] edgeAlpha = new float[4], edgeHold = new float[4], edgeRate = new float[4];
        Color edgeColor;
        float whiteAlpha, whiteHold, needleKmh, needleVel, digitalTimer;
        Screen screen;
        Rect lastSafe;

        public Hud(TMP_FontAsset bold, TMP_FontAsset semi, TMP_FontAsset black, Material boldShadow, Material semiShadow)
        {
            this.bold = bold; this.semi = semi; this.black = black;
            this.boldShadow = boldShadow; this.semiShadow = semiShadow;
            MakeSprites();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var root = (RectTransform)canvasGo.transform;

            dim = Img(Stretch(root, "Dim"), null, new Color(0.02f, 0.03f, 0.04f, 0));
            var flash = Stretch(root, "EdgeFlash");
            edges[0] = EdgeImage(flash, edgeH, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(220, 0), 1);
            edges[1] = EdgeImage(flash, edgeH, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(220, 0), -1);
            edges[2] = EdgeImage(flash, edgeV, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 160), 1);
            edges[3] = EdgeImage(flash, edgeV, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 160), -1);
            white = Img(Stretch(root, "CrashFlash"), null, new Color(1, 1, 1, 0));
            safe = Stretch(root, "SafeArea");

            // HUD
            hud = Panel("Panel_HUD");
            distance = Label(hud.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -48), new Vector2(480, 96), bold, boldShadow, 80, TextAlignmentOptions.Center, Color.white);
            // Round 911-style speedometer, top-right (ui-spec 6.2); pause moves top-left.
            gauge = Place(hud.transform, "Gauge", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-36, -24), new Vector2(320, 320));
            Img(Place(gauge, "Face", Center, Center, Vector2.zero, new Vector2(320, 320)), Resources.Load<Sprite>("Icons/gauge-face"), Color.white);
            var numerals = new[] { (0, -76.4f, -76.4f), (60, -107.7f, 7.7f), (120, -64.7f, 86.5f), (180, 23f, 105.5f), (240, 94.8f, 51.8f), (300, 101.2f, -37.7f) };
            foreach (var (n, x, y) in numerals)
                Label(gauge, Center, Center, new Vector2(x, y), new Vector2(48, 28), semi, null, 26, TextAlignmentOptions.Center, new Color(0.957f, 0.957f, 0.949f, 0.9f)).text = n.ToString();
            speedValue = Label(gauge, Center, Center, new Vector2(0, -96), new Vector2(100, 56), bold, null, 56, TextAlignmentOptions.Center, Color.white);
            Label(gauge, Center, Center, new Vector2(0, -128), new Vector2(100, 24), semi, null, 20, TextAlignmentOptions.Center, new Color(1, 1, 1, 0.65f)).text = "km/h";
            needle = Place(gauge, "Needle", Center, new Vector2(0.5f, 0.136f), Vector2.zero, new Vector2(12, 162));
            Img(needle, Resources.Load<Sprite>("Icons/gauge-needle"), new Color(0.957f, 0.957f, 0.949f));
            Icon(gauge, "gauge-hub", Vector2.zero, 32, Color.white);

            var pb = Place(hud.transform, "Pause", new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -28), new Vector2(128, 128));
            var pbImg = Img(pb, null, new Color(0, 0, 0, 0));
            pbImg.raycastTarget = true;
            Img(Place(pb, "Bg", Center, Center, Vector2.zero, new Vector2(96, 96)), circle, new Color(0, 0, 0, 0.35f));
            Icon(pb, "pause", Vector2.zero, 44, Color.white);
            pb.gameObject.AddComponent<Button>().onClick.AddListener(() => Click(Pause, false));
            PauseButton = pb;

            // Touch ring (left half steering)
            touch = Panel("Panel_Touch");
            ring_ = Place(touch.transform, "Ring", Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(168, 168));
            Img(ring_, ring, new Color(1, 1, 1, 0.16f));
            dot = Place(ring_, "Dot", Center, Center, Vector2.zero, new Vector2(32, 32));
            Img(dot, circle, new Color(1, 1, 1, 0.4f));

            // Start
            start = Panel("Panel_Start");
            var title = Label(start.transform, Center, Center, new Vector2(0, 190), new Vector2(1400, 160), black, null, 128, TextAlignmentOptions.Center, Color.white);
            title.text = "GEISTERFAHRER";
            title.characterSpacing = 6;
            startBestSpeed = BestGroup(start.transform, new Vector2(-260, 10));
            startBestDist = BestGroup(start.transform, new Vector2(260, 10));
            Btn(start.transform, new Vector2(0, -230), 208, "play", 96, true, () => Click(Play, false));

            // Pause
            pause = Panel("Panel_Pause");
            Btn(pause.transform, new Vector2(-128, 0), 176, "home", 72, false, () => Click(Home, true));
            Btn(pause.transform, new Vector2(128, 0), 176, "play", 80, true, () => Click(Resume, true));

            // Game over: two columns
            over = Panel("Panel_GameOver");
            for (int i = 0; i < 2; i++)
            {
                var col = Place(over.transform, i == 0 ? "TopSpeed" : "Distance", Center, Center, new Vector2(i == 0 ? -320 : 320, 120), new Vector2(560, 360));
                Icon(col, i == 0 ? "speed" : "distance", new Vector2(0, 140), 56, IconDim);
                star[i] = Icon(col, "new-best", new Vector2(52, 160), 36, Good);
                runValue[i] = Label(col, Center, Center, new Vector2(0, 40), new Vector2(560, 140), bold, boldShadow, 120, TextAlignmentOptions.Center, Color.white);
                var best = Place(col, "Best", Center, Center, new Vector2(0, -80), new Vector2(560, 64));
                Icon(best, "trophy", Vector2.zero, 36, IconDim);
                bestValue[i] = Label(best, Center, new Vector2(0, 0.5f), Vector2.zero, new Vector2(400, 64), semi, semiShadow, 52, TextAlignmentOptions.Left, DimText);
            }
            Btn(over.transform, new Vector2(-128, -300), 176, "home", 72, false, () => Click(Home, true));
            Btn(over.transform, new Vector2(128, -300), 176, "retry", 80, true, () => Click(Retry, true));

            panels = new[] { hud, start, pause, over };
            Show(Screen.Start);
        }

        readonly CanvasGroup[] panels;

        public void Show(Screen s)
        {
            screen = s;
            panelShownAt = Time.realtimeSinceStartup;
            Set(start, s == Screen.Start);
            Set(hud, s == Screen.Hud || s == Screen.Pause);
            Set(pause, s == Screen.Pause);
            Set(over, s == Screen.Over);
            PauseButton.gameObject.SetActive(s == Screen.Hud);
            dimTarget = s == Screen.Start ? 0.4f : s == Screen.Pause ? 0.55f : s == Screen.Over ? 0.6f : 0;
        }

        public void SetBest(float bestSpeed, float bestDist)
        {
            Row(startBestSpeed, bestSpeed > 0 ? $"{bestSpeed:F1}<size=36><alpha=#B3> km/h" : "", 12);
            Row(startBestDist, bestDist > 0 ? $"{bestDist / 1000:F2}<size=36><alpha=#B3> km" : "", 12);
        }

        public void SetRun(double kmh, double meters)
        {
            distance.text = $"{Mono((meters / 1000).ToString("F2"))}<size=44><alpha=#BF> km";
            // Needle: critically damped spring (w 14 rad/s), at most 600 deg/s; digital value at 10 Hz from the real speed.
            float dt = Time.unscaledDeltaTime, w = 14;
            float acc = w * w * ((float)kmh - needleKmh) - 2 * w * needleVel;
            needleVel += acc * dt;
            needleVel = Mathf.Clamp(needleVel, -600 * 330 / 270f, 600 * 330 / 270f);
            needleKmh += needleVel * dt;
            needle.localRotation = Quaternion.Euler(0, 0, -(-135 + 270 * Mathf.Clamp(needleKmh, 0, 330) / 330));
            if ((digitalTimer -= dt) <= 0) { digitalTimer = 0.1f; speedValue.text = Mono(((int)kmh).ToString()); }
            float h = ((RectTransform)gauge.root).rect.height;
            gauge.localScale = Vector3.one * Mathf.Clamp(h * 0.3f / 320, 0.875f, 1);
        }

        public void SetResult(int i, string value, string unit, string best, bool newBest)
        {
            runValue[i].text = $"{Mono(value)}<size=48><alpha=#BF> {unit}";
            runValue[i].color = newBest ? Good : Color.white;
            Row(bestValue[i], $"{best} {unit}", 14);
            bestValue[i].color = newBest ? Good : DimText;
            star[i].enabled = newBest;
        }

        // Fixed-width digits; the decimal point keeps its own narrow width.
        static string Mono(string s) => "<mspace=0.6em>" + s.Replace(".", "</mspace>.<mspace=0.6em>") + "</mspace>";

        // Steering ring: screen-space origin and finger x, or null when no steering finger.
        public void SetTouch(Vector2? origin, float fingerX)
        {
            var c = (RectTransform)ring_.parent;
            if (origin is Vector2 o)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(c, o, null, out var p);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(c, new Vector2(fingerX, o.y), null, out var f);
                ring_.anchoredPosition = p + c.rect.size * c.pivot;
                dot.anchoredPosition = new Vector2(Mathf.Clamp(f.x - p.x, -56, 56), 0);
            }
            ringAlpha = Mathf.MoveTowards(ringAlpha, origin.HasValue ? 1 : 0, Time.unscaledDeltaTime / (origin.HasValue ? 0.08f : 0.15f));
            touch.alpha = screen == Screen.Hud ? ringAlpha : 0;
        }

        // side: 0 left, 1 right, -1 all four. Full at once, hold, then fade out over `fadeOut` seconds.
        // Light hits flash amber, a crash flashes white (red belongs to the player car).
        public void Flash(int side, float alpha, float hold, float fadeOut)
        {
            edgeColor = side < 0 ? Color.white : Amber;
            if (side < 0) { whiteAlpha = 0.35f; whiteHold = hold; }
            for (int i = 0; i < 4; i++)
                if (side < 0 || i == side) { edgeAlpha[i] = Mathf.Max(edgeAlpha[i], alpha); edgeHold[i] = hold; edgeRate[i] = alpha / fadeOut; }
        }

        public void Tick()
        {
            float dt = Time.unscaledDeltaTime;
            var c = dim.color;
            c.a = Mathf.MoveTowards(c.a, dimTarget, dt / 0.25f);
            dim.color = c;
            if ((whiteHold -= dt) <= 0) whiteAlpha = Mathf.MoveTowards(whiteAlpha, 0, dt * 0.35f / 0.5f);
            white.color = new Color(1, 1, 1, whiteAlpha);
            for (int i = 0; i < 4; i++)
            {
                if ((edgeHold[i] -= dt) <= 0) edgeAlpha[i] = Mathf.MoveTowards(edgeAlpha[i], 0, edgeRate[i] * dt);
                var e = edgeColor; e.a = edgeAlpha[i];
                edges[i].color = e;
            }
            foreach (var g in panels)
                g.alpha = Mathf.MoveTowards(g.alpha, g.blocksRaycasts ? 1 : 0, dt / 0.2f);
            var sa = UnityEngine.Screen.safeArea;
            if (sa != lastSafe)
            {
                lastSafe = sa;
                safe.anchorMin = sa.min / new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
                safe.anchorMax = sa.max / new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
            }
        }

        // Buttons on a newly shown panel ignore taps for 0.6 s (except Start), so a thumb left on the glass can't press Retry.
        void Click(Action a, bool guard)
        {
            if (guard && Time.realtimeSinceStartup - panelShownAt < 0.6f) return;
            a?.Invoke();
        }

        // ---- building blocks ----

        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        static Color Hex(int rgb) => new Color((rgb >> 16 & 255) / 255f, (rgb >> 8 & 255) / 255f, (rgb & 255) / 255f);

        static void Set(CanvasGroup g, bool on)
        {
            g.blocksRaycasts = g.interactable = on;
            if (on) g.gameObject.SetActive(true);
        }

        CanvasGroup Panel(string name)
        {
            var g = Stretch(safe, name).gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0;
            return g;
        }

        static RectTransform Stretch(Transform parent, string name)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false);
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            return r;
        }

        static RectTransform Place(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false);
            r.anchorMin = r.anchorMax = anchor; r.pivot = pivot;
            r.anchoredPosition = pos; r.sizeDelta = size;
            return r;
        }

        static Image Img(RectTransform r, Sprite s, Color c)
        {
            var i = r.gameObject.AddComponent<Image>();
            i.sprite = s; i.color = c; i.raycastTarget = false;
            if (s != null && s.border != Vector4.zero) i.type = Image.Type.Sliced;
            return i;
        }

        static Image Icon(Transform parent, string name, Vector2 pos, float size, Color c)
            => Img(Place(parent, name, Center, Center, pos, new Vector2(size, size)), Resources.Load<Sprite>("Icons/" + name), c);

        static Image EdgeImage(Transform parent, Sprite s, Vector2 min, Vector2 max, Vector2 pivot, Vector2 size, float flip)
        {
            var r = new GameObject("Edge", typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false);
            r.anchorMin = min; r.anchorMax = max; r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = Vector2.zero;
            r.localScale = s == edgeH ? new Vector3(flip, 1, 1) : new Vector3(1, flip, 1);
            return Img(r, s, Color.clear);
        }

        static TMP_Text Label(Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, TMP_FontAsset font, Material mat, float fontSize, TextAlignmentOptions align, Color c)
        {
            var t = Place(parent, "Text", anchor, pivot, pos, size).gameObject.AddComponent<TextMeshProUGUI>();
            if (font) t.font = font;
            if (mat) t.fontSharedMaterial = mat;
            t.fontSize = fontSize; t.alignment = align; t.color = c;
            t.raycastTarget = false; t.textWrappingMode = TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }

        // Icon + text row, centred as a whole: the icon is the parent's first child, the text starts after it.
        static void Row(TMP_Text t, string text, float gap)
        {
            t.text = text;
            var icon = (RectTransform)t.transform.parent.GetChild(0);
            icon.gameObject.SetActive(text.Length > 0);
            float iw = icon.sizeDelta.x, total = iw + gap + t.GetPreferredValues(text).x;
            icon.anchoredPosition = new Vector2(-total / 2 + iw / 2, 0);
            t.rectTransform.anchoredPosition = new Vector2(-total / 2 + iw + gap, 0);
        }

        TMP_Text BestGroup(Transform parent, Vector2 pos)
        {
            var g = Place(parent, "Best", Center, Center, pos, new Vector2(460, 72));
            Icon(g, "trophy", Vector2.zero, 40, IconDim);
            return Label(g, Center, new Vector2(0, 0.5f), Vector2.zero, new Vector2(400, 72), semi, semiShadow, 52, TextAlignmentOptions.Left, new Color(1, 1, 1, 0.9f));
        }

        static void Btn(Transform parent, Vector2 pos, float size, string icon, float iconSize, bool primary, Action onClick)
        {
            var r = Place(parent, icon, Center, Center, pos, new Vector2(size, size));
            var bg = Img(r, rounded, primary ? Primary : new Color(1, 1, 1, 0.1f));
            bg.raycastTarget = true;
            if (!primary) Img(Stretch(r, "Outline"), roundedOutline, new Color(1, 1, 1, 0.55f));
            Icon(r, icon, new Vector2(icon == "play" ? 6 : 0, 0), iconSize, primary ? Color.white : new Color(1, 1, 1, 0.9f));
            var b = r.gameObject.AddComponent<Button>();
            var cb = b.colors;
            cb.pressedColor = primary ? new Color(0.77f, 0.8f, 0.86f) : new Color(1, 1, 1, 1.8f);
            cb.fadeDuration = 0.06f;
            b.colors = cb;
            b.onClick.AddListener(() => onClick());
        }

        // Rounded-rect sprites from a signed distance: fill when stroke = 0, outline otherwise.
        static Sprite Shape(int w, int h, float radius, float stroke, int border)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float qx = Mathf.Abs(x + 0.5f - w / 2f) - (w / 2f - radius), qy = Mathf.Abs(y + 0.5f - h / 2f) - (h / 2f - radius);
                    float d = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0) - radius;
                    float a = stroke > 0 ? Mathf.Clamp01(0.5f - (Mathf.Abs(d + stroke / 2) - stroke / 2)) : Mathf.Clamp01(0.5f - d);
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new UnityEngine.Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        static Sprite Gradient(bool horizontal)
        {
            int n = 256;
            var tex = new Texture2D(horizontal ? n : 4, horizontal ? 4 : n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int i = 0; i < n; i++)
                for (int j = 0; j < 4; j++)
                {
                    float a = Mathf.SmoothStep(1, 0, i / (n - 1f));
                    if (horizontal) tex.SetPixel(i, j, new Color(1, 1, 1, a));
                    else tex.SetPixel(j, n - 1 - i, new Color(1, 1, 1, a));
                }
            tex.Apply();
            return Sprite.Create(tex, new UnityEngine.Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        static void MakeSprites()
        {
            if (circle) return;
            circle = Shape(128, 128, 64, 0, 0);
            ring = Shape(256, 256, 128, 6, 0);
            rounded = Shape(128, 128, 44, 0, 48);
            roundedOutline = Shape(128, 128, 44, 4, 48);
            edgeH = Gradient(true);
            edgeV = Gradient(false);
        }
    }
}
