﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Unicord.Universal.Utilities
{
    internal class SwipeOpenHelper
    {
        private class AdditionalElementHandlerData
        {
            public PointerEventHandler Pressed;
            public PointerEventHandler Moved;
            public PointerEventHandler Released;
            public PointerEventHandler Cancelled;
        }

        private GestureRecognizer _recogniser;
        private UIElement _target;
        private UIElement _reference;
        private TranslateTransform _transform;
        private Storyboard _openAnimation;
        private Storyboard _closeAnimation;
        private Dictionary<UIElement, AdditionalElementHandlerData> _attachedElements;
        private double _xTotal = 0.0;
        private bool _isActive;

        public bool IsEnabled { get; set; }

        public SwipeOpenHelper(UIElement target, UIElement reference, Storyboard openAnimation, Storyboard closeAnimation)
        {
            _attachedElements = new Dictionary<UIElement, AdditionalElementHandlerData>();
            _recogniser = new GestureRecognizer() { GestureSettings = GestureSettings.ManipulationTranslateX };
            _target = target;
            _reference = reference;
            _transform = _target.RenderTransform as TranslateTransform;
            _openAnimation = openAnimation;
            _closeAnimation = closeAnimation;

            target.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnPointerPressed), false);
            target.AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(OnPointerMoved), false);
            target.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased), false);
            target.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(OnPointerCanceled), false);

            _recogniser.ManipulationStarted += OnManipulationStarted;
            _recogniser.ManipulationUpdated += OnManipulationUpdated;
            _recogniser.ManipulationCompleted += OnManipulationCompleted;
        }

        public void AddAdditionalElement(UIElement element, bool handled = true)
        {
            var data = new AdditionalElementHandlerData()
            {
                Pressed = new PointerEventHandler(OnPointerPressed),
                Moved = new PointerEventHandler(OnPointerMoved),
                Released = new PointerEventHandler(OnPointerReleased),
                Cancelled = new PointerEventHandler(OnPointerCanceled)
            };

            if (!_attachedElements.ContainsKey(element))
            {
                _attachedElements.Add(element, data);
                element.AddHandler(UIElement.PointerPressedEvent, data.Pressed, handled);
                element.AddHandler(UIElement.PointerMovedEvent, data.Moved, handled);
                element.AddHandler(UIElement.PointerReleasedEvent, data.Released, handled);
                element.AddHandler(UIElement.PointerCanceledEvent, data.Cancelled, handled);

                if (element is FrameworkElement fuck)
                {
                    fuck.Unloaded += OnAdditionalElementUnloaded;
                }
            }
        }

        private void OnAdditionalElementUnloaded(object sender, RoutedEventArgs e)
        {
            var element = sender as UIElement;
            var data = _attachedElements[element];
            (sender as FrameworkElement).Unloaded -= OnAdditionalElementUnloaded;

            element.RemoveHandler(UIElement.PointerPressedEvent, data.Pressed);
            element.RemoveHandler(UIElement.PointerMovedEvent, data.Moved);
            element.RemoveHandler(UIElement.PointerReleasedEvent, data.Released);
            element.RemoveHandler(UIElement.PointerCanceledEvent, data.Cancelled);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!IsEnabled || _recogniser.IsActive)
                return;

            (sender as UIElement).CapturePointer(e.Pointer);
            _recogniser.ProcessDownEvent(e.GetCurrentPoint(sender as UIElement));
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsEnabled)
                return;

            _recogniser.ProcessMoveEvents(e.GetIntermediatePoints(_reference));
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!IsEnabled)
                return;

            _recogniser.ProcessUpEvent(e.GetCurrentPoint(sender as UIElement));
            _target.ReleasePointerCapture(e.Pointer);
        }

        private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (!IsEnabled)
                return;

            _recogniser.CompleteGesture();
            _target.ReleasePointerCapture(e.Pointer);
        }

        private void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            if (!IsEnabled)
                return;

            _xTotal = _transform.X;
        }

        private void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            if (!IsEnabled)
                return;

            _xTotal += args.Delta.Translation.X;

            if (_xTotal > 16)
                _isActive = true;

            if (_isActive)
            {
                //var delta = Math.Max(Math.Abs(276 - _xTotal), 64) / 64d;
                _transform.X = Math.Max(Math.Min(_xTotal, 276), 0);
            }
        }

        private void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            if (!IsEnabled)
                return;

            if (_xTotal > 128)
            {
                _openAnimation.Begin();
            }
            else
            {
                _closeAnimation.Begin();
            }

            _xTotal = 0;
            _isActive = false;
        }

        internal void Cancel()
        {
            _recogniser.CompleteGesture();
            _isActive = false;
            _xTotal = 0;
        }
    }
}
