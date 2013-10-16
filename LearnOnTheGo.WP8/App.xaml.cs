﻿using System.Windows;
using Common.WP8;

namespace LearnOnTheGo.WP8
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            new AppMetadata(this, "Learn On The Go", "learnonthego@codebeside.org", false);
        }

        public static Crawler Crawler { get; set; }
    }
}