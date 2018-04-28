using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SunVoxExample1 : MonoBehaviour {

  /*
     You can use SunVox library freely, 
     but the following text should be included in your products (e.g. in About window):

     SunVox modular synthesizer
     Copyright (c) 2008 - 2017, Alexander Zolotov <nightradio@gmail.com>, WarmPlace.ru

     Ogg Vorbis 'Tremor' integer playback codec
     Copyright (c) 2002, Xiph.org Foundation
  */

  //
  // * Loading SunVox song from file.
  // * Playing SunVox song.
  // * Stop/Play song.
  //

  public Text Text;

  private void Start () {
    log ("-Press Space for toggle music-\n");

    try {
      int ver = SunVox.sv_init ("0", 44100, 2, 0);
      if (ver >= 0) {
        int major = (ver >> 16) & 255;
        int minor1 = (ver >> 8) & 255;
        int minor2 = (ver) & 255;
        log (String.Format ("SunVox lib version: {0}.{1}.{2}", major, minor1, minor2));

        SunVox.sv_open_slot (0);

        log ("Loading SunVox song from file...");
        var path = "Assets/StreamingAssets/test.sunvox"; // This path is correct only for standalone
        if (SunVox.sv_load (0, path) == 0) {
          log ("Loaded.");
        } else {
          log ("Load error.");
          SunVox.sv_volume (0, 256);
        }

        SunVox.sv_play_from_beginning (0);

      } else {
        log ("sv_init() error " + ver);
      }

    } catch (Exception e) {
      log ("Exception: " + e);
    }
  }

  private void log (string msg) {
    Debug.Log (msg);
    Text.text = Text.text + "\n" + msg;
  }

  private void Update () {
    if (Input.GetKeyDown (KeyCode.Space)) {
      if (SunVox.sv_end_of_song (0) == 1) {
        SunVox.sv_play (0);
      } else {
        SunVox.sv_stop (0);
      }
    }
  }

  private void FixedUpdate () {
    if (SunVox.sv_end_of_song (0) == 0) {
      Debug.LogFormat ("Line counter: {0} Module 7 -> {1} = {2}",
        (float) SunVox.sv_get_current_line2 (0) / 32,
        Marshal.PtrToStringAnsi (SunVox.sv_get_module_ctl_name (0, 7, 1)), //Get controller name
        SunVox.sv_get_module_ctl_value (0, 7, 1, 0) //Get controller value
      );
    }
  }

  private void OnDestroy () {
    if (!enabled) return;

    SunVox.sv_close_slot (0);
    SunVox.sv_deinit ();
  }

}