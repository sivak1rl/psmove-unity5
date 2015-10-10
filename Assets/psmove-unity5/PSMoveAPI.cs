/**
* PSMove API - A Unity5 plugin for the PSMove motion controller.
*              Derived from the psmove-ue4 plugin by Chadwick Boulay
*              and the UniMove plugin by the Copenhagen Game Collective
* Copyright (C) 2015, PolyarcGames (http://www.polyarcgames.com)
*                   Brendan Walker (brendan@polyarcgames.com)
* 
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*    1. Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*
*    2. Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
* AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
* ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
* LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
* CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
* SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
* INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
* CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
* ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
* POSSIBILITY OF SUCH DAMAGE.
**/

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

/* The following functions are bindings to Thomas Perl's C API for the PlayStation Move (http://thp.io/2010/psmove/)
 * See README for more details.
 */

public class PSMoveAPI
{
    /*! Library version number */
    public enum PSMove_Version
    {
        /**
         * Version format: AA.BB.CC = 0xAABBCC
         *
         * Examples:
         *  3.0.1 = 0x030001
         *  4.2.11 = 0x04020B
         **/
        PSMOVE_CURRENT_VERSION = 0x030001, /*!< Current version, see psmove_init() */
    }

    [DllImport("psmoveapi.dll")]
    public static extern PSMove_Bool psmove_init(PSMove_Version version);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_shutdown();

    // Move Controller API
    [DllImport("psmoveapi.dll")]
    public static extern IntPtr psmove_connect();

    [DllImport("psmoveapi.dll")]
    public static extern IntPtr psmove_connect_by_id(int id);

    [DllImport("psmoveapi.dll")]
    public static extern int psmove_count_connected();

    [DllImport("psmoveapi.dll")]
    public static extern PSMoveConnectionType psmove_connection_type(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern int psmove_has_calibration(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_enable_orientation(IntPtr move, PSMove_Bool enable);

    [DllImport("psmoveapi.dll")]
    public static extern PSMove_Bool psmove_has_orientation(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_get_orientation(IntPtr move, ref float oriw, ref float orix, ref float oriy, ref float oriz);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_set_leds(IntPtr move, char r, char g, char b);

    [DllImport("psmoveapi.dll")]
    public static extern int psmove_update_leds(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_set_rumble(IntPtr move, char rumble);

    [DllImport("psmoveapi.dll")]
    public static extern uint psmove_poll(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern uint psmove_get_buttons(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern uint psmove_get_button_events(IntPtr move, ref uint pressed, ref uint released);

    [DllImport("psmoveapi.dll")]
    public static extern char psmove_get_trigger(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern float psmove_get_temperature(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern PSMove_Battery_Level psmove_get_battery(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_get_accelerometer(IntPtr move, ref int ax, ref int ay, ref int az);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_get_accelerometer_frame(IntPtr move, PSMove_Frame frame, ref float ax, ref float ay, ref float az);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_get_gyroscope(IntPtr move, ref int gx, ref int gy, ref int gz);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_get_gyroscope_frame(IntPtr move, PSMove_Frame frame, ref float gx, ref float gy, ref float gz);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_get_magnetometer(IntPtr move, ref int mx, ref int my, ref int mz);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_disconnect(IntPtr move);

    [DllImport("psmoveapi.dll")]
    public static extern void psmove_reset_orientation(IntPtr move);

    // Tracker API
    [StructLayout(LayoutKind.Sequential)]
    public struct PSMoveTrackerSmoothingSettings
    {
        // Low Pass Filter Options
        public int filter_do_2d_xy;        /* [1] specifies to use a adaptive x/y smoothing on pixel location */
        public int filter_do_2d_r;         /* [1] specifies to use a adaptive radius smoothing on 2d blob  */
        public PSMoveTracker_Smoothing_Type filter_3d_type;

        // Kalman Filter Options
        float acceleration_variance;
        public float cov00, cov01, cov02;
        public float cov10, cov11, cov12;
        public float cov20, cov21, cov22;
    };

    /*!< Structure for storing tracker settings */
    [StructLayout(LayoutKind.Sequential)]
    public struct PSMoveTrackerSettings
    {
        /* Camera Controls*/
        public int camera_frame_width;                     /* [0=auto] */
        public int camera_frame_height;                    /* [0=auto] */
        public int camera_frame_rate;                      /* [0=auto] */
        public PSMove_Bool camera_auto_gain;          /* [PSMove_False] */
        public int camera_gain;                            /* [0] [0,0xFFFF] */
        public PSMove_Bool camera_auto_white_balance; /* [PSMove_False] */
        public int camera_exposure;                        /* [(255 * 15) / 0xFFFF] [0,0xFFFF] */
        public int camera_brightness;                      /* [0] [0,0xFFFF] */
        public PSMove_Bool camera_mirror;             /* [PSMove_False] mirror camera image horizontally */
        public PSMoveTracker_Camera_type camera_type; /* [PSMove_Camera_PS3EYE_BLUEDOT] camera type. Used for focal length when OpenCV calib missing */

        /* Settings for camera calibration process */
        public PSMoveTracker_Exposure exposure_mode;  /* [Exposure_LOW] exposure mode for setting target luminance */
        public int calibration_blink_delay;                /* [200] number of milliseconds to wait between a blink  */
        public int calibration_diff_t;                     /* [20] during calibration, all grey values in the diff image below this value are set to black  */
        public int calibration_min_size;                   /* [50] minimum size of the estimated glowing sphere during calibration process (in pixel)  */
        public int calibration_max_distance;               /* [30] maximum displacement of the separate found blobs  */
        public int calibration_size_std;                   /* [10] maximum standard deviation (in %) of the glowing spheres found during calibration process  */
        public int color_mapping_max_age;                  /* [2*60*60] Only re-use color mappings "younger" than this time in seconds  */
        public float dimming_factor;                       /* [1.f] dimming factor used on LED RGB values  */

        /* Settings for OpenCV image processing for sphere detection */
        public int color_hue_filter_range;                 /* [20] +- range of Hue window of the hsv-colorfilter  */
        public int color_saturation_filter_range;          /* [85] +- range of Sat window of the hsv-colorfilter  */
        public int color_value_filter_range;               /* [85] +- range of Value window of the hsv-colorfilter  */

        /* Settings for tracker algorithms */
        public int use_fitEllipse;                         /* [0] estimate circle from blob; [1] use fitEllipse */

        public float color_adaption_quality_t;             /* [35] maximal distance (calculated by 'psmove_tracker_hsvcolor_diff') between the first estimated color and the newly estimated  */
        public float color_update_rate;                    /* [1] every x seconds adapt to the color, 0 means no adaption  */
        // size of "search" tiles when tracking is lost
        public int search_tile_width;                      /* [0=auto] width of a single tile */
        public int search_tile_height;                     /* height of a single tile */
        public int search_tiles_horizontal;                /* number of search tiles per row */
        public int search_tiles_count;                     /* number of search tiles */

        /* THP-specific tracker threshold checks */
        public int roi_adjust_fps_t;                       /* [160] the minimum fps to be reached, if a better roi-center adjusment is to be perfomred */
        // if tracker thresholds not met, sphere is deemed not to be found
        public float tracker_quality_t1;                   /* [0.3f] minimum ratio of number of pixels in blob vs pixel of estimated circle. */
        public float tracker_quality_t2;                   /* [0.7f] maximum allowed change of the radius in percent, compared to the last estimated radius */
        public float tracker_quality_t3;                   /* [4.7f] minimum radius  */
        // if color thresholds not met, color is not adapted
        public float color_update_quality_t1;              /* [0.8] minimum ratio of number of pixels in blob vs pixel of estimated circle. */
        public float color_update_quality_t2;              /* [0.2] maximum allowed change of the radius in percent, compared to the last estimated radius */
        public float color_update_quality_t3;              /* [6.f] minimum radius */

        /* CBB-specific tracker parameters */
        public float xorigin_cm;                           /* [0.f] x-distance to subtract from calculated position */
        public float yorigin_cm;                           /* [0.f] y-distance to subtract from calculated position */
        public float zorigin_cm;                           /* [0.f] z-distance to subtract from calculated position */
    }

    [DllImport("psmoveapi_tracker.dll")]
    public static extern IntPtr psmove_tracker_new();

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_tracker_free(IntPtr psmove_tracker);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_tracker_settings_set_default(ref PSMoveTrackerSettings settings);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern IntPtr psmove_tracker_new_with_settings(ref PSMoveTrackerSettings settings);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern PSMoveTracker_ErrorCode psmove_tracker_get_last_error();

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_tracker_get_smoothing_settings(IntPtr tracker, ref PSMoveTrackerSmoothingSettings smoothing_settings);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_tracker_set_smoothing_settings(IntPtr tracker, ref PSMoveTrackerSmoothingSettings smoothing_settings);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_tracker_set_exposure(IntPtr tracker, PSMoveTracker_Exposure exposure);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_tracker_get_size(IntPtr tracker, ref int tracker_width, ref int tracker_height);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern PSMoveTracker_Status psmove_tracker_enable(IntPtr tracker, IntPtr psmove);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern PSMoveTracker_Status psmove_tracker_get_status(IntPtr tracker, IntPtr psmove);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_tracker_update_image(IntPtr tracker);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern int psmove_tracker_update(IntPtr tracker, IntPtr psmove);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_tracker_reset_location(IntPtr tracker, IntPtr psmove);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern int psmove_tracker_cycle_color(IntPtr tracker, IntPtr psmove);

    // Tracker Fusion API
    [DllImport("psmoveapi_tracker.dll")]
    public static extern IntPtr psmove_fusion_new(IntPtr psmove_tracker, float z_near, float z_far);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_fusion_free(IntPtr psmove_fusion);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_fusion_get_position(IntPtr psmove_fusion, IntPtr psmove, ref float xcm, ref float ycm, ref float zcm);

    [DllImport("psmoveapi_tracker.dll")]
    public static extern void psmove_fusion_get_transformed_location(IntPtr psmove_fusion, IntPtr psmove, ref float xcm, ref float ycm, ref float zcm);
}