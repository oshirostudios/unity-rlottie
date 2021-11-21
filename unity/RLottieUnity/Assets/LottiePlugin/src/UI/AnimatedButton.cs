using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LottiePlugin.UI
{
    public sealed class AnimatedButton : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [System.Serializable]
        public struct State
        {
            public string Name;
            public int FrameNumber;
            public bool StayHere;
        }
        [System.Serializable]
        public class ButtonClickedEvent : UnityEngine.Events.UnityEvent<int, State> { }


        internal TextAsset AnimationJson => _animationJson;
        internal uint TextureWidth => _textureWidth;
        internal uint TextureHeight => _textureHeight;

        internal Graphic Graphic => _graphic;
        internal State[] States => _states;

        [SerializeField] private TextAsset _animationJson;
        [SerializeField] private uint _textureWidth;
        [SerializeField] private uint _textureHeight;
        [SerializeField] private Graphic _graphic;
        [SerializeField] private State[] _states;
        [SerializeField] private ButtonClickedEvent _onClick = new ButtonClickedEvent();

        private int _currentStateIndex;
        private LottieAnimation _lottieAnimation;

        protected override void Start()
        {
            base.Start();
            _lottieAnimation = LottieAnimation.LoadFromJsonData(
                _animationJson.text,
                string.Empty,
                _textureWidth,
                _textureHeight);
            _lottieAnimation.DrawOneFrame(_states[0].FrameNumber);
            ((RawImage)_graphic).texture = _lottieAnimation.Texture;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _lottieAnimation?.Dispose();
            _lottieAnimation = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }
        public void OnSubmit(BaseEventData eventData)
        {
            Press();
        }

        private void Press()
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }
            _onClick.Invoke(_currentStateIndex, _states[_currentStateIndex]);
            StartCoroutine(AnimateToNextState());
        }
        private IEnumerator AnimateToNextState()
        {
            _currentStateIndex++;
            if (_currentStateIndex >= _states.Length)
            {
                _currentStateIndex = 0;
            }
            _lottieAnimation.Play();
            State nextState = _states[_currentStateIndex];
            while (_lottieAnimation.CurrentFrame < nextState.FrameNumber)
            {
                _lottieAnimation.Update();
                yield return null;
            }
        }
    }
}