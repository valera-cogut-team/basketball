using Basketball.Domain;
using Basketball.Facade;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Basketball.Presentation
{
    /// <summary>
    /// HUD bound to <see cref="IBasketballFacade"/> reactive state (UniRx).
    /// </summary>
    public sealed class BasketballHud : MonoBehaviour
    {
        private IBasketballFacade _basketball;
        private bool _uiBuilt;
        private CompositeDisposable _bindings;

        private TMP_Text _scoreLine;
        private TMP_Text _statusLine;
        private TMP_Text _controlsLine;

        public void Initialize(IBasketballFacade basketball)
        {
            if (!_uiBuilt)
            {
                BuildUi();
                _uiBuilt = true;
            }

            _bindings?.Dispose();
            _bindings = new CompositeDisposable();
            _basketball = basketball;

            if (_basketball == null)
                return;

            Observable
                .CombineLatest(_basketball.ScoreRx, _basketball.BestScoreRx, (score, best) => (score, best))
                .Subscribe(t => RefreshScoreLine(t.score, t.best))
                .AddTo(_bindings);

            Observable
                .CombineLatest(_basketball.PhaseRx, _basketball.AimChargeRx,
                    (phase, aim) => (phase, aim))
                .Subscribe(t => RefreshStatusLine(t.phase, t.aim))
                .AddTo(_bindings);
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
            _bindings = null;
            _basketball = null;
        }

        private void RefreshScoreLine(int score, int best)
        {
            if (_scoreLine == null)
                return;
            _scoreLine.text =
                $"<b>SCORE</b>  <color=#FFC94A>{score}</color>     " +
                $"<b>BEST</b>  <color=#6ED9BE>{best}</color>";
        }

        private void RefreshStatusLine(BasketballBallPhase phase, float aim01)
        {
            if (_statusLine == null)
                return;
            var charge = Mathf.RoundToInt(aim01 * 100f);
            _statusLine.text =
                $"<color=#B8C5D6>{phase}</color>   " +
                $"<color=#8899AA>power</color> <color=#E8F0FF>{charge}</color><color=#8899AA>%</color>  " +
                "<size=-2>up — power & arc · left/right — aim · release — throw</size>";
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("BasketballHud_Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.layer = 5; // UI

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2500;
            canvas.overrideSorting = true;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.55f;

            canvasGo.AddComponent<GraphicRaycaster>();

            var panelGo = new GameObject("Panel", typeof(RectTransform));
            panelGo.transform.SetParent(canvasGo.transform, false);
            panelGo.layer = 5;

            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);
            panelRt.pivot = new Vector2(0f, 1f);
            panelRt.anchoredPosition = new Vector2(28f, -24f);
            panelRt.sizeDelta = new Vector2(720f, 0f);

            var shadow = panelGo.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(3f, -3f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.09f, 0.14f, 0.88f);
            bg.raycastTarget = false;

            var vlg = panelGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(22, 22, 18, 18);
            vlg.spacing = 10f;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var fitter = panelGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var panelLayout = panelGo.AddComponent<LayoutElement>();
            panelLayout.minWidth = 720f;
            panelLayout.preferredWidth = 720f;

            _scoreLine = CreateTmpLine(panelGo.transform, "ScoreLine", 32f, FontStyles.Bold, new Color(0.96f, 0.97f, 1f));
            _scoreLine.margin = new Vector4(0f, 0f, 0f, 2f);
            var scoreLe = _scoreLine.gameObject.AddComponent<LayoutElement>();
            scoreLe.minHeight = 44f;
            scoreLe.preferredHeight = 48f;
            scoreLe.flexibleWidth = 1f;

            _statusLine = CreateTmpLine(panelGo.transform, "StatusLine", 22f, FontStyles.Normal, new Color(0.88f, 0.92f, 0.96f));
            var statusLe = _statusLine.gameObject.AddComponent<LayoutElement>();
            statusLe.minHeight = 52f;
            statusLe.preferredHeight = 56f;
            statusLe.flexibleWidth = 1f;

            _controlsLine = CreateTmpLine(panelGo.transform, "ControlsLine", 17f, FontStyles.Normal, new Color(0.65f, 0.72f, 0.82f));
            _controlsLine.text =
                "WASD — move · hold Fire2 / RMB + mouse — look · gamepad via Input System";
            var ctrlLe = _controlsLine.gameObject.AddComponent<LayoutElement>();
            ctrlLe.minHeight = 28f;
            ctrlLe.preferredHeight = 30f;
            ctrlLe.flexibleWidth = 1f;
        }

        private static TMP_Text CreateTmpLine(Transform parent, string name, float fontSize, FontStyles style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = 5;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            var font = TMP_Settings.defaultFontAsset;
            if (font == null)
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null)
            {
                tmp.font = font;
                tmp.fontSharedMaterial = font.material;
            }

            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.richText = true;
            tmp.raycastTarget = false;
            tmp.outlineWidth = 0.15f;
            tmp.outlineColor = new Color32(0, 0, 0, 140);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, fontSize + 14f);

            return tmp;
        }
    }
}
