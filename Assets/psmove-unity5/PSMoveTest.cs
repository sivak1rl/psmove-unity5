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
using System;
using System.Collections.Generic;

public class PSMoveTest : MonoBehaviour 
{
    private PSMoveController move;

	void Start() 
	{
        move = gameObject.GetComponent<PSMoveController>();
        move.OnButtonPSPressed += move_OnButtonPSPressed;
        move.OnButtonPSReleased += move_OnButtonPSReleased;
        move.OnButtonCirclePressed += move_OnButtonCirclePressed;
        move.OnButtonCircleReleased += move_OnButtonCircleReleased;
	}

	void Update() 
	{
        // Set the rumble based on how much the trigger is down
        move.SetRumble(move.TriggerValue);
	}

    void move_OnButtonCirclePressed(object sender, EventArgs e)
    {
        Debug.Log("Circle button pressed");
        move.CycleTrackingColor();
    }

    void move_OnButtonCircleReleased(object sender, EventArgs e)
    {
        Debug.Log("Circle button released");
    }

    void move_OnButtonPSReleased(object sender, EventArgs e)
    {
        Debug.Log("PS button pressed");
        move.ResetOrientation();
    }

    void move_OnButtonPSPressed(object sender, EventArgs e)
    {
        Debug.Log("PS button released");
    }

	void OnGUI() 
	{
        string display = "";

        if (move.IsConnected)
        {
            display = string.Format("PS Move {0} - Connected\n", move.PSMoveID);
        }
        else
        {
            display = string.Format("PS Move {0} - NOT CONNECTED", move.PSMoveID);
        }

        GUI.Label(new Rect(10, 10 + move.PSMoveID*20, 500, 100), display);
    }
}
