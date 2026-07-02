using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CursorTail.Core
{
    public class ContentPanel :ContentControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Head.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ContentPanel), new PropertyMetadata("123"));



        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Subtitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(ContentPanel), new PropertyMetadata("123"));



        public bool IsNextRow
        {
            get { return (bool)GetValue(IsNextRowProperty); }
            set { SetValue(IsNextRowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsNextRow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNextRowProperty =
            DependencyProperty.Register(nameof(IsNextRow), typeof(bool), typeof(ContentPanel), new PropertyMetadata(false));

    }
}
