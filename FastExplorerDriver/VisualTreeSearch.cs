using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FastExplorerDriver
{
    /// <summary>
    /// Targetプロセス側で動くビジュアルツリー検索ユーティリティ（FriendlyのDLL injection用）。
    /// </summary>
    public sealed class VisualTreeSearch
    {
        private VisualTreeSearch() { }

        public static DependencyObject? FindByName(DependencyObject root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
                return null;

            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current is FrameworkElement fe && fe.Name == name)
                    return fe;

                if (current is FrameworkContentElement fce && fce.Name == name)
                    return fce;

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child != null)
                        queue.Enqueue(child);
                }
            }

            return null;
        }

        public static DependencyObject? FindVisibleByName(DependencyObject root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
                return null;

            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current is FrameworkElement fe && fe.Name == name && fe.Visibility == Visibility.Visible)
                    return fe;

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child != null)
                        queue.Enqueue(child);
                }
            }

            return null;
        }

        public static DependencyObject? FindByTypeFullName(DependencyObject root, string typeFullName)
        {
            if (root == null || string.IsNullOrEmpty(typeFullName))
                return null;

            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentType = current.GetType();

                if (currentType.FullName == typeFullName)
                    return current;

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child != null)
                        queue.Enqueue(child);
                }
            }

            return null;
        }

        public static DependencyObject? FindInAllPresentationSourcesByTypeFullName(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName))
                return null;

            var sources = PresentationSource.CurrentSources;
            if (sources == null)
                return null;

            foreach (PresentationSource source in sources)
            {
                var root = source.RootVisual as DependencyObject;
                if (root == null)
                    continue;

                var found = FindByTypeFullName(root, typeFullName);
                if (found != null)
                    return found;
            }

            return null;
        }

        public static DependencyObject? FindByTypeFullNameAndContentText(DependencyObject root, string typeFullName, string contentText)
        {
            if (root == null || string.IsNullOrEmpty(typeFullName) || string.IsNullOrEmpty(contentText))
                return null;

            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentType = current.GetType();

                if (currentType.FullName == typeFullName)
                {
                    var content = (current as FrameworkElement)?.GetValue(ContentControl.ContentProperty);
                    var contentString = content?.ToString() ?? string.Empty;
                    if (contentString.Equals(contentText, StringComparison.OrdinalIgnoreCase))
                        return current;
                }

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child != null)
                        queue.Enqueue(child);
                }
            }

            return null;
        }

        public static DependencyObject? FindMenuItemByName(DependencyObject root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
                return null;

            if (root is ItemsControl itemsControl)
            {
                foreach (var item in itemsControl.Items)
                {
                    if (item is MenuItem menuItem)
                    {
                        if (menuItem.Name == name)
                            return menuItem;

                        var nested = FindMenuItemByName(menuItem, name);
                        if (nested != null)
                            return nested;
                    }
                }
            }

            return null;
        }

        public static void RaiseRightClick(DependencyObject target)
        {
            if (target is not UIElement element)
                return;

            var down = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = UIElement.MouseRightButtonDownEvent,
                Source = element
            };
            element.RaiseEvent(down);

            var up = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = UIElement.MouseRightButtonUpEvent,
                Source = element
            };
            element.RaiseEvent(up);
        }

        public static bool InvokeMenuItemClick(DependencyObject target)
        {
            if (target is MenuItem menuItem)
            {
                return menuItem.Dispatcher.Invoke(() =>
                {
                    menuItem.Focus();
                    var down = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                    {
                        RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                        Source = menuItem
                    };
                    menuItem.RaiseEvent(down);

                    var up = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                    {
                        RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                        Source = menuItem
                    };
                    menuItem.RaiseEvent(up);

                    if (menuItem.Command != null && menuItem.Command.CanExecute(menuItem.CommandParameter))
                        menuItem.Command.Execute(menuItem.CommandParameter);

                    menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, menuItem));
                    return true;
                }, System.Windows.Threading.DispatcherPriority.Input);
            }

            if (target is FrameworkElement element)
            {
                return element.Dispatcher.Invoke(() =>
                {
                    element.Focus();
                    element.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, element));
                    return true;
                }, System.Windows.Threading.DispatcherPriority.Input);
            }

            return false;
        }

        public static bool InvokeMenuItemClickByName(DependencyObject root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
                return false;

            var target = FindByName(root, name);
            if (target == null)
                return false;

            return InvokeMenuItemClick(target);
        }

        public static bool OpenContextMenu(DependencyObject owner)
        {
            if (owner == null)
                return false;

            var menu = (owner as FrameworkElement)?.ContextMenu
                       ?? ContextMenuService.GetContextMenu(owner);
            if (menu == null)
                return false;

            menu.PlacementTarget = owner as UIElement;
            menu.IsOpen = true;
            return true;
        }

        public static bool ClickContextMenuItemByHeaderContains(DependencyObject owner, string headerContains)
        {
            if (owner == null || string.IsNullOrEmpty(headerContains))
                return false;

            var menu = (owner as FrameworkElement)?.ContextMenu
                       ?? ContextMenuService.GetContextMenu(owner);
            if (menu == null)
                return false;

            foreach (var item in menu.Items)
            {
                if (item is not MenuItem menuItem)
                    continue;

                var header = menuItem.Header?.ToString() ?? string.Empty;
                if (!header.Contains(headerContains, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!menuItem.IsEnabled)
                    return false;

                menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                return true;
            }

            return false;
        }
    }
}

