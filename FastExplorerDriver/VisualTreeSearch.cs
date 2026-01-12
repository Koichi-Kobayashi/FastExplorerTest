using System.Collections.Generic;
using System.Windows;
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
    }
}

