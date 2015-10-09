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

public class PSMovePose
{
    public Vector3 WorldPosition;
    public Vector3 ZeroPosition;
    public Vector3 UncorrectedWorldPosition;
    public Quaternion WorldOrientation;
    public Quaternion ZeroYaw;
    public Quaternion UncorrectedWorldOrientation;

    public PSMovePose()
    {
        Clear();
    }

    public void Clear()
    {
        WorldPosition = Vector3.zero;
        ZeroPosition = Vector3.zero;
        UncorrectedWorldPosition = Vector3.zero;
        WorldOrientation = Quaternion.identity;
        ZeroYaw = Quaternion.identity;
        UncorrectedWorldOrientation = Quaternion.identity;
    }

    public void PoseUpdate(PSMoveDataContext DataContext)
    {
        Matrix4x4 TrackingSpaceToWorldSpacePosition = Matrix4x4.identity;
        Quaternion OrientationTransform= Quaternion.identity;

        if (ComputeTrackingToWorldTransforms(
                ref TrackingSpaceToWorldSpacePosition,
                ref OrientationTransform))
        {
            // The PSMove position is given in the space of the rift camera in centimeters
            Vector3 PSMPosTrackingSpace = DataContext.GetTrackingSpacePosition();
            // Transform to world space
            Vector3 PSMPosWorldSpace = TrackingSpaceToWorldSpacePosition.MultiplyPoint3x4(PSMPosTrackingSpace);

            // The PSMove orientation is given in its native coordinate system
            Quaternion PSMOriNative = DataContext.GetTrackingSpaceOrientation();
            // Apply controller orientation first, then apply orientation transform
            Quaternion PSMOriWorld = PSMoveQuatToUnityQuat(PSMOriNative) * OrientationTransform;

            // Save the resulting pose, updating for internal offset/zero yaw
            UncorrectedWorldPosition = PSMPosWorldSpace;
            WorldPosition = PSMPosWorldSpace - ZeroPosition;
            UncorrectedWorldOrientation = PSMOriWorld;
            WorldOrientation = ZeroYaw * PSMOriWorld;
        }
    }

    public void ResetYawSnapshot()
    {
        ZeroYaw = Quaternion.identity;
    }

    public void SnapshotOrientationYaw()
    {

        float Magnitude =
            Mathf.Sqrt(UncorrectedWorldOrientation.y * UncorrectedWorldOrientation.y +
                        UncorrectedWorldOrientation.w * UncorrectedWorldOrientation.w);

        // Strip out the x and z (pitch and roll) components of rotation and negate the yaw-axis
        // Then normalize the resulting quaternion
        ZeroYaw.x = 0;
        ZeroYaw.y = -UncorrectedWorldOrientation.y / Magnitude;
        ZeroYaw.z = 0;
        ZeroYaw.w = UncorrectedWorldOrientation.w / Magnitude;
    }

    public void ResetPositionSnapshot()
    {
        ZeroPosition = Vector3.zero;
    }

    public void SnapshotPosition()
    {
        ZeroPosition = UncorrectedWorldPosition;
    }

    #region Private Helper Methods
    private static Quaternion PSMoveQuatToUnityQuat(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, q.w);
    }

    private static Matrix4x4 RHSInCMToUnityInMetersTransform()
    {
        // Convert from Right Handed (e.g., OpenGL, PSMove native) to Unity Left Handed coordinate system.
        // Also convert units from centimeters to unity units (typically 100 cm == 1 Unity Unit).

        // Conversion from centimeters to unity units
        float CMToUU = 1.0f / 100.0f;

        // PSMove Coordinate System -> Unreal Coordinate system ==> (x, y, z) -> (x, y, -z)
        return Matrix4x4.Scale(new Vector3(CMToUU, CMToUU, -CMToUU));
    }

    private static bool ComputeTrackingToWorldTransforms(
        ref Matrix4x4 TrackingSpaceToWorldSpacePosition,
        ref Quaternion OrientationTransform)
    {
        float TrackingCameraNearPlane= 0.0f;
        float TrackingCameraFarPlane = 0.0f;
        float TrackingCameraHHalfRadians = 0.0f;
        float TrackingCameraVHalfRadians = 0.0f;

        return 
            ComputeTrackingToWorldTransformsAndFrustum(
                ref TrackingSpaceToWorldSpacePosition, // In Unity Units
                ref OrientationTransform, // In Unity Coordinate System
                ref TrackingCameraNearPlane, // In Unity Units
                ref TrackingCameraFarPlane, // In Unity Units
                ref TrackingCameraHHalfRadians, 
                ref TrackingCameraVHalfRadians);
    }

    static bool ComputeTrackingToWorldTransformsAndFrustum(
        ref Matrix4x4 TrackingSpaceToWorldSpacePosition, // In Unreal Units
        ref Quaternion OrientationTransform, // In Unity Coordinate System
        ref float TrackingCameraNearPlane, // In Unity Units
        ref float TrackingCameraFarPlane, // In Unity Units
        ref float TrackingCameraHHalfRadians, 
        ref float TrackingCameraVHalfRadians)
    {
        bool success = false;

        if (Camera.current != null)
        {
            // Get the world game camera transform for the player
            Quaternion GameCameraOrientation = Camera.current.transform.rotation;
            Vector3 GameCameraLocation = Camera.current.transform.position;

            if (OVRManager.tracker != null && OVRManager.tracker.isPresent && OVRManager.tracker.isEnabled)
            {
                Vector3 TrackingCameraOrigin = Vector3.zero;
                Quaternion TrackingCameraOrientation= Quaternion.identity;
                float TrackingCameraHFOVDegrees= 0.0f;
                float TrackingCameraVFOVDegrees= 0.0f;

                // Get the HMD pose in player reference frame, Unity CS (LHS), Unity Units
                Quaternion HMDOrientation= UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);

                // Get the camera pose in player reference frame, UE4 CS (LHS), Unreal Units
                GetPositionalTrackingCameraProperties(
                        ref TrackingCameraOrigin, ref TrackingCameraOrientation,
                        ref TrackingCameraHFOVDegrees, ref TrackingCameraVFOVDegrees,
                        ref TrackingCameraNearPlane, ref TrackingCameraFarPlane);

                TrackingCameraHHalfRadians= Mathf.Deg2Rad*TrackingCameraHFOVDegrees/2.0f;
                TrackingCameraVHalfRadians= Mathf.Deg2Rad*TrackingCameraVFOVDegrees/2.0f;

                // HMDToGameCameraRotation = Undo HMD orientation THEN apply game camera orientation
                Quaternion HMDToGameCameraRotation = Quaternion.Inverse(HMDOrientation) * GameCameraOrientation;
                Quaternion TrackingCameraToGameRotation = TrackingCameraOrientation * HMDToGameCameraRotation;

                // Compute the tracking camera location in world space
                Vector3 TrackingCameraWorldSpaceOrigin =
                    HMDToGameCameraRotation*TrackingCameraOrigin + GameCameraLocation;

                // Compute the Transform to go from Rift Tracking Space (in unreal units) to World Tracking Space (in unreal units)
                TrackingSpaceToWorldSpacePosition = 
                    RHSInCMToUnityInMetersTransform() *
                    Matrix4x4.TRS(TrackingCameraWorldSpaceOrigin, TrackingCameraToGameRotation, Vector3.one);
            }
            else 
            {
                // DK2 Camera Frustum properties
                const float k_default_tracking_hfov_degrees = 74.0f; // degrees
                const float k_default_tracking_vfov_degrees = 54.0f;  // degrees
                const float k_default_tracking_distance = 1.5f; // meters
                const float k_default_tracking_near_plane_distance = 0.4f; // meters
                const float k_default_tracking_far_plane_distance = 2.5f; // meters

                // All default camera constants are in meters
                const float MetersToUU = 1.0f;

                // Pretend that the tracking camera is directly in front of the game camera
                const float FakeTrackingCameraOffset= k_default_tracking_distance * MetersToUU;
                Vector3 FakeTrackingCameraWorldSpaceOrigin = 
                    GameCameraLocation + (GameCameraOrientation*Vector3.forward)*FakeTrackingCameraOffset;

                // Get tracking frustum properties from defaults
                TrackingCameraHHalfRadians= Mathf.Deg2Rad*k_default_tracking_hfov_degrees/2.0f;
                TrackingCameraVHalfRadians= Mathf.Deg2Rad*k_default_tracking_vfov_degrees/2.0f;
                TrackingCameraNearPlane= k_default_tracking_near_plane_distance * MetersToUU;
                TrackingCameraFarPlane= k_default_tracking_far_plane_distance * MetersToUU;

                // Compute the Transform to go from Rift Tracking Space (in unreal units) to World Tracking Space (in unreal units)
                TrackingSpaceToWorldSpacePosition = 
                    // Convert from Right Handed (e.g., OpenGL, Oculus Rift native) to Unreal Left Handed coordinate system.
                    // Also scale cm to unreal units
                    RHSInCMToUnityInMetersTransform() *
                    // Put in the orientation of the game camera
                    Matrix4x4.TRS(FakeTrackingCameraWorldSpaceOrigin, GameCameraOrientation, Vector3.one);
            }

            // Transform the orientation of the controller from world space to camera space
            OrientationTransform= GameCameraOrientation;

            success = true;
        }

        return success;
    }

    private static void GetPositionalTrackingCameraProperties(
        ref Vector3 position,
        ref Quaternion rotation,
        ref float cameraHFov,
        ref float cameraVFov,
        ref float cameraNearZ,
        ref float cameraFarZ)
    {
        OVRPose ss = OVRManager.tracker.GetPose(0.0f);

        rotation = new Quaternion(ss.orientation.x,
                                  ss.orientation.y,
                                  ss.orientation.z,
                                  ss.orientation.w);

        position = new Vector3(ss.position.x,
                               ss.position.y,
                               ss.position.z);

        OVRTracker.Frustum ff = OVRManager.tracker.frustum;

        cameraHFov = ff.fov.x; // degrees
        cameraVFov = ff.fov.y; // degrees
        cameraNearZ = ff.nearZ; // meters
        cameraFarZ = ff.farZ; // meters
    }
    #endregion
};