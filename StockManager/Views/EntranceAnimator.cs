using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StockManager.Views;

public static class EntranceAnimator
{
    public static void AnimateSequence(params UIElement?[] elements)
    {
        const int baseDelayMs = 40;

        for (var index = 0; index < elements.Length; index++)
        {
            if (elements[index] is not UIElement element)
                continue;

            AnimateIn(element, index * baseDelayMs);
        }
    }

    public static void AnimateIn(UIElement element, int delayMs = 0, double offsetY = 18)
    {
        var transform = element.RenderTransform as TranslateTransform;
        if (transform == null)
        {
            transform = new TranslateTransform();
            element.RenderTransform = transform;
        }

        element.Opacity = 0;
        transform.Y = offsetY;

        var duration = TimeSpan.FromMilliseconds(320);
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

        var opacityAnimation = new DoubleAnimation(0, 1, duration)
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = easing
        };

        var translateAnimation = new DoubleAnimation(offsetY, 0, duration)
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = easing
        };

        element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        transform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
    }
}
