using UnityEngine;
using System.Collections;

public class PSMoveUtility : MonoBehaviour 
{
    // Conversion from centimeters to unity units
    public static float CMToUU = 1.0f / 100.0f;
    public static float UUToCM = 100.0f;

    public static Quaternion PSMoveQuatToUnityQuat(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, q.w);
    }

    public static Matrix4x4 RHSInCMToUnityInMetersTransform()
    {
        // Convert from Right Handed (e.g., OpenGL, PSMove native) to Unity Left Handed coordinate system.
        // Also convert units from centimeters to unity units (typically 100 cm == 1 Unity Unit).

        // PSMove Coordinate System -> Unreal Coordinate system ==> (x, y, z) -> (x, y, -z)
        return Matrix4x4.Scale(new Vector3(CMToUU, CMToUU, -CMToUU));
    }

    public static bool ComputeTrackingToWorldTransforms(
        ref Matrix4x4 TrackingSpaceToWorldSpacePosition,
        ref Quaternion OrientationTransform)
    {
        float TrackingCameraNearPlane = 0.0f;
        float TrackingCameraFarPlane = 0.0f;
        float TrackingCameraHHalfRadians = 0.0f;
        float TrackingCameraVHalfRadians = 0.0f;

        return
            ComputeTrackingToWorldTransformsAndFrustum(
                ref TrackingSpaceToWorldSpacePosition, // From CM -> Unity Units
                ref OrientationTransform, // In Unity Coordinate System
                ref TrackingCameraNearPlane, // In Unity Units
                ref TrackingCameraFarPlane, // In Unity Units
                ref TrackingCameraHHalfRadians,
                ref TrackingCameraVHalfRadians);
    }

    public static bool ComputeTrackingToWorldTransformsAndFrustum(
        ref Matrix4x4 TrackingSpaceToWorldSpacePosition, // From CM -> Unity Units
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
                Quaternion TrackingCameraOrientation = Quaternion.identity;
                float TrackingCameraHFOVDegrees = 0.0f;
                float TrackingCameraVFOVDegrees = 0.0f;

                // Get the HMD pose in player reference frame, Unity CS (LHS), Unity Units
                Quaternion HMDOrientation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);

                // Get the camera pose in player reference frame, UE4 CS (LHS), Unreal Units
                GetPositionalTrackingCameraProperties(
                        ref TrackingCameraOrigin, ref TrackingCameraOrientation,
                        ref TrackingCameraHFOVDegrees, ref TrackingCameraVFOVDegrees,
                        ref TrackingCameraNearPlane, ref TrackingCameraFarPlane);

                TrackingCameraHHalfRadians = Mathf.Deg2Rad * TrackingCameraHFOVDegrees / 2.0f;
                TrackingCameraVHalfRadians = Mathf.Deg2Rad * TrackingCameraVFOVDegrees / 2.0f;

                // HMDToGameCameraRotation = Undo HMD orientation THEN apply game camera orientation
                Quaternion HMDToGameCameraRotation = Quaternion.Inverse(HMDOrientation) * GameCameraOrientation;
                Quaternion TrackingCameraToGameRotation = TrackingCameraOrientation * HMDToGameCameraRotation;

                // Compute the tracking camera location in world space
                Vector3 TrackingCameraWorldSpaceOrigin =
                    HMDToGameCameraRotation * TrackingCameraOrigin + GameCameraLocation;

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
                const float FakeTrackingCameraOffset = k_default_tracking_distance * MetersToUU;
                Vector3 FakeTrackingCameraWorldSpaceOrigin =
                    GameCameraLocation + (GameCameraOrientation * Vector3.forward) * FakeTrackingCameraOffset;

                // Get tracking frustum properties from defaults
                TrackingCameraHHalfRadians = Mathf.Deg2Rad * k_default_tracking_hfov_degrees / 2.0f;
                TrackingCameraVHalfRadians = Mathf.Deg2Rad * k_default_tracking_vfov_degrees / 2.0f;
                TrackingCameraNearPlane = k_default_tracking_near_plane_distance * MetersToUU;
                TrackingCameraFarPlane = k_default_tracking_far_plane_distance * MetersToUU;

                // Compute the Transform to go from Rift Tracking Space (in unreal units) to World Tracking Space (in unreal units)
                TrackingSpaceToWorldSpacePosition =
                    // Convert from Right Handed (e.g., OpenGL, Oculus Rift native) to Unreal Left Handed coordinate system.
                    // Also scale cm to unreal units
                    RHSInCMToUnityInMetersTransform() *
                    // Put in the orientation of the game camera
                    Matrix4x4.TRS(FakeTrackingCameraWorldSpaceOrigin, GameCameraOrientation, Vector3.one);
            }

            // Transform the orientation of the controller from world space to camera space
            OrientationTransform = GameCameraOrientation;

            success = true;
        }

        return success;
    }

    public static void GetPositionalTrackingCameraProperties(
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

    // Debug Rendering
    public static void DebugDrawHMDFrustum()
    {
        Matrix4x4 TrackingToWorldTransform = Matrix4x4.identity;
        Quaternion OrientationTransform= Quaternion.identity;
        float TrackingCameraNearPlane= 0.0f;
        float TrackingCameraFarPlane= 0.0f;
        float TrackingCameraHHalfRadians= 0.0f;
        float TrackingCameraVHalfRadians= 0.0f;

        if (ComputeTrackingToWorldTransformsAndFrustum(
                ref TrackingToWorldTransform,
                ref OrientationTransform,
                ref TrackingCameraNearPlane,
                ref TrackingCameraFarPlane,
                ref TrackingCameraHHalfRadians,
                ref TrackingCameraVHalfRadians))
        {
            float HorizontalRatio = Mathf.Tan(TrackingCameraHHalfRadians);
            float VerticalRatio = Mathf.Tan(TrackingCameraVHalfRadians);

            // Tracking camera distance given in unity units.
            // Tracking world transform goes from cm -> unity units.
            // There covert these distance to cm.
            TrackingCameraNearPlane *= UUToCM;
            TrackingCameraFarPlane *= UUToCM;

            float HalfNearWidth = TrackingCameraNearPlane * HorizontalRatio;
            float HalfNearHeight = TrackingCameraNearPlane * VerticalRatio;

            float HalfFarWidth = TrackingCameraFarPlane * HorizontalRatio;
            float HalfFarHeight = TrackingCameraFarPlane * VerticalRatio;

            Vector3 Origin = TrackingToWorldTransform.GetColumn(3);

            Vector3 NearV0 = TrackingToWorldTransform.MultiplyPoint3x4(new Vector3(HalfNearWidth, HalfNearHeight, TrackingCameraNearPlane));
            Vector3 NearV1 = TrackingToWorldTransform.MultiplyPoint3x4(new Vector3(-HalfNearWidth, HalfNearHeight, TrackingCameraNearPlane));
            Vector3 NearV2 = TrackingToWorldTransform.MultiplyPoint3x4(new Vector3(-HalfNearWidth, -HalfNearHeight, TrackingCameraNearPlane));
            Vector3 NearV3 = TrackingToWorldTransform.MultiplyPoint3x4(new Vector3(HalfNearWidth, -HalfNearHeight, TrackingCameraNearPlane));

            Vector3 FarV0 = TrackingToWorldTransform.MultiplyPoint3x4(new Vector3(HalfFarWidth, HalfFarHeight, TrackingCameraFarPlane));
            Vector3 FarV1 = TrackingToWorldTransform.MultiplyPoint3x4(new Vector3(-HalfFarWidth, HalfFarHeight, TrackingCameraFarPlane));
            Vector3 FarV2 = TrackingToWorldTransform.MultiplyPoint3x4(new Vector3(-HalfFarWidth, -HalfFarHeight, TrackingCameraFarPlane));
            Vector3 FarV3 = TrackingToWorldTransform.MultiplyPoint3x4(new Vector3(HalfFarWidth, -HalfFarHeight, TrackingCameraFarPlane));

            Debug.DrawLine(Origin, FarV0, Color.yellow);
            Debug.DrawLine(Origin, FarV1, Color.yellow);
            Debug.DrawLine(Origin, FarV2, Color.yellow);
            Debug.DrawLine(Origin, FarV3, Color.yellow);

            Debug.DrawLine(NearV0, NearV1, Color.yellow);
            Debug.DrawLine(NearV1, NearV2, Color.yellow);
            Debug.DrawLine(NearV2, NearV3, Color.yellow);
            Debug.DrawLine(NearV3, NearV0, Color.yellow);

            Debug.DrawLine(FarV0, FarV1, Color.yellow);
            Debug.DrawLine(FarV1, FarV2, Color.yellow);
            Debug.DrawLine(FarV2, FarV3, Color.yellow);
            Debug.DrawLine(FarV3, FarV0, Color.yellow);
        }
    }
}
