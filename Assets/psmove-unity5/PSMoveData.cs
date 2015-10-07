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
using System;

/*! Boolean type. Use them instead of 0 and 1 to improve code readability. */
public enum PSMove_Bool
{
    PSMove_False = 0, /*!< False, Failure, Disabled (depending on context) */
    PSMove_True = 1, /*!< True, Success, Enabled (depending on context) */
};

/// <summary>
/// The Move controller can be connected by USB and/or Bluetooth.
/// </summary>
public enum PSMoveConnectionType
{
    Bluetooth,
    USB,
    Unknown,
};

// Not entirely sure why some of these buttons (R3/L3) are exposed...
public enum PSMoveButton
{
    L2 = 1 << 0x00,
    R2 = 1 << 0x01,
    L1 = 1 << 0x02,
    R1 = 1 << 0x03,
    Triangle = 1 << 0x04,
    Circle = 1 << 0x05,
    Cross = 1 << 0x06,
    Square = 1 << 0x07,
    Select = 1 << 0x08,
    L3 = 1 << 0x09,
    R3 = 1 << 0x0A,
    Start = 1 << 0x0B,
    Up = 1 << 0x0C,
    Right = 1 << 0x0D,
    Down = 1 << 0x0E,
    Left = 1 << 0x0F,
    PS = 1 << 0x10,
    Move = 1 << 0x13,
    Trigger = 1 << 0x14,	/* We can use this value with IsButtonDown() (or the events) to get 
                             * a binary yes/no answer about if the trigger button is down at all.
                             * For the full integer/analog value of the trigger, see the corresponding property below.
                             */
};

// Used by psmove_get_battery().
public enum PSMove_Battery_Level
{
    Batt_MIN = 0x00, /*!< Battery is almost empty (< 20%) */
    Batt_20Percent = 0x01, /*!< Battery has at least 20% remaining */
    Batt_40Percent = 0x02, /*!< Battery has at least 40% remaining */
    Batt_60Percent = 0x03, /*!< Battery has at least 60% remaining */
    Batt_80Percent = 0x04, /*!< Battery has at least 80% remaining */
    Batt_MAX = 0x05, /*!< Battery is fully charged (not on charger) */
    Batt_CHARGING = 0xEE, /*!< Battery is currently being charged */
    Batt_CHARGING_DONE = 0xEF, /*!< Battery is fully charged (on charger) */
};

public enum PSMove_Frame
{
    Frame_FirstHalf = 0, /*!< The older frame */
    Frame_SecondHalf, /*!< The most recent frame */
};

/*! Status of the tracker */
public enum PSMoveTracker_Status
{
    Tracker_NOT_CALIBRATED, /*!< Controller not registered with tracker */
    Tracker_CALIBRATION_ERROR, /*!< Calibration failed (check lighting, visibility) */
    Tracker_CALIBRATED, /*!< Color calibration successful, not currently tracking */
    Tracker_TRACKING, /*!< Calibrated and successfully tracked in the camera */
};

/*! Known camera types. Used for calculating focal length when calibration not present. */
public enum PSMoveTracker_Camera_type
{
    PSMove_Camera_PS3EYE_BLUEDOT,
    PSMove_Camera_PS3EYE_REDDOT,
    PSMove_Camera_Unknown
};

/*! Exposure modes */
public enum PSMoveTracker_Exposure
{
    Exposure_LOW, /*!< Very low exposure: Good tracking, no environment visible */
    Exposure_MEDIUM, /*!< Middle ground: Good tracking, environment visibile */
    Exposure_HIGH, /*!< High exposure: Fair tracking, but good environment */
    Exposure_INVALID, /*!< Invalid exposure value (for returning failures) */
};

public enum PSMoveTracker_Smoothing_Type
{
    Smoothing_None,		// Don't use any smoothing
    Smoothing_LowPass,	// A basic low pass filter (default)
    Smoothing_Kalman,	// A more expensive Kalman filter 
};

public class PSMoveButtonEventArgs : EventArgs
{
    public readonly PSMoveButton button;

    public PSMoveButtonEventArgs(PSMoveButton button)
    {
        this.button = button;
    }
}

// Per-Move Controller data needed by the worker.
public class PSMoveRawControllerData_Base
{
    // -------
    // Data items that typically get filled by the Worker thread
    // then are made available to the consuming game object (i.e. component)
    public Vector3 PSMovePosition;
    public Quaternion PSMoveOrientation;
    public UInt32 Buttons;
    public byte TriggerValue;
    public bool IsConnected; // Whether or not the controller is connected
    public bool IsTracking; // Whether or not the tracker saw the controller on the latest frame.
    public bool IsEnabled; // Whether or not the tracker is tracking this specific controller
    public int SequenceNumber; // Increments with every update

    // -------
    // Data items that typically get filled by the game object (i.e., PSMoveComponent)
    // then are passed to the worker thread.
    public byte RumbleRequest; // Please make the controller rumbled
    public bool ResetPoseRequest; // Please use the current pose as zero-pose.
    public bool CycleColourRequest; // Please change to the next available color.

    public PSMoveRawControllerData_Base()
    {
        Clear();
    }

    public void Clear()
    {
        PSMovePosition = Vector3.zero;
        PSMoveOrientation = Quaternion.identity;
        Buttons = 0;
        TriggerValue = 0;
        RumbleRequest = 0;
        ResetPoseRequest = false;
        CycleColourRequest = false;
        IsConnected = false;
        IsTracking = false;
        IsEnabled = false;
        SequenceNumber = 0;
    }
};

public class PSMoveRawControllerData_Concurrent : PSMoveRawControllerData_Base
{
    // Used to make sure we're accessing the data in a thread safe way
    public object Lock;

    public PSMoveRawControllerData_Concurrent() : base()
    {
        Lock = new object();
    }
};

public class PSMoveRawControllerData_TLS : PSMoveRawControllerData_Base
{
    // This is a pointer to the 
    private PSMoveRawControllerData_Concurrent ConcurrentData;

    public PSMoveRawControllerData_TLS(PSMoveRawControllerData_Concurrent concurrentData)
        : base()
    {
        ConcurrentData = concurrentData;
    }

    public bool IsValid()
    {
        return ConcurrentData != null;
    }

    public void ComponentPost()
    {
        using (new PSMoveHitchWatchdog("PSMoveRawControllerData_TLS::ComponentPost", 500))
        {
            lock (ConcurrentData.Lock)
            {
                // Post the component thread's data to the worker thread's data
                ConcurrentData.RumbleRequest = this.RumbleRequest;
                ConcurrentData.ResetPoseRequest = this.ResetPoseRequest;
                ConcurrentData.CycleColourRequest = this.CycleColourRequest;

                // Clear the edge triggered flags after copying to concurrent state
                this.ResetPoseRequest = false;
                this.CycleColourRequest = false;
            }
        }
    }

    public void ComponentRead()
    {
        using (new PSMoveHitchWatchdog("PSMoveRawControllerData_TLS::ComponentRead", 500))
        {
            lock (ConcurrentData.Lock)
            {
                // Read the worker thread's data into the component thread's data
                this.PSMovePosition = ConcurrentData.PSMovePosition;
                this.PSMoveOrientation = ConcurrentData.PSMoveOrientation;
                this.Buttons = ConcurrentData.Buttons;
                this.TriggerValue = ConcurrentData.TriggerValue;
                this.IsConnected = ConcurrentData.IsConnected;
                this.IsTracking = ConcurrentData.IsTracking;
                this.IsEnabled = ConcurrentData.IsEnabled;
                this.SequenceNumber = ConcurrentData.SequenceNumber;
            }
        }
    }

    public void WorkerRead()
    {
        using (new PSMoveHitchWatchdog("PSMoveRawControllerData_TLS::WorkerRead", 500))
        {
            lock (ConcurrentData.Lock)
            {
                // Read the component thread's data into the worker thread's data
                this.RumbleRequest = ConcurrentData.RumbleRequest;
                this.ResetPoseRequest = ConcurrentData.ResetPoseRequest;
                this.CycleColourRequest = ConcurrentData.CycleColourRequest;

                // Clear the edge triggered flags after copying from concurrent state
                ConcurrentData.ResetPoseRequest = false;
                ConcurrentData.CycleColourRequest = false;
            }
        }
    }

    public void WorkerPost()
    {
        using (new PSMoveHitchWatchdog("PSMoveRawControllerData_TLS::WorkerPost", 500))
        {
            lock (ConcurrentData.Lock)
            {
                // Since we just updated the controller data, bump the sequence data
                this.SequenceNumber++;

                // Post the worker thread's data to the component thread's data
                ConcurrentData.PSMovePosition = this.PSMovePosition;
                ConcurrentData.PSMoveOrientation = this.PSMoveOrientation;
                ConcurrentData.Buttons = this.Buttons;
                ConcurrentData.TriggerValue = this.TriggerValue;
                ConcurrentData.IsConnected = this.IsConnected;
                ConcurrentData.IsTracking = this.IsTracking;
                ConcurrentData.IsEnabled = this.IsEnabled;
                ConcurrentData.SequenceNumber = this.SequenceNumber;
            }
        }
    }
};

public class PSMoveDataContext
{
    public int PSMoveID { get; private set; }
    public PSMovePose Pose { get; private set; }

    // Controller Data bound to data in the worker thread    
    PSMoveRawControllerData_TLS RawControllerData;

    // Controller Data from previous frame
    UInt32 RawControllerPreviousButtons;
    byte RawControllerPreviousTriggerValue;

    public PSMoveDataContext(
        int moveID,
        PSMoveRawControllerData_Concurrent controllerConcurrentData)
    {
        PSMoveID = moveID;
        Pose = new PSMovePose();
        RawControllerData = new PSMoveRawControllerData_TLS(controllerConcurrentData);
    }

    public void Clear()
    {
        PSMoveID = -1;
        Pose.Clear();
        RawControllerData.Clear();
        RawControllerPreviousButtons = 0;
        RawControllerPreviousTriggerValue = 0;
    }

    public void ComponentPost()
    {
        if (RawControllerData.IsValid())
        {
            RawControllerData.ComponentPost();
        }
    }

    public void ComponentRead()
    {
        if (RawControllerData.IsValid())
        {
            // Backup controller state from the previous frame that we want to compute deltas on
            // before it gets stomped by InputManagerPostAndRead()
            int PreviousSequenceNumber = RawControllerData.SequenceNumber;
            byte PreviousTriggerValue = RawControllerData.TriggerValue;
            UInt32 PreviousButtons = RawControllerData.Buttons;

            // If the worker thread updated the controller, the sequence number will change
            RawControllerData.ComponentRead();

            // If the worker thread posted new data then the sequence number should changes
            if (RawControllerData.SequenceNumber != PreviousSequenceNumber)
            {
                // Actually update the previous controller state when we get new data
                RawControllerPreviousTriggerValue = PreviousTriggerValue;
                RawControllerPreviousButtons = PreviousButtons;

                // Refresh the world space controller pose
                Pose.PoseUpdate(this);
            }
        }
    }

    public void PostRumbleRequest(byte RequestedRumbleValue)
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            RawControllerData.RumbleRequest = RequestedRumbleValue;
            ComponentPost();
        }
    }

    public void PostCycleColourRequest()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            RawControllerData.CycleColourRequest = true;
            ComponentPost();
        }
    }

    public void PostResetPoseRequest()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            Pose.ResetYawSnapshot();
            RawControllerData.ResetPoseRequest = true;
            ComponentPost();
        }
    }

    // Controller Data Functions
    public Vector3 GetTrackingSpacePosition()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return RawControllerData.PSMovePosition;
        } 
        else 
        {
            return Vector3.zero;
        }
    }

    public Quaternion GetTrackingSpaceOrientation()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return RawControllerData.PSMoveOrientation;
        }
        else
        {
            return Quaternion.identity;
        }
    }

    public bool GetIsConnected()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return RawControllerData.IsConnected;
        }
        else
        {
            return false;
        }
    }

    public bool GetIsTracking()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return RawControllerData.IsTracking;
        }
        else 
        {
            return false;
        }
    }
    
    public bool GetIsEnabled()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return RawControllerData.IsEnabled;
        }
        else 
        {
            return false;
        }
    }
    
    // Current Button State
    public bool GetButtonTriangle()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return (RawControllerData.Buttons & (UInt32)PSMoveButton.Triangle) != 0;
        }
        else
        {
            return false;
        }
    }
    
    public bool GetButtonCircle()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return (RawControllerData.Buttons & (UInt32)PSMoveButton.Circle) != 0;
        }
        else
        {
            return false;
        }
    }
    
    public bool GetButtonCross()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return (RawControllerData.Buttons & (UInt32)PSMoveButton.Cross) != 0;
        }
        else
        {
            return false;
        }
    }
    
    public bool GetButtonSquare()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return (RawControllerData.Buttons & (UInt32)PSMoveButton.Square) != 0;
        }
        else 
        {
            return false;
        }
    }
    
    public bool GetButtonSelect()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return (RawControllerData.Buttons & (UInt32)PSMoveButton.Select) != 0;
        }
        else 
        {
            return false;
        }
    }
    
    public bool GetButtonStart()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return (RawControllerData.Buttons & (UInt32)PSMoveButton.Start) != 0;
        }
        else 
        {
            return false;
        }
    }
    
    public bool GetButtonPS()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return (RawControllerData.Buttons & (UInt32)PSMoveButton.PS) != 0;
        }
        else
        {
            return false;
        }
    }
    
    public bool GetButtonMove()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return (RawControllerData.Buttons & (UInt32)PSMoveButton.Move) != 0;
        }
        else
        {
            return false;
        }
    }

	public byte GetPreviousTriggerValue()
	{
		return RawControllerPreviousTriggerValue;
	}
    
    public byte GetTriggerValue()
    {
        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            return RawControllerData.TriggerValue;
        }
        else
        {
            return 0;
        }
    }

    // Pressed This Frame
    public bool GetButtonPressedThisFrame(PSMoveButton ButtonMask)
    {
        bool PressedThisFrame = false;

        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            PressedThisFrame =
                (RawControllerPreviousButtons & (UInt32)ButtonMask) == 0 &&
                (RawControllerData.Buttons & (UInt32)ButtonMask) != 0;
        }

        return PressedThisFrame;
    }

    public bool GetButtonTrianglePressed()
    {
        return GetButtonPressedThisFrame(PSMoveButton.Triangle);
    }

    public bool GetButtonCirclePressed()
    {
        return GetButtonPressedThisFrame(PSMoveButton.Circle);
    }

    public bool GetButtonCrossPressed()
    {
        return GetButtonPressedThisFrame(PSMoveButton.Cross);
    }

    public bool GetButtonSquarePressed()
    {
        return GetButtonPressedThisFrame(PSMoveButton.Square);
    }

    public bool GetButtonSelectPressed()
    {
        return GetButtonPressedThisFrame(PSMoveButton.Select);
    }

    public bool GetButtonStartPressed()
    {
        return GetButtonPressedThisFrame(PSMoveButton.Start);
    }

    public bool GetButtonPSPressed()
    {
        return GetButtonPressedThisFrame(PSMoveButton.PS);
    }

    public bool GetButtonMovePressed()
    {
        return GetButtonPressedThisFrame(PSMoveButton.Move);
    }

    public bool GetButtonTriggerPressed()
    {
        return GetButtonPressedThisFrame(PSMoveButton.Trigger);
    }

    // Released This Frame
    public bool GetButtonReleasedThisFrame(PSMoveButton ButtonMask)
    {
        bool ReleasedThisFrame = false;

        if (RawControllerData.IsValid() && RawControllerData.IsConnected)
        {
            ReleasedThisFrame =
                (RawControllerPreviousButtons & (UInt32)ButtonMask) != 0 &&
                (RawControllerData.Buttons & (UInt32)ButtonMask) == 0;
        }

        return ReleasedThisFrame;
    }

    public bool GetButtonTriangleReleased()
    {
        return GetButtonReleasedThisFrame(PSMoveButton.Triangle);
    }

    public bool GetButtonCircleReleased()
    {
        return GetButtonReleasedThisFrame(PSMoveButton.Circle);
    }

    public bool GetButtonCrossReleased()
    {
        return GetButtonReleasedThisFrame(PSMoveButton.Cross);
    }

    public bool GetButtonSquareReleased()
    {
        return GetButtonReleasedThisFrame(PSMoveButton.Square);
    }

    public bool GetButtonSelectReleased()
    {
        return GetButtonReleasedThisFrame(PSMoveButton.Select);
    }

    public bool GetButtonStartReleased()
    {
        return GetButtonReleasedThisFrame(PSMoveButton.Start);
    }

    public bool GetButtonPSReleased()
    {
        return GetButtonReleasedThisFrame(PSMoveButton.PS);
    }

    public bool GetButtonMoveReleased()
    {
        return GetButtonReleasedThisFrame(PSMoveButton.Move);
    }

    public bool GetButtonTriggerReleased()
    {
        return GetButtonReleasedThisFrame(PSMoveButton.Trigger);
    }  
}
