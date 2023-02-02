﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpdateTool.Controls
{
    static class BorderElement
    {
        public const int DefaultCornerRadius = -1;

        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create(nameof(IBorderElement.BorderColor), typeof(Color), typeof(IBorderElement), null,
                                    propertyChanged: OnBorderColorPropertyChanged);

        public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(IBorderElement.BorderWidth), typeof(double), typeof(IBorderElement), -1d);

        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(IBorderElement.CornerRadius), typeof(int), typeof(IBorderElement), defaultValue: DefaultCornerRadius);

        static void OnBorderColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((IBorderElement)bindable).OnBorderColorPropertyChanged((Color)oldValue, (Color)newValue);
        }
    }
}
