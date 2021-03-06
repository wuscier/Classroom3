﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Common.Behavior
{
    public class KeyDownBehavior:Behavior<UIElement>
    {
        

        public ICommand KeyDownCommand
        {
            get { return (ICommand)GetValue(KeyDownCommandProperty); }
            set { SetValue(KeyDownCommandProperty, value); }
        }

        public static readonly DependencyProperty KeyDownCommandProperty =
            DependencyProperty.Register("KeyDownCommand", typeof(ICommand), typeof(KeyDownBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            
            AssociatedObject.PreviewKeyDown += new KeyEventHandler(AssociatedObjectKeyDown);
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= new KeyEventHandler(AssociatedObjectKeyDown);
            base.OnDetaching();
        }

        private void AssociatedObjectKeyDown(object sender, KeyEventArgs e)
        {
            KeyDownCommand?.Execute(e);
        }
    }
}
