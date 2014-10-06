﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Diagnostics.Contracts;
using Codeplex.Reactive.Extensions;
#if WINDOWS_PHONE
using Microsoft.Phone.Reactive;
#else
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
#endif

namespace Codeplex.Reactive.Extensions
{
    public static class INotifyDataErrorInfoExtensions
    {
        /// <summary>Converts ErrorsChanged to an observable sequence.</summary>
        public static IObservable<DataErrorsChangedEventArgs> ErrorsChangedAsObservable<T>(this T subject)
            where T : INotifyDataErrorInfo
        {
            return Observable.FromEvent<EventHandler<DataErrorsChangedEventArgs>, DataErrorsChangedEventArgs>(
                h => (sender, e) => h(e),
                h => subject.ErrorsChanged += h,
                h => subject.ErrorsChanged -= h);
        }

        /// <summary>
        /// Converts target property's ErrorsChanged to an observable sequence.
        /// </summary>
        /// <param name="propertySelector">Argument is self, Return is target property.</param>
        public static IObservable<TProperty> ObserveErrorInfo<TSubject, TProperty>(
            this TSubject subject, Expression<Func<TSubject, TProperty>> propertySelector)
            where TSubject : INotifyDataErrorInfo
        {
            string propertyName;
            var accessor = AccessorCache<TSubject>.LookupGet(propertySelector, out propertyName);

            var result = subject.ErrorsChangedAsObservable()
                .Where(e => e.PropertyName == propertyName)
                .Select(_ => ((Func<TSubject, TProperty>)accessor).Invoke(subject));

            return result;
        }
    }
}