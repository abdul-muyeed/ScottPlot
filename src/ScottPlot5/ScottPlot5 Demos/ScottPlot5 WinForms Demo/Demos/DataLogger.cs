﻿namespace WinForms_Demo.Demos;

public partial class DataLogger : Form, IDemoWindow
{
    public string Title => "Data Logger";
    public string Description => "Plots live streaming data as a growing line plot.";

    readonly System.Windows.Forms.Timer AddNewDataTimer = new() { Interval = 10, Enabled = true };
    readonly System.Windows.Forms.Timer UpdatePlotTimer = new() { Interval = 50, Enabled = true };

    readonly ScottPlot.Plottables.DataLogger Logger;

    public DataLogger()
    {
        InitializeComponent();

        Logger = formsPlot1.Plot.Add.DataLogger();

        // disable mouse interaction by default
        formsPlot1.Interaction.Disable();

        // setup a timer to add data to the streamer periodically
        AddNewDataTimer.Tick += (s, e) =>
        {
            var newValues = ScottPlot.Generate.RandomWalker.Next(10);
            Logger.Add(newValues);
        };

        // setup a timer to request a render periodically
        UpdatePlotTimer.Tick += (s, e) =>
        {
            if (Logger.HasNewData)
            {
                formsPlot1.Plot.Title($"Processed {Logger.Data.CountTotal:N0} points");
                formsPlot1.Refresh();
            }
        };

        // setup configuration actions
        btnFull.Click += (s, e) => Logger.ViewFull();
        btnJump.Click += (s, e) => Logger.ViewJump();
        btnSlide.Click += (s, e) => Logger.ViewSlide();

        // control automatic axis limit modification behavior
        chkManageLimits.CheckedChanged += (s, e) =>
        {
            if (chkManageLimits.Checked)
            {
                Logger.ManageAxisLimits = true;
                formsPlot1.Interaction.Disable();
                formsPlot1.Plot.Axes.AutoScale();
            }
            else
            {
                Logger.ManageAxisLimits = false;
                formsPlot1.Interaction.Enable();
            }
        };

        // switch between using primary and secondary Y axes
        chkRightAxis.CheckedChanged += (s, e) =>
        {
            lock (formsPlot1.Plot.Sync)
            {
                // reset old axis limits so ticks are not displayed on unused axes 
                formsPlot1.Plot.Axes.Left.Range.Reset();
                formsPlot1.Plot.Axes.Right.Range.Reset();

                // tell the datalogger which axis to use and modify moving forward
                Logger.Axes.YAxis = chkRightAxis.Checked
                    ? formsPlot1.Plot.Axes.Right
                    : formsPlot1.Plot.Axes.Left;
            }
        };
    }
}
