﻿using GalaSoft.MvvmLight;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CharacterMap.Controls
{
    public class XamlTitleBarTemplateSettings : ViewModelBase
    {
        private GridLength _leftColumnWidth = new GridLength(0);
        public GridLength LeftColumnWidth
        {
            get => _leftColumnWidth;
            internal set => Set(ref _leftColumnWidth, value);
        }

        private GridLength _rightColumnWidth = new GridLength(0);
        public GridLength RightColumnWidth
        {
            get => _rightColumnWidth;
            internal set => Set(ref _rightColumnWidth, value);
        }

        private Color _backgroundColor = Colors.Transparent;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            internal set => Set(ref _backgroundColor, value);
        }
    }

    public sealed class XamlTitleBar : ContentControl
    {
        private UISettings _settings;
        private CoreApplicationViewTitleBar _titleBar;
        private CoreWindow _window;
        private FrameworkElement _backgroundElement;

        public XamlTitleBarTemplateSettings TemplateSettings { get; } = new XamlTitleBarTemplateSettings();

        public XamlTitleBar()
        {
            DefaultStyleKey = typeof(XamlTitleBar);
            Loaded += XamlTitleBar_Loaded;
            Unloaded += XamlTitleBar_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("BackgroundElement") is FrameworkElement e)
            {
                _backgroundElement = e;
                UpdateDragElement();
            }
        }

        private void XamlTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            HookListeners();
            UpdateDragElement();
            _titleBar.ExtendViewIntoTitleBar = true;
        }

        private void XamlTitleBar_Unloaded(object sender, RoutedEventArgs e)
        {
            UnhookListeners();
        }

        private void HookListeners()
        {
            UnhookListeners();

            CoreApplicationView view = CoreApplication.GetCurrentView();

            _titleBar = view.TitleBar;
            _titleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;

            _window = view.CoreWindow;
            _window.Activated += _window_Activated;

            _settings = new UISettings();
            _settings.ColorValuesChanged += _settings_ColorValuesChanged;

            UpdateColors();
            UpdateMetrics(_titleBar);
        }

        private void UnhookListeners()
        {
            if (_settings != null)
            {
                _settings.ColorValuesChanged -= _settings_ColorValuesChanged;
            }
            _settings = null;

            if (_titleBar != null)
            {
                _titleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
            }
            _titleBar = null;

            if (_window != null)
            {
                _window.Activated -= _window_Activated;
            }
            _window = null;
        }

        private void _settings_ColorValuesChanged(UISettings sender, object args)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, UpdateColors);
        }

        private void _window_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            UpdateColors();
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateMetrics(sender);
        }

        private void UpdateColors()
        {
            bool active = _window.ActivationMode == CoreWindowActivationMode.ActivatedInForeground;

            TemplateSettings.BackgroundColor = _settings.GetColorValue(active ? UIColorType.Accent : UIColorType.AccentDark1);

            var accentColor = _settings.GetColorValue(UIColorType.Accent);
            var darkAccent = _settings.GetColorValue(UIColorType.AccentDark1);
            var btnHoverColor = _settings.GetColorValue(UIColorType.AccentLight1);

            Edi.UWP.Helpers.UI.ApplyColorToTitleBar(
                accentColor,
                Colors.White,
                darkAccent,
                Colors.Gray);

            Edi.UWP.Helpers.UI.ApplyColorToTitleButton(
                Colors.Transparent, Colors.White,
                btnHoverColor, Colors.White,
                accentColor, Colors.White,
                Colors.Transparent, Colors.Gray);

            RequestedTheme = !IsAccentColorDark() ? ElementTheme.Light : ElementTheme.Dark;
        }

        private void UpdateMetrics(CoreApplicationViewTitleBar bar)
        {
            bool ltr = FlowDirection == FlowDirection.LeftToRight;

            Height = bar.Height;

            TemplateSettings.LeftColumnWidth 
                = new GridLength(ltr ? bar.SystemOverlayLeftInset : bar.SystemOverlayRightInset);

            TemplateSettings.RightColumnWidth 
                = new GridLength(ltr ? bar.SystemOverlayRightInset : bar.SystemOverlayLeftInset);
        }

        private void UpdateDragElement()
        {
            if (_backgroundElement != null)
            {
                Window.Current.SetTitleBar(_backgroundElement);
            }
        }

        private bool IsAccentColorDark()
        {
            var c = _settings.GetColorValue(UIColorType.Accent);
            var isDark = (5 * c.G + 2 * c.R + c.B) <= 8 * 128;
            return isDark;
        }
    }
}
