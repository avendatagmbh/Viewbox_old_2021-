using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils {
    public static class LinqExtensions {
        public static IEnumerable<T> AsDepthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc) {
            yield return head;
            foreach (var node in childrenFunc(head)) {
                foreach (var child in AsDepthFirstEnumerable(node, childrenFunc)) {
                    yield return child;
                }
            }
        }

        public static IEnumerable<T> AsBreadthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc) {
            yield return head;
            var last = head;
            foreach (var node in AsBreadthFirstEnumerable(head, childrenFunc)) {
                foreach (var child in childrenFunc(node)) {
                    yield return child;
                    last = child;
                }
                if (last.Equals(node))
                    yield break;
            }
        }

        public static void ForEachElse<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> action, Action @else) {
            foreach (TSource i in source) {
                if (!action(i))
                    return;
            }
            @else();
        }
    }
}
