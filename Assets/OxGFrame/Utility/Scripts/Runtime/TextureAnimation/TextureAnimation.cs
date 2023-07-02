using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OxGFrame.Utility.TextureAnime
{
    [AddComponentMenu("OxGFrame/Utility/TextureAnime/TextureAnimation")]
    [ExecuteInEditMode]
    public class TextureAnimation : MonoBehaviour
    {
        enum PlayMode
        {
            Normal,
            Reverse,
            PingPong,
            PingPongReverse
        }

        [SerializeField]
        private List<Sprite> _sprites = new List<Sprite>();
        [SerializeField]
        private bool _isLoop = false;
        [SerializeField]
        private PlayMode _playMode = PlayMode.Normal;
        [SerializeField]
        private int _frameRate = 30;
        [SerializeField]
        private bool _ignoreTimeScale = true;

        private float _dt = 0;
        private int _spIdx = 0;

        private bool _pingPongStart = false;
        private int _pingPongCount = 0;

        private SpriteRenderer _spr = null;
        private Image _image = null;

        private void Awake()
        {
            do
            {
                this._spr = this.transform.GetComponent<SpriteRenderer>();
                if (this._spr != null) break;
                this._image = this.transform.GetComponent<Image>();
                if (this._image != null) break;
            } while (false);
        }

        private void Start()
        {
            this.ResetAnime();
        }

        private void Update()
        {
            if (this._sprites.Count == 0) return;

            this._UpdateTextureAnimation(this._ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            // Ensure continuous Update calls.
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
#endif
        }

        private void OnValidate()
        {
            this.ResetAnime();
        }

        private void OnEnable()
        {
            this.ResetAnime();
        }

        #region Public Methods
        public void SetIgnoreScale(bool ignore)
        {
            this._ignoreTimeScale = ignore;
        }

        public void SetFrameRate(int frameRate)
        {
            this._frameRate = frameRate;
        }

        public void ResetAnime()
        {
            this._dt = 0;
            this._pingPongCount = 0;
            if (this._playMode == PlayMode.PingPong) this._pingPongStart = true;
            else if (this._playMode == PlayMode.PingPongReverse) this._pingPongStart = false;
        }
        #endregion

        private void _AutoRefreshSprite(Sprite sp)
        {
            if (this._spr != null) this._spr.sprite = sp;
            else if (this._image != null) this._image.sprite = sp;
        }

        private void _UpdateTextureAnimation(float dt)
        {
            float fps = this._frameRate;
            this._dt += dt;

            this._spIdx = Mathf.FloorToInt(this._dt * fps);

            switch (this._playMode)
            {
                case PlayMode.Normal:
                    this._ModeNormal();
                    break;

                case PlayMode.Reverse:
                    this._ModeReverse();
                    break;

                case PlayMode.PingPong:
                    this._ModePingPong();
                    break;

                case PlayMode.PingPongReverse:
                    this._ModePingPongReverse();
                    break;
            }
        }

        private void _ModeNormal()
        {
            if (!this._isLoop && this._spIdx >= this._sprites.Count) return;

            this._spIdx %= this._sprites.Count;
            this._AutoRefreshSprite(this._sprites[this._spIdx]);
        }

        private void _ModeReverse()
        {
            if (!this._isLoop && this._spIdx >= this._sprites.Count) return;

            this._spIdx %= this._sprites.Count;
            int lastFrame = this._sprites.Count - 1;
            int revSpIdx = lastFrame - this._spIdx;
            this._AutoRefreshSprite(this._sprites[revSpIdx]);
        }

        private void _ModePingPong()
        {
            if (this._pingPongStart)
            {
                if (!this._isLoop)
                {
                    if (this._pingPongCount >= 2) return;
                }

                if (this._spIdx >= (this._sprites.Count - 1))
                {
                    if (!this._isLoop) this._pingPongCount++;

                    this._pingPongStart = false;
                    this._dt = 0;
                }

                this._spIdx %= this._sprites.Count;
                this._AutoRefreshSprite(this._sprites[this._spIdx]);
            }
            else
            {
                if (this._spIdx >= (this._sprites.Count - 1))
                {
                    if (!this._isLoop) this._pingPongCount++;

                    this._pingPongStart = true;
                    this._dt = 0;
                }

                this._spIdx %= this._sprites.Count;
                int lastFrame = this._sprites.Count - 1;
                int reverseSpIdx = lastFrame - this._spIdx;
                this._AutoRefreshSprite(this._sprites[reverseSpIdx]);
            }
        }

        private void _ModePingPongReverse()
        {
            if (this._pingPongStart)
            {
                if (this._spIdx >= (this._sprites.Count - 1))
                {
                    if (!this._isLoop) this._pingPongCount++;

                    this._pingPongStart = false;
                    this._dt = 0;
                }

                this._spIdx %= this._sprites.Count;
                this._AutoRefreshSprite(this._sprites[this._spIdx]);
            }
            else
            {
                if (!this._isLoop)
                {
                    if (this._pingPongCount >= 2) return;
                }

                if (this._spIdx >= (this._sprites.Count - 1))
                {
                    if (!this._isLoop) this._pingPongCount++;

                    this._pingPongStart = true;
                    this._dt = 0;
                }

                this._spIdx %= this._sprites.Count;
                int lastFrame = this._sprites.Count - 1;
                int reverseSpIdx = lastFrame - this._spIdx;
                this._AutoRefreshSprite(this._sprites[reverseSpIdx]);
            }
        }
    }
}