﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Windows.Media;
using System.Windows.Controls;

namespace AvdWpfControls.Themes
{
    partial class BlueDesign
    {
        private void AssistantControlLast_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var assistant = button.TemplatedParent as AssistantControl;
            if (assistant != null) assistant.NavigateBack();
        }

        private void AssistantControlNext_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var assistant = button.TemplatedParent as AssistantControl;
            if (assistant != null)
            {
                if (!assistant.NextAllowed && !string.IsNullOrEmpty(assistant.NextButtonCaptionLastPage))
                {
                    assistant.OnFinish();
                }
                else
                    assistant.NavigateNext();
            }
        }

        private void AssistantControlOk_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var assistant = button.TemplatedParent as AssistantControl;
            if (assistant != null) assistant.OnOk();
        }

        private void AssistantControlCancel_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var assistant = button.TemplatedParent as AssistantControl;
            if (assistant != null)
            {
                assistant.OnCancel();
                if (assistant.CloseOnCancel)
                {
                    var window = TryFindParent<Window>(button);
                    if (window != null)
                        window.Close();
                }
            }

        }

        private static DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null) return null;
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }
            return VisualTreeHelper.GetParent(child);
        }

        private static T TryFindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            DependencyObject parentObject = GetParentObject(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            return parent ?? TryFindParent<T>(parentObject);
        }

        private void AvdSlideOutDialogMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var element = ((UIElement)sender);
            var parent = TryFindParent<AvdSlideOutDialog>(element);
            if (!parent.AnimationTriggerEnabled) return;
            new Thread(parent.Expand).Start();
        }

        private void AvdSlideOutDialogMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var element = ((UIElement)sender);
            var parent = TryFindParent<AvdSlideOutDialog>(element);
            if (!parent.AnimationTriggerEnabled) return;
            new Thread(parent.Collapse).Start();
        }
    }
}
