using System.Windows.Data;
using System.Windows.Controls;
using System;
using System.Windows;

namespace AvdWpfControls.Handlers
{
    /// <summary>
    /// Use BindingExpression with exception handling.
    /// </summary>
    public static class BindingExpressionWithException
    {
        #region BindingExpressionExceptionHandler
        /// <summary>
        /// Bindings the expression update with exception handler.
        /// </summary>
        /// <param name="pControl">The control.</param>
        /// <param name="pDepProp">The dependency property.</param>
        public static void BindingExpressionUpdateWithExceptionHandler(Control pControl, DependencyProperty pDepProp)
        {
            if (pControl == null || pDepProp == null)
                return;

            BindingExpression myBindingExpression = pControl.GetBindingExpression(pDepProp);
            if (myBindingExpression == null)
                return;

            Binding myBinding = myBindingExpression.ParentBinding;
            if (myBinding == null)
                return;

            myBinding.UpdateSourceExceptionFilter = new UpdateSourceExceptionFilterCallback(BindingExpressionExceptionHandler);
            myBindingExpression.UpdateSource();
        }

        /// <summary>
        /// Bindings the expression exception handler.
        /// </summary>
        /// <param name="bindingExpression">The binding expression.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private static object BindingExpressionExceptionHandler(object bindingExpression, Exception exception)
        {
            throw exception;
            return exception.Message.ToString();
        }
        #endregion
    }
}
