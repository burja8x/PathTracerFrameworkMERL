﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PathTracer
{
  public partial class MainWindow : Form
  {
    System.Windows.Forms.Timer updateUI = new System.Windows.Forms.Timer();
    Stopwatch sw = new Stopwatch();
    Renderer r = null;
    const int bitmapWidth = 320; //160
    Bitmap bmp;

    CancellationTokenSource tokenSource;
    Task renderTask;

    public MainWindow()
    {
      InitializeComponent();     
      pbxRender.Image = bmp;

      updateUI.Tick += UpdateUI_Tick;
      updateUI.Interval = 200;
      updateUI.Start();
    }

    private void UpdateUI_Tick(object sender, EventArgs e)
    {
            try
            {
              if (bmp != null)
                r?.CopyBitmap(bmp);
            }
            catch (Exception)
            {
                Console.WriteLine("ignore this ERROR");
            }
      lblSPP.Text = $"SPP: {r?.SPP ?? 0}";
      lblTime.Text = $"Time: {sw.Elapsed.ToString()}";
      pbxRender.Invalidate();
      pbxRender.Update();
    }

    private void btnDoit_Click(object sender, EventArgs e)
    {
      if (renderTask != null && !renderTask.IsCompleted && !renderTask.IsCanceled && !renderTask.IsFaulted)
      {
        TryAgain_Save:
            try
            {
                bmp.Save(@"D:\NRG seminarska\" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-").Replace(" ", "_") + ".png", ImageFormat.Png);
            }
            catch (Exception)
            {
                Thread.Sleep(1);
                goto TryAgain_Save;
            }
            //tokenSource.Cancel();
            //renderTask.Wait();
            //tokenSource.Dispose();
            //sw.Stop();
            return;
      }

      tokenSource = new CancellationTokenSource();

      Scene s = Scene.AllMerlInOne();

        //Scene s = Scene.CornellBox();

        bmp = new Bitmap(bitmapWidth, (int)Math.Round(bitmapWidth / s.AspectRatio), PixelFormat.Format24bppRgb);
      pbxRender.Image = bmp;

      using (Graphics grD = Graphics.FromImage(bmp))
      {
        grD.Clear(Color.Black);
      }

      r = new Renderer(bmp);

      var token = tokenSource.Token;
      sw.Restart();
      renderTask = Task.Run(() => r.Render(s, token), token);

            btnDoit.Text = "Save";
    }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
