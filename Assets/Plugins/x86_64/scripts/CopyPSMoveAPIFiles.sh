#!/bin/bash 

PSMOVE_ROOT_PATH=../../../../../psmoveapi

cp -R $PSMOVE_ROOT_PATH/build/Debug/assets ../assets
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

cp $PSMOVE_ROOT_PATH/build/Debug/magnetometer_calibration ../
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

cp $PSMOVE_ROOT_PATH/build/Debug/libpsmoveapi.3.1.0.dylib ../psmoveapi.dylib
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

cp $PSMOVE_ROOT_PATH/build/Debug/libpsmoveapi_tracker.3.1.0.dylib ../psmoveapi_tracker.dylib 
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

cp $PSMOVE_ROOT_PATH/build/Debug/psmovepair ../
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

cp $PSMOVE_ROOT_PATH/build/Debug/test_calibration ../
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

cp $PSMOVE_ROOT_PATH/build/Debug/test_opengl ../
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

cp $PSMOVE_ROOT_PATH/build/Debug/test_tracker ../
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

cp $PSMOVE_ROOT_PATH/build/Debug/tracker_camera_calibration ../
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

cp $PSMOVE_ROOT_PATH/build/Debug/visual_coregister_dk2 ../
if [ "$?" -ne "0" ]; then
  echo "Copy failed"
  exit 1
fi

echo [SUCCESS] PSMoveAPI Debug build copy complete
