using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;

namespace AvdWpfControls
{
    public class CheckBoxWithInfoImage : CheckBox
    {
        #region [ Dependency properties ]

        public static DependencyProperty OuterBorderWidthProperty =
            DependencyProperty.Register(
                "OuterBorderWidth",
                typeof(int),
                typeof(CheckBoxWithInfoImage));

        public static DependencyProperty CheckboxBorderWidthProperty =
            DependencyProperty.Register(
                "CheckboxBorderWidth",
                typeof(int),
                typeof(CheckBoxWithInfoImage));

        public static DependencyProperty InfoTextProperty =
            DependencyProperty.Register(
                "InfoText",
                typeof(string),
                typeof(CheckBoxWithInfoImage));

        private static DependencyPropertyKey CheckboxBorderNotZeroPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "CheckboxBorderNotZero",
                typeof(bool),
                typeof(CheckBoxWithInfoImage),
                new PropertyMetadata());
        public static DependencyProperty CheckboxBorderNotZeroProperty = CheckboxBorderNotZeroPropertyKey.DependencyProperty;

        private static DependencyPropertyKey OuterBorderNotZeroPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "OuterBorderNotZero",
                typeof(bool),
                typeof(CheckBoxWithInfoImage),
                new PropertyMetadata());
        public static DependencyProperty OuterBorderNotZeroProperty = OuterBorderNotZeroPropertyKey.DependencyProperty;

        private static DependencyPropertyKey HasInfoTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasInfoText",
                typeof(bool),
                typeof(CheckBoxWithInfoImage),
                new PropertyMetadata());
        public static DependencyProperty HasInfoTextProperty = HasInfoTextPropertyKey.DependencyProperty;

        public static DependencyProperty ShowOuterBorderProperty =
            DependencyProperty.Register(
                "ShowOuterBorder",
                typeof(bool),
                typeof(CheckBoxWithInfoImage));
        #endregion

        #region [ Constructor ]
        static CheckBoxWithInfoImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBoxWithInfoImage),
                new FrameworkPropertyMetadata(typeof(CheckBoxWithInfoImage)));            
        }

        public CheckBoxWithInfoImage()
            : base()
        {
            Initialized += new EventHandler(CheckBoxWithInfoImage_Initialized);
        }
        #endregion

        #region [ Events ]
        void CheckBoxWithInfoImage_Initialized(object sender, EventArgs e)
        {
            HasInfoText = !string.IsNullOrWhiteSpace(InfoText);
            CheckboxBorderNotZero = (CheckboxBorderWidth > 0);
            OuterBorderNotZero = (OuterBorderWidth > 0);
        } 
        #endregion

        #region [ Properties ]
        public int OuterBorderWidth
        {
            get { return (int)GetValue(OuterBorderWidthProperty); }
            set { SetValue(OuterBorderWidthProperty, value); }
        }

        public int CheckboxBorderWidth
        {
            get { return (int)GetValue(CheckboxBorderWidthProperty); }
            set { SetValue(CheckboxBorderWidthProperty, value); }
        }

        public string InfoText
        {
            get { return (string)GetValue(InfoTextProperty); }
            set { SetValue(InfoTextProperty, value); }
        }

        public bool HasInfoText
        {
            get { return (bool)GetValue(HasInfoTextProperty); }
            private set { SetValue(HasInfoTextPropertyKey, value); }
        }

        public bool CheckboxBorderNotZero
        {
            get { return (bool)GetValue(CheckboxBorderNotZeroProperty); }
            private set { SetValue(CheckboxBorderNotZeroPropertyKey, value); }
        }

        public bool OuterBorderNotZero
        {
            get { return (bool)GetValue(OuterBorderNotZeroProperty); }
            private set { SetValue(OuterBorderNotZeroPropertyKey, value); }
        }

        public bool ShowOuterBorder
        {
            get { return (bool)GetValue(ShowOuterBorderProperty); }
            set { SetValue(ShowOuterBorderProperty, value); }
        } 
        #endregion
    }
}
