﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinect_Demo
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;

    using VisioForge.Kinect;
    using VisioForge.MediaFramework.MFP;
    using VisioForge.Types;
    using VisioForge.Types.OutputFormat;

    public partial class Form1 : Form
    {
        private KinectSource kinect;

        public Form1()
        {
            InitializeComponent();
        }

        private void ApplyMP4Settings(ref VFMP4Output mp4Output)
        {
            // Video H264 settings
            mp4Output.Video_H264.Profile = VFH264Profile.ProfileBaseline;

            mp4Output.Video_H264.Level = VFH264Level.LevelAuto;

            mp4Output.Video_H264.TargetUsage = VFH264TargetUsage.BestSpeed;

            mp4Output.Video_H264.PictureType = VFH264PictureType.Auto;

            mp4Output.Video_H264.RateControl = VFH264RateControl.CBR;
            mp4Output.Video_H264.MBEncoding = VFH264MBEncoding.CABAC;
            mp4Output.Video_H264.GOP = false;
            mp4Output.Video_H264.BitrateAuto = false;

            mp4Output.Video_H264.IDR_Period = 15;

            mp4Output.Video_H264.P_Period = 3;

            mp4Output.Video_H264.Bitrate = 2000;

            // Audio AAC settings
            mp4Output.Audio_AAC.Bitrate = 128;

            mp4Output.Audio_AAC.Version = VFAACVersion.MPEG4;
            mp4Output.Audio_AAC.Output = VFAACOutput.RAW;
            mp4Output.Audio_AAC.Object = VFAACObject.Main;
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            kinect.SensorID = cbKinectDevice.SelectedIndex;
            kinect.Video_Format = (VFKinectVideoFormat)cbVideoSourceFormat.SelectedIndex;
            kinect.Video_DepthFormat = (VFKinectDepthFormat)cbDepthSourceFormat.SelectedIndex;

            if (rbUseVideoSource.Checked)
            {
                kinect.Video_Source = VFKinectVideoSource.Video;
            }
            else
            {
                kinect.Video_Source = VFKinectVideoSource.DepthGrayscale;
            }

            VideoCapture1.Audio_CaptureDevice = cbAudioCaptureDevice.Text;
            VideoCapture1.Audio_CaptureDevice_Format = cbAudioCaptureFormat.Text;
            VideoCapture1.Audio_RecordAudio = true;
            VideoCapture1.Audio_PlayAudio = true;

            if (rbPreview.Checked)
            {
                VideoCapture1.Mode = VFVideoCaptureMode.KinectPreview;
            }
            else
            {
                VideoCapture1.Mode = VFVideoCaptureMode.KinectCapture;  

                VideoCapture1.Output_Filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VisioForge\\" + "output.mp4";

                var mp4Output = new VFMP4Output();
                ApplyMP4Settings(ref mp4Output);
                VideoCapture1.Output_Format = mp4Output;
            }

            VideoCapture1.Audio_PlayAudio = false;

            kinect.Gestures_Recognizer_Enabled = cbDetectGestures.Checked;

            edGestures.Text = string.Empty;

            kinect.Init(VideoCapture1.Core);
            kinect.Start();
        }

        public void kinect_OnKinectReadyToStart(object sender, EventArgs e)
        {
            tbElevationAngle.Maximum = kinect.Video_ElevationAngleMax;
            tbElevationAngle.Minimum = kinect.Video_ElevationAngleMin;
            tbElevationAngle.Value = kinect.Video_ElevationAngle;

            VideoCapture1.Start();
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            VideoCapture1.Stop();
            kinect.Stop();
        }

        private void tbElevationAngle_Scroll(object sender, EventArgs e)
        {
            if (kinect != null)
            {
                kinect.Video_ElevationAngle = tbElevationAngle.Value;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            kinect = new KinectSource();
            kinect.OnKinectReadyToStart += kinect_OnKinectReadyToStart;
            kinect.OnGestureDetected += kinect_OnGestureDetected;
            
            for (int i = 0; i < kinect.DevicesCount; i++)
            {
                cbKinectDevice.Items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            if (cbKinectDevice.Items.Count > 0)
            {
                cbKinectDevice.SelectedIndex = 0;
            }

            cbVideoSourceFormat.SelectedIndex = 0;
            cbDepthSourceFormat.SelectedIndex = 0;

            foreach (var device in VideoCapture1.Audio_CaptureDevicesInfo)
            {
                if (device.Name.Contains("Kinect"))
                {
                    cbAudioCaptureDevice.Items.Add(device);
                }
            }

            if (cbAudioCaptureDevice.Items.Count > 0)
            {
                cbAudioCaptureDevice.SelectedIndex = 0;
                cbAudioCaptureDevice_SelectedIndexChanged(null, null);
            }
        }

        private void kinect_OnGestureDetected(object sender, KinectGestureEventArgs e)
        {
            edGestures.Text += e.Gesture.ToString() + Environment.NewLine;
        }

        private void cbDetectGestures_CheckedChanged(object sender, EventArgs e)
        {
            kinect.Gestures_Recognizer_Enabled = cbDetectGestures.Checked;
        }

        private void cbAudioCaptureDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbAudioCaptureDevice.SelectedIndex != -1)
            {
                VideoCapture1.Audio_CaptureDevice = cbAudioCaptureDevice.Text;
                cbAudioCaptureFormat.Items.Clear();

                var deviceItem = VideoCapture1.Audio_CaptureDevicesInfo.First(device => device.Name == cbAudioCaptureDevice.Text);
                foreach (string format in deviceItem.Formats)
                {
                    cbAudioCaptureFormat.Items.Add(format);
                }

                if (cbAudioCaptureFormat.Items.Count > 0)
                {
                    cbAudioCaptureFormat.SelectedIndex = 0;
                }
            }
        }

    }
}
