﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Common.WP8;
using FSharp.Control;
using Microsoft.FSharp.Collections;
using Microsoft.Phone.Shell;

namespace LearnOnTheGo.WP8
{
    public class LazyBlockUI<T> : ILazyBlockUI<T>
    {
        private Page page;
        private Action<T> setItems;
        private Func<bool> hasItems;
        private TextBlock messageTextBlock;

        public LazyBlockUI(Page page, Action<T> setItems, Func<bool> hasItems, TextBlock messageTextBlock)
        {
            this.page = page;
            this.setItems = setItems;
            this.hasItems = hasItems;
            this.messageTextBlock = messageTextBlock;
        }

        private ProgressIndicator GetIndicator()
        {
            return SystemTray.GetProgressIndicator(page) ?? new ProgressIndicator();
        }

        public FSharpList<LazyBlockUIState> GlobalState
        {
            get { return (FSharpList<LazyBlockUIState>)page.Tag ?? FSharpList<LazyBlockUIState>.Empty; }
            set { page.Tag = value; }
        }

        public void SetGlobalProgressMessage(string value)
        {
            var indicator = GetIndicator();
            indicator.Text = value;
            if (value != "")
            {                    
                indicator.IsVisible = true;
                indicator.IsIndeterminate = true;
                SystemTray.SetProgressIndicator(page, indicator);
            }
            else
            {
                indicator.IsVisible = false;
                indicator.IsIndeterminate = false;
                SystemTray.SetProgressIndicator(page, null);
            }
        }

        public void SetLocalProgressMessage(string value)
        {
            if (messageTextBlock != null)
            {
                messageTextBlock.Text = value;
                messageTextBlock.Visibility = value != "" ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string GetWebExceptionMessage(WebException webException)
        {
            if (webException.Response != null && webException.Response.ResponseUri.IsAbsoluteUri && webException.Response.ResponseUri.AbsoluteUri == URLs.Login)
            {
                return "Login did not work, please check your email and password in the Settings and try again.";
            }
            else
            {
                return "Unable to establish an internet connection. Please check your internet status and try again.";
            }
        }

        public bool HasItems
        {
            get { return hasItems(); }
        }

        public void SetItems(T items)
        {
            setItems(items);
        }

        public void SetLastUpdated(string value)
        {
        }

        private DispatcherTimer timer;

        public void StartTimer(Action action)
        {
            StopTimer();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(60);
            timer.Tick += (sender, args) => action();
            timer.Start();
        }

        public void StopTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        public void OnException(string message, Exception e)
        {
            ErrorReporting.ReportException(e, message);
            ErrorReporting.CheckForPreviousException(false);
        }
    }
}
